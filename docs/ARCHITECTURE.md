# Architecture and coding decisions

## Request flow

Browser (Razor forms) → HomeController → AssignmentService → IAssignmentRepository → JsonAssignmentRepository → JSON file.

## Decisions

1. **ASP.NET Core MVC + Razor:** Familiar server-rendered web framework with accessible HTML and a small dependency footprint.
2. **Application service:** All assignment rules are centralized and unit-testable without an HTTP server.
3. **Repository abstraction:** JSON is used because the exercise supplies JSON. A SQL Server implementation can replace the repository in production.
4. **Data integrity:** One number per member and one member per number. IDs are validated on every write. Updates and audit entries are saved together in one JSON document.
5. **Persistence:** Write a temporary file and replace the target; update in-memory state only after a successful write.
6. **Concurrency:** One process-level lock around read-modify-write. A production multi-instance deployment requires DB transactions and uniqueness constraints.
7. **Security:** MVC anti-forgery on state-changing forms, Razor HTML encoding, optional local demo login, required demo password in production, HTTP-only auth cookie, login rate limit, no real employee data.
8. **Testing:** xUnit tests cover business rules and failure handling; manual checklist covers browser and deployment behavior.
9. **Cloud:** Multi-stage Docker build; Render terminates HTTPS at its reverse proxy. The container listens on `0.0.0.0:10000`.

## Production roadmap

- Entra ID / OIDC SSO, role-based permissions, identity-based audit records.
- SQL Server with unique index on Assignments.PhoneNumberId and Assignments.TeamMemberId; transactions.
- Centralized structured logging and monitoring; operational alerting.
- API/integration and browser end-to-end tests, security scanning, deployment approvals.
- Data retention and compliance review with the organization's security team.

## Explicit scope

This is an interview demonstration, not a production financial application. It does not store member banking data or financial account information.
