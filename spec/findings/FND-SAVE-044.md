---
id: FND-SAVE-044
title: Startup constructs a separate VMSHELL fallback pathname while controlled offset queries retain cleanup-path publication gaps
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067321..0x00067441
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 167B:000B..167B:0029
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1383..1425:13C8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0325..15F3:0360
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D230..0x0004D23C
tool: shared x86-scan-index through tools/evidence/report.mjs at 9cb82114; executable-reader 4.3.0; scientific-method-engine 19.10.0; Capstone distribution 5.0.9; window_operands.py at 5764eabf and committed resident/overlay readers; Python 3.12.10
environment: null
---

## Observation

This reviews Q-SAVE-007's remaining cleanup-path publication question
using FND-SAVE-043's actual empty-path observation. FND-EXE-398/403/405/406
read the file-backed producer, constructor and manager; FND-EXE-558/559
read cleanup's separate stored-path argument. A constructed fallback name
is not automatically the pathname that cleanup later passes to deletion.
Source: installed DSUN.EXE,634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Other executables/editions are outside
this audit.

### Controlled offset and caller queries

The retained declared scope contains205 regions and588250 source bytes,
including mixed resident spans and overlay code. The shared index scanned
every offset in those regions. It recorded262652 candidate rows,
9235 undecoded offsets in8575 ranges, and1748 unresolved call candidates.
The matching rows, undecoded ranges and unresolved calls were not truncated
by the10000-row query limits. All these are raw decodes, not established
instruction starts, reachability or an absence proof.

The index identity includes the source hash/size, Capstone distribution,
engine source digest and prepared-config digest. Initial build and four
subsequent queries shared cache key
e1138eb3579777496a6dcc064f664c856638eb41ae05e401fa6308d059773b2b;
the later queries reported cache hits. The decoder bindings report module
version5.0.7 despite distribution5.0.9; the locked distribution is the
index identity, as the engine requires.

Independent controls were the known pointer argument at167B:0032
(immediate4022), known byte read at167B:001D(displacement4022),
manager gate store at1425:1468(displacement00C4), and the game-cleanup
call at file6798F to167B:0029. All matching-site controls passed.
The existing window enumeration also included the established complete
manager and callback ranges as explicit extras; its duplicate callback
row is one site, not another use.

For literal4022 the shared queries returned one immediate candidate,
the known cleanup argument, and two displacement candidates: the known
startup-error byte read and an unestablished decode at file7A73B,
overlay190+23FB. The latter's candidate memory operand is a read through
SS with BP+SI. It is not an established access to the cleanup buffer or
a reported writer. The legacy401F..4023 window enumerated the same two
unique listed uses and no listed store. This does not exclude writes via
other displacements, larger overlapping operands, computed addresses,
string/block operations, changed code or the retained decoder/transfer gaps.

### The constructor's connected startup caller

A call-kind query found one raw caller of167B:003F, file67439.
It was qualified by reading from the already established exported startup
entry, overlay180+0141,file67321(FND-CONFIG-149), through that call.
The prefix copies its incoming path into data44F2. It asks1000:381E
for a backslash and, on a zero result, a colon. A returned pointer is
advanced one byte for the cut; with neither, the cut is the buffer start.
It writes a terminator at that cut, then passes pointer44F2 and the
literal32-bit value1500000 to167B:003F. The outgoing pushes and resolved
far call are connected; no extra input producer is inferred from the raw hit.

FND-EXE-403's recorded arithmetic therefore supplies manager quantity95
for this fixed caller, under its admitted frame/continuation: addC000
modulo32 bits, divide upward by4000 and narrow to a word. This is a
bound input, not proof of successful native capacity or all manager outputs.

The constructor's fifth record has type4 and a local pathname at
SS:BP-88. It copies the directory prefix there, then appends the loaded
suffix at data0230. A bounded source-data read identifies that suffix as
VMSHELL.TMP, eleven bytes followed by a terminator. With disjoint valid
strings, no offset wrap, unchanged suffix and prefix length0..52,
FND-EXE-405's full-append branch produces prefix plus that basename
within the local64-byte field. Longer or unterminated inputs retain its
signed/truncation/copy behavior; they are not admitted as this short case.

This supplies the known local file-backed pathname construction consumed
by FND-EXE-398. No explicit binding of that local string to data4022
appears in the reviewed constructor/caller bodies. Actual frame/field
aliases, file-backed selection and live native handle identity remain open. The distinct basename
is a string-domain observation, not proof that every possible filesystem
alias or malformed prefix names a different file.

### Separate startup-error consumer and remaining mechanisms

FND-EXE-403 calls167B:000B on its manager-error route. That helper calls
1425:1383,138D and13BE, then reads the first byte at current data4022,
pushes the word AX and calls13D2. FND-EXE-404 shows that13D2 consumes
only the low byte. The preceding query return words are not used as a
pathname or replacement argument. The helper returns the last query's
word without a local cleanup-path store. Its pointer source remains the
current DS after those calls, not an unconditional canonical data identity.

The connected queries call the already recorded interfaces in
FND-EXE-394/581. The small15F3:0325 body performs its fixed CS-relative
open/device test, closes the returned handle on its continuing open path,
and conditionally requests interrupt67/AH42. It restores its saved DS,
DX,CX and BX under an intact native frame. No caller pathname destination
is supplied by this query. These local facts do not certify every native
or indirect callee effect, storage identity or frame alias.

Two concrete excluded writer mechanisms remain relevant. The known slot
producers address read-target words from3F42 with fourteen-byte stride;
index16 would place such a word at4022. FND-EXE-391/393/396/398 do
not independently bound every reloaded index. This arithmetic overlap is
not evidence that the original reaches index16. FND-EXE-405's initial
unbounded copy and partial-append cases also retain actual source lengths,
stack extents and aliases; a valid short local filename does not admit
every source. Other computed/block writes and decoded-scope gaps remain.

## Interpretation

The actual constructor caller and its conditional short fallback name are
now bound; the startup-error helper is a separate byte consumer with no explicit
pathname-field store in its own body. The controlled queries enumerate decoded uses
and retain explicit gaps. They do not prove that the cleanup path always
stays empty or identify every nonempty-path producer. Q-SAVE-007 retains
actual publication/alias/index admission and native file-backed outcomes.
No queue closure, complete reading or parity promotion follows.

## Alternatives

Equating the local VMSHELL name with cleanup's stored field invents a
copy not present in the reviewed bodies. Treating an index hit as a
reached instruction ignores its raw meaning. Treating a cache hit or the
absence of a matched store as a global writer proof discards decoder,
transfer and computed-write limits. Treating an index16 arithmetic overlap
as an observed state invents its producer reachability.

## How to reproduce

Keep configurations, reports and the SQLite cache under GAME_DIR/analysis/
handler-census-20261106/. Reuse the retained q007_dsun_census-config.json
source scope: source,xxh3,sourceKind and regions, with loadSegment1000
hex and scanLimit1048576. Its region bounds come from committed
reach_config.py's hash-checked segment-table/overlay procedure and the
current inventory. The inventory SHA-256 is
820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2.
Changing entry/range metadata requires a rebuilt index, not reuse of the
old cache key. Preserve the declared source views and all reported limits.

Run the committed wrapper's x86-scan-index command on the retained
cleanup-path-*-index-query.json files with these queries. Values are hex;
call values and control sites are source-file offsets.

| Query | Kind | Value | Matching-site control |
| --- | --- | --- | --- |
| pointer | immediate | 4022 | B9E2 |
| reader | displacement | 4022 | B9CD |
| store-control | displacement | 00C4 | A8B8 |
| startup-callers | call | B9EF | none |
| callback-call-control | call | B9D9 | 6798F |

The last independently controls the call mechanism. The local cache
directory is cleanup-path-scan-index. Never treat these reports as absence
proofs.
Run window_operands.py with the inventory, extras1425:13FE..1425:167A
and167B:0029..167B:003F, and windows401F..4023/00C4..00C5 for its
separate enumeration.

Use resident_listing.py over the listed resident ranges and
1425:13FE..14A0 for the independent store control; overlay_listing.py
180,file67321..67441 qualifies the caller. The generic table command
reads64 bytes at file4D230, stride1, one byte field, as a bounded
lookahead rather than an asserted string length; require its first
terminator after eleven bytes. No original process or emulated call ran.
Keep all original-derived results local.
