using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class FilamentDbContext(DbContextOptions<FilamentDbContext> options) : DbContext(options)
{
}
