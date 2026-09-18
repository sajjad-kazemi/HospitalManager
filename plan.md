# HospitalManager — 10-step learning and implementation plan

## Purpose and current checkpoint

Build a small, realistic hospital workflow backend in incremental, understandable steps. PatientService is the only public API. IdentityService owns employee authentication and is contacted through internal gRPC. Admission, transfer, and discharge will be implemented one at a time.

The solution already contains eight projects, health endpoints, Visual Studio launch profiles, and PatientService Swagger. PatientService.Infrastructure has EF Core packages installed, but no DbContext, entities, repositories, or migrations have been created in this rebuild. Steps 1 and 2 are complete; Step 3 is next. This file is a roadmap, not an implementation of the remaining steps.

## Architecture and working rules

- Each service has **Api**, **Application**, **Domain**, and **Infrastructure** projects. Domain has no infrastructure dependencies; Application defines use cases and required interfaces; Infrastructure implements persistence and external calls; Api handles transport and dependency registration.
- The frontend calls only PatientService REST endpoints. IdentityService owns ASP.NET Core Identity and exposes internal gRPC operations for login, employee lookup, creation, and role assignment. Each service owns its own database and DbContext; there are no cross-service database foreign keys.
- PatientService uses **focused repositories** introduced alongside real use cases, plus one unit-of-work boundary backed by its scoped DbContext. Avoid a generic `IRepository<T>`, a repository for every table, and `IQueryable` in repository contracts. IdentityService uses `UserManager` and `RoleManager` directly.
- Controllers remain thin. FluentValidation checks request shape; application/domain logic enforces business eligibility and state transitions. Record important transitions in ProcessHistory. Use asynchronous EF Core calls and cancellation tokens.
- Finish each step with an understandable checkpoint and a successful solution build. Run only verification relevant to that step. Stop development services started during assisted work.
- Decide how hands-on the collaboration should be before each future increment; this plan does not assume Codex writes every step. Commit and push completed milestones when requested.

## 1. Solution structure — complete

Create the two four-project services, configure project references and solution folders, and verify dependency direction.

**Checkpoint:** Eight projects build without cross-service project references.

## 2. Run the APIs — complete

Understand `Program.cs`, middleware, configuration, and dependency injection. Run both APIs from Visual Studio on fixed development ports. Provide `/health/live` in each service and Swagger UI in PatientService.

**Checkpoint:** Both services start together; PatientService Swagger and both health endpoints respond; stopping debugging stops both services.

## 3. Prepare persistence

Review the EF Core packages already installed in PatientService.Infrastructure. Add SQL Server configuration and introduce each service's DbContext when its model is ready. Learn DbContext lifetime, entity tracking, and how connection strings enter through configuration. Do not create migrations against the old schemas yet.

**Checkpoint:** Explain which project owns each database and how its DbContext is registered; the solution still builds.

## 4. IdentityService foundation

Create a Guid-based `ApplicationUser` with first name, last name, email, and creation time. Configure ASP.NET Core Identity, its SQL Server DbContext, password and lockout rules, and Admin, Receptionist, Doctor, and Nurse roles. Bootstrap one Admin using development secrets; provide no public registration.

Immediately before the first Identity migration, inspect the `identity` database, then reset its old schema and data as chosen for this rebuild. Apply the new migration explicitly. Never delete a database or apply migrations as a normal API startup side effect.

**Checkpoint:** Identity tables and roles exist, the bootstrap Admin can be found, and repeat startup does not reset its password.

## 5. JWT authentication and authorization

Verify credentials through Identity. Issue short-lived RSA-signed JWTs with user ID and role claims. IdentityService holds the private signing key; PatientService validates tokens with the public key, issuer, audience, signature, algorithm, and lifetime. Define named authorization policies and enable Bearer authentication in Swagger. Existing tokens retain role claims until expiration; defer refresh tokens and revocation.

**Checkpoint:** Valid, invalid, and expired tokens behave correctly; role-restricted requests receive the expected 401 or 403 response.

## 6. Secure gRPC between services

Create a small versioned protobuf contract for login and employee information. Implement IdentityService's gRPC server and a typed PatientService client. PatientService exposes `POST /api/auth/login` and forwards credentials over TLS/gRPC; it does not store or verify passwords. Require an internal service credential on every RPC and forward the caller's JWT for protected employee operations. Add deadlines, cancellation, and safe handling when IdentityService is unavailable.

**Checkpoint:** A client logs in through PatientService alone; protected patient requests validate JWTs locally; unavailable IdentityService produces a controlled error for operations that require it.

## 7. Employee administration

Add internal employee lookup, creation, and role assignment. Expose REST operations only from PatientService. Restrict account and role management to Admin in **both** PatientService and IdentityService. Add FluentValidation and consistent ProblemDetails responses.

**Checkpoint:** Admin can create and manage an employee through PatientService; a non-Admin is denied through both REST and direct protected gRPC calls.

## 8. Patient domain and persistence

Model Patient, Department, Admission, MedicalRecord, Transfer, Discharge, and ProcessHistory with Guid IDs where practical. Store employee references as user IDs only. Seed Emergency, ICU, Cardiology, and General. Enforce unique NationalCode and Department Code, one Draft or Active admission per patient, historical admissions, restricted deletion of history, and row-version concurrency for mutable workflow records.

Add focused repository interfaces to Application only as needed, with EF Core implementations in Infrastructure. Share one scoped PatientDbContext across repositories and save through a small unit-of-work interface.

Immediately before the first PatientService migration, inspect the `hospital-manager` database, then reset its old schema and data as chosen for this rebuild. Apply the new migration explicitly.

**Checkpoint:** The model maps cleanly, departments exist, uniqueness constraints work, and repositories retrieve and persist their intended entities.

## 9. Implement workflows separately

Complete and verify one process before starting the next:

1. **Admission:** Receptionist/Admin registers the patient and creates a Draft admission; Doctor/Admin creates its medical record; Receptionist/Admin assigns a department and activates the admission. A patient has at most one open admission.
2. **Transfer:** Nurse/Admin requests; Doctor/Admin approves or rejects; Nurse/Admin completes. Completion changes the active admission's department. Reject completion before approval, repeat approval, transfers after discharge, and transfers to the current department.
3. **Discharge:** Nurse/Admin requests; Doctor/Admin approves or rejects; Receptionist/Admin completes. Completion marks the admission Discharged, stores DischargeDate, and leaves no active admission. Reject completion before approval.

Admin may perform any workflow action as an administrative override. Do not create separate database tables for individual steps. Each state change and its history entry commit together through one DbContext transaction boundary.

**Checkpoint:** Each workflow can be executed through PatientService Swagger; invalid transitions leave the database unchanged and return clear errors.

## 10. Test, document, and review

Add focused tests for domain transitions, application services, authorization, gRPC failures, database uniqueness, concurrency, and atomic workflow updates. Use repository substitutes for application tests and real SQL Server only where database behavior matters. Document Visual Studio startup, configuration and secrets, migrations, Swagger login, and shutdown.

**Checkpoint:** The solution builds, relevant tests pass, the three workflows can be demonstrated end to end, and another developer can run the project from its documentation.

## Planned interfaces

PatientService will expose authentication and employee-management endpoints, then resource-oriented Patients, Admissions, Transfers, and Discharges controllers. Transfer and discharge actions use routes such as `POST /api/transfers/{id}/approve` and `/complete`; controllers delegate transition decisions to application/domain logic. History is queryable by process. The gRPC contract stays internal and contains only authentication and employee information needed by PatientService.

## Assumptions and deferred work

- Use .NET 10, EF Core, SQL Server, ASP.NET Core Identity, JWT Bearer, gRPC, Protocol Buffers, FluentValidation, and Swagger.
- The old `identity` and `hospital-manager` databases contain tables from the previous implementation. The chosen approach is to **reset their old schemas and data at their respective migration steps**. Verify the exact target database names and current contents before those destructive operations. Writing this plan changes no SQL data.
- Use the existing Windows-integrated SQL Server connection strings for local development. Keep passwords, signing keys, and service credentials outside source control.
- Do not add RabbitMQ, a gateway, a generic workflow engine, MediatR, CQRS infrastructure, distributed caching, or Kubernetes. Reconsider messaging only when a real asynchronous use case is added.
- Git remote: `https://github.com/sajjad-kazemi/HospitalManager.git`. This roadmap does not authorize implementing subsequent steps automatically.
