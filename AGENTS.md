# AGENTS.md

## Project

- Unity project: **Pet Sim Lite**
- Goal: single-player, gacha-focused pet farming prototype inspired by Pet Simulator 99.

## Single Source of Truth

- The **authoritative design and technical spec** is:
  - `Docs/PetSimLite-Requirements.md`
- That file is the single source of truth for:
  - Core game loop and features
  - Controls and input scheme
  - Pet / egg / zone behaviour
  - Data model (CSV / ScriptableObjects)
  - Script/folder organisation in `Assets/`

> When in doubt about how something should work, **open and follow that file**, not this one.

## How agents should behave

- Before making large structural changes, **read**:
  - `Docs/PetSimLite-Requirements.md`
- Follow the conventions and architecture described there.
- Do not introduce new systems that contradict the requirements unless explicitly asked.

## Coding & repo behaviour

- Use C# and Unity as described in the requirements doc.
- Make small, focused changes rather than huge refactors.
- Avoid deleting/recreating CSVs; edit in place to preserve user data/meta.
- When editing code:
  - Explain what you plan to do in a few bullet points.
  - Clearly indicate which files you changed.
- Git:
  - Show diffs or summaries of edits.
  - **Do not commit or push** unless explicitly requested.

