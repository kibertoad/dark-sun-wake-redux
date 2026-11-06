---
id: FND-SOUND-066
title: Music-selector RNG children have bounded local writes and preserve segment bindings under intact frames
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:07D9..1000:07F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0822..1000:0848
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:0519..2834:063F
tool: resident_listing.py at 8b1658c3; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-RNG-006/011, FND-SOUND-012/064/065 and RULE-SOUND-003
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-064/065 retain RNG-child effects when applying the
shipped music-list bounds. Existing FND-RNG-006/011 already
supply the chance and signed-remainder contracts; this finding
bounds the connected local write/segment interface used by the
selector rather than rereporting those arithmetic rules as new.

### Connected child footprint

2834:061C saves BP/SI, reserves two local bytes and holds its
argument in SI. Argument zero returns zero with no child call.
Otherwise it calls resident 1000:0822 once, sign-extends AX
with the unprefixed word instruction, uses signed word IDIV SI,
and returns the saved remainder. Its own writes are stack/frame
writes. It makes no DS/ES load and restores its saved SI/BP.

0822 reads the two words DS:384A/384C and calls near 07D9.
That arithmetic helper uses register exchanges and multiplies,
saves/restores SI and returns with ordinary near RET. It has no
non-stack memory operand, child call, interrupt or segment load.
0822 then stores the updated high and low RNG words at DS:384C
and DS:384A and masks its returned AX to 0..32767 before RETF.
It has no local DS/ES load or other non-stack store.

The complete 0519 chance body compares its argument with ten
first. Ten sets its local result register to one and reaches the
common AX result/RETF without a child call or stack-local store.
The one-through-nine paths call 061C with ten; other arguments
return zero. Its explicit writes are register/stack operations and
the writes of that same connected generator chain. No branch of
these four bodies performs an indirect transfer, interrupt or port
request. None changes SS locally.

Thus, under unchanged code, intact balanced frames and the stated
ordinary returns, these children preserve current DS and ES. Their
non-stack write footprint is exactly DS:384A..384E, half-open.
With that same DS binding it does not overlap music globals
DS:424E..4255. Actual table/list backing, frame aliases and external
interruption remain prerequisites before extending this exclusion to
those buffers. The fixed state words can still alias a buffer under
another segment/address mapping; this interface is not a global
harmlessness or lifetime proof.

### Applying the already-recorded arithmetic

FND-SOUND-065's unchanged mode-one fallback list has nine entries.
The selector passes eight, not nine, to 061C. Under FND-RNG-011's
admitted generator/frame contract, its index range is 0..7. For
mode-three lists of four/three/three, the ordinary passed last-index
values are three/two/two, producing 0..2, 0..1 and 0..1. Health
eight instead passes zero, returns index zero and consumes no draw.
Those are the last-entry exclusion and health-eight cases already
recorded in RULE-SOUND-003, now composed with measured populations.
They are ranges, not a claim of uniform selection or a native trace.

The shipped mode-three chance bytes are ten. On those retained
inputs each chance request returns true without an additional draw.
Even the found-region branch's two chance requests do not themselves
advance RNG in this case. Changed bytes and input/frame mutation
require separate admission. The selector's random-index request can
still draw once for a nonzero passed value. Physical song dispatch
and the driver children remain Q-SOUND-004, not RNG-helper effects.

## Interpretation

Q-SOUND-011 can reuse a concrete RNG-child segment/write interface
instead of treating these known calls as unspecified table writers.
Remaining alias evidence names the exact RNG-state interval, actual
table/list/frame backing and external interference. The random helpers'
unrelated callers and whole RULE-RNG-001 reading are not absorbed into
this format. Unknown accesses elsewhere, changed payload domains and
driver effects remain open. No complete reading, whole question closure,
validated replay or native playback result is claimed.

## Alternatives

- These children may invoke an unspecified callback: all transfers in
  the reviewed connected bodies are direct local arithmetic calls.
- Chance ten consumes another random draw: its first branch returns
  true without calling the reducer.
- Preserving DS/ES proves all table bytes survive: actual pointer,
  frame/state aliasing and external interference remain separate.

## How to reproduce

Read all three location ranges with resident_listing.py at the revision
above, retaining music-selector-rng-child-contracts.txt under
GAME_DIR/analysis/handler-census-20261106/. Check all chance branches,
zero/nonzero reducer paths, the near multiply return and every memory
store/segment write. Compose only with FND-SOUND-064/065's retained
input and frame conditions and FND-RNG-011's signed-division domain.
Do not turn synthetic range arithmetic into a native draw observation.
No original game, emulator session or emulated call supplied evidence.
