# ADR-010 — OpenSpec Spec-Driven Workflow

**Status:** Accepted  
**Date:** 2026-10-09  
**Deciders:** Marvin Storage

## Context

CLAUDE.md and the ADRs explain the architecture and why decisions were made, but not precisely what each capability must do. Changes to behaviour (new provider, new sync target) had no single place where requirements and scenarios were agreed before coding, and AI assistants working on the repository had no checkable description of current behaviour.

## Decision

Adopt [OpenSpec](https://github.com/Fission-AI/OpenSpec):

- `openspec/specs/` holds the requirements of the system as it behaves today, one folder per capability, indexed in `openspec/specs/README.md`. Platform-neutral except `platform-android`.
- `openspec/changes/` holds proposed work: `proposal.md`, delta specs, `design.md` and `tasks.md`. Finished changes move to `openspec/changes/archive/`.
- `openspec/config.yaml` sets the schema `spec-driven-with-adr` and the project context and rules.
- The workflow is driven by the slash commands `/opsx-explore`, `/opsx-propose`, `/opsx-update`, `/opsx-apply`, `/opsx-sync` and `/opsx-archive` (definitions in `.agents/workflows/`).
- `.github/workflows/specs.yml` validates all specs and changes in strict mode when `openspec/` changes.
- ADRs stay the place for architecture decisions; specs describe behaviour. A new feature still needs an ADR ([`.agents/AGENTS.md`](../../.agents/AGENTS.md)).

## Consequences

### Positive
- Behaviour is reviewable before it is built, and specs double as test sources.
- Assistants and new contributors get an accurate, validated description of the system.

### Negative / Trade-offs
- Extra writing for small changes; typo fixes, small bug fixes and doc changes are exempt.
- Specs must be kept in sync with the code (via `/opsx-sync` and `/opsx-archive`).
- Needs Node (`npx`) to validate locally.
