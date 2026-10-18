---
id: FND-UI-043
title: The button tail supplies zero-terminated caption bytes to measurement and positioned text requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0008..3EBE:0067
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0097..3EBE:00CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:02FF..3EBE:042B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:05EE..3EBE:0672
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3CE8:00BE..3CE8:00F2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:094B..4328:09D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050342..0x00050349
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00038E91..0x00038EA1
tool: tools/research/exec-census/resident_listing.py and immediate_search.py; tools/evidence/report.mjs table; Python 3.12.10, Capstone wheel 5.0.9 (module version 5.0.7), xxhash 4.0.1
environment: null
---

## Observation

FMT-UI-003's BUTN byte 6D gives the number of tail bytes at 6E.
FND-UI-004 records printable words, placeholders and zero-only
tails but leaves their purpose open. The literal 64/6D/6E operand
query locates consumers in module 3EBE. FND-PARTY-044 already
corrects its constructor's start to 0008: the inventory's 001F
start lies inside the preceding far call. This reading uses 0008.

The constructor reserves 96 stack bytes in total. A nonzero byte
at supplied button +6D enters a copy loop. It forms a far source
at button +6E with word-offset arithmetic and copies the current
declared number of bytes to a BP-relative local region at BP-60.
The count byte is reread at the loop condition. It then appends
zero at the derived local offset plus that count. The byte copies
use BP-relative SS addressing; the appended zero uses a DS-near
address derived from BP. Correspondence therefore requires an
admitted DS/SS relationship and nonaliasing valid local storage.
The reserved frame is not a declared 96-byte caption capacity:
other live local fields occupy that frame too.

The constructor, painter and metric helper have conditional
stack-limit calls to 1000:2E48. Their guard/error outcomes are not
characterized here; the described continuations require admitted
stack state and normal child returns/preservation.

On one nonzero-tail sizing path, and on later admitted caption
alignment paths, the constructor passes the SS far address of
that local string to 3CE8:00BE. That complete 52-byte helper
iterates until a zero byte. It sign-extends each nonzero byte to a
word, calls 1BF3:599E, and accumulates returned words modulo
65536. It increments only the supplied offset word. It has no own
length cap, segment-carry advancement or input-extent check.
The printed conversion mnemonic at 00D7 is the pinned decoder's
known 16-bit mnemonic mismatch; the actual operation extends AL
into AX, not into EAX.

FND-CONFIG-206 reads 1BF3:599E's complete local table lookup
and register preservation; FND-CONFIG-207 records a conditional
FONT-selector producer for its current record. Thus these are
concrete caption metric requests, with current table validity,
glyph-word meaning and native advance still conditional. A byte
at least 80 hexadecimal is not silently treated as a zero-extended
character index by the measurement helper.

The constructor's later nonzero-tail branch writes words at loaded
button +60 and +62. Its exact calculations depend on the loaded
bounds, flags, mask selectors and metric helper results. The painter
3EBE:042B subsequently supplies those two words as the first two
arguments to 4328:0859 at 0660. This qualifies them as the stored
caption-position request, not native pixel coordinates validated by
this finding. The complete alignment/flag and producer contract is
not assigned here.

At painter 05F1, zero tail length skips its caption path. Nonzero
length causes a fresh copy of the declared tail to a local region,
then an appended zero. Here both byte copies and terminator writes
use DS-near addresses derived from BP; the later helper receives
the SS far address. The same local-storage/segment conditions
matter. The painter does not use a persistent string prepared by
the constructor: it reads the current loaded tail again.

The call supplies the format at DS:3342, two word pairs, and that
local string pointer, besides the stored caption coordinates. A
bounded source query identifies two C directives followed by one
lowercase-s directive and a zero terminator. The helper scans four
code words at CS:0A11 and jumps through their paired targets.
The s entry targets 0983: it consumes a four-byte argument as a
far string pointer and sends every nonzero byte to local 06B5,
incrementing only the offset until zero. The C entry targets 09AE
and consumes two words; the two such directives therefore precede
the caption pointer in the argument stream as supplied. Those
state writes and the character helper's rendering effects are not
given an unconditional font/color/pixel interpretation here.

The formatter reaches this path only within its admitted region
iteration and setup. Its near argument cursor also depends on
the appropriate DS/SS relationship. This establishes caption text
requests, not a successful native paint or a complete formatter.

For valid nonaliasing input, an embedded zero in the tail remains
in the copy and ends the metric/string traversal before the declared
length. A nonzero length containing only zero bytes still takes the
copy/request path but supplies an empty string. Zero length skips
that path. FND-PARTY-232's eight class-choice buttons have source
length zero, while FND-PARTY-154 identifies the shared EXIT
button's short source label. These are static controls, not runs.

## Interpretation

The tail's basic consumer role is caption input, with temporary
zero termination, metric requests and positioned string output.
This removes its merely speculative caption meaning from Q-UI-001.
Remaining caption writers, layout flags, admitted storage, current
font state and backend/native outcomes stay open. Source tail bytes
and loaded caption-position words must not be conflated.

## Alternatives

- Treating the tail as unused editor text contradicts its reached
  copy, measurement and string-directive paths.
- Treating its count as a zero-terminated string length omits the
  declared-count copy followed by a separate appended zero.
- Treating the constructor's temporary string as the painter's
  persistent input omits the painter's fresh loaded-tail copy.
- Treating a 96-byte frame reservation as an admitted caption limit
  ignores other locals and the absence of an own count bound.
- Treating the request as proof of glyph pixels omits the formatter,
  table, region, alias and graphics dependencies.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and locked evidence Python.
Run immediate_search.py with 64 6d 6e, then decode resident
3EBE:0008..042B and 042B..0686 from their actual entries.
Use FND-PARTY-044's entry correction rather than starting at the
inventory's 001F. Follow the stated caption branches; this is not
a complete reading of every constructor, painter or child effect.

Read 3CE8:00BE..00F2 and compare its child with
FND-CONFIG-206/207. Read 4328:0859..0A11 and the four-word
code/target tables at 0A11/0A19. The code words C/P/c/s select
targets 09AE/09D6/0977/0983 respectively. The bounded numeric
query is caption-format-dispatch-query.json. The small zero-terminated
format query begins at file 50342 and stops interpreting bytes at
the first zero; its definition is button-format-query.json.
All retained queries and reports are under
GAME_DIR/analysis/ui-window-image-20261018/.

Keep source extent, live count, local-frame aliases, actual DS/SS,
current font/table and formatter setup as conditions. No original
game or function was executed and no image was viewed.
