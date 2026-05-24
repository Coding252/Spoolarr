# Milestone 5 — Web UI

> Build the browser-based dashboard — spool list, spool detail, NFC scan page, register form, and active spool indicator.

---

## Goal

By the end of this milestone you have a fully working mobile-friendly web UI accessible on your local network. Tapping a spool with your phone triggers the scan flow and updates the UI in real time via SignalR.

---

## Depends On

- Milestone 2 — Spool API
- Milestone 3 — NFC Scan Flow

---

## Tasks

- [ ] Spool list dashboard
- [ ] Spool detail page
- [ ] NFC scan page (Web NFC + QR fallback for iOS)
- [ ] Register spool form
- [ ] Active spool indicator

---

## Pages

### 1. Dashboard — `/`

Shows all spools as cards. Each card displays:
- Color swatch (from `colorHex`)
- Brand + material + color name
- Remaining grams as a progress bar
- `ACTIVE` badge if `isActive = true`
- Last scanned timestamp

### 2. Scan page — `/scan`

The main interaction page. User opens this on their phone before loading a spool.

**Android (Web NFC):**
- "Hold phone to spool" animation
- `Start Scanning` button triggers `NDEFReader`
- On read → `POST /api/spools/scan` with tag UID
- SignalR pushes result back → show activated banner or register form

**iOS fallback:**
- QR code displayed for the active spool
- Scanning the QR opens the spool detail page directly

### 3. Register form — `/spools/register`

Shown automatically after scanning an unknown tag.
Fields: Brand, Material, Color name, Color hex (color picker), Initial weight, Notes.
Submits to `POST /api/spools`.

### 4. Spool detail — `/spools/{id}`

Shows full spool info + print history table.

### 5. Active spool indicator

Persistent header/banner across all pages showing currently active spool name, color dot, and remaining grams.

---

## Key Code Snippets

### Web NFC scan (JavaScript)

```javascript
// Only available in Chrome on Android
async function startNfcScan() {
    if (!('NDEFReader' in window)) {
        showQrFallback();
        return;
    }

    const reader = new NDEFReader();
    await reader.scan();

    reader.onreading = async ({ serialNumber }) => {
        const tagUid = serialNumber.toUpperCase();
        const response = await fetch('/api/spools/scan', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ tagUid })
        });
        const result = await response.json();
        handleScanResult(result);
    };
}

function handleScanResult(result) {
    if (result.status === 'activated') {
        showActivatedBanner(result.spool);
    } else if (result.status === 'unknown') {
        showRegisterForm(result.tagUid);
    }
}
```

### SignalR connection

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/nfc')
    .withAutomaticReconnect()
    .build();

connection.on('ScanResult', (result) => {
    handleScanResult(result);
});

await connection.start();
```

### Detect Android vs iOS

```javascript
function canUseWebNfc() {
    return 'NDEFReader' in window;
}

function showQrFallback() {
    // Generate QR from active spool URL
    // e.g. https://spoolarr.local/spools/{id}
    document.getElementById('qr-container').style.display = 'block';
}
```

---

## Docker container for Web UI

```dockerfile
# docker/Dockerfile.web
FROM node:20-alpine AS build
WORKDIR /app
COPY src/Spoolarr.Web/ .
RUN npm install && npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
```

### Add to `docker-compose.yml`

```yaml
  web:
    build:
      context: ..
      dockerfile: docker/Dockerfile.web
    ports:
      - "3000:80"
```

---

## How to Test

1. Open `https://spoolarr.local` on desktop — confirm spool list loads
2. Open `https://spoolarr.local/scan` on an Android phone
3. Tap `Start Scanning`, hold phone to a registered spool tag
4. Confirm spool activates and banner appears without page refresh
5. Open on iOS — confirm QR code fallback is shown instead
6. Scan an unknown tag — confirm register form appears pre-filled with tag UID

---

## Definition of Done

- [ ] Dashboard loads and shows all spools from API
- [ ] Active spool is visually highlighted
- [ ] Scan page works on Android with Web NFC
- [ ] QR fallback is shown on iOS / unsupported browsers
- [ ] Unknown tag scan opens register form with UID pre-filled
- [ ] Register form submits and new spool appears in dashboard
- [ ] SignalR updates dashboard in real time after scan
