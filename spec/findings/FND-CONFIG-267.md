---
id: FND-CONFIG-267
title: Setup candidate marking, selection and fallback use different producer result gates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1861:0008..1861:0963
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:012A..17FB:01B0
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1ABD:0092..1ABD:00A6
tool: Ghidra 12.1.3 ReportFunctionSummary.java; resident_listing.py; tools/evidence/report.mjs table
environment: null
---

## Observation

Q-CONFIG-005 retains producer admission after FND-CONFIG-259..266.
This reads the connected candidate/selector/dispatch graph and its immediate
caller result use. The source is SOUND_DS.EXE,204593 bytes,
XXH3-128236c2dc23c071eca421eb5b427caee57. Offsets, card numbers and
word literals are hexadecimal; list sizes below are decimal. All contracts
require admitted table/frame backing and preservation across children.
No native source, interrupt, file, dialog or device result is demonstrated.

### Candidate marking is an ordered, mutating pass

1861:06DB initializes six candidate indicators to zero, plus two local
words for type/card selection. It performs all stages below in order,
without short-circuiting the later stages after finding one candidate.
FND-CONFIG-262 supplies307B's exact-name source-vector lookup and02CD's
any-group membership;259 supplies00B0's conditional group marks and
unconditional DS:ECCE assignment. Source-name pointers are DS-relative.

| Stage | Admission gate | Local candidate and immediate effects |
| --- | --- | --- |
| 66 | `BLASTER` at0BDA returns nonzero DX:AX, then02CD(66) nonzero | Set the BLASTER indicator to1. No00B0 call yet. |
| 81 | `SNDSCAPE` at0BE2 returns nonzero DX:AX, then02CD(81) nonzero | Set81 indicator, clear BLASTER indicator, call00B0(81). No15FD file parser at this stage. |
| 6A |17DA:000B(0) returns exactly AX1, then02CD(6A) nonzero | Set6A indicator, clear BLASTER indicator, call00B0(6A). |
| 7C | `ULTRASND` at0BEB returns nonzero DX:AX, then02CD(7C) nonzero | Set7C indicator, clear BLASTER indicator, call00B0(7C). No154E field parser at this stage. |
| 7D | `ARIA` at0BF4 returns nonzero DX:AX, then02CD(7D) nonzero | Set7D indicator, clear BLASTER indicator, call00B0(7D). No1783 field parser at this stage. |
| 82 |158E:0006(0) returns exactly AX1, then02CD(82) nonzero | Set82 indicator and call00B0(82); do not clear BLASTER indicator. |

The6A and82 producers execute before their membership gates and can
already perform their own conditional record writes. Reuse263/266's
contracts rather than treating this as a side-effect-free presence scan.
Earlier candidate indicators other than BLASTER are not cleared by later
stages. Thus the82 stage can coexist with an otherwise surviving BLASTER
indicator; the81/6A/7C/7D gates suppress that indicator only when admitted.

If BLASTER remains nonzero, the pass calls164C:03BE with a local type
destination initialized to zero and ignores its return. It maps the
resulting type6 to the current DS:ECCE,4/5 to73,2/3 to67 and1 to66;
other values leave the local card number zero. It calls00B0 with that
number in every such continuation. Repeated sources and DS:ECCE must be
preserved to infer a single coherent native selection.

The final branch graph returns AX0 for all-zero indicators or exactly
one indicator equal to1 with the others zero. It returns AX1 otherwise.
Under the admitted initialized/preserved0-or1 indicator domain, this is
AX1 exactly when two or more indicators survive. This return is therefore
not a count and not proof that no candidate exists when it is zero.

### Marked-record lookup and choice list

17FB:012A accepts a card number. In order MainGroup, MusicOnlyGroup,
DigitalGroup, it tests that group's membership and, if present, reads a
record EC word and compares it exactly with1. A matching read returns1;
otherwise it continues and ultimately returns0. An arbitrary nonzero
EC is not enough for this helper, although constructors use nonzero EC.

In all three branches the index call is17FB:01B0, the MainGroup index
helper. The later two branches use that index with the MusicOnlyGroup
or DigitalGroup base respectively. They do not call01DE/020C for those
reads. This is corroborated by actual near-call targets, not inferred
from the decompiler's repeated function names. A MainGroup index is not
automatically a same-card member of another group. Preserve this behavior
and its index/backing conditions rather than substituting matching-group
indices. No observed native mismatch is claimed.

1861:011D initializes ten local card words from20 bytes at DS:087C and
ten80-byte label slots from800 bytes at0890 via1000:03A9. All820 bytes
are zero in the verified loaded image. It checks012A for card numbers
66,6A,7C,7D,81,82 in that order. A nonzero result admits one entry.
For each entry, fresh membership tests choose the first present group in
Main/MusicOnly/Digital order, and that group's own01B0/01DE/020C index
selects a record whose leading terminated text is copied into the next
label slot with3A7A. Thus this label lookup differs from012A's EC read.
It then stores the card number and increments its local count, even if
the fresh group tests found none. At most six entries are added by the
root's own control flow; termination and label capacity remain conditions.

After a190F:0000 notice whose result is ignored, it calls19CF:000D with
the count, label array, DS:0BB0 and initial index zero. FND-SOUND-021
supplies the existing count/index/dialog limitations. The returned AX
directly indexes the local card words with low-word index*2. There is
no local FFFF guard or range check before that read. Therefore FFFF
from the dialog is not itself a guaranteed FFFF selector result; actual
returned index and selected local backing remain admission obligations.
The selector's own explicit candidate stores include81.

### Dispatcher distinguishes explicit choices from fallback

1861:0008 initializes its selected word toFFFF, calls06DB and calls011D
only when06DB returns nonzero. If called,011D's AX replaces that word.
A selected word other thanFFFF takes this exact dispatch:

| Selected word | Producer call | Dispatcher result after the child returns |
| --- | --- | --- |
| 66 |164C:044C | AX1, ignoring producer AX |
| 6A |17DA:000B(1) | AX1, ignoring producer AX |
| 7C |154E:000D | AX1, ignoring producer AX |
| 7D |1783:02F0 | AX1, ignoring producer AX |
| 82 |158E:0006(1) | AX1, ignoring producer AX |
| Any other word, including81 | None | AX0 |

Thus the selector can supply81 under an admitted entry/index route,
but this explicit branch has no81 producer arm. It does not fall back
on an unsupported selected word. This is local behavior, not an observed
UI defect or proof of a particular native selection.

When the selected word isFFFF, the fallback tries81,82,7D,7C,6A,66 in
that order. Each first requires nonzero17FB:02CD membership for its
literal number, then calls the corresponding producer above (81 uses
15FD:000F). Here a nonzero producer AX returns dispatcher AX1 immediately;
zero continues to the next candidate. Exhaustion returns AX0. The6A/82
calls use argument1; there is no rollback of earlier pass or failed-child
effects. All calls and result widths were checked against instructions.

At the recognized caller1ABD:0092, a nonzero dispatcher AX stores word1
to DS:D470; zero skips that store without clearing the word. Both routes
continue to17FB:03D3. FND-SOUND-021 supplies the later combined/separate
selection routes. Effects across03D3, initial D470 and later consumers
remain unresolved here. No successful driver installation, settings-file
write or complete producer lifetime follows from the dispatcher result.

## Interpretation

This removes local candidate suppression/multiplicity, marked-choice and
explicit-versus-fallback return gating from Q-CONFIG-005's incoming-producer
obligations. Existing producer contracts are now connected to that caller
without equating local AX1 with native success. Remaining source/table/frame
history, actual choice index, cross-group EC-read backing,03D3 effects,
other entry mechanisms and game-side consumers remain open. No complete
reading, format-status promotion or parity validation is claimed.

## Alternatives

Candidate marking is not always deferred until a selected producer succeeds.
The pass's zero result does not mean no candidate. The explicit branch does
not check producer success as the fallback does. A dialog FFFF return is
not checked before the selector's local-word load. MainGroup index use in
the other marker-read branches is not silently corrected in the finding.

## How to reproduce

Use the verified read-only SETUP-blocks project under
GAME_DIR/analysis/config-block-construction-20261108/.
producer-dispatch-ghidra.log summarizes0008/06DB;
producer-selection-consumers-ghidra.log summarizes011D and1ABD:000A;
producer-selection-marker-ghidra.log summarizes17FB:012A.
producer-dispatch-instructions.txt covers1861:0008..1861:011D and
1861:06DB..1861:0963. producer-selection-instructions.txt covers
1861:011D..1861:06DB,17FB:012A..17FB:01B0 and
1ABD:0078..1ABD:00B0. Reproduce with resident_listing.py and verify the
branch graph, argument widths, actual helper targets and all return paths.

dispatch-source-names-query.json reads31 bytes from file1033A, DS:0BDA
through0BF8; table result/repeat SHA256 both equal
37b00939f50894a25dccb02f9e3138976549585c8a3864a701c5e5abad150796.
selection-templates-query.json reads205 rows of four byte fields at
fileFFDC, covering820 bytes by the two literal initialization counts;
result/repeat SHA256 both equal
daac65ea53bbba0ba623448496b5e9b4f0e7f506d3b3734f796a450b7f423100.
Only successful bounded reports support the observations; retain all
original-derived output and query data in GAME_DIR.
