# Squad Decisions

## Active Decisions

### 2026-08-14T17:05:45+0000: Squad repository documentation language
**By:** Cyrille NDOUMBE (via Copilot)
**What:** All documents related to Squad's operation that are stored in the repository must be written in English.
**Why:** User directive to keep Squad's repository-based operational documentation consistent and accessible to the team.

### 2026-08-13T19:31:39+0000: Investigation startup API/DB - dual root cause likely
**By:** Scribe (from Trinity, Morpheus, Fact Checker)
**What:** The leading hypothesis identifies a dual root cause: 1) an image/runtime/dependency mismatch between the build and containerized execution; 2) a risk that environment interpolation/resolution on the apphost side could alter the values expected at runtime.
**Why:** The combined backend, platform, and fact-check analyses converge on behavioral differences between local and containerized execution, with a specific signal involving environment-variable resolution and the runtime publishing pipeline.

### 2026-08-13T19:31:39+0000: Rollout sequencing for fix - publish/runtime first
**By:** Scribe (from Trinity, Morpheus, Fact Checker)
**What:** Sequence the remediation in two steps: 1) first secure the container publish/runtime settings; 2) then normalize the connection string (and its parsing) consistently across all data consumers.
**Why:** This sequence reduces the risk of regression by isolating the execution layer first, then harmonizing the connection configuration across all dependent components.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
