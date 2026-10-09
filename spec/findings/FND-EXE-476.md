---
id: FND-EXE-476
title: Sound utility field caller maps three-byte conversion low word before reading one-byte Irq field
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:0284..158E:0347
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 164C:0042..164C:00F1
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000FD57..0x0000FD5C
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x000002D2..0x000002D6
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-475's second marker success continues at 158E:0284. The caller
makes three consecutive 2F81 calls with the retained second record pair.
Each removes four incoming bytes, stores AL at SS:BP-0A and in
successive bytes BP-14, BP-13 and BP-12, then tests that stored byte
for FF. FF takes the recorded AX-zero common exit. It does not compare
full returned AX with FFFF; FND-EXE-379 reads that distinction.

After three continuing byte results it writes zero to BP-11 and passes
SS:(BP-14) to 2713. It removes four bytes and passes only returned AX
to far-returning 164C:0042; DX is not tested or passed. After two-byte
argument removal it stores returned AX at BP-2C. Only FFFF selects
the common exit. The string converter's complete arithmetic, classification
table and admitted input contract remain dependencies of this caller reading.

The complete local mapper at 164C:0042 loads its word argument into
DX and checks the following exact equalities. It returns the listed AX
for a match and FFFF for every other word. It contains no call, interrupt,
table-index calculation or signed range classification.

| Incoming word, decimal | Returned AX, hexadecimal |
| --- | --- |
| 220 | 0220 |
| 210 | 0210 |
| 230 | 0230 |
| 240 | 0240 |
| 250 | 0250 |
| 260 | 0260 |
| 330 | 0330 |
| 300 | 0300 |
| 332 | 0332 |
| 334 | 0334 |
| 336 | 0336 |
| 320 | 0320 |

All match routes join the same BP restoration and far return without
incoming cleanup. The helper changes DX to the incoming word and does
not locally change SI, DI, DS or ES. This word mapping alone establishes
neither the preceding conversion radix nor a hardware operation.
The call's segment word at shipped offset 6FCC is targeted by MZ
relocation record 165, zero-based. Its encoded segment 064C becomes
164C under modeled load segment 1000; this binds the native target
independently of a flat analyzer address.

The continuing caller passes the same retained record pair to 1BD4:02AA
with current DS:05F7. Under FND-CONFIG-004's segment 1E36 and
MZ header size 1400, that bounded shipped string is Irq= followed by
zero. Actual DS admission remains separate. Full returned AX zero exits;
any nonzero result continues. No position-reset call intervenes between
the prior field consumption and this matcher. FND-EXE-377 reads the
matcher's ordinary consumed-byte progression.

It then calls 2F81 once, stores AL at BP-0A and exits on byte FF.
Otherwise it copies that byte to BP-18, writes zero at BP-17, passes
SS:(BP-18) to 2713, removes four bytes and stores returned AX at BP-2E.
It performs no local result, digit or range test after this conversion and
continues at 158E:0347, outside the reading. Returned DX is again ignored.
The local field buffers, conversion returns and later stored-word meanings
retain their separate admission and consumer obligations.

## Interpretation

This follows the actual configuration consumer into two differently sized
local field strings and differently checked conversion results. The first
conversion's low word must match the fixed mapper to continue; the second
conversion's low word is merely stored. A returned non-FFFF mapped word
does not establish a valid hardware setting or successful downstream use.

Q-EXE-007 retains 2713's full contract and classification inputs, later
continuation from 0347 and stored-word consumers, actual DS/frame and
source admission, conversion extents, native preservation, aliases and
lifetime/re-entry. No complete reading or execution exclusion is claimed.

## Alternatives

Treating the three-byte field as a checked numeric string adds a digit test
the caller does not perform. Treating the mapping as proof of conversion
radix assumes the unread converter's contract. Treating either full conversion
pair as consumed ignores DX. Treating the one-byte field as range-validated
ignores its unconditional AX store. Treating the marker search as independently
positioned ignores the shared record state and absent reset.

## How to reproduce

At revision cb296d2 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00006F64..0x00007027 at IP 0284,
modeled CS 158E, and 0x00007902..0x000079B1 at IP 0042,
modeled CS 164C, with locked Capstone 5.0.7 in sixteen-bit mode.
Read file-data only over 0x0000FD57..0x0000FD5C, binding source
offset 05F7 under segment 1E36 and MZ header size 1400. Use
FND-EXE-360's native call bindings and the far operand's native target
164C:0042, not a flat analyzer alias. For this latter binding, read the
MZ relocation table at shipped offset 003E with 958 records; record
165 at 02D2 targets load-image offset 5BCC, the segment word at
shipped 6FCC for the far operand starting at 6FCA. Apply load segment
1000 to encoded 064C. Track the byte stores and FF tests,
terminators, low-word arguments and result stores, every mapper equality
and the sequential marker continuation. Original bytes stay outside Git;
no original process, DOSBox or emulated call is executed.
