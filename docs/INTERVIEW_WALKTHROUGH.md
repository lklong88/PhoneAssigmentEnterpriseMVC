# Five-minute technical interview walkthrough

**0:00–0:40 — Business problem**
"The business currently tracks phone-number assignments in a spreadsheet. I built an MVC application that shows current assignments and lets authorized demo users assign, change, and remove phone numbers."

**0:40–1:25 — Architecture**
"Razor views handle the UI, MVC controllers accept form requests, AssignmentService enforces rules, and a repository persists JSON. The repository interface allows a future SQL Server implementation."

**1:25–2:35 — Demonstration**
Show initial data; assign 1003 to Michael; change it to 1004; remove Jane's number; refresh and show the persisted result. Show the recent activity panel.

**2:35–3:25 — Security and data integrity**
"Every write is checked server-side for valid IDs and duplicate numbers. POST forms have anti-forgery tokens. The public demo is password protected; this is not a substitute for enterprise SSO."

**3:25–4:20 — Testing**
Open Test Explorer and show the actual xUnit results. Explain duplicate assignment, invalid IDs, removal, and simulated save failure. Show completed manual test checklist. Do not claim unexecuted tests passed.

**4:20–5:00 — Tradeoffs**
"JSON matches the supplied source data and keeps the MVP focused. In production I would move to SQL Server, enforce uniqueness with database constraints, integrate Entra ID, and add auditable identity and CI/CD approval controls."

## Interview follow-up questions

**Why MVC?** Clear separation of concerns and easy to demonstrate in Visual Studio.

**Why not React?** Razor is sufficient for a small form-driven application; adding a SPA would increase complexity without a business need.

**Why a repository interface?** Decouples business rules from file storage and makes unit tests straightforward.

**What happens when two users assign the same phone simultaneously?** One process-level lock serializes changes; the second request is rejected. Across multiple servers, use database transactions and unique constraints.

**What is the deployment limitation?** Render Free has an ephemeral filesystem; JSON changes can be lost on restart or redeploy. The demo login password is only a basic access gate.
