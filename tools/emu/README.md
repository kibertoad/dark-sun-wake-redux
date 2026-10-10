# Resident emulated calls

This research harness calls one declared resident MZ function in Unicorn 2.1.4.
It starts no process of the game and has no window, input, sound or timer.
The shared executable-reader loads and bounds the resident image and its own
relocations. The original's FBOV overlay payload is never loaded.

`tools/Test.ps1` restores hash-locked, test-only Unicorn separately from the
published runtime requirements and runs the synthetic CPU/loader controls.
Unicorn is not included in the game or its packages. To restore it alone:

```powershell
./tools/emu/Restore-Emulator.ps1
```

The CLI finds the licensed installation with `tools/game-dir.mjs`: the directory
of `original.analysisExecutable` in `tools/project-config.json`, or
`DARK_SUN_WAKE_REDUX_GAME_DIR` when set, after checking the executable's length
and XXH3 against the config. It ignores the machine-wide `GAME_DIR`. Keep a call
configuration and its report beneath that directory, then run:

```powershell
node tools/emu/resident-call.mjs "C:/GOG Games/Dark Sun 2/analysis/reporter-audit/call.json"
```

The configuration names `build: BLD-GOG-EN-1.1`, `source: DSUN.EXE`, the build's
`xxh3`, `sourceKind: mz`, the documented resident `regions`/`entry`, and a new
`report` filename. Regions follow the executable-reader's source mapping contract.
The source must match the existing Extractor edition manifest. An existing
report is never overwritten. Prepared image bytes travel only through a pipe;
they are never written to the checkout.

The current root calling convention is a far return without argument cleanup.
The harness owns its stack and return sentinel. `registers` declares CPU setup;
the result records every initial register, including default values. Memory
outside the loaded image is synthetic zero-filled RAM below one MiB. Named
`observations` read bounded field spans before and after the call. Raw memory
patches and argument seeding are rejected: adding them requires supported format
fields and named parameter bindings, not invented native state.

Every executed instruction and write stays inside explicit limits. Execution
outside declared instruction starts, writes to declared code, overlay interrupts,
and an interrupt or port without an explicit stub fail with a named error.
Interrupt stubs currently support named stops only; they cannot imply an unknown
handler returned or preserved state. A port model separately declares its read
or write direction, width and glossary name; reads give one explicit value.
Mapped video addresses are ordinary RAM. Recorded RAM and port writes do not
prove pixels, sound, timing, native storage admission or device preservation.

Reports contain numeric instruction positions, the declared-body branch census
and each observed branch direction, bounded writes, register results, and named
observations. An unexecuted branch has no outcomes. The census is over the
supplied code bodies, not an exhaustive function/caller inventory. Original
reports remain local; only wholly synthetic controls are committed.

The first actual control uses FND-CONFIG-193's no-argument initializer. It does
not seed any original memory fields and checks loaded-state high bytes against
source-produced roots/free flags. This does not join startup to a later transfer
or raise a game spec/parity status. The five connected-evidence exits stay open
until their whole source and caller/input contracts pass.
