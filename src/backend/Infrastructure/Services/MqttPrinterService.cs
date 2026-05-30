using System.Buffers;
using System.Text;
using System.Text.Json;
using Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace Infrastructure.Services;

public class MqttPrinterService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDataProtector _protector;
    private readonly ILogger<MqttPrinterService> _logger;
    private IMqttClient? _mqttClient;
    private CancellationTokenSource? _cts;
    private Guid _connectedPrinterId;

    public MqttPrinterService(
        IServiceScopeFactory scopeFactory,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<MqttPrinterService> logger)
    {
        _scopeFactory = scopeFactory;
        _protector = dataProtectionProvider.CreateProtector("MqttCloudPassword");
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = Task.Run(() => ConnectLoopAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    private async Task ConnectLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await TryConnectAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[{Time}] {Message} — retrying in 30 seconds", DateTime.UtcNow, ex.Message);
            }

            try { await Task.Delay(TimeSpan.FromSeconds(30), ct); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task TryConnectAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var printerRepo = scope.ServiceProvider.GetRequiredService<IPrinterRepository>();

        var printers = await printerRepo.GetActiveAsync();
        var printer = printers.FirstOrDefault();

        if (printer is null)
        {
            _logger.LogWarning("No active printer configured — add one via the Web UI");
            throw new InvalidOperationException("No active printer configured");
        }

        string host;
        string username;
        string password;
        int port = printer.Port ?? 8883;

        if (printer.Protocol == "bambu_cloud")
        {
            if (string.IsNullOrEmpty(printer.CloudEmail) || string.IsNullOrEmpty(printer.CloudPassword))
                throw new InvalidOperationException("CloudEmail and CloudPassword are required for cloud mode");

            host = "us.mqtt.bambulab.com";
            username = printer.CloudEmail;
            var decrypted = _protector.Unprotect(printer.CloudPassword);
            password = await FetchCloudTokenAsync(username, decrypted, ct);
        }
        else
        {
            if (string.IsNullOrEmpty(printer.IpAddress) || string.IsNullOrEmpty(printer.AccessCode))
                throw new InvalidOperationException("IpAddress and AccessCode are required for LAN mode");

            host = printer.IpAddress;
            username = "bblp";
            password = printer.AccessCode;
        }

        _mqttClient?.Dispose();
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();
        _connectedPrinterId = printer.Id;

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithTlsOptions(o => o.WithCertificateValidationHandler(_ => true))
            .WithCredentials(username, password)
            .WithClientId($"spoolarr-{Guid.NewGuid():N}")
            .Build();

        var disconnectedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        _mqttClient.DisconnectedAsync += _ =>
        {
            disconnectedTcs.TrySetResult();
            return Task.CompletedTask;
        };

        await _mqttClient.ConnectAsync(options, ct);
        _logger.LogInformation("Connected to printer via {Protocol} ({Host}:{Port})", printer.Protocol, host, port);

        var serial = printer.SerialNumber ?? string.Empty;
        await _mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter($"device/{serial}/report")
            .Build(), ct);
        _logger.LogInformation("Subscribed to device/{Serial}/report", serial);

        using var reg = ct.Register(() => disconnectedTcs.TrySetResult());
        await disconnectedTcs.Task;

        if (!ct.IsCancellationRequested)
            _logger.LogWarning("[{Time}] MQTT connection dropped", DateTime.UtcNow);
    }

    private static async Task<string> FetchCloudTokenAsync(string email, string password, CancellationToken ct)
    {
        using var http = new HttpClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["account"] = email,
            ["password"] = password
        });
        var response = await http.PostAsync("https://bambulab.com/api/sign-in/form", content, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("accessToken").GetString()
               ?? throw new InvalidOperationException("Cloud auth did not return an access token");
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload.ToArray());
            using var scope = _scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IMqttMessageProcessor>();
            await processor.ProcessAsync(payload, _connectedPrinterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MQTT message");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        if (_mqttClient?.IsConnected == true)
        {
            await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("MQTT client disconnected cleanly");
        }
        _mqttClient?.Dispose();
    }
}
