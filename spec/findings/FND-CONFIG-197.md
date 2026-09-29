---
id: FND-CONFIG-197
title: Refresh wrappers temporarily replace one shared word and normalize returning completion to zero
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0DE5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0E10
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:0B4C
tool: Python 3.14.7 and Capstone 5.0.7 complete bounded resident 16-bit readings and MZ relocation checks
environment: null
---

## Observation

FND-CONFIG-171's refresh helper selects 3D72:0DE5 before
its local 2FB8 clear and 3D72:0E10 afterward when the respective
fresh word DS:3330 test is nonzero. Each receives current word
DS:33B8. These tests and arguments can observe different state
across intervening calls.

The complete wrapper bodies occupy file 0x00033705..0x0003372F
and 0x00033730..0x0003375A inclusive, respectively. Each saves
BP, allocates one local word and performs the runtime stack-limit
guard through 1000:2E48. Their guard segment words at file
0x00033714 and 0x0003373F are declared MZ relocations.

After the guard returns, each snapshots current DS:A033 into
its SS-frame local word, copies its stacked word argument into
current DS:A033, calls one same-segment service using a push-CS
and near-call far-return frame, and writes its saved local word
back to current DS:A033. 0DE5 calls 3D72:0942; 0E10 calls
3D72:0B84. Both then explicitly clear AX and return far. They
have no own incoming-word validation or callee-result test.

FND-CONFIG-189 reads both active services. Their calls, early
exits, stored handles and commit gates therefore run with the
wrapper's temporary A033 assignment initially in place, conditional
on valid DS, SS-frame storage and intervening state preservation.
The restoration uses DS at the later instruction; it does not
independently restore DS or other fields the callee changed.
The services' early exits do not bypass the wrapper's restoration
when they return normally. Nonreturning guards or services do.

The saved-word write restores one value under unchanged segment
and unmodified local-storage conditions. It is not a transaction
covering the service's graphics releases, handle requests, callback
calls, state assignments or native VGA effects. The wrapper's zero
return is its own normalization, not proof of successful rendering.

## Interpretation

These wrappers supply a temporary producer for A033 and connect
the refresh branch to the two active services in a specific order.
The first nonzero-3330 branch uses 0942, and the later one uses
0B84; those bodies have distinct effects in FND-CONFIG-189.
A caller that sees zero cannot infer which callee paths ran or
whether a graphics request succeeded.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain the 3330/33B8 producers,
DS and SS-frame alias conditions, callback changes, later writers,
accepted handles and native hardware outcomes. One reading keeps
the caller's segments and frame intact through returning services;
another changes shared or aliased state before restoration. Complete
callee/input provenance would distinguish the code-decided parts;
owner observations remain necessary where hardware decides output.

A reading that these wrappers preserve the entire pre-call display
state is unsupported: their own restoration covers only A033, and
the active services have other conditional writes. A reading that
the wrappers forward a service failure result is ruled out by their
common AX clear. No native or emulated result is claimed.

## How to reproduce

Read 3D72:0DE5 through its far return at 0E0F and 0E10 through
0E3A. Verify both runtime-call relocations and the push-CS/near-call
frames targeting 0942 and 0B84. Follow the SS-local snapshot,
argument assignment, post-call current-DS restoration and AX clear.
Compare FND-CONFIG-171 for the two refresh branch gates and
FND-CONFIG-189 for the services' complete local effects. Keep
shared-state restoration separate from native success or rollback.
