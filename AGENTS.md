# Dungeon Game — agent entry point

All detailed AI development plans, project reports, architecture, status and handoff documentation MUST live under `Assets/Plan/`, the single source of truth. Do not create parallel game plans in `docs/`, `Assets/Plans/`, script folders or the project root. Vendor documentation/licenses and technical source/binary recovery artifacts remain with their owners. This file is the entry point, not a second game design document.

## Before starting any task

1. Read [Assets/Plan/PROJECT_CONTEXT.md](Assets/Plan/PROJECT_CONTEXT.md).
2. Read [Assets/Plan/ARCHITECTURE.md](Assets/Plan/ARCHITECTURE.md) if present.
3. Read the relevant subsystem documents in `Assets/Plan/SYSTEMS/`.
4. Review [Assets/Plan/DEVELOPMENT_ROADMAP.md](Assets/Plan/DEVELOPMENT_ROADMAP.md) when the task affects planned work.
5. Inspect the actual code and Unity assets before changing anything.
6. Check whether the requested feature already exists.
7. Identify dependencies and integration points with existing systems.

Do not implement features based solely on documentation. Archived reports are evidence of earlier scopes/runs, not current instructions or guaranteed current defects. Verify them against the project and the user's current request.

## Definition of done — after every task

Every agent MUST update the relevant documentation under `Assets/Plan/` as part of completing the task:

- Relevant subsystem document when implementation or design changed.
- PROJECT_CONTEXT.md when overall architecture or development status changed.
- DEVELOPMENT_ROADMAP.md when milestones or planned work changed.
- SCRIPT_INDEX.md when scripts were added, moved, renamed or removed.
- ARCHITECTURE.md when system boundaries or integration points changed.

Do not update unrelated documents unnecessarily. Record what was actually implemented, changed files, integration with existing systems, tests actually executed/results, unfinished work, known limitations and the next agent's integration requirements. Clearly distinguish implemented, partially implemented, planned and unverified features. Never claim tests passed unless they actually ran.

## Documentation maintenance rules

- Update an existing plan/system reference instead of creating a duplicate plan.
- Do not create a report for every small task; prefer existing subsystem documents.
- Keep documentation concise, useful and internally consistent.
- Remove completed work from active TODOs or mark it completed.
- Preserve important architectural decisions and useful implementation history.
- Archive outdated history when needed; never silently delete useful documentation.
- Update plans to reality when implementation differs from the original proposal.
- Always record unfinished work and requirements for the next agent.
- Keep one authoritative location for each major fact; link instead of copying conflicting descriptions.

## Unity compatibility and validation

Preserve .meta GUIDs, serialized references, class/namespace names, enum values, public UnityEvent callbacks and assembly boundaries during organization. Prefer Unity AssetDatabase moves when connected. Do not move third-party/Starter Assets code unnecessarily, invent gameplay changes to justify a move, overwrite unrelated dirty work, or import backup C# into Assets. Inspect current scene paths/build entries rather than assuming historical names. Use available Editor tooling and record tests that could not run. Preserve the user's active Play Mode/unsaved scene state.
