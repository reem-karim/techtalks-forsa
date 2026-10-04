# Forsa: Link-Up Your Education

ASP.NET Core Track — TechTalks Summer Internship 2026

## Overview

Forsa connects people who want to trade skills for free — post what you
can teach, what you want to learn, browse other members' listings, and
propose a swap. Once a swap is accepted, message each other and
schedule a session.

**Status:** Web MVP complete.

## Features

- Register / login (ASP.NET Core Identity)
- Profile with display name, education, bio, teach/learn summary
- Guardian accounts with linked minor profiles
- Skill listings (offer to teach / want to learn), with category + search
- Browse & filter listings, suggested matches by category
- Skill-swap requests (propose, accept, decline)
- Private messaging between matched users
- Session scheduling with status tracking (Requested → Confirmed → Completed)

**Out of scope for this version:** payments, real-time chat, ratings,
credential verification, AI features, mobile app.

## Tech Stack

- **Frontend/Backend:** ASP.NET Core Razor Pages
- **Database:** SQL Server LocalDB (dev), EF Core Code-First
- **Auth:** ASP.NET Core Identity

## Team

- Reem Karim
- Rey Abou Rjeily
- Johnny Azar

## Getting Started

**Prerequisites:** .NET SDK 10, SQL Server LocalDB (Windows)

bash
git clone https://github.com/reem-karim/techtalks-forsa.git
cd techtalks-forsa
dotnet restore
dotnet ef database update
dotnet run


Open the HTTPS URL shown in the terminal, register an account, and
start exploring.

## Database

Run `dotnet ef database update` after pulling new migrations to keep
your local LocalDB schema in sync.

## Contribution Workflow

- One branch per Jira task: `feature/<JIRA-KEY>-<short-description>`
- Never push directly to `main`
- Open a Pull Request, get at least one teammate review, then merge
- Link the Jira item in the PR description

## Notes

- Profile information (education, bio) is self-reported, not
  independently verified.
- No secrets or real user data are ever committed — use `.env.example`
  with placeholders only, real values stay local.