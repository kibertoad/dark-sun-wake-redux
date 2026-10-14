---
id: FND-EXE-581
title: Twelve of DSUN.EXE's unresolved calls go to the XMS driver entry that interrupt 2F AX=4310 returns, with AH 00, 08, 09, 0A, 0B, 0C or 0D
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0F3D..4AE5:0FB3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:10C9..4AE5:10DD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:121A..4AE5:1224
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:00BC..15F3:00E9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:05C8..15F3:05D9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0632..15F3:0670
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0697..15F3:069D
tool: Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/store_values.py), with the walk of FND-EXE-580
environment: null
---

## Observation

Addresses and the walk are those of FND-EXE-580. Twelve of the 69 transfers its walk leaves
unresolved are far calls through two words:

| Calls | Pointer |
|---|---|
| `4AE5:0F55`, `0F60`, `0F6D`, `0F7B`, `0FA5`, `0FAF`, `10CB`, `10D9`, `1220` | `[0043]` with DS loaded from `CS:0005`, the relocated word `45CE`, so `55CE:0043..0047` |
| `15F3:066C`, `15F3:0699` | `[00B8]` with DS loaded from CS, so `15F3:00B8..00BC` |
| `15F3:05D4` | `CS:[00B8]`, the same word |

Each routine that makes the calls loads DS that way before them: `4AE5:0ECD`, `4AE5:107D` and
`4AE5:11A9` with `mov ds, cs:[0005]`, and `15F3:0632` and `15F3:00BC` with `mov ax, cs` and
`mov ds, ax`. Both pointers hold 0 at load. A search of every byte offset of the regions for a
direct store to `[0043]`, `[0045]`, `[00B8]` or `[00BA]` finds one writer for each pointer, in the
module that calls it:

- `4AE5:0F4B` and `4AE5:0F4F` store BX and ES after `int 2Fh` with AX 4310 at `4AE5:0F49`, which
  runs only when the preceding AX 4300 request at `4AE5:0F40` returned AL 80.
- `15F3:00DA` and `15F3:00DE` store BX and ES after `int 2Fh` with AX 4310 at `15F3:00D8`, in the
  same order after `15F3:00CC` (FND-EXE-394).

The only other hit, `add [00B8], bx` decoded at `1000:01AF`, starts on the last byte of the
`jmp` at `1000:01AD`.

The value in AH at each call, from the last instruction before it that writes AH:

| Call | AH |
|---|---|
| `4AE5:0F55` | 00 (`xor ax, ax`) |
| `4AE5:0F60` | 08 |
| `4AE5:0F6D`, `4AE5:10CB` | 09 |
| `4AE5:0F7B`, `4AE5:10D9` | 0C |
| `4AE5:0FA5` | 0D |
| `4AE5:0FAF` | 0A |
| `4AE5:1220` | 0B |
| `15F3:05D4`, `15F3:066C` | 08 |
| `15F3:0699` | 09 |

The three `15F3` calls follow a near call to `15F3:00BC`, the routine that stores the pointer,
and a `test ax, ax` and `je` past the call when it returns 0.

## Interpretation

All twelve calls go to the extended-memory driver's entry point, the far address the multiplex
interrupt returns for AX 4310. That entry lies outside the file. The program passes it only the
function numbers 00, 08, 09, 0A, 0B, 0C and 0D. In the DOSBox 0.74-2.1 source that the GOG
distribution ships, the XMS callback reads these as version, free-memory query, allocate, free,
move, lock and unlock (SRC-DOSBOX-GOG-0742). None of them loads or runs a program.

## Alternatives

- The store search finds direct stores only, so an indexed or block store into either pointer is
  not excluded.
- Whether `4AE5:107D` and `4AE5:11A9` check that the probe succeeded before calling was not read.
  If they call with the pointer still 0, the call goes to `0000:0000`, the interrupt 0 vector's
  bytes, outside the file as well.
- The source shows what the interpreter's own XMS provider does. It is not evidence that the
  shipped interpreter binary was built from it, or that a different XMS driver could not be loaded
  in its place. Another driver's functions with these numbers are its own.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: `store_values.py <dsun> 43 45 b8 ba` lists the direct
stores. The unresolved calls are the ones FND-EXE-580's report lists, and AH at each is read by
decoding from the start of the range that report decoded around it.
