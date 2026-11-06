---
id: FND-EXE-645
title: All 144 first-call overlay pointer candidates match relocated callback arguments under the retained argument-reader contract
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0699..1000:06BA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:077F..4842:07C2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49DE:00A7..49DE:00B4
tool: Pinned Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9); segment_table_readers.py, callback_args.py, overlay_listing.py, trampoline_target.py and trampoline_census.layout at a100acba
environment: null
---

## Observation

FND-EXE-644 resolves the nine first-jump candidates of the 153-site
overlay fixup/push report. The remaining 144 are first-call candidates:
128 reach another consumer directly, and sixteen first call the runtime
structure-argument copier `1000:0699`.

For the 128 direct cases, the retained `callback_args.py` procedure
resolves the callback slot in the table below to an immediate far pair
with a segment operand in the overlay fixup list. For every result the
raw segment equals eight times the descriptor of the header named by
the source candidate, and its offset equals that candidate's trampoline
offset. The source descriptor plus consumer call offset identifies 128
unique results; none is UNREAD, unrelocated, unknown, duplicated or
mismatched. This is an exact argument-reader correspondence, not a new
proof that all its candidate instruction starts are reached.

| Consumer | Callback slot in far frame | Candidates | Retained connected reading |
| --- | --- | --- | --- |
| 1000:030B | BP+06 | 1 | FND-EXE-579 exit registration |
| 1000:0361 | BP+06 | 1 | FND-EXE-579 interrupt-23 registration |
| Overlay 190 +11A1, near call within 190 | BP+06 | 1 | FND-EXE-587 global callback forwarder |
| 39D1:0418 | BP+06 | 5 | FND-EXE-587 global hook |
| 39D1:0430 | BP+06 | 6 | FND-EXE-587 global hook |
| 3A8E:0CDC | BP+0A | 5 | FND-EXE-587 window +F5 |
| 3A8E:0D0C | BP+0A | 15 | FND-EXE-587 window +F9 |
| 3A8E:0D3C | BP+0A | 18 | FND-EXE-587 window +FD |
| 3F96:02AB | BP+0E | 25 | FND-EXE-588 application-frame +62 |
| Overlay 204 +1A68 | BP+0A | 2 | FND-EXE-586, FND-PARTY-156 per-slot callback |
| 4842:077F | BP+06 | 1 | FND-EXE-646 timer callback publication |
| 49DE:00A7 | BP+06 | 1 | FND-EXE-583 stored callback |
| Overlay 182 +016B | BP+0C | 29 | FND-EXE-587 window forwarder |
| Overlay 182 +01EF | BP+0C | 6 | FND-EXE-588 application-frame forwarder |
| Overlay 190 +11A1, trampoline call | BP+06 | 12 | FND-EXE-587 global callback forwarder |

The sixteen copy-helper calls are immediately followed in their encoded
continuations by a far trampoline call to overlay 190: four to +0802,
four to +0D89, and eight to +139B. At those eventual calls the argument
reader's BP+1E slot resolves to the exact source candidate's relocated
trampoline pair in all sixteen unique cases. Together with the direct
cases this gives 144 unique expected/actual call-site and pair matches.
FND-EXE-587 already reads these three consumers and their indirect calls.

The copier consumes a CX-sized structure argument in the reader model;
here the model recognizes 24 bytes. FND-CONFIG-128 and FND-CONFIG-189
retain its own stack-reservation/copy contract. A static match does not
prove native frame, source/destination or alias admission for every
execution. The callback pair lies after the 24-byte structure, rather
than being the source pointer copied as header data. An ordinary returned
helper and intact caller stack are conditions, not observations of a run.

The timer case is overlay 196 +041D, after pushes at +0417/+041A.
FND-EXE-646 corrects the old contextual +041E label. Its argument is the
pair 5755:003E, targeting overlay 196 +040B. The timer publisher loads
that far argument and stores its two words in its slot table; loading a
far pair into registers is not a dereference of the callback as data.
The 49DE setter likewise copies its four-byte argument into DS:3475.
No new native timer, driver or callback execution was performed.

## Interpretation

Together with FND-EXE-644, the finite 153-candidate set now has a local
callback-argument classification under the retained reader and consumer
contracts. Repeating that pair/slot classification is not the next
header-access obligation. Q-EXE-033 still owns actual instruction/caller
and frame admission, destination/header aliasing, excluded computed or
copied-pointer routes and reached header-relevant target effects. This
result does not close that general census or qualify a complete reading
of FMT-EXE-003.

## Alternatives

- The argument reader chooses a contiguous decode from at most 96 bytes
  before each call, then scans backward, stopping at unsupported branches,
  calls, pops and stack adjustments. A result is conditional on that
  decode and stack model; it does not independently establish all starts,
  alternative predecessors or preservation by child calls.
- Near calls counted as far require the preceding push CS. Native frame
  integrity, SS/BP identity and non-overlapping valid stack/source areas
  remain requirements. The copy helper's modeled byte count is not an
  observation that a live copy returned or preserved every other field.
- FND-EXE-579/583/646/586/587/588 retain their writer/search, tag, record,
  adjusted-base, block-copy and dynamic-pointer exclusions. Source or
  destination aliases and transitive header writes are not ruled out by
  classifying the callback argument itself.
- The 153 records are overlay fixup/push candidates from FND-EXE-567's
  mechanism. They exclude arbitrary runtime-computed segments and are not
  a new inventory of all executable paths or all header readers/writers.

## How to reproduce

Use the scripts at a100acba with the pinned evidence Python and licensed
installed DSUN.EXE, size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Rebuild the pointer report with
`segment_table_readers.py <dsun> --pointer-sites` and select first-call
records. Resolve each source overlay's code base using the existing
`trampoline_census.layout` metadata (code span's first element), or
`trampoline_target.py`: subtract its printed target offset from its file
offset. Add the reported following-transfer offset to obtain a file call
site. No raw call segment should be treated as a relocated address.

For each direct group invoke `callback_args.py <dsun> <label> <slot>
<file call sites>` using the table's hexadecimal slot. For the sixteen
records whose first call is raw 0000:0699, use `overlay_listing.py` from
five bytes after that call to verify the immediate next far call. Invoke
`callback_args.py` there with slot 1E; it recognizes the preceding copy
helper and CX count. Verify 128 direct plus sixteen structure results,
unique source descriptor/consumer call offsets, exact original candidate
trampoline offset and raw segment equal to target descriptor times eight.
Any UNREAD, missing/duplicate site, unrelocated pair or mismatch rejects
the complete 144-result correspondence. Retain all reader exclusions.

Local outputs are GAME_DIR/analysis/header-pointer-first-call-arguments-20261106.txt
and header-pointer-structure-arguments-20261106.txt beside it. They are
reproducible locating/argument reports, not committed original content.
The exact candidate consumer offsets follow; the structure row names the
eventual calls, five bytes after the helper calls.

- Raw first transfer 0, 0x30b, 1 candidates: 180+014F.
- Raw first transfer 0, 0x361, 1 candidates: 180+015D.
- Raw first transfer 0, 0x699, 16 candidates: 175+0B87, 181+0034, 189+0604, 189+05C6, 189+0AD5, 189+08BF, 202+0646, 202+062C, 202+05FC, 203+011E, 211+09AD, 211+0880, 211+104F, 212+0A03, 212+09E9, 212+09AB.
- Raw first transfer 0x11a1, 1 candidates: 190+378B.
- Raw first transfer 0x138, 0x418, 5 candidates: 184+08E3, 199+07D6, 199+06B7, 199+0B31, 199+0855.
- Raw first transfer 0x138, 0x430, 6 candidates: 190+1185, 190+111A, 190+38F2, 202+051A, 211+070B, 212+08BB.
- Raw first transfer 0x140, 0xcdc, 5 candidates: 199+07BB, 199+08DA, 202+0310, 211+03BF, 212+06B8.
- Raw first transfer 0x140, 0xd0c, 15 candidates: 172+0941, 175+01F0, 190+37BB, 199+0601, 199+0B4A, 199+09A4, 199+0878, 202+034C, 203+008D, 203+09EA, 209+030B, 209+0B65, 211+040F, 212+074B, 213+00AC.
- Raw first transfer 0x140, 0xd3c, 18 candidates: 171+0214, 172+032E, 172+095A, 175+0203, 178+0567, 181+04DA, 190+37A2, 192+02E7, 199+066C, 199+05D3, 199+0B63, 199+09BD, 199+0847, 202+035F, 203+007A, 211+0422, 212+075E, 213+00C5.
- Raw first transfer 0x170, 0x2ab, 25 candidates: 172+0981, 175+01CA, 175+0191, 175+14C0, 175+1478, 178+05FA, 181+054E, 181+0523, 190+3804, 190+39A1, 190+3860, 190+381D, 202+03FA, 202+03C6, 202+03A8, 203+00AC, 203+0A09, 203+094E, 211+0493, 211+0459, 211+043B, 212+072B, 212+06F8, 212+088D, 213+00E8.
- Raw first transfer 0x1a68, 2 candidates: 204+029C, 204+01A1.
- Raw first transfer 0x258, 0x77f, 1 candidates: 196+041D.
- Raw first transfer 0x268, 0xa7, 1 candidates: 180+007B.
- Raw first transfer 0x5b0, 0x48, 29 candidates: 171+0194, 172+0696, 172+0913, 172+1273, 175+0165, 178+03FF, 178+04C4, 181+04B5, 183+0EE5, 184+08CE, 186+0348, 190+3767, 192+02B2, 194+049E, 199+062F, 199+05AC, 199+0AD4, 199+0972, 199+0820, 201+009B, 202+02ED, 203+0055, 209+02DD, 209+0B37, 209+12E2, 211+038F, 212+0688, 213+006E, 213+172F.
- Raw first transfer 0x5b0, 0x4d, 6 candidates: 202+0388, 209+010A, 209+0D1C, 211+04BB, 211+1D33, 213+18A9.
- Raw first transfer 0x5f0, 0x34, 12 candidates: 171+01B0, 172+03B7, 175+01D8, 181+04FF, 192+02CE, 194+04D1, 201+00BE, 202+036D, 203+00BA, 209+12C3, 211+03F0, 212+06D3.
