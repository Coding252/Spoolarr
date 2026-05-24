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

### Project setup
- [ ] Decide on frontend approach — React or plain HTML/JS
- [ ] Create `Spoolarr.Web` project inside `src/`
- [ ] Add `Spoolarr.Web` to `Spoolarr.sln`
- [ ] Install SignalR JavaScript client library
- [ ] Create `Dockerfile.web` inside `docker/`
- [ ] Add `web` service to `docker-compose.yml`
- [ ] Configure Caddy to serve the web UI alongside the API

### Pages and routing
- [ ] Set up client-side routing with the following routes:
  - [ ] `/` — dashboard
  - [ ] `/scan` — NFC scan page
  - [ ] `/spools/:id` — spool detail
  - [ ] `/spools/register` — register new spool form

### Active spool indicator
- [ ] Create a persistent header component shown on all pages
- [ ] Fetch and display currently active spool name
- [ ] Show spool color dot using `colorHex`
- [ ] Show remaining grams
- [ ] Show `No active spool` state when none is set
- [ ] Update automatically when a scan activates a new spool

### Dashboard page — `/`
- [ ] Fetch all spools from `GET /api/spools`
- [ ] Display each spool as a card
- [ ] Show color swatch using `colorHex`
- [ ] Show brand, material, and color name
- [ ] Show remaining grams as a progress bar
- [ ] Show `ACTIVE` badge on the currently active spool
- [ ] Show `LOW` warning badge when below `lowStockThresholdG`
- [ ] Show last scanned timestamp
- [ ] Link each card to the spool detail page

### Spool detail page — `/spools/:id`
- [ ] Fetch spool from `GET /api/spools/{id}`
- [ ] Display all spool fields
- [ ] Show print history as a table — date, grams used, print name, source
- [ ] Show total grams used across all prints

### NFC scan page — `/scan`
- [ ] Detect if Web NFC is available via `'NDEFReader' in window`
- [ ] Android path — show pulsing animation and `Start Scanning` button
- [ ] On button tap — request NFC permission and start `NDEFReader`
- [ ] On tag read — send tag UID to `POST /api/spools/scan`
- [ ] iOS / unsupported path — show QR code for the active spool URL
- [ ] QR code encodes `https://spoolarr.local/spools/{id}`
- [ ] Connect to SignalR hub at `/hubs/nfc`
- [ ] Listen for `ScanResult` event from SignalR
- [ ] On `"activated"` result — show success banner with spool name and color
- [ ] On `"unknown"` result — redirect to register form with tag UID pre-filled

### Register spool form — `/spools/register`
- [ ] Pre-fill `NfcTagUid` from scan if coming from scan page
- [ ] Input field for brand
- [ ] Input field for material
- [ ] Input field for color name
- [ ] Color picker for color hex
- [ ] Number input for initial weight in grams
- [ ] Textarea for notes, optional
- [ ] Submit to `POST /api/spools`
- [ ] On success — redirect to dashboard and activate new spool
- [ ] Show validation errors inline

### Environment variables
- [ ] Create `.env.development` with `VITE_API_URL=http://localhost:5000` (or equivalent)
- [ ] Create `.env.production` with `VITE_API_URL=https://spoolarr.local`
- [ ] Use environment variable for all API base URL references — no hardcoded URLs
- [ ] Use environment variable for SignalR hub URL

### Docker
- [ ] `Dockerfile.web` builds and serves the frontend
- [ ] Frontend is accessible at `https://spoolarr.local`
- [ ] Confirm environment variables are correctly injected at build time inside Docker

---

## Definition of Done

- [ ] Dashboard loads and shows all spools from the API
- [ ] Active spool is visually highlighted with badge and color
- [ ] Low stock spools show a warning badge
- [ ] Scan page works on Android with Web NFC
- [ ] QR fallback is shown on iOS and unsupported browsers
- [ ] Unknown tag scan opens register form with UID pre-filled
- [ ] Register form submits and new spool appears in dashboard
- [ ] SignalR updates the active spool indicator in real time after scan
- [ ] All pages are mobile-friendly
- [ ] API base URL is driven by environment variable — no hardcoded URLs
- [ ] Frontend works correctly in both local dev and Docker environments
