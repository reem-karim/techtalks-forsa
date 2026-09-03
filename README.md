# Forsa: Link-Up Your Education

ASP.NET Core Track — TechTalks Summer Internship 2026

## Overview

Forsa connects tutors with learners who need affordable help — whether 
that's academic tutoring or a practical skill. Tutors build a profile 
showing their background and post listings for what they teach; 
learners browse, message a tutor directly, and book a session — 
replacing informal word-of-mouth searching with something structured 
and easy to trust.

**Status:** Planning and setup phase.

## MVP Scope

**In scope:**
- Tutor/learner profiles (background, bio, what they teach)
- Listings & search (browse and filter by subject/level)
- Private inbox messaging
- Booking with status tracking (requested → confirmed → complete)

**Out of scope for now** (may be revisited later):
- In-app payments
- Real-time chat
- Ratings, credential verification, AI features, mobile app

## Tech Stack

- **Frontend/Backend:** ASP.NET Core Razor Pages
- **Database:** SQL Server LocalDB (dev), EF Core Code-First
- **Auth:** ASP.NET Identity

## Team

- Reem Karim
- Rey Abou Rjeily
- Johnny Azar

Roles are assigned per feature as work is planned in Jira, not fixed 
in advance — everyone contributes across the app over the course of 
the project.

## Getting Started

**Prerequisites:** .NET SDK 8.0+, Visual Studio (ASP.NET workload), 
SQL Server LocalDB

```bash
git clone https://github.com/reem-karim/techtalks-forsa.git
cd techtalks-forsa
# Setup/run commands added here once the initial project is committed
```

## Contribution Workflow

- One branch per task: `feature/<short-description>` (e.g. `feature/profile-page`)
- Commit messages: short, present tense (e.g. "Add profile edit form")
- No direct pushes to `main` — open a pull request, one teammate reviews before merge
- Link the related Jira item in the branch name or PR when possible

## Notes

- Profile information is self-reported by users, not independently verified.
- No secrets or real user data are ever committed — use `.env.example` 
  with placeholders only, real values stay local.
  
