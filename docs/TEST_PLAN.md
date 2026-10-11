# Test plan — phone-number assignment MVP

## Scope
Test the three required user workflows: view, assign/change, and remove phone-number assignments. Validate negative paths, persistence, and deployment behavior. Sample names and phone numbers are fictional.

## Automated unit tests

Run from the repository root:

```bash
dotnet restore CUSoCalPhoneAssignments.sln
dotnet build CUSoCalPhoneAssignments.sln -c Release
dotnet test CUSoCalPhoneAssignments.sln -c Release --logger "trx;LogFileName=unit-tests.trx"
```

The `tests/CUSoCalPhoneAssignments.Tests/AssignmentServiceTests.cs` suite covers:

| ID | Scenario | Expected result |
| --- | --- | --- |
| UT-01 | Load initial dashboard | Three members, two assignments, four numbers |
| UT-02 | Assign available phone to Michael | Assignment saved; audit entry created |
| UT-03 | Change John's phone | Previous phone becomes available |
| UT-04 | Assign John's phone to Michael | Rejected; data unchanged |
| UT-05 | Unknown member | Validation error; no mutation |
| UT-06 | Unknown phone | Validation error; no mutation |
| UT-07 | Remove Jane's assignment | Number becomes available; audit recorded |
| UT-08 | Remove unassigned member | Validation error |
| UT-09 | Remove unknown member | Validation error |
| UT-10 | Reassign same phone to same member | No duplicate assignment or audit |
| UT-11 | Simulated save failure | Original state retained |

## Manual functional tests

Use a fresh data file (delete `src/CUSoCalPhoneAssignments.Web/Data/assignments.json` only when the app is stopped). Start the app and record **actual result**, **pass/fail**, **date**, and **tester**.

| ID | Action | Expected result | Actual result | Pass/Fail |
| --- | --- | --- | --- | --- |
| MT-01 | Open `/` | Dashboard loads; 3 team members | Not run | Pending |
| MT-02 | Inspect John and Jane | 1001 and 1002 assigned | Not run | Pending |
| MT-03 | Inspect Michael | Unassigned | Not run | Pending |
| MT-04 | Assign 1003 to Michael | Success notice and new assignment | Not run | Pending |
| MT-05 | Refresh browser | Assignment remains | Not run | Pending |
| MT-06 | Change Michael to 1004 | 1003 becomes available | Not run | Pending |
| MT-07 | Remove Jane's number | Jane unassigned; 1002 available | Not run | Pending |
| MT-08 | Submit without phone selection | Required-field validation | Not run | Pending |
| MT-09 | Attempt duplicate assignment with modified HTTP POST | Rejected server-side | Not run | Pending |
| MT-10 | POST without anti-forgery token | Rejected (400) | Not run | Pending |
| MT-11 | With DEMO_PASSWORD configured, open `/` anonymously | Redirects to login | Not run | Pending |
| MT-12 | Wrong demo password | Sign-in denied | Not run | Pending |
| MT-13 | Correct demo password | Dashboard accessible | Not run | Pending |
| MT-14 | Restart app locally | JSON changes retained | Not run | Pending |
| MT-15 | Open `/health` | HTTP 200 and status ok | Not run | Pending |

## Deployment verification

| ID | Action | Expected result | Actual result | Pass/Fail |
| --- | --- | --- | --- | --- |
| DP-01 | Deploy Docker image to Render | Build and start succeed | Not run | Pending |
| DP-02 | Open public HTTPS URL | Login page appears | Not run | Pending |
| DP-03 | Login and perform assignment | Changes visible immediately | Not run | Pending |
| DP-04 | Restart free Render service | Data may reset; document observed result | Not run | Pending |

## Evidence

Attach build log, test runner output / TRX report, and screenshots *only after you have generated them*. Do not mark a test as passed without actually executing it.

## Known limitations

- Single application instance: process-level locking is not distributed across instances.
- JSON file storage is ephemeral on Render Free; a restart/redeploy may reset data.
- Demo password is not corporate identity management; no SSO or RBAC.
- Audit history records a generic demo actor, not verified employee identity.
- Automated unit tests cover service logic, not full browser flows or HTTP integration.
