# Forsa: Link-Up Your Education

> A skill-swap platform where people trade knowledge for free: teach what you know, learn what you want, and connect with someone who wants the opposite.

**ASP.NET Core Track, TechTalks Summer Internship 2026**

---

## Table of Contents

1. [Overview](#overview)
2. [Problem & Target Users](#problem--target-users)
3. [Features & Status](#features--status)
4. [User Flow](#user-flow)
5. [Technologies Used](#technologies-used)
6. [Getting Started](#getting-started)
7. [Trying the App (Demo Walkthrough)](#trying-the-app-demo-walkthrough)
8. [Configuration & Secrets](#configuration--secrets)
9. [Project Structure](#project-structure)
10. [Business Rules](#business-rules)
11. [Testing & Validation](#testing--validation)
12. [Security & Privacy](#security--privacy)
13. [Known Limitations](#known-limitations)
14. [Roadmap](#roadmap)
15. [Team](#team)
16. [Contribution Workflow](#contribution-workflow)
17. [Troubleshooting](#troubleshooting)

---

## Overview

Forsa lets members post what they **can teach** and what they **want to learn**, browse other members' listings, and propose a skill swap. When a swap request is accepted, both people can message each other privately and schedule a session to arrange the exchange.

## Problem & Target Users

Learning a new skill usually means paying for a course or tutor, while most people already have a skill they could share. Forsa removes the money from the equation by matching people who can teach each other.

**Target users**

- Students and young professionals who want to exchange skills (languages, programming, design, academics, sports, cooking, and more).
- Parents or guardians who want to find learning exchanges for the children they look after (via guardian accounts with linked minor profiles).

## Features & Status

| Feature | Status |
|---|---|
| Registration, login, logout (ASP.NET Core Identity) | Complete |
| Guided first-time setup: register, create profile, add skills, find a swap | Complete |
| Profile: display name, bio, optional education | Complete |
| Guardian accounts with linked minor profiles (name, age 5 to 17, grade) | Complete (simplified) |
| Skill listings, "I can teach this" / "I want to learn this", with category (create, edit, delete) | Complete |
| Browse and search listings by category and keyword | Complete |
| "Matched for you": your "learn" listings matched to others' "offer" listings by category | Complete |
| Swap requests: propose, accept, decline, sent/received views | Complete |
| Add a skill to offer directly while proposing a swap | Complete |
| Private messaging between users with an accepted swap | Complete (refresh-based, not real-time) |
| Session scheduling (Requested, Confirmed, Completed, Cancelled) | Complete |
| Concurrency and duplicate protection (double-clicks, two tabs, race conditions) | Complete |
| Simplified navigation and a "next step" home page for signed-in users | Complete |

**Out of scope for this MVP:** payments, real-time chat, ratings and reviews, credential verification, AI features, mobile app, email delivery, deployment.

## User Flow

1. **Register**: you are signed in immediately (no email confirmation, see [Known Limitations](#known-limitations)).
2. **Set up your profile**: only a display name is required. Anyone signed in without a profile is redirected here first.
3. **Add your skills**: one skill you can teach and one you want to learn (both optional, can be skipped).
4. **Find a swap**: *Find a skill* has two tabs, *All skills* and *Matched for you*.
5. **Propose a swap**: choose one of your skills to offer (or add a new one on the spot) and write a message.
6. **Requests**: the owner accepts or declines under *Requests → Received*. The requester follows it under *Sent*.
7. **Message and schedule**: after acceptance, both people can message each other and propose a session time (*Requests → Sessions*).

The top navigation has four items: **Find a skill**, **My skills**, **Requests** (Received, Sent, Sessions), and **Messages**.

## Technologies Used

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core Razor Pages on .NET 10 |
| Language | C# |
| Database | SQL Server (LocalDB for development) |
| ORM | Entity Framework Core 10, Code-First migrations |
| Authentication | ASP.NET Core Identity (default UI) |
| Front end | Razor views, Bootstrap, custom CSS (`wwwroot/css/site.css`, `wwwroot/css/flow.css`), Google Fonts (Fraunces, Inter) |
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

## Trying the App (Demo Walkthrough)

The swap flow needs **two accounts**. Use a normal browser window for Anna and a private/incognito window for Sam.

1. **Anna** registers (`anna@test.com`, any password with an uppercase letter, a number and a symbol). She lands on the profile step, enters her name, then on the skills step adds *I can teach: Origami for beginners* (Art & Crafts) and *I want to learn: Python basics* (Technology - Programming).
2. **Sam** registers, sets his name, and adds *I can teach: Python basics* (Technology - Programming) and *I want to learn: Origami for beginners* (Art & Crafts).
3. Sam sees Anna's origami listing under **Matched for you**, opens it, clicks **Propose a skill swap**, picks *Python basics* as his offer, writes a message, and sends the request.
4. Anna opens **Requests → Received** and clicks **Accept**.
5. Either of them clicks **Send a message** to chat, or **Schedule a session** to propose a time. The other person confirms it under **Requests → Sessions**.

## Configuration & Secrets

- `appsettings.json` contains a **LocalDB connection string that uses Windows authentication**: it holds no username, password, or API key.
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
├── Program.cs                  # App startup: services, Identity, middleware, page filter
├── appsettings.json            # Configuration (no secrets)
├── Data/
│   ├── ApplicationDbContext.cs # EF Core context, indexes and constraints
│   └── Migrations/             # Code-First database migrations
├── Infrastructure/
│   └── ProfileSetupFilter.cs   # Redirects signed-in users without a profile to profile setup
├── Models/                     # Profile, MinorProfile, Listing, SwapRequest, Message, Session
├── Pages/
│   ├── Index.cshtml            # Home page ("your next step" for signed-in users)
│   ├── Welcome/                # Add-your-skills step of first-time setup
│   ├── Profile/                # View/edit profile, minor profiles
│   ├── Listings/               # Create, edit, delete my skills
│   ├── Browse/                 # Search and view other members' listings
│   ├── Matches/                # "Matched for you"
│   ├── SwapRequests/           # Propose, received, sent
│   ├── Messages/               # Inbox and conversation thread
│   ├── Sessions/               # Propose, confirm, complete, cancel sessions
│   └── Shared/                 # Layout and partial views (tabs)
└── wwwroot/                    # CSS, JS, static assets
```

## Business Rules

These rules are enforced **on the server** (not only in the browser):

- Every page except the home, privacy, error, and authentication pages requires login.
- A signed-in user without a profile name is redirected to profile setup before using the app.
- One profile per user.
- Only *"I can teach"* listings can be the target of a swap request, and you cannot request your own listing.
- The listing you offer in exchange must be one of your own *"I can teach"* listings (or left empty). A new skill added during the proposal is saved as one of your own listings.
- One open (Pending or Accepted) request per person per listing, enforced by a unique database index.
- After a request is declined, the same person must wait **7 days** before requesting that listing again.
- A request can only be answered once; double-clicks and simultaneous answers are handled gracefully.
- Messaging is available only between users who have an **accepted** swap. Existing conversations stay readable afterwards.
- A session can only be scheduled for an **accepted** swap, only by the two people in it, and only for a time in the future.
- One active session per swap. Allowed changes: Requested to Confirmed to Completed, or Requested/Confirmed to Cancelled. A cancelled session can be rescheduled.
- Edits to listings, profiles, and requests use optimistic concurrency (`RowVersion`), so two open tabs cannot silently overwrite each other.
- Input limits are validated (e.g. messages up to 2000 characters, swap notes up to 500).

## Testing & Validation

- **Manual end-to-end testing** of the full flow with two accounts (register, profile, skills, browse, propose, accept, message, schedule a session).
- **Edge cases tested by hand:** double-clicking submit buttons, two browser tabs acting on the same item, proposing a swap with no skill of your own, requesting your own listing, invalid or empty input, and accessing pages while logged out.
- **Bug-report-driven fixes:** race conditions, duplicates and state inconsistencies from the internal bug report were fixed (PR #12).
- **No automated test suite yet** (see [Roadmap](#roadmap)).

## Security & Privacy

- Authentication, password hashing, and cookies are handled by ASP.NET Core Identity.
- Pages are protected with `[Authorize]`; ownership and participation are checked server-side before editing or deleting listings, answering swap requests, messaging, and changing sessions.
- Database access goes through Entity Framework Core (parameterized queries); Razor encodes output by default.
- HTTPS redirection is enabled, and HSTS is applied outside Development.
- Minor profiles store only a name, age, and optional grade, and are managed by an adult guardian. Minors have no independent login.
- Profile information (education, bio) is **self-reported and not verified**.
- Email addresses are not displayed on profiles or listings; users communicate through in-app messaging, which opens only after a swap is accepted.
- No secrets are stored in the repository (see [Configuration & Secrets](#configuration--secrets)).

## Known Limitations

Stated honestly so the next developer knows where things stand:

- **No email delivery, so email confirmation is turned off.** Accounts work immediately after registering (`RequireConfirmedAccount = false` in `Program.cs`). This means emails are not verified, and password reset by email will not work until an email sender is configured.
- **Sessions:** either person in a swap can confirm a proposed session, including the one who proposed it. There are no notifications, and times are compared with the server's local clock (no time-zone handling).
- **Messaging is not real-time.** The page must be refreshed to see new messages.
- **Matching is basic:** same category only.
- **Windows-centric default setup** (LocalDB). Other platforms need the Docker option above.
- **Not deployed.** The app runs locally only.
- **No automated test suite.** Validation was manual.
- Profile and credential information is not verified, and there is no report/block feature.
- Some older database columns (`Teach` / `WantToLearn` on the profile) still exist but are no longer used. Skills now live in listings.

## Roadmap

- [x] Session scheduling pages (propose a time, confirm, mark completed, cancel)
- [x] Guided first-time setup flow and simplified navigation
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
| **Reem Karim** | Project setup (Razor Pages + Identity), guardian/minor profile flow, listings (CRUD), browse & search, messaging, navigation and UI/UX polish |
| **Rey Abou Rjeily** | Profile edit form, design tweaks, concurrency/duplicate/race-condition fixes from the bug report |
| **Johnny Azar** | Profile model and migration, swap request flow (propose / accept / decline), session data model and migration |


Detailed history: see the [commit log](https://github.com/reem-karim/techtalks-forsa/commits/main) and merged pull requests.

## Contribution Workflow

- One branch per Jira task: `feature/<JIRA-KEY>-<short-description>` (sometimes one branch = the whole epic)
- Never push directly to `main`
- Open a pull request, get at least one teammate review, then merge
- Link the Jira item in the pull request description
- After pulling new migrations, run `dotnet ef database update`

## Note

- You may find merged pull-requests unassigned and directly merged, we were all learning together so we thought there was no need to assign someone to review (we will learn to do better next time)

## Troubleshooting

| Problem | Fix |
|---|---|
| `Install the [x.y.z] .NET SDK or update global.json` | Install .NET SDK 10, or adjust the version in `global.json` to match your installed SDK |
| `dotnet ef` is not recognized | Run `dotnet tool install --global dotnet-ef`, then reopen the terminal |
| `Cannot open database` / pending migrations | Run `dotnet ef database update` |
| LocalDB not found / cannot connect | Install LocalDB (Windows) or use the Docker option above |
| Browser warns about an untrusted certificate | Run `dotnet dev-certs https --trust` |
| Password rejected when registering | Use at least 6 characters with an uppercase letter, a lowercase letter, a number and a symbol |
| Stuck on the profile page after registering | Enter a display name and save; setup is required once |
| Port already in use | Stop the other process, or change the ports in `Properties/launchSettings.json` |

---

*Forsa, TechTalks Summer Internship 2026.*