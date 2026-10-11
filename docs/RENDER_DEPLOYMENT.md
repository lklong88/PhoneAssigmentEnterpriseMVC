# Deploy to Render — corrected MVC build

## Fixed build error

The original version failed with `CS1061: RateLimiterOptions does not contain a definition for AddFixedWindowLimiter` because `Program.cs` lacked `using Microsoft.AspNetCore.RateLimiting;`. This corrected project includes the import.

## Publish from GitHub

1. Extract the archive. Commit the **contents** of the extracted project root to a **private** GitHub repository. Ensure `Dockerfile` and `render.yaml` are at the repository root.
2. In Render, create **New > Web Service**, connect your repository, and select **Docker**. Dockerfile path: `./Dockerfile`. Root directory: leave blank.
3. In **Environment**, add `DEMO_PASSWORD` with a strong random value (do not commit it). Production startup requires this setting.
4. Deploy. The container listens on `0.0.0.0:10000`; Render supplies HTTPS externally. `/health` should return `{"status":"ok"}`.
5. Visit your Render URL, sign in using `DEMO_PASSWORD`, and test assignment, change, removal, duplicate rejection, and page refresh.

## Local build and tests

```sh
dotnet restore CUSoCalPhoneAssignments.sln
dotnet build CUSoCalPhoneAssignments.sln -c Release --no-restore
dotnet test CUSoCalPhoneAssignments.sln -c Release --no-build
dotnet run --project src/CUSoCalPhoneAssignments.Web --launch-profile http
```

Open `http://localhost:5080`.

## Important caveats

- **Not verified by an actual .NET build in the packaging environment**; use GitHub Actions and Render logs to confirm.
- Free Render storage is ephemeral; changes can reset on restart or redeploy. `DATA_DIRECTORY` may point to a mounted persistent disk on eligible plans.
- Login is a shared demo password, not enterprise identity. Use fictional data only. Keep the repository private.
- The supplied unit tests require NuGet package restoration.
