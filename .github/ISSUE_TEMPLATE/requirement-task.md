---
name: "Requirement Implementation Task"
about: "Create a detailed implementation task for a specific requirement (Req#)"
title: "Req<NUMBER>: <Short Title>"
labels: ["requirement","planning"]
assignees: []
---

## Requirement

ID: ReqX (replace X)  
Title: (short, imperative)  
Classification: (Covered | Partial | Missing)  
Source: `requirements-traceability-review.md` row X

## Objective

Describe what achieving this requirement means in concrete, testable terms.

## Current Status

Brief summary of what exists (code refs, services, UI components).

## Scope (In / Out)

In:

- (Functional pieces to deliver now)

Out:

- (Deferred items)

## Acceptance Criteria

1. <Criterion 1>
2. <Criterion 2>
3. <Criterion 3>

## Tasks

- [ ] Design: Update architecture / sequence diagram if needed
- [ ] Backend: <Service/Controller additions>
- [ ] Frontend: <Components / routes>
- [ ] Persistence: <Migrations / indexes>
- [ ] Security: Permissions / roles adjustments
- [ ] Localization: New keys added & extracted
- [ ] Tests: Unit (list), Integration (list), E2E (list)
- [ ] Docs: Update `requirements-traceability-review.md` status → ✅ when AC met

## Edge Cases / Risks

- <Latency spikes / concurrency / validation edge>

## Metrics / SLA

- KPI: <e.g. Broadcast latency p95 < 100ms>

## Test Strategy

| Layer | Case | Description |
|-------|------|-------------|
| Unit | ...  | ... |
| Integration | ... | ... |
| E2E | ... | ... |

## Rollout / Migration

Steps if data model or configuration changes.

## Follow-ups (Post-Completion)

Items intentionally deferred (future optimization, scaling, UI polish).

## References

Code pointers, similar implementations, external specs.
