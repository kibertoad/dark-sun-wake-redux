---
id: SRC-DOSBOX-GOG-0742
title: DOSBox 0.74-2.1 source archive shipped with the GOG distribution
superseded_by: []
author: The DOSBox Team and contributors
date: unknown
location: dosbox-0.74-2.1.tar.gz in the DOSBOX directory of the installation of BLD-GOG-EN-1.1
xxh3: 4099c9880bcc0eba22e7ccf1b0537f3f
licence: GPL-2.0-or-later, as stated in the inspected source headers; COPYING supplies GPL version 2
---

## Use

This is the source archive that accompanies GOG's interpreter distribution,
not the game's source. The archive is 1334686 bytes. Its package name is
not proof that the installed DOSBox executable was compiled from exactly
these files, with known patches or build options. The readings below are
external-source leads for Q-EXE-006 and Q-EXE-009, not original-runtime
observations or a complete reading of the shipped executable.

Selected fingerprinted members are under `dosbox-0.74-2.1/`:

| Member | Bytes | XXH3-128 |
|---|---|---|
| src/shell/shell_misc.cpp | 17059 | f4a3f1ffdc5a7f2999b1556e1d8a209c |
| src/shell/shell_batch.cpp | 5534 | 7608684dea106b5777910dbdb1acdf3f |
| src/shell/shell_cmds.cpp | 31920 | c151f2e30abdd2f471469042dddb6384 |
| src/shell/shell.cpp | 25590 | 866b71fc7e4cd8c7b119d991db51604c |
| include/shell.h | 3979 | a8055205228a02b5bdcee6058f4e930f |
| COPYING | 17992 | 95ba191925e071364b68b7eda4757d94 |

### Command and batch reading

In `shell_misc.cpp`, `DOS_Shell::Which` first tests the supplied name,
then that name with COM, EXE and BAT extensions, in that order, before
walking PATH entries. Each PATH entry uses the same name/extension order.
`DOS_Shell::Execute` handles a selected BAT by deleting the active batch
when its call flag is false, then constructing the replacement. For an
existing extensionless file selected by Which, Execute separately retries
COM, EXE and BAT; it does not execute the extensionless file itself. This
is a reading of those source paths, not proof of a particular mounted file.

In `shell_batch.cpp`, the batch constructor remembers the host's current
batch as its predecessor. Destruction restores that predecessor and the
saved echo state. In `shell_cmds.cpp`, `CMD_CALL` sets the call flag around
ParseLine, then clears it. Thus a direct BAT selection deletes the current
wrapper before recording its predecessor, while a CALL selection retains
the wrapper. The source's normal shell starts with call false. Applied
conditionally to the bare commands in FND-EXE-010, this predicts replacement
rather than returning to the wrapper's textual jump after the helper.

`CMD_EXIT` sets the shell's exit flag. The normal `DOS_Shell::Run` loop in
`shell.cpp` tests that flag after each iteration. Its distinct `/C` path
uses a temporary shell and `RunInternal`, whose loop instead continues
while a batch exists and yields a line; that loop does not test exit.
Therefore this source does not justify an unconditional claim that EXIT
immediately stops every batch context. The first-shell setup supplies
`/INIT AUTOEXEC.BAT` and calls Run. AUTOEXEC installs config text before
command-line `-c` commands unless its suppression options apply. Config
loading, drive mounts, overlay lookup, DOS file existence and compiled
correspondence have not been read completely here.

### Label reading

`CMD_GOTO` strips leading spaces and one leading colon from its target,
and terminates the target at a space or tab. It does not remove a trailing
colon. `BatchFile::Goto` reads the batch from its beginning, recognizes a
trimmed line beginning with a colon, skips spaces and equals signs after
that colon, and takes the label up to whitespace, an equals sign or line
end. It compares the resulting label and target case-insensitively.

A successful match saves the position after that label line. At an
unsuccessful end-of-file search it closes the file and deletes the active
batch, restoring its predecessor through the destructor. It returns false;
CMD_GOTO then emits a label-not-found diagnostic. Failure to open the batch
also deletes it and returns false. An empty target instead produces a
missing-label diagnostic without that search; GOTO with no active batch
returns without searching. These are distinct source paths.

Under this source's matching rules the absent `G_SUCCESS` and `RUN_ARIA`
definitions in FND-EXE-009 cannot match, and target `INS_GM1:` does not match
label `INS_GM1`. Those are conditional predictions for Q-EXE-006, not
observations of a shell error or of what a user sees afterward.

### Repeating the reading

Use the archive at the location above, checking its exact size and XXH3
before extraction. Extract only the six named members into local-only
research storage. Bound archive/member reads to 16777216 bytes and verify
the selected member sizes and hashes. Inspect the named routines and their
consumers: Execute and Which; batch constructor, destructor, ReadLine and
Goto; DoCommand, CMD_CALL, CMD_EXIT, CMD_GOTO and CMD_IF; shell constructor,
RunInternal, Run, AUTOEXEC constructor and first-shell setup. Inspect the
local StripSpaces helpers in shell_cmds.cpp and ltrim/rtrim/trim in
src/misc/support.cpp too: those helpers strip whitespace, not a trailing
colon. CMD_IF's
ERRORLEVEL path compares the stored return code against its parsed threshold
with greater-than-or-equal before dispatching the remaining command.

Keep command dispatch, label matching and batch cleanup separate from
external-program return codes and DOS file/drive operations. Do not treat
this source snapshot as a binary match, run DOSBox, compile or execute a
shell harness, or commit the archive or extracted code.

## Known errors

No error in these source readings is known. The source-to-shipped-binary
correspondence is unverified. Native mounted state, overlay shadowing,
external command outcomes and other interpreters are not covered. In
particular a prediction of lost wrapper continuation must not be promoted
to an observed original bug solely from this archive (Q-EXE-009).
