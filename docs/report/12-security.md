# 12. Security considerations

Mapped to the PLAN.md section 10 checklist.

| Checklist item | Implementation | Status |
|----------------|----------------|--------|
| JWT (60 min) signed with a key from the environment, role claim | `JwtTokenService`, `AuthenticationSetup` (issuer, audience, lifetime, signature, 1-min skew) | done |
| Passwords hashed (PBKDF2) | ASP.NET Core Identity `PasswordHasher<User>` | done |
| `[Authorize]` everywhere, roles on sensitive endpoints | fallback policy + `[Authorize(Roles = …)]` | done |
| Resource-based checks in services | `TripRequestService.EnsureCanAccess`, `WorkflowQueryService`, `PassportPhotoService` | done |
| FluentValidation on every DTO; ProblemDetails, no stack traces | validators per DTO; `ExceptionHandlingMiddleware` | done |
| CORS restricted to the React origin | `ALLOWED_ORIGINS` (`CorsSetup`) | done |
| Structured logging, no tokens or passwords | Serilog, JSON outside Development; third-party keys never logged | done |
| Swagger with JWT bearer | `SwaggerSetup` | done |
| Internal agent service reachable only from the API | `X-Internal-Key` (constant-time compare, unset key rejects all) in both directions | done |
| Tool inputs validated; objective treated as data | Pydantic input models per tool; escaped `<DATA>` block | done |
| Agent timeouts, retries, re-plan limit, safe failure | 30 s, 2 repairs, 3 re-plans, `FailedSafely` | done |
| Only state and summaries persisted | `agent_steps` summaries ≤ 8,000 characters; prompts never sent to the API | done |
| `.env` and dev settings ignored; `.env.example` names only | `.gitignore`; `render.yaml` `sync: false` | done |
| Passport stored masked; photo with a random filename | last 4 characters; `LocalPassportPhotoStore` (private folder, content-checked JPEG/PNG ≤ 5 MB) | done |
| Login rate limiting (5/min) | `RateLimitingSetup` per client IP (forwarded headers behind Render) | done |

Additional: strict CSP and security headers on Vercel; mobile token in the Android Keystore
(`flutter_secure_storage`) and HTTPS-only release builds; non-root containers.

## Known limitations

- The web token is in `sessionStorage` (readable by script if XSS occurred; mitigated by the CSP).
- `X-Forwarded-For` is trusted for one hop; if the API were exposed without Render's proxy a client could spoof its IP for the rate limiter.
- Passport photos on Render's free tier are not persistent and not encrypted at rest.
- TODO: threats specific to Students B and C's endpoints once merged.
