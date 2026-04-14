# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Architecture, slicing, and code review | Ripley | Design choices, trade-offs, reviewer gates, cross-project coordination |
| Razor Pages and HTMX UX | Bishop | Page handlers, partials, HTMX flows, server-rendered interaction |
| Backend and Azure Functions | Dallas | Managers, data flow, APIs, integrations, Functions endpoints |
| Data architecture and schema design | Hicks | SQL scripts, DataStore design, EF mappings, data integrity, query architecture |
| Testing and quality review | Vasquez | xUnit coverage, FluentAssertions, Moq, Bogus, regression cases |
| CI/CD and Azure deployment | Parker | GitHub Actions, Azure publishing, environment wiring, release hardening |
| Scope & priorities | Ripley | What to build next, sequencing, risks, decisions |
| Session logging | Scribe | Automatic — never needs routing |
| Backlog monitoring | Ralph | Work queue, issue pickup, PR follow-through |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Lead |
| `squad:ripley` | Triage architecture-heavy issues and reviewer escalations | Ripley |
| `squad:bishop` | Pick up UI, Razor Pages, and HTMX work | Bishop |
| `squad:dallas` | Pick up backend, Functions, and integration work | Dallas |
| `squad:hicks` | Pick up schema, DataStore, query, and persistence-architecture work | Hicks |
| `squad:vasquez` | Pick up test and verification work | Vasquez |
| `squad:parker` | Pick up pipeline, infrastructure, and deployment work | Parker |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, the **Lead** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Lead review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn the tester to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. The Lead handles all `squad` (base label) triage.
