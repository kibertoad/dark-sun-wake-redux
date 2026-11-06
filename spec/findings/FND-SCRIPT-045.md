---
id: FND-SCRIPT-045
title: MAS1 registers GPL37 0A41 as the region TALK trigger for normalized key minus99
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: retained script_decode.py report from 211ea53c; composition with FND-SCRIPT-015/028/029/038 and FND-SCRIPT-044 at 1b3edeeb
environment: null
---

## Observation

This connected producer-to-consumer assessment reuses the retained shipped
script index from FND-SOUND-077. GPLDATA.GFF is 2191945 bytes,
XXH3-128 2bd1d2b989eef2dcb0545feb95975fca; OBJEX.GFF is
6816516 bytes, XXH3-128 e08d3cac153c547f6804ca27b155915d.
The whole-file location identifies that resource census scope.
No original game or emulated call was used.

### A concrete direct registration supplies the script pair

MAS/1 at 0135 has opcode 6E with three literal parameters: offset
2625 decimal (0A41), GPL number 37 and signed key -99. It appears
among the neighboring one-key registrations at 0105..014D in the
retained decode. FND-SCRIPT-038 independently reads opcode 6E's
native handler at 172C:1D24 and its builder/inserter interface:
it reads three parameters, builds a seven-byte prefix with offset at
zero, script at two, signed key at four and a MAS99-origin byte at six,
then inserts it through 172C:2EA1 on head 4C10:000F.

FND-SCRIPT-015 reads the one-key insertion: it follows signed key order,
uses the existing equal-key node or attempts to obtain one from the free
list, and copies the seven-byte prefix when a node is available. A
reached instruction does not therefore guarantee a new live record:
free-list/capacity, actual near-structure binding, list topology and
publication/retention remain conditions (Q-SCRIPT-019).

Under the recorded current-script binding MAS/1 is not MAS/99, so its
origin byte is zero. FND-SCRIPT-028 reads region change 1695:000B:
on the ready-state path it prunes origin-byte-zero records from this
one-key list before running the new region's MAS script. The registration
is consequently subject to that regional lifetime, not a permanent
MAS99 registration. Actual admission to MAS/1, later equal-key
replacement, pruning, restores and other producers remain unresolved.

FND-SCRIPT-044 maps the direct pair (GPL/37, 0A41) to GPLI entry
168 for conversion. The registration itself uses offset 2625 and script
37, not entry number 168. A search for literal 168 in registrations
would therefore miss this known producer. The index is a conversion
representation, not proof of how the live caller stores the pair.

### TALK dispatch has an evidenced conditional consumer

FND-SCRIPT-028 reads wrapper 1695:0218 and walker 1695:06FF:
the wrapper uses head 4C10:000F; the walker requires script state
two, normalizes its second object argument through the recorded
number helper, publishes the actor/object context and calls the inner
one-key walker 1695:075D. That walker invokes selector-one script
execution from the first record whose key equals the normalized event
key. For a selected retained registration above, that call is GPL/37
at 0A41 when the normalized key is -99.

The normalized key must not be equated with the caller's raw slot or
object argument. FND-SCRIPT-033/034 distinguish the field-zero getter,
metadata/kind binding and actual object-domain obligations. This finding
does not identify a particular live object or assert that passing raw
-99 necessarily supplies the normalized key.

FND-SCRIPT-029 binds the TALK event to wrapper 1695:0218 from
overlay 178 +032F and +0B93 with actor DS:426D and the selected
slot, after 2C5F:018C(slot, 1). Those are concrete interface routes,
not observed clicks or proof of their earlier state and child results.
FND-SCRIPT-043 supplies the subsequent GPL/37 local control paths.

The same positive registration query also identifies GPL/48 at 0363,
which uses opcode 6E for GPL/37 offset one and key -96. This is a
distinct route corresponding to conversion entry 167, not a producer
for offset 0A41. No exhaustive absence claim about other registration
opcodes, computed operands or native/restored records is made.

## Interpretation

Q-SCRIPT-024 no longer needs an unidentified producer for the 0A41 pair.
It needs actual MAS/1 execution and registration admission/retention,
ready trigger state, normalized TALK key -99 and selection of that
record, then the state/frame/cursor conditions already held there.
Converted entry 168, independent entries and changed records remain
separate admission mechanisms. Shared post-error effects remain
Q-COMBAT-059 and Q-SCRIPT-003. No whole-entry complete reading,
actual malformed execution or end-to-end TALK outcome is established.

## Alternatives

Treating the conversion ID as the live registration operand conflicts
with the three literal parameters. Treating registration as an invocation
omits pool/list and event-selection conditions. Treating the raw event
object as the normalized key omits the field-zero conversion. Conflating
the GPL/48 registration with the MAS/1 registration erases their different
entry offsets and keys.

## How to reproduce

Reuse music-health-all-scripts.txt and music-health-script-sweep.txt under
GAME_DIR/analysis/handler-census-20261106/, generated by committed
script_decode.py at 211ea53c with --all and --sweep and verified source
fingerprints above. Read MAS/1 at 0105..014D and GPL/48 at 0363.
The positive query is decoded opcode 6E with literal offset/script pair
2625/37 or 1/37; keep the third parameter and resource context. This
query makes no negative census claim.

Reuse FND-SCRIPT-038's complete native registration bindings,
FND-SCRIPT-015's insertion, FND-SCRIPT-028's event/head/walker and
pruning interfaces, FND-SCRIPT-029's TALK callers and
FND-SCRIPT-033/034's normalized-key distinctions. FND-SCRIPT-044 gives
the separate bounded GPLI conversion query and controls. No new reader
or original-derived output is committed.
