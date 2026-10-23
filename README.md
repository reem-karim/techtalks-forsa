# Forsa — Link-Up Your Education

> A skill-swap platform where people trade knowledge for free: teach what you know, learn what you want, and connect with someone who wants the opposite.

**ASP.NET Core Track — TechTalks Summer Internship 2026**

---

## Table of Contents

1. [Overview](#overview)
2. [Problem & Target Users](#problem--target-users)
3. [Features & Status](#features--status)
4. [Technologies Used](#technologies-used)
5. [Getting Started](#getting-started)
6. [Using the App (First Run)](#using-the-app-first-run)
7. [Configuration & Secrets](#configuration--secrets)
8. [Project Structure](#project-structure)
9. [Business Rules](#business-rules)
10. [Security & Privacy](#security--privacy)
11. [Known Limitations](#known-limitations)
12. [Roadmap](#roadmap)
13. [Team](#team)
14. [Contribution Workflow](#contribution-workflow)
15. [Troubleshooting](#troubleshooting)

---

## Overview

Forsa lets members post what they **can teach** and what they **want to learn**, browse other members' listings, and propose a skill swap. When a swap request is accepted, both people can message each other privately to arrange the exchange.

## Problem & Target Users

Learning a new skill usually means paying for a course or tutor, while most people already have a skill they could share. Forsa removes the money from the equation by matching people who can teach each other.

**Target users**

- Students and young professionals who want to exchange skills (languages, programming, design, academics, sports, cooking, and more).
- Parents or guardians who want to find learning exchanges for the children they look after (via guardian accounts with linked minor profiles).

## Features & Status

| Feature | Status |
|---|---|
| Registration, login, logout (ASP.NET Core Identity) | ✅ Complete |
| Profile: display name, education, bio, "I teach" / "I want to learn" | ✅ Complete |
| Guardian accounts with linked minor profiles (name, age 5–17, grade) | ✅ Complete (simplified) |
| Skill listings — "I can teach this" / "I want to learn this" with category (create, edit, delete) | ✅ Complete |
| Browse and search listings by category and keyword | ✅ Complete |
| Suggested matches (your "learn" listings matched to others' "offer" listings by category) | ✅ Complete |
| Swap requests: propose, accept, decline, sent/received views | ✅ Complete |
| Private messaging between users with an accepted swap | ✅ Complete (page-refresh based, not real-time) |
| Concurrency and duplicate protection (double-clicks, two tabs, race conditions) | ✅ Complete |
| Session scheduling (Requested → Confirmed → Completed)

**Out of scope for this MVP:** payments, real-time chat, ratings and reviews, credential verification, AI features, mobile app, email delivery.

## Technologies Used

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core Razor Pages on .NET 10 |
| Language | C# |
| Database | SQL Server (LocalDB for development) |
| ORM | Entity Framework Core 10, Code-First migrations |
| Authentication | ASP.NET Core Identity (default UI) |
| Front end | Razor views, Bootstrap, custom CSS (`wwwroot/css/site.css`), Google Fonts (Fraunces, Inter) |
| Version control | Git and GitHub, feature branches and pull requests |
| Project management | Jira |

## Getting Started

### Prerequisites

| Requirement | Notes |
|---|---|
| [.NET SDK 10](https://dotnet.microsoft.com/download) | Check with `dotnet --version` |
| SQL Server LocalDB | **Windows only.** Included with Visual Studio ("ASP.NET and web development" workload) or SQL Server Express. macOS/Linux: see [Using SQL Server in Docker](#using-sql-server-in-docker-macos--linux) |
| [Git](https://git-scm.com/) | To clone the repository |
| Internet connection | The UI loads fonts from Google Fonts |

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/reem-karim/techtalks-forsa.git
cd techtalks-forsa

# 2. Install the EF Core command-line tool (one time per machine)
dotnet tool install --global dotnet-ef

# 3. Restore dependencies
dotnet restore

# 4. Create the database and apply all migrations
dotnet ef database update

# 5. (First time only) trust the local HTTPS development certificate
dotnet dev-certs https --trust

# 6. Run the app
dotnet run --launch-profile https
```

Open **https://localhost:7088** in your browser. (The HTTP profile uses `http://localhost:5059`.)

### Resetting the database

To start again from an empty database:

```bash
dotnet ef database drop --force
dotnet ef database update
```

### Using SQL Server in Docker (macOS / Linux)

LocalDB only exists on Windows. Elsewhere, run SQL Server in Docker and point the app at it using an environment variable (no files are edited, no secrets are committed):

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<choose-a-strong-password>" \
  -p 1433:1433 --name forsa-sql -d mcr.microsoft.com/mssql/server:2022-latest

export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=forsa;User Id=sa;Password=<choose-a-strong-password>;TrustServerCertificate=True"

dotnet ef database update
dotnet run --launch-profile https
```

## Using the App (First Run)

1. Click **Get started** and register with an email and password.
2. **Account confirmation:** the app requires a confirmed account, but no email service is configured in this MVP. After registering, the confirmation page shows a **"Click here to confirm your account"** link — click it, then log in.
3. Open **Edit profile** and set your display name.
4. Go to **My Listings → + New listing** and create at least one *"I can teach"* listing and one *"I want to learn"* listing.
5. To try the full swap flow you need **two accounts** — use a normal browser window for one and a private/incognito window for the other.
   - Account B: **Browse** → open Account A's listing → **Propose a skill swap** → **Send request**.
   - Account A: **Requests → Received** → **Accept**.
   - Either account: **Send a message** to start chatting.

## Configuration & Secrets

- `appsettings.json` contains a **LocalDB connection string that uses Windows authentication** — it holds no username, password, or API key.
- No credentials are committed to this repository. To use a different database, override the connection string without editing tracked files, using either:
  - an environment variable: `ConnectionStrings__DefaultConnection`, or
  - .NET user secrets (the project already has a `UserSecretsId`):
```bash
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>"
```
- Never commit real connection strings, passwords, or API keys.

## Project Structure

```
techtalks-forsa/
├── Program.cs                  # App startup: services, Identity, middleware
├── appsettings.json            # Configuration (no secrets)
├── Data/
│   ├── ApplicationDbContext.cs # EF Core context, indexes and constraints
│   └── Migrations/             # Code-First database migrations
├── Models/                     # Profile, MinorProfile, Listing, SwapRequest, Message, Session
├── Pages/
│   ├── Index.cshtml            # Home page
│   ├── Profile/                # View/edit profile, minor profiles
│   ├── Listings/               # Create, edit, delete my listings
│   ├── Browse/                 # Search and view other members' listings
│   ├── Matches/                # Suggested matches
│   ├── SwapRequests/           # Propose, received, sent
│   ├── Messages/               # Inbox and conversation thread
│   ├── Sessions/               # Placeholder (scheduling not built yet)
│   └── Shared/                 # Layout and partial views
└── wwwroot/                    # CSS, JS, static assets
```

## Business Rules

These rules are enforced **on the server** (not only in the browser):

- Every page except the home, privacy, error, and authentication pages requires login.
- One profile per user.
- Only *"I can teach"* listings can be the target of a swap request, and you cannot request your own listing.
- The listing you offer in exchange must be one of your own *"I can teach"* listings (or left empty).
- One open (Pending or Accepted) request per person per listing, enforced by a unique database index.
- After a request is declined, the same person must wait **7 days** before requesting that listing again.
- A request can only be answered once; double-clicks and simultaneous answers are handled gracefully.
- Messaging is available only between users who have an **accepted** swap. Existing conversations stay readable afterwards.
- Edits to listings, profiles, and requests use optimistic concurrency (`RowVersion`), so two open tabs cannot silently overwrite each other.
- Input limits are validated (e.g. messages ≤ 2000 characters, swap notes ≤ 500).

## Security & Privacy

- Authentication, password hashing, and cookies are handled by ASP.NET Core Identity.
- Pages are protected with `[Authorize]`; ownership is checked server-side before editing or deleting listings and before answering swap requests.
- Database access goes through Entity Framework Core (parameterized queries); Razor encodes output by default.
- HTTPS redirection is enabled, and HSTS is applied outside Development.
- Minor profiles store only a name, age, and optional grade, and are managed by an adult guardian — minors have no independent login.
- Profile information (education, bio) is **self-reported and not verified**.
- Email addresses are not displayed on profiles or listings; users communicate through in-app messaging, which opens only after a swap is accepted.

## Known Limitations

Stated honestly so the next developer knows where things stand:

- **Session scheduling is not implemented.** The `Sessions` table and model exist, but there are no pages for it.
- **No email delivery.** Account confirmation uses the on-screen link described above; password reset by email will not work until an email sender is configured.
- **Messaging is not real-time.** The page must be refreshed to see new messages.
- **Windows-centric default setup** (LocalDB). Other platforms need the Docker option above.
- **Not deployed.** The app runs locally only.
- **No automated test suite yet.** Validation so far has been manual testing and bug-report-driven fixes.
- Profile and credential information is not verified, and there is no report/block feature.

## Roadmap

- [ ] Session scheduling pages (propose a time, confirm, mark completed)
- [ ] Email sender (account confirmation, password reset, notifications)
- [ ] Real-time chat (SignalR)
- [ ] Ratings and reviews after completed sessions
- [ ] Automated unit and integration tests
- [ ] Deployment (cloud hosting and hosted database)
- [ ] Moderation tools (report and block users)
- [ ] Smarter matching beyond same-category

## Team

| Member | Main contributions |
|---|---|
| **Reem Karim** | Project setup (Razor Pages + Identity), guardian/minor profile flow, listings (CRUD), browse & search, messaging, navigation and UI/UX polish, repository and pull request management |
| **Rey Abou Rjeily** | Profile edit form, design tweaks, concurrency/duplicate/race-condition fixes from the bug report |
| **Johnny Azar** | Profile model and migration, swap request flow (propose / accept / decline), session data model and migration |

Detailed history: see the [commit log](https://github.com/reem-karim/techtalks-forsa/commits/main) and merged pull requests.

## Contribution Workflow

- One branch per Jira task: `feature/<JIRA-KEY>-<short-description>`
- Never push directly to `main`
- Open a pull request, get at least one teammate review, then merge
- Link the Jira item in the pull request description
- After pulling new migrations, run `dotnet ef database update`

## Troubleshooting

| Problem | Fix |
|---|---|
| `Install the [x.y.z] .NET SDK or update global.json` | Install .NET SDK 10, or adjust the version in `global.json` to match your installed SDK |
| `dotnet ef` is not recognized | Run `dotnet tool install --global dotnet-ef`, then reopen the terminal |
| `Cannot open database` / pending migrations | Run `dotnet ef database update` |
| LocalDB not found / cannot connect | Install LocalDB (Windows) or use the Docker option above |
| Browser warns about an untrusted certificate | Run `dotnet dev-certs https --trust` |
| Cannot log in after registering | Confirm your account using the on-screen link (see [Using the App](#using-the-app-first-run)) |
| Port already in use | Stop the other process, or change the ports in `Properties/launchSettings.json` |

---

*Forsa — TechTalks Summer Internship 2026.*