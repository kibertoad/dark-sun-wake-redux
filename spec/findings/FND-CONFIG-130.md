---
id: FND-CONFIG-130
title: The resident record-taking helper returns an index with a conditional record remapping
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0568
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2DBD
tool: Python 3.14.7 and Capstone 5.0.7 bounded disassembly; verified MZ relocation and FBOV fixup segment mapping
environment: null
---

## Observation

This entry corrects the raw segment labels in FND-CONFIG-116.
MZ relocations at 0x0001E417 and 0x0001E429 map raw segment 3F49 to loaded segment 4F49.
The bounded control-flow description is retained with mapped addresses.

The local helper called four times by the state-two/three block in
FND-CONFIG-129 begins at file offset `0x0001E3F8`, resident
28C9:0568, and returns at `0x0001E45E`. It passes DS:1408,
DS:140A and its input words at BP+12 and BP+14 to resident
31E0:2DBD at file offset `0x00029DBD`.

It retains the returned word as an index. With three-byte stride,
it reads a byte and word at `4F49:0C33` and `4F49:0C34`.
Unless that byte is one and the word differs from 9999, the original
index is returned. Otherwise the word selects a 23-byte record
through DS:19C1. If that record's first word differs from 5879,
the original index is still returned. When all three comparisons
pass, the helper returns DS:67DE plus 37 times the original index
as a word read, replacing the original result.

The wrapper's complete body has one callee and no direct store to
DS:0DAB. It reads the listed table and record fields; it does not
assign a gameplay role to the result. Its callee 31E0:2DBD has a
separate prologue at `0x00029DBD` and return at
`0x0002A1A6`; its body includes further local and far calls.
This reading does not claim those dependencies preserve DS:0DAB.

## Interpretation

The caller's signed zero-through-three or minus-one tests apply to
a potentially remapped returned index, not automatically the first
callee's result. The wrapper itself provides no direct state-word
transition to one, while the transitive callee effects remain open.

## Alternatives

The initial index's range, table producers, record identity, and
31E0:2DBD's helper effects remain unread (Q-CONFIG-008).
A callee can change state even when this wrapper only reads it;
absence of a direct wrapper store does not prove state preservation.
The conditional remapping alone does not establish a player action
or a complete interpretation of the selected record.

## How to reproduce

Read the complete bounded wrapper `0x0001E3F8..0x0001E45F`.
Track the one call's four word inputs, the retained index, three-byte
table selection, 23-byte record check and final 37-byte-stride word
read. Check each bypass to the common return. Resolve the relocated
callee operand to 31E0:2DBD using the resident load segment, then
locate its prologue and return without importing its unread callees'
effects into this wrapper finding. Compare the result tests in
FND-CONFIG-129 and argument forwarding in FND-CONFIG-128.

For the segment-label correction, select each named operand from the
MZ relocation list or its overlay's declared FBOV fixup list. Apply the
recorded load segment to MZ operands; decode an overlay operand's shifted
descriptor index and resolve its segment-table entry before labelling an
address. Raw segment operands and mapped addresses are different forms.
