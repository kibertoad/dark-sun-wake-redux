---
id: FND-PARTY-284
title: Four event-window publications share one exact pointer-byte condition for the wrapped alias
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3BA6:12B8..3BA6:13A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:04BB..3D72:04F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0BFD..3EBE:0CC1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4066:0003..4066:00C9
tool: tools/research/exec-census/resident_listing.py at 6d7c0303; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-CONFIG-215/216 and FND-PARTY-079
environment: null
---

## Observation

Q-PARTY-124 retains eleven unclassified writer values after
FND-PARTY-283. Four write double words at DS:A0FD and overlap
selected word A0FE. Installed DSUN.EXE is 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Offsets below are hexadecimal;
decimal resource number 1001 is word 03E9.

FND-CONFIG-215/216 and FND-PARTY-079 already connect pointer and
keyboard input handling with the event's window target. The bounded
source reading binds these four final publication operands:

| Store | Immediate pointer source |
|---|---|
| 3BA6:138C | Far argument at BP+6, reloaded into EAX at 1388. Its recursive route forwards that same first argument while changing the child argument. |
| 3D72:04E0 | Current global double word DS:A121, reloaded at 04DC after clearing separate selection fields. |
| 3EBE:0CA8 | Local far pointer BP-4, loaded from first far argument BP+6 at 0C26..0C2A and reloaded at 0CA4. |
| 4066:00A3 | Local far pointer BP-4, loaded from first far argument BP+6 at 0015..0019 and reloaded at 009F. |

The three handlers' continuing branches write an identifier to the
output event before publishing the window pointer. That identifier is
not the value written to A0FD. The pointer-hit route reads A121 at
publication time; it is not proved equal to an earlier saved selection.
Argument/local/global preservation and distinct output storage remain
conditions on composing these observations with the caller contracts.

Let O and S be the offset and segment words of the pointer actually
loaded for a publication. The selected A0FE word consists of O's high
byte as its low byte and S's low byte as its high byte. The store
therefore produces 03E9 exactly when:

- O is E900..E9FF; and
- S has low byte 03.

Equivalently, for the loaded double-word encoding P,
P AND 00FFFF00 equals 0003E900. This byte relation holds independently
for each of the four stores at its instruction point. None of these
publication instructions masks or normalizes the loaded pointer.

An admitted pointer with offset four, for example, cannot satisfy the
condition. But a window's existence, a nonnull test or a resource
identifier does not prove that offset. Actual window construction,
copied/reused pointers, selection writers, handler caller inputs,
frame/alias effects and preservation remain the producer obligations.
No whole window allocation or lifetime contract is inferred here.

The 3D72 source context confirms fallthrough from the input handler
through the inventory's separate 04BB tail range and the final return
at 04F0. This reading changes no inventory ownership or body ranges;
the store's value is the explicit current-global load in that tail.

## Interpretation

These four publication expressions are answered. Seven other writer
values remain unclassified. The four sites now retain a shared concrete
pointer-byte admission condition with their distinct sources and
preservation obligations. They are not unconditional exclusions and
do not reduce the 83 other direct getter sites or repeated-hook scope.
FND-PARTY-279/281/282/283's named conditions remain. FMT-PARTY-007
stays supported; no queue closure or complete reading is declared.

## Alternatives

- Event control numbers and far window pointers are different producers.
  A bounded control-number domain cannot exclude these pointer bytes.
- The selected overlap is neither the pointer's full offset nor its full
  segment. Testing only one complete word against 03E9 is insufficient.
- A saved argument is not proof that the live frame or copied pointer
  survives output writes and reached calls unchanged.

## How to reproduce

Run resident_listing.py on the fingerprinted DSUN.EXE with the four
half-open locations above. For incoming context also read
3D72:0009..3D72:04F1; retain the inventory's separately owned tail.
Reports event-pointer-alias-producers.txt and
event-pointer-selection-context.txt remain under
GAME_DIR/analysis/class-table-closure-20261106/.
Trace each publication's final load rather than an earlier pointer
snapshot. Reuse FND-CONFIG-215/216 and FND-PARTY-079 for the existing
handler and caller meanings. Intersect A0FD's four bytes with
A0FE..A100 to derive the two-byte condition. No original game ran.
