# Milestone 1 — Data Model

> Define the database entities, run EF Core migrations, set up repositories, and seed test data.

---

## Goal

By the end of this milestone the database has four tables — `Spools`, `Printers`, `PrintJobs`, and `NfcTags` — with the correct fields, relationships, and indexes. Repositories are in place to read and write data. Seed data exists so the app can be tested without manually inserting rows.

---

## Depends On

- Milestone 0 — Project Bootstrap

---

## Tasks

### Models
- [ ] Create `Spool` entity class inside `src/back-end/Domain/Models/`
- [ ] Create `Printer` entity class inside `src/back-end/Domain/Models/`
- [ ] Create `PrintJob` entity class inside `src/back-end/Domain/Models/`
- [ ] Create `NfcTag` entity class inside `src/back-end/Domain/Models/`

### Spool fields
- [ ] `Id` — Guid, primary key
- [ ] `Brand` — string
- [ ] `Material` — string
- [ ] `ColorName` — string
- [ ] `ColorHex` — string
- [ ] `InitialWeightG` — float
- [ ] `CurrentWeightG` — float
- [ ] `SpoolWeightG` — float, default 200
- [ ] `DiameterMm` — float, default 1.75
- [ ] `LowStockThresholdG` — float, default 100
- [ ] `IsActive` — bool, default false
- [ ] `IsArchived` — bool, default false
- [ ] `CreatedAt` — DateTime
- [ ] `LastScannedAt` — DateTime, nullable
- [ ] `Notes` — string, nullable
- [ ] `NfcTags` — navigation property to `NfcTag` (one spool can have one or more tags)
- [ ] `PrintJobs` — navigation property to `PrintJob`

### Printer fields
- [ ] `Id` — Guid, primary key
- [ ] `Name` — string, user label (e.g. "Garage X1C")
- [ ] `Brand` — string (e.g. "Bambu Lab", "Prusa", "Voron")
- [ ] `Model` — string (e.g. "X1 Carbon", "MK4", "P1S")
- [ ] `SerialNumber` — string, nullable
- [ ] `IpAddress` — string
- [ ] `Protocol` — string (values: `"bambu-mqtt"`, `"moonraker"`, `"prusalink"`, `"octoprint"`)
- [ ] `AccessCode` — string, nullable (Bambu LAN access code or equivalent)
- [ ] `Port` — int, nullable
- [ ] `HasAms` — bool, default false
- [ ] `AmsSlotCount` — int, default 0
- [ ] `IsActive` — bool, default true
- [ ] `LastSeenAt` — DateTime, nullable
- [ ] `CreatedAt` — DateTime
- [ ] `Notes` — string, nullable
- [ ] `PrintJobs` — navigation property to `PrintJob`

### PrintJob fields
- [ ] `Id` — Guid, primary key
- [ ] `PrinterId` — Guid, foreign key to `Printer`
- [ ] `SpoolId` — Guid, foreign key to `Spool`
- [ ] `PrintFileName` — string, nullable
- [ ] `Status` — string (values: `"running"`, `"completed"`, `"failed"`, `"interrupted"`)
- [ ] `GramsUsed` — float, default 0
- [ ] `LastReportedGramsUsed` — float, default 0
- [ ] `StartedAt` — DateTime
- [ ] `FinishedAt` — DateTime, nullable
- [ ] `LastUpdatedAt` — DateTime
- [ ] `Source` — string, default `"mqtt"` (values: `"mqtt"`, `"manual"`)
- [ ] `Notes` — string, nullable
- [ ] `Printer` — navigation property to `Printer`
- [ ] `Spool` — navigation property to `Spool`

### NfcTag fields
- [ ] `Id` — Guid, primary key
- [ ] `TagUid` — string, unique (physical NFC tag UID, e.g. "04:A1:B2:C3")
- [ ] `Type` — string (values: `"ntag215"`, `"bambu"`, `"openprinttag"`, `"opentag3d"`)
- [ ] `SpoolId` — Guid, foreign key to `Spool`
- [ ] `CreatedAt` — DateTime
- [ ] `Spool` — navigation property to `Spool`

> NFC tags are read-only in Spoolarr — the app never writes to them.

### Database context
- [ ] Add `DbSet<Spool>` to `FilamentDbContext`
- [ ] Add `DbSet<Printer>` to `FilamentDbContext`
- [ ] Add `DbSet<PrintJob>` to `FilamentDbContext`
- [ ] Add `DbSet<NfcTag>` to `FilamentDbContext`
- [ ] Configure unique index on `NfcTag.TagUid`
- [ ] Configure foreign key relationship between `NfcTag` and `Spool`
- [ ] Configure foreign key relationship between `PrintJob` and `Spool`
- [ ] Configure foreign key relationship between `PrintJob` and `Printer`
- [ ] Configure cascade delete — deleting a spool deletes its NFC tags
- [ ] Configure cascade delete — deleting a spool deletes its print jobs
- [ ] Configure cascade delete — deleting a printer deletes its print jobs

### Migrations
- [ ] Run `dotnet ef migrations add InitialCreate` — run from the `Infrastructure` project
- [ ] Run `dotnet ef database update` — run from the `Infrastructure` project
- [ ] Confirm `Spools`, `Printers`, `PrintJobs`, and `NfcTags` tables are created in the SQLite file

### Spool repository
- [ ] Create `ISpoolRepository` interface inside `src/back-end/Infrastructure/Repositories/`
- [ ] Create `SpoolRepository` class implementing `ISpoolRepository`
- [ ] Add `GetAllAsync` — return all non-archived spools ordered by last scanned
- [ ] Add `GetByIdAsync` — return spool by ID including NFC tags and print jobs
- [ ] Add `GetActiveAsync` — return the currently active spool
- [ ] Add `CreateAsync` — insert new spool
- [ ] Add `UpdateAsync` — update existing spool
- [ ] Add `ArchiveAsync` — set `IsArchived = true`
- [ ] Add `DeleteAsync` — delete spool by ID

### Printer repository
- [ ] Create `IPrinterRepository` interface inside `src/back-end/Infrastructure/Repositories/`
- [ ] Create `PrinterRepository` class implementing `IPrinterRepository`
- [ ] Add `GetAllAsync` — return all printers ordered by name
- [ ] Add `GetByIdAsync` — return printer by ID
- [ ] Add `GetActiveAsync` — return all printers where `IsActive = true`
- [ ] Add `CreateAsync` — insert new printer
- [ ] Add `UpdateAsync` — update existing printer
- [ ] Add `DeleteAsync` — delete printer by ID

### PrintJob repository
- [ ] Create `IPrintJobRepository` interface inside `src/back-end/Infrastructure/Repositories/`
- [ ] Create `PrintJobRepository` class implementing `IPrintJobRepository`
- [ ] Add `GetBySpoolIdAsync` — return all print jobs for a spool ordered by date
- [ ] Add `GetByPrinterIdAsync` — return all print jobs for a printer ordered by date
- [ ] Add `GetRunningAsync` — return all jobs with status `"running"`
- [ ] Add `GetByIdAsync` — return job by ID
- [ ] Add `CreateAsync` — insert new print job
- [ ] Add `UpdateAsync` — update existing print job (status, grams, finished date)

### NfcTag repository
- [ ] Create `INfcTagRepository` interface inside `src/back-end/Infrastructure/Repositories/`
- [ ] Create `NfcTagRepository` class implementing `INfcTagRepository`
- [ ] Add `GetByTagUidAsync` — return NFC tag by its physical UID, includes the linked Spool
- [ ] Add `GetBySpoolIdAsync` — return all NFC tags for a spool
- [ ] Add `CreateAsync` — insert new NFC tag linked to a spool
- [ ] Add `DeleteAsync` — delete NFC tag by ID

### Dependency injection
- [ ] Register `ISpoolRepository` → `SpoolRepository` in `Program.cs`
- [ ] Register `IPrinterRepository` → `PrinterRepository` in `Program.cs`
- [ ] Register `IPrintJobRepository` → `PrintJobRepository` in `Program.cs`
- [ ] Register `INfcTagRepository` → `NfcTagRepository` in `Program.cs`

### Seed data
- [ ] Create `SeedData` class inside `src/back-end/Infrastructure/Data/`
- [ ] Add at least 2 test spools with different materials and colors
- [ ] Add 1 test NFC tag linked to one of the test spools
- [ ] Add 1 test printer with placeholder values
- [ ] Call `SeedData` on app startup only if the database is empty
- [ ] Run migrations automatically on app startup
- [ ] Handle the case where the database file already exists — do not re-seed

### Error handling
- [ ] Wrap migration on startup in try/catch — log error and continue if migration fails
- [ ] Log a clear message if the database file cannot be created or accessed

---

## Definition of Done

- [ ] `Spools`, `Printers`, `PrintJobs`, and `NfcTags` tables exist in the SQLite database
- [ ] `NfcTag.TagUid` has a unique index
- [ ] All foreign keys and cascade deletes work correctly
- [ ] All 4 repositories registered in DI and resolve without errors
- [ ] Seed data inserts test rows on first run only
- [ ] `dotnet ef migrations list` shows `InitialCreate` as applied
- [ ] Inserting a duplicate `TagUid` throws a database constraint error
- [ ] Deleting a spool also deletes its NFC tags and print jobs
- [ ] Deleting a printer also deletes its print jobs