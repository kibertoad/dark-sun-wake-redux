---
id: FND-SOUND-060
title: Eleven direct refill-service sites include a repeat route inside the overlay-201 reseeding caller
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2322..28C9:24EA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0A22..2C5F:0C8D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000699B7..0x00069D75
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000720E3..0x00072213
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000722C6..0x00072376
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000884C1..0x00088521
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00088868..0x0008892C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00088D52..0x00088DA2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089D95..0x0008A181
tool: direct_callers.py, resident_listing.py and overlay_listing.py at e88d0ef8; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-007/008/059, FND-CONFIG-162/166, FND-EXE-596/603 and FND-RNG-053/096
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex;
overlay locations use descriptor plus code offset. The direct-call
query for 4611:0051 reports four resident and seven overlay sites.
All eleven align with calls in the reviewed containing entry streams.

| Containing entry | Site | Immediate use of returned state |
|---|---|---|
| 28C9:2322 | 28C9:235C | Continues to DS:0D9C test; no immediate result test. |
| 2C5F:0A22 | 2C5F:0A5E | Tests AL and calls 4611:03A5 when nonzero. |
| 2C5F:0AFA | 2C5F:0BEC | Tests AL and calls 03A5 when nonzero. |
| 2C5F:0AFA | 2C5F:0C1F | Repeats the service call while AL is nonzero. |
| 182 +1167 | 182 +138D | Repeats while AL is nonzero, then continues the enclosing operation. |
| 187 +22B3 | 187 +22EA | Continues to a DS:4454 double-word test. |
| 187 +2496 | 187 +24E9 | Repeats while AL is nonzero, then tests DS:13F7. |
| 199 +0BC1 | 199 +0C16 | Repeats while AL is nonzero, then LEAVE/RETF. |
| 199 +0F68 | 199 +0FA3 | Continues to DS:25BA comparison with FF and its outer loop. |
| 199 +1452 | 199 +1494 | Continues to DS:143D comparison with one and its outer loop. |
| 201 +05D5 | 201 +0767 | Repeats an offset-loop call followed by service while AL is nonzero. |

FND-SOUND-007's resident 0A5E call and FND-CONFIG-162's
overlay 199 +0C16 call are independent positive controls for MZ
and FBOV far-call forms. A separate query for 4611:0211 finds
the known PUSH CS/near CALL at 4611:01B4 (FND-SOUND-054),
controlling the same-segment near-call matcher. No near call of
0051 is reported. These are controls for query mechanisms, not
proof that every runtime transfer uses those mechanisms.

Known roots are the caller entries of FND-SOUND-007/008,
FND-EXE-596/603 and FND-RNG-053. The root query also identifies
182 +1167 through trampoline 56BD:00AC and direct calls from
overlays 192/194; 28C9:2322 has a resident direct caller. Actual
caller admission is not inferred merely from an inventory row.

### The finite overlay-201 repeat route

201 +05D5 is one of FND-RNG-053's reseeding callers. Its body
saves BP/SI/DI but has no local DS load or saved-DS restoration.
After earlier sound-on and digital gates, +0755 jumps directly
to service +0767. When returned AL is nonzero, +076E branches
to +0757: it passes words two, four and one to 204 +0000 via
trampoline 5787:006B, removes six bytes and calls 0051 again.
Thus the offset-loop entry at +075F can follow a service return
within the same enclosing invocation. There is no intervening
local DS assignment in this repeat edge.

If AL is zero, continuation starts at +0770 and makes several
resource/drawing calls. A later branch tests current DS:13F7
nonzero and SI equal to seven, requests sound eight, then calls
204 +0000 at +0810 with the same three words. Those intervening
children and current state must be assessed before claiming that
later call retains a changed DS. The repeat edge is the shorter
concrete route; it still requires actual service/frame admission,
a nonzero service result and the offset-loop callee's effects.

FND-SOUND-059 supplies the conditional changed-DS service return;
this caller topology does not establish its actual native state or
that the offset loop reseeds with a changed segment. FND-RNG-053's
own callee preconditions and the external clock-service contract
remain necessary. No new seed, frame address or native outcome is
assumed from the presence of a call.

The five direct AL-poll sites also do not prove a passive wait:
0051 can refill, publish fields and submit descriptors. In particular,
199 +0BC1 has the preceding stop/clear consequence bounded by
FND-CONFIG-166; under its stable conditions it returns zero on
the first poll instead of entering active refill.

## Interpretation

Q-RNG-040 can replace an unspecified later-route component with
201 +0767 -> +075F and its explicit AL/admission/callee conditions.
The other ten direct sites remain individually identifiable caller
contracts; their subsequent restoration and routes are not declared
closed. Q-SOUND-002 can reuse the caller inventory and result widths.
Far/indirect/computed entries, jumps, alternate starts, modified code,
actual argument/storage histories and complete external effects remain
outside this direct-call census. No complete reading, whole question
closure or observed native RNG failure follows.

## Alternatives

- Refill can reach a reseeding entry only after returning through
  unrelated callers: one repeat edge is inside the known 201 caller.
- A nonzero service result is ignored everywhere: five sites use
  AL-controlled repeat edges and two use AL-controlled stop requests.
- Direct-call coverage is exhaustive runtime admission: excluded
  transfers and actual selected state remain unresolved.

## How to reproduce

Run direct_callers.py for 4611:0051 and control 4611:0211;
retain sound-refill-direct-callers-controls.txt under
GAME_DIR/analysis/handler-census-20261106/. Review the resident
location ranges and overlay file ranges with their descriptors:
182, 187, 187, 199, 199, 199 and 201 in listed order. Reports are
sound-refill-resident-ui-caller.txt, sound-refill-resident-sound-callers.txt,
and sound-refill-overlay182-caller.txt, overlay187-caller-a/b.txt,
overlay199-caller-a/b/c.txt and overlay201-caller.txt, each overlay
name prefixed sound-refill-. Root controls are retained in
sound-refill-caller-root-controls.txt. Do not interpret paragraph aliases
from the raw caller report as independent containing functions.
No original game, emulator session or emulated call supplied evidence.
