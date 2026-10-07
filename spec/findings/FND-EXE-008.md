---
id: FND-EXE-008
title: Seven manifest-identified batch files contain CRLF command text with distinct launch and setup roles
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RAVAGER.BAT
    offset: 0x00..0x14
  - build: BLD-GOG-EN-1.1
    file: SOUND.BAT
    offset: 0x00..0x72
  - build: BLD-GOG-EN-1.1
    file: CD:AA.BAT
    offset: 0x00..0x85
  - build: BLD-GOG-EN-1.1
    file: CD:AC.BAT
    offset: 0x00..0x81
  - build: BLD-GOG-EN-1.1
    file: CD:CA.BAT
    offset: 0x00..0x85
  - build: BLD-GOG-EN-1.1
    file: CD:CC.BAT
    offset: 0x00..0x81
  - build: BLD-GOG-EN-1.1
    file: CD:SOUND.BAT
    offset: 0x00..0x1309
tool: Node.js 24.21.0, repository MODE2 listing reader, executable-reader 2.2.0 XXH3
environment: null
---

## Observation

Every selected file matches its path's byte length and XXH3 in the build
manifest before interpretation. All line endings are CRLF; none contains
NUL, byte 26, an unpaired CR or an unpaired LF. The installed launcher has
two lines, the installed sound helper six, each editor helper four, and the
disc sound helper 157. These are complete file reads, with exclusive end
offsets in Locations.

All files except `CD:SOUND.BAT` use only bytes below 128. That file has 376
bytes above 127, all in ECHO display commands: byte values 185, 186, 187,
188, 200, 201, 204 and 205 occur respectively 2, 46, 2, 2, 2, 2, 2 and 318
times. Neither these bytes nor the ASCII-only files identify a unique code
page.

The installed launcher names `DSUN` with options `-W0` and `-L`, then exits
the shell. The installed sound helper suppresses command echo, prints a
progress notice, copies `D:\*.ADV` into the current directory with output
sent to NUL, names `SOUND_DS`, deletes current-directory `*.ADV` with output
sent to NUL, and exits. It has no conditional result checks; its deletion
wildcard is not restricted to a remembered list of copied files.

Each disc editor helper displays two notices, pauses, and names
`C:\DOS\EDIT`. Their requested targets are:

| File | Editor target |
|---|---|
| CD:AA.BAT | A:\AUTOEXEC.BAT |
| CD:AC.BAT | A:\CONFIG.SYS |
| CD:CA.BAT | C:\AUTOEXEC.BAT |
| CD:CC.BAT | C:\CONFIG.SYS |

The disc sound helper tests whether `ULTRADIR` is empty first, and tests
`ARIA` only when it is. With neither set, its ordinary setup path clears
the screen, names `SOUND_DS`, and jumps to the terminal label.

The ULTRADIR path tests for `midi\ACPIANO.PAT`, then `midi\HONKY.PAT` under
that directory. Missing the first selects the old-revision message path;
missing the second selects copying `UM200.ini` to `ssi1.ini`. With both
present, it tests `TREMSTR.PAT`, `CHARANG.PAT` and `ECHOVOX.PAT` in that
same MIDI directory: all three present select `UM206A.ini`, otherwise
`UM206.ini`. Each selected file is copied to `ssi1.ini`, with output sent
to NUL. The 200 path checks ERRORLEVEL before its success jump; the 206A
path checks it before a pause and success jump; the 206 path pauses before
checking ERRORLEVEL and proceeding to the TSR-install label. All those
checks use threshold 1. The first two success jumps target missing labels
(FND-EXE-009).

The TSR-install path names `lh ultramid -nssi1.ini`; its threshold-1
ERRORLEVEL branch goes to the failure label, otherwise to the installer
label. There it clears the screen, names `SOUND_DS`, names `ultramid -F`,
clears the screen and jumps to the terminal label. Copy failure and old
revision paths display errors and pause before the shared failure label.
That label clears both `ULTRASND` and `BLASTER`, pauses, and jumps to the
ordinary non-TSR setup path.

The ARIA path names `lh miditsr gm2.bnk /I`, then uses an ERRORLEVEL
threshold-1 jump whose target token ends in a colon (FND-EXE-009). Its
fall-through path clears the screen, names `SOUND_DS`, names
`miditsr /U /I`, and jumps to the terminal label. The small-bank label
names `lh miditsr gm1.bnk /I`, then jumps to another missing label. REM
lines, including alternative TSR options, are comments rather than active
commands.

## Interpretation

These bytes directly support the command-text description in FMT-EXE-006.
The installed and disc sound helpers are different scripts. Their contents
alone do not show who launches each helper, successful external commands,
ERRORLEVEL values after intervening commands, active code page, or shell
handling of missing and differently spelled labels. No script was run.

## Alternatives

Treating both sound helpers as equivalent is ruled out by their different
complete contents. Inferring a unique OEM code page from the high-byte
values is not justified; several code pages can assign the same display
glyphs there. Inferring helper reachability from names alone remains open.

## How to reproduce

Use the installation and MODE2/2352 disc identified by BLD-GOG-EN-1.1.
Read installed `RAVAGER.BAT` and `SOUND.BAT` in full. Use `listDisc` in
`tools/evidence/build-listing.mjs` at commit
`a9793964fd191b4edbaf54352d0740a03369fda8`, layout `MODE2/2352`, to walk the
disc. At its existing final payload/hash pass retain only `CD:AA.BAT`,
`CD:AC.BAT`, `CD:CA.BAT`, `CD:CC.BAT` and `CD:SOUND.BAT`; keep the existing
sector and directory interpretation. Require exactly those five paths.
Reject any selected file above 8192 bytes. Verify each of the seven lengths
and XXH3 values against `BLD-GOG-EN-1.1.files.yaml` before interpreting any
text. Require unchanged disc length and modification time across the read.

View each byte one-to-one (Latin-1 is a byte-preserving view here, not an
asserted original encoding). Split on CRLF, count control and high bytes,
and inspect every line, distinguishing REM comments, display text, commands,
labels and jump tokens. Keep payloads and reports outside Git. Do not invoke
any script, executable, editor, TSR, DOSBox or shell from the original.
