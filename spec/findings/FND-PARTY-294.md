---
id: FND-PARTY-294
title: Existing startup setter and dynamic-service readings narrow vector admission while the loader vector remains mutable
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:01B0..1000:0220
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1E32..1000:1E41
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2F03..1000:2F13
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0140..4AE5:0193
tool: Existing-evidence review of FND-EXE-176/251/270/578 and FND-PARTY-293; census procedures at c334b967, scientific-method-engine 19.10.0
environment: null
---

## Observation

Q-PARTY-134 retains actual interrupt routing before applying the built-in
file-service contract of FND-PARTY-292. Existing findings answer more of
the encoded-site question than its previous general caveat stated.

FND-EXE-270 reads the startup installation request as AX=2500 and the
four adjacent restoration requests as AX=2500/2504/2505/2506. Thus these
five own instruction paths request vectors zero, four, five or six,
not vector 21. This is an argument exclusion, not proof the requests
succeed or that no other route enters their interiors.

FND-EXE-578 reads the dynamic-AH wrapper's single encoded direct caller
at 1000:1E3C as passing AH=54. That verified call path does not request
AH=25. Its raw pointer/call search exclusions remain: unknown indirect,
jump, bypass and modified arguments are not answered by a direct caller.

FND-EXE-176 binds the loader's shipped vector-selector byte to 3F.
FND-EXE-251 shows that the initializer reloads the current byte for the
set-vector request and exchanges mutable pointer words. The initial byte
is not a proof of its value at every invocation. Keep its actual selector
writers, effective state-segment identity and intervening effects open.
The 3F initial value therefore cannot exclude every later vector-21
request through this loader mechanism.

The refreshed base census, with the repaired inventory and explicit
1000:2BDB root, still lists eight reached immediate-AH=25 sites and the
dynamic wrapper as an unreached candidate. Its interior handler control
passes; the callback remains unresolved. It retains 84 unresolved
transfers, five gaps, four contested sites and negativeUsable=false.
No new universal routing exclusion follows from those counts.

## Interpretation

The fixed startup/restoration arguments and the known dynamic-wrapper
caller's service argument need no further speculative reading. Together
with FND-PARTY-293, these are bounded exclusions of known paths.
The remaining routing obligation specifically includes the loader's live
selector, direct IVT writes, computed/stack-built service arguments,
indirect or bypass inputs, drivers and external effects. Actual active
vector identity, ordinary-file admission, release correspondence and
memory/interleaving conditions remain Q-PARTY-134.

FMT-PARTY-007 remains supported. No complete reading or queue closure
is declared; the three word clients and eighty other sites remain open.

## Alternatives

- An unchanged shipped selector would keep the loader request at 3F;
  a writer or changed segment could admit another value. Neither is
  settled by the initial byte alone.
- Unknown entry paths can invalidate a direct-argument exclusion; the
  census explicitly retains them instead of treating absence as proof.

## How to reproduce

Review the cited findings' instruction paths and search exclusions first.
Rebuild the base query using reach_config.py with the current inventory
and q007_dsun_census.json, run the pinned engine's reach command, then
dsun_interrupt_census.py with both report and configuration. Require all
three instruction controls and retain every unresolved/gap/contested row.
Reports are in GAME_DIR/analysis/handler-census-20261106/ under after
and q007_dsun_census stems. Earlier findings retain their own historical
engine and inventory counts. No original game ran.
