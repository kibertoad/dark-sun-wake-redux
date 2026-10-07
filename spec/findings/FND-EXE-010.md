---
id: FND-EXE-010
title: GOG launch metadata selects a configuration with separate game and sound-helper branches
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: goggame-1432903719.info
    offset: 0x00..0x06CA
  - build: BLD-GOG-EN-1.1
    file: dosbox_darksun2.conf
    offset: 0x00..0x2CB9
  - build: BLD-GOG-EN-1.1
    file: dosbox_darksun2_single.conf
    offset: 0x00..0x0434
tool: Node.js 24.21.0, byte-preserving config inspection and UTF-8 JSON parsing, executable-reader 2.2.0 XXH3
environment: null
---

## Observation

The shipped metadata's `playTasks` array marks one task as primary. It names
`DOSBOX\dosbox.exe`, with working directory `DOSBOX`, and arguments selecting
`..\dosbox_darksun2.conf` followed by `..\dosbox_darksun2_single.conf`, then
`-noconsole` and a command-line `exit` command. These are declared launch
parameters, not a recording of a process being started.

The first configuration has no autoexec section. The second has one beginning
at byte offset 64. It names the parent installation directory as the C drive,
then overlays its `cloud_saves` directory on C; it names `..\game.ins` as
the D-drive ISO mount, changes to C, and jumps to the launcher label.

The menu command at byte offset 848 admits choices `123` with `/s` and `/n`
options. The following tests are ordered as ERRORLEVEL threshold 3 to the
exit label, threshold 2 to the setup label, then threshold 1 to the game
label. The game label names bare command `ravager` at byte offset 1006,
followed by a jump to exit. The setup label names bare command `sound` at
1041, followed by a jump to launcher. The exit label names `exit` at 1072.
All those label definitions are in this same complete file read.

The named command stems match the installed `RAVAGER.BAT` and `SOUND.BAT`
whose contents are recorded in FND-EXE-008. The configuration does not spell
an extension or use CALL for either command. Both installed batch files
contain their own EXIT command. Consequently the textual continuation after
each bare command is not evidence that the wrapper actually resumes there.
Command search, batch chaining, EXIT behavior and overlay shadowing must be
read before claiming the resolved file and continuation.

All three whole files were read with a 16384-byte per-file bound; each size
and XXH3 now belongs to the build manifest. Their lengths and modification
times were unchanged across the reads. Locations use exclusive end offsets.
The configuration's display prose is not retained in this finding.

## Interpretation

The supported distribution declares separate game and sound-setup command
branches, rather than only a direct unconditional game command. This is direct
wrapper evidence relevant to FMT-EXE-006. It neither proves that the game or
sound executable launches other batch helpers nor decides which of the two
sound scripts an installer elsewhere uses. No configuration, batch file,
executable, shell or DOSBox was executed.

## Alternatives

An unconditional `ravager` startup with no setup choice is ruled out by the
complete menu and branch text. A setup branch that explicitly names disc
`SOUND.BAT` is ruled out: it names only bare `sound` after changing to C.
Resolution to the installed batch file is consistent with that drive and the
manifest, but is not yet a complete reading of the command interpreter or
mutable overlay contents. A menu return after setup is textual intent, not
a demonstrated continuation.

## How to reproduce

From the BLD-GOG-EN-1.1 installation read exactly
`goggame-1432903719.info`, `dosbox_darksun2.conf` and
`dosbox_darksun2_single.conf`, rejecting any larger than 16384 bytes. Verify
length and XXH3 against the build manifest before interpretation. Check
unchanged file length and modification time across each read. Parse the
metadata as UTF-8 JSON; select `playTasks` records whose `isPrimary` is true
and inspect each selected `path`, `workingDir` and `arguments` field. Inspect
all sections of both configs, then every line of the second config's
autoexec section. Use a one-byte-to-one-character view for offsets, such as
Latin-1, without asserting that it is the original display decoder.

Follow each literal GOTO to its definition and preserve the exact descending
ERRORLEVEL test order. Inspect the complete named installed batch files using
FND-EXE-008 before making any statement about their continuation. Do not
infer command resolution from extensionless tokens. Keep raw configs and
reports outside Git and launch nothing.
