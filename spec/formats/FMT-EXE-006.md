---
id: FMT-EXE-006
title: BAT launch files
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RAVAGER.BAT", "SOUND.BAT", "CD:*.BAT"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-EXE-008, FND-EXE-009]
conflicting: []
split_with: []
related: []
---

## Layout

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| line | byte text | command_text | A command, label, comment or blank line terminated by CRLF | supported | FND-EXE-008 |
| EOF | file boundary | end | Immediately after the final CRLF, without a byte-26 marker | supported | FND-EXE-008 |


The seven manifest-listed files are CRLF-delimited command text without NUL
or DOS end-of-file marker bytes. Six contain only ASCII bytes. Disc
`SOUND.BAT` additionally uses bytes 185, 186, 187, 188, 200, 201, 204 and
205 in display commands; their intended code page remains unknown.

Installed `RAVAGER.BAT` names `DSUN` with options `-W0` and `-L`, then exits
the shell. Installed `SOUND.BAT` suppresses command echo, displays a notice,
copies `D:\*.ADV` into the current directory, names `SOUND_DS`, deletes
current-directory `*.ADV`, and exits. Copy and delete output goes to NUL;
there are no conditional result checks or remembered copied-file list.

Disc `AA.BAT`, `AC.BAT`, `CA.BAT` and `CC.BAT` display notices and pause
before naming `C:\DOS\EDIT` with, respectively, `A:\AUTOEXEC.BAT`,
`A:\CONFIG.SYS`, `C:\AUTOEXEC.BAT` and `C:\CONFIG.SYS`.

Disc `SOUND.BAT` selects its ULTRADIR path when that variable is nonempty;
otherwise it tests ARIA. With neither set, it names `SOUND_DS` without
installing a TSR. The ULTRADIR path tests MIDI patch-file presence and
selects `UM200.ini`, `UM206A.ini` or `UM206.ini` for copying to `ssi1.ini`.
The tests, ERRORLEVEL thresholds and ordering are recorded in FND-EXE-008.
The 200 and 206A success paths jump to the undefined `G_SUCCESS` label.
The 206 path reaches the Ultramid load command; its successful fall-through
names `SOUND_DS`, then unloads Ultramid. Error-message paths reach the
shared cleanup that clears ULTRASND and BLASTER and then selects non-TSR
setup. ARIA selects the GM2 bank load command and a threshold-1 branch
whose target token is `INS_GM1:` while the label is `INS_GM1`. Its ordinary
fall-through names `SOUND_DS` and unloads that TSR. The GM1 label loads
the smaller bank then jumps to undefined `RUN_ARIA`. These describe the
shipped command text, not an observed execution or shell error outcome.

## Enumerations and flags

No binary enumeration or byte-order field applies to this text listing.
The meanings of executable command-line options are outside this entry.

## Differences between builds

None known. Installed and disc sound scripts differ within this build.

## Coverage

Complete command-text reads of `RAVAGER.BAT`, `SOUND.BAT`, and the five
`CD:` `.BAT` files of BLD-GOG-EN-1.1 (FND-EXE-008, FND-EXE-009). No complete
reading of callers, external commands or interpreter behavior is claimed.

## Open questions

- Which helpers are launched by the shipped game, setup or distribution
  wrapper (Q-EXE-004)? Invocation from their filenames is one possible
  reading; direct manual use is another. FND-EXE-008 shows their contents,
  but provides no caller evidence for either. Launch references settle it.
- Which code page interprets the disc sound display bytes (Q-EXE-005)?
  Several OEM code pages may agree on those byte values; ASCII-only helpers
  are compatible with multiple encodings too. FND-EXE-008 identifies the
  bytes, not an exclusive decoder. Interpreter configuration or font/code
  page selection evidence settles it.
- How does the supported interpreter resolve missing labels and the
  colon-suffixed target (Q-EXE-006)? FND-EXE-009 rules out hidden definitions
  in this file, but not interpreter normalization or error handling. A
  static reading of the actual interpreter's label handling settles its
  behavior; any environment-dependent outcome needs an owner observation.
