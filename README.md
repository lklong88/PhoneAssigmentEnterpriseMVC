# CUSoCal Phone Assignments — ASP.NET Core MVC

**Interview demonstration only — fictional data.** This is not an official CU SoCal product. Keep the repository private because the original exercise is confidential.

## Features

- ASP.NET Core 8 MVC and Razor responsive interface
- View, assign, change, and remove phone numbers
- One phone per employee, one employee per phone; server-side validation
- Anti-forgery protection and user-facing success/error messages
- Repository abstraction + JSON persistence
- Demo audit history with UTC timestamps
- Demo-password cookie login when configured; mandatory in Production
- xUnit business-rule tests and GitHub Actions CI
- Dockerfile + Render Blueprint

## Prerequisites

- Visual Studio 2022 with **ASP.NET and web development** workload and .NET 8 SDK, or .NET 8 SDK CLI
- Docker Desktop optional for container testing

## Run locally (Visual Studio)

1. Extract ZIP; open `CUSoCalPhoneAssignments.sln`.
2. Set `CUSoCalPhoneAssignments.Web` as the startup project.
3. Press F5. The HTTP profile is `http://localhost:5080`.
4. By default, development mode allows access without a password. To exercise login locally, set `DEMO_PASSWORD` in your terminal/user secrets and restart.
5. The first run copies `Data/seed.json` to `Data/assignments.json`. To reset, stop the app and delete `Data/assignments.json`.

## Run locally (CLI)

```bash
dotnet restore CUSoCalPhoneAssignments.sln
dotnet build CUSoCalPhoneAssignments.sln
# Using launchSettings.json with Development profile:
dotnet run --project src/CUSoCalPhoneAssignments.Web --launch-profile http
```

Browse `http://localhost:5080`. Health check: `http://localhost:5080/health`.

### Optional local password

PowerShell:

```powershell
$env:DEMO_PASSWORD = 'choose-a-strong-demo-password'
dotnet run --project src/CUSoCalPhoneAssignments.Web --launch-profile http
```

The password is read from the environment, never committed to Git.

## Run tests

```bash
dotnet test CUSoCalPhoneAssignments.sln -c Release --logger "trx;LogFileName=unit-tests.trx"
```

Detailed test cases and blank manual execution record: `docs/TEST_PLAN.md`.

## Run in Docker

```bash
docker build -t cusocal-phone-assignments .
docker run --rm -p 10000:10000 -e DEMO_PASSWORD='choose-a-strong-demo-password' cusocal-phone-assignments
```

Open `http://localhost:10000`. Docker's Production cookie is `Secure`, so **login requires HTTPS**; for a fully functional local Docker demo, use an HTTPS reverse proxy or run locally with the .NET Development profile instead. Render provides HTTPS on the public URL.

## Deploy to Render.com

1. Push the project to a **private GitHub repository**.
2. On Render, create a **New Web Service** from the repository, choose **Docker** runtime (or create a Blueprint from `render.yaml`).
3. Use Dockerfile `./Dockerfile`, root directory repository root, and a Free plan if available.
4. **Set environment variable `DEMO_PASSWORD`** to a strong unique password in Render's dashboard. The Production app intentionally refuses to start without it.
5. Deploy. Render provides a `https://...onrender.com` URL; open it and sign in with the password.
6. `/health` provides a simple unauthenticated health check.

**Storage limitation:** Free Render containers have ephemeral local storage; assignment changes can reset after a restart or redeploy. The app accepts `DATA_DIRECTORY` for an external persistent disk on eligible paid Render instances, but this JSON repository is still single-instance only. Do not present cloud data retention as guaranteed on Free.

**Note:** The Dockerfile sets `DATA_DIRECTORY=/tmp/cusocal-data`, which is writable by the non-root runtime user. To use a persistent disk, override `DATA_DIRECTORY` to its mounted path and ensure permissions allow writes.

## Structure

```text
CUSoCalPhoneAssignments.sln
src/CUSoCalPhoneAssignments.Web/
  Controllers/      HomeController, AccountController
  Models/           AssignmentModels
  Services/         AssignmentService
  Repositories/     IAssignmentRepository, JsonAssignmentRepository
  Views/            Razor views and layout
  Data/seed.json    Supplied fictional data
  wwwroot/css/      Responsive UI styling
tests/CUSoCalPhoneAssignments.Tests/   xUnit tests
docs/                 Test plan, architecture, interview walkthrough, submission checklist
Dockerfile, render.yaml, .github/workflows/ci.yml
```

## Security / limitations

- The demo-password login is a **simple gate**, not enterprise-grade authentication, SSO, MFA, or RBAC.
- Audit actor is a generic demo identity, not a verified individual.
- No confidential data, secrets, or real banking information should be added.
- Render Free has an ephemeral filesystem; use a transactional database for production.
- Anti-forgery tokens are included in MVC POST forms; Razor encodes text output.

## Verification status

**Corrected the missing `Microsoft.AspNetCore.RateLimiting` namespace that caused Render error CS1061, and statically reviewed the package; not compiled or executed in the authoring environment.** No .NET SDK or network access was available to restore NuGet packages. Automated tests are provided but **have not been run** here. Build, test, and fill out the manual test record before submission.
