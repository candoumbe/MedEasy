# Project Context

- **Owner:** Cyrille NDOUMBE
- **Project:** MedEasy management platform
- **Stack:** .NET, Aspire, microservices, web frontend
- **Role:** Defensive application security
- **Created:** 2026-11-13T20:27:52+0000

## Learnings

- Security reviews should account for cross-service trust boundaries and shared Aspire orchestration, not only individual API endpoints.
- Keep security work scoped to explicitly authorized MedEasy assets and use non-invasive validation unless active testing is explicitly approved.
- Never read `.env` files or record secrets or personal data in Squad state.
