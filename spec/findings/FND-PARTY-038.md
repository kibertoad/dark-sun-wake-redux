---
id: FND-PARTY-038
title: The C run time copies the program path after the environment as argv[0] on DOS 3 or later, and passes an empty string on earlier versions
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:0059
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0143..1000:015D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2877..1000:298E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:02AD..1000:02C4
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/resident_listing.py, store_values.py, direct_callers.py, overlay_listing.py)
environment: null
---

## Observation

**Program entry.** At `1000:0000` the program loads `0x57E0` into DX and stores it at
`cs:[0x2C4]`, runs interrupt `0x21` with AH `0x30`, reads the words at `ES:0002` and `ES:002C`
(ES still holds the segment DOS gave it), moves DX into DS, and stores AX at `DS:0092`, ES at
`DS:0090` and the word from `ES:002C` at `DS:008C`. After a call to `1000:01B0` it scans the
segment in `DS:008C` from offset 0 for the first pair of zero bytes, at most `0x7FFF` bytes, and
stores the offset of the byte after that pair at `DS:008A` (`1000:0046`).

**The argument list.** `1000:2877` is one of the seven start-up records that `1000:0220` walks over `DS:3994..39BE`
before the main routine is called. It saves its return address and two
more words at `DS:3916..391B`, reads the byte count at offset `0x80` of the segment in
`DS:0090`, and sets SI to the word at `DS:008A` plus 2 and CX to 1. When the byte at `DS:0092` is
3 or more, it scans the segment in `DS:008C` from that SI for a zero byte, at most `0x7F` bytes;
with none it jumps to `1000:02AD`, and otherwise CX becomes the string's length with its zero.
When the byte is below 3, CX stays 1. It then moves SP down by the even size of that string plus
the command tail, copies CX − 1 bytes from that environment offset and a zero byte onto the stack
as the first string, splits the command tail from offset `0x81` into zero-ended strings at
spaces, tabs and the carriage return, and builds the list of their offsets below them, ending with a zero word. It stores the string count at
`DS:0084` and the list's offset at `DS:0086`, and returns through `jmp [0x3916]`.

`1000:02AD` writes the 30 bytes at `DS:0056` ("Abnormal program termination") through
`1000:02A5` and calls `1000:03EE` with 3.

**The call of the main routine.** After the start-up walk, `1000:014C..1000:0158` pushes the words
at `DS:0088`, `DS:0086` and `DS:0084` and calls `277B:0024`, which is `277D:0004`
(FND-PARTY-034); it is that routine's only direct call.

**Other stores.** A search for every instruction that writes `DS:008A`, `DS:008C`, `DS:0090`,
`DS:0092`, `DS:0084` or `DS:0086` through a displacement with no base or index register, at
every byte of the load image and of every overlay's code, finds besides those above:

- `15F3:0782`, `15F3:0829`, `15F3:073F` and `15F3:0743` (found as `166B:0002`, `1675:0009`,
  `1666:000F` and `1667:0003`), in the routines `15F3:076B` and `15F3:06B6`, which set DS to CS
  first;
- `4AE5:09B9`, in `4AE5:08EB`, which loads DS from `cs:[0x0005]`, a relocated word naming
  `55CE`;
- byte patterns at `1BF3:649F`, `1BF3:65A7` and `1BF3:6658`, `48C0:0002` and a store with an
  `es:` prefix to segment `0x40` at overlay 205 `+007E`, which are inside other instructions, use
  CS, or go through another segment.

With a base or index register, the stores at those displacements address other records.

## Interpretation

The routine is Borland C's `_setargv`; `DS:0084`, `DS:0086`, `DS:008A`, `DS:008C`, `DS:0090` and
`DS:0092` are `_argc`, `_argv`, `_envLng`, `_envseg`, `_psp` and `_version`. On DOS 3 or later
the main routine's `argv[0]` is a copy of the zero-ended string DOS places after the environment's
final zero byte and its count word, which DOS sets to the full path of the program it loaded.
On earlier versions `argv[0]` is the empty string, and a path longer than 126 characters ends
the program with status 3 before the main routine runs.

## Alternatives

- `argv[0]` is taken from the command tail: ruled out; the tail fills only the later strings.
- A start-up record that runs after `1000:2877` rewrites `DS:0084` or `DS:0086`: ruled out for
  displacement stores by the search; a store through a computed address was not searched for.
- What DOS writes after the environment, and the version it reports, come from the operating
  system the game runs on, here the DOS of GOG's DOSBox (FND-EXE-010); neither was observed.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `resident_listing.py <dsun>
1000:0000..1000:0059 1000:0140..1000:0163 1000:2877..1000:29AA 1000:02AD..1000:02C4
15F3:06B6..15F3:06C4 15F3:076B..15F3:0786 4AE5:08EB..4AE5:0910 4AE5:0990..4AE5:09C0`,
`store_values.py <dsun> 008A 008C 0090 0092 0093`, `store_values.py <dsun> --indexed 0084 0086`,
`direct_callers.py <dsun> 277D:0004` and `overlay_listing.py <dsun> 205 0x8E910 0x8E935`.
