---
id: FND-EXE-611
title: The 7 routines of overlay 176 that no entry placed test whether a member's power in effect can be kept, derive four bytes from a signed byte, clear a known power's rank, return a power's cost or third byte, do nothing, and return the name of a power's DATA record
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061674..0x000616D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061B36..0x00061B8E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061BB4..0x00061BE2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061C82..0x00061CAC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061E8F..0x00061E94
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062038..0x00062067
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 7 rows of overlay 176 (header segment `5691`, 19
trampolines, code from file offset `0x61540`, 2,855 bytes) that no spec location covered, 326
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x61540` plus it, and `DS` is `57E0`. Every routine has a trampoline in `5691`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. Overlay 176 holds the psionic power routines (FND-PARTY-210). Member
`m`'s byte for power `p` is at `4E71:09CD + 34 * m + p`, with the rank in bits 1 to 7 and bit 0
set while the power is in effect (FMT-PARTY-004). The power table psionic_powers has three bytes
per power at `51F1:07E0 + 3 * p`: byte 1 is the cost and byte 2 the byte that keeps a use from
marking the power in effect when it is 99 (FND-RNG-046, FND-PARTY-210). The word `+2` of a
49-byte record is the current psionic points (FMT-PARTY-001). Overlay 176 `+0000` returns 1 for
`m` of 4 or more and otherwise whether the rank of `p` is nonzero.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0134` | `0x61674..0x616D6` | `5691:002F` | Takes a member `m` and a power `p`. Returns 0 for `m` of 4 or more and when bit 0 of `m`'s byte for `p` is clear. Otherwise returns 1 when byte 2 of `p`'s table entry is at most word `+2` of the 49-byte record of the slot that word `4F49:0C34 + 3 * m` names (unsigned), and 0 when it is greater. Called from `+0196` at `+0412`. |
| `+05F6` | `0x61B36..0x61B8E` | `5691:007A` | Takes a signed byte `c` and four near pointers to bytes, and writes `(c + 6) / 4`, `(c + 1) / 2`, `2c + 1` for `c` below 4 and `c + 5` otherwise, and `(c + 1) / 2` for `c` below 9 and 5 otherwise, each quotient signed and rounded toward zero. No direct call was found. |
| `+0674` | `0x61BB4..0x61BE2` | `5691:003E` | Takes `m` and `p`. When `+0000` with `m` and `p` returns nonzero it ANDs `m`'s byte for `p` with 1, clearing the rank and keeping bit 0. For `m` of 4 or more `+0000` returns 1, so the AND lands past the four members' rows. Called from overlay 184 `+1913`. |
| `+0742` | `0x61C82..0x61C97` | `5691:0057` | Takes `p` and returns byte 1 of its table entry, the cost. Called from overlay 174 `+075C` and `+07B3` and overlay 198 `+03F9`. |
| `+0757` | `0x61C97..0x61CAC` | `5691:005C` | Takes `p` and returns byte 2 of its table entry. No direct call was found. |
| `+094F` | `0x61E8F..0x61E94` | `5691:0025` | `push bp`, `mov bp, sp`, `pop bp`, `retf`: returns at once. Called from overlay 193 `+20C9`, in `+20C6` of FND-EXE-606. |
| `+0AF8` | `0x62038..0x62067` | `5691:0020` | Takes a power `n`. For `n` from 0 to 33 it calls `2C5F:0003` with `n + 235`, which returns the `DATA` record of that number, and returns the far pointer to its byte `+0x20`, the record's name; for any other `n` it returns `DS:064F`, the text `ERR`. A second test of `n + 235` against 235 and 269 always passes. Called from overlay 202 `+0738`, overlay 209 `+1022`, `+105B` and `+112F`, overlay 211 `+12DC`, and overlay 213 `+0890` and `+08CB`. |

## Interpretation

`+0134` tests whether a power the member has in effect can go on: it compares the third table
byte with the member's psionic points, which reads that byte as the points it takes to keep the
power going, in line with the maintenance of RULE-MAGIC-003 and with 99 marking a power that
FND-PARTY-210 never leaves in effect. `+0742` and `+0757` return the cost and the third byte to
other overlays. `+0674` makes a known power unknown again; overlay 184 calls it during
generation. `+0AF8` gives a power's name from `DATA` 235 to 268, the numbers FND-PARTY-108 gives
the powers. `+05F6` derives four small numbers from one value; with no caller its use is unknown.
`+094F` is an empty routine. The names stay `fn_176_<offset>` until a reading shows what the
fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+05F6` and `+0757` may be reached only through their
trampolines by a computed call, or be unused. Byte 2 of a power's entry may be a minimum score or
level of some kind that `+0134` compares with the psionic points by accident; nothing here reads
the values. The member index `m` of `+0134` and `+0674` is taken as a party member from the
`4F49:0C34` and `4E71:09CD` tables it indexes; overlay 184 `+1913` was not read for its
arguments.

## How to reproduce

Run `overlay_listing.py` for overlay 176 against the installed `DSUN.EXE` over each file range in
the table and over `+0000..+005B`, run `direct_callers.py` for each offset (`176+<offset>`), read
the text at file offset `0x4D64F`, and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
