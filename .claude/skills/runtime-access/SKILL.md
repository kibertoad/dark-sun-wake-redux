---
name: runtime-access
description: Find out and record in docs/RUNTIME.md what can be done with the original game running, and who can do it - start it, send input, read memory, load a patched save, capture frames and sound, play back recordings. Use in the Runtime access stage, and whenever a tool, emulator or machine may have changed an answer.
---

# Runtime access

The rules are in the [work protocol](../../../vendor/upstream/work-protocol.md#runtime-access) (lines 42-62).
Open a linked section only when a step leaves a question it answers, read
only the lines the link gives, and never a section already read this session.
Static reading is the main source of evidence. Agents run the original only
through `dinorefurb-dosbox-session` with host sound muted (`AGENTS.md`, "Runs
of the original"), and never operate GOG's DOSBox. Answers come from attempts
in session-package runs, owner live sessions and the emulator harness.

1. **Collect what changed**: a change to the owner's DOSBox rule in
   `AGENTS.md`, a live session that showed what the owner can do (load a save,
   record sound, use a debugger build), a new DOSBox or DOSBox-X version the
   owner uses, or a change to the harness in `tools/emu/`. Ask the owner
   where you cannot tell.
2. **For each build that is run**, record how it runs (the DOSBox build, its
   configuration files and the settings that matter), and answer each
   capability `agent`, `person` (only while the owner runs the game) or
   `none`:
   - start the original and bring it to a given state without a person;
   - send it input;
   - read its memory, set breakpoints and dump structures while it runs;
   - load a patched save;
   - capture frames and sound;
   - play back a recording the original made;
   - call a single function of the executable in the emulator harness in
     `tools/emu/` (this starts no process of the game and needs no lock).

   Treat each part of a capability separately: each input device the game
   reads, memory reads/breakpoints/dumps, and frames/sound where applicable.
   A common answer requires evidence for every part; otherwise retain distinct
   answers and their provenance. An unverified part cannot admit a run that
   needs it. This guidance grants no original-game access to an agent.
3. **Write `docs/RUNTIME.md`** from its headings: each answer names what it
   comes from (the rule, the live session or the tool and version tried) and
   each `none` or `person` says what would change it. For the harness, record
   the Unicorn version, the builds it loads and the stubs it has. Replace
   answers that are no longer true; do not append. Record Probe as `none`
   until a session-package run has shown the probe works.
4. **Move queue items** between `Emulated call` and `Live session` where an
   answer changed, in the same commit. Agent runs, through the session package,
   take the machine's run lock as the protocol's
   [Running the original](../../../vendor/upstream/work-protocol.md#running-the-original) (lines 331-365)
   says, and items move to `Agent run`.
   Each queued run names the capability parts it needs in Settles it. An
   unavailable part blocks that run; any required person-only part makes it
   an owner live session. Only an owner policy change can admit an Agent run.
5. **Commit**, then print the status block from `research-item` with
   `Batch: runtime access`.
