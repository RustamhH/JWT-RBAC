Secure ASP.NET Core API - Identity & RBAC
A robust, enterprise-grade boilerplate built with ASP.NET Core, focusing on secure authentication, role-based access control (RBAC), and clean architecture patterns.

🚀 Key Features
Authentication: ASP.NET Core Identity with JWT Bearer tokens.

Authorization: Claims-based Role-Based Access Control (RBAC).

Token Management: Full implementation of Login, Registration, and Refresh Token logic.

Email Integration: SMTP Service for account confirmation and password resets.

Security: Password hashing, token validation middleware, and secure claim storage.

Audit Logging: Automatic tracking of user actions using IHttpContextAccessor.

🏗️ Architecture & Patterns
This project follows a decoupled approach to ensure scalability and maintainability:

Service Pattern: Business logic is encapsulated in services, keeping controllers thin.

DTO Mapping: Uses Data Transfer Objects to prevent over-posting and hide internal database structures.

Entity Framework Core: Code-first approach for database management and migrations.

Repository/Service Layer: Clear separation between data access and business rules.

🔐 Security Workflow
Authentication Flow
Registration: User signs up; an email confirmation is sent via SMTP.

Login: User provides credentials; the system validates and returns a JWT Access Token and a Refresh Token.

Authorization: The client sends the JWT in the Authorization: Bearer header.

Token Refresh: When the Access Token expires, the client uses the Refresh Token to obtain a new pair without re-logging.

Access Control
Admin Role: Full CRUD capabilities over the user database.

User Role: Restricted access; logic ensures users can only view or modify their own data using User.FindFirstValue(ClaimTypes.NameIdentifier).
