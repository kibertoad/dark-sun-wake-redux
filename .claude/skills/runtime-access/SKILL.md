---
name: runtime-access
description: Find out and record in docs/RUNTIME.md what can be done with the original game running, and who can do it - start it, send input, read memory, load a patched save, capture frames and sound, play back recordings. Use in the Runtime access stage, and whenever a tool, emulator or machine may have changed an answer.
---

# Runtime access

The rules are in the [work protocol](https://dinorefurb.com/work-protocol/#runtime-access).
Static reading is the main source of evidence. In this repository agents never
launch, control, capture or stop DOSBox (`AGENTS.md`, "Native runtime visual
validation"), so this check tries nothing against the running game itself: the
answers come from the owner's rule, the owner's live sessions and the emulator
harness.

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
3. **Write `docs/RUNTIME.md`** from its headings: each answer names what it
   comes from (the rule, the live session or the tool and version tried) and
   each `none` or `person` says what would change it. For the harness, record
   the Unicorn version, the builds it loads and the stubs it has. Replace
   answers that are no longer true; do not append.
4. **Move queue items** between `Emulated call` and `Live session` where an
   answer changed, in the same commit. If the owner ever allows agent runs,
   they take the machine's run lock as the protocol's
   [Running the original](https://dinorefurb.com/work-protocol/#running-the-original)
   says, and items move to `Agent run`.
5. **Commit**, then print the status block from `research-item` with
   `Batch: runtime access`.
