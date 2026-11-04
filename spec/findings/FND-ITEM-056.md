---
id: FND-ITEM-056
title: Canonical item hook inputs exclude negative DATA selectors and the repeated acquisition branch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00000000..0x00680304
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077C1A..0x00077D26
tool: Existing rdff-look-chain-census controls/report of FND-ITEM-043; tools/research/exec-census/overlay_listing.py at 45032581; composition with FND-PARTY-254
environment: null
---

## Observation

FND-PARTY-254 already reads both complete equipment-hook bodies, 189 +326A
and +332B. This composes them with the complete canonical attached-item
census of FND-ITEM-043 rather than repeating those local branch findings.
It removes a source-domain alternative in Q-ITEM-003 and Q-PARTY-109,
without treating imported values as immutable runtime state.

### Complete source partition

The retained item-input-domains.json report has SHA-256
ffc738c98dd640dfad4f11b04f784e71728b1cfd99efedc8ac4262bf67301bcb.
Its source is OBJEX.GFF length 6816516, SHA-256
8e1e2db24df80dd605bb1d55df709cc550cc8b379dc4d2c2c23becaa5975b9a6.
The independently controlled census includes all 1931 complete kind-one
payloads before each resource's first complete end header, including later
payloads rather than only the first item of each resource.

Partition the unsigned attached word +10 hex, W, into the hook's branches:

| Source W | Payloads |
|---|---:|
| 0 | 1671 |
| 1..319 | 240 |
| 320..327 | 20 |
| Outside 0..327 | 0 |

The twenty high cases are W=320 four times, 321 once, 322 four times,
323 once, 324 once, 326 three times and 327 six times. W=325 is absent.
The groups sum to 1931. The same complete census has signed bonus byte
+16 hex only in 0..4, with no negative value. These are separate marginals;
no unmeasured correlation or pairing of specific W and bonus rows is needed
for the universal nonnegative-bonus bound.

### Before hook +332B

With captured W preserved, W zero returns without a child call. The 20
source payloads in 320..327 take the direct ability-rebuild request
197 +1E7B(owner). The other 240 nonzero source inputs reach the unsigned
FFFF comparison and return without the encoded later 197 +38C7 call.
FND-PARTY-254 proves that latter branch locally unreachable for every word,
not only the canonical set. Nothing here reopens it as a possible source
hook merely because the call bytes exist.

This is a count of source input classes, not twenty observed rebuilds or
240 observed calls. Record selection, invocation counts and rebuilt ability
state remain native/caller questions.

### After hook +326A

W zero again returns without a child call. Every nonzero source value
first requests status 193 +1C95(owner,4); nonzero AX exits. For zero AX:

- W in 320..327 has decremented selector 319..326. The signed threshold
  selects ability rebuild 197 +1E7B followed by reset 190 +365A, in order.
  It does not reach this helper's low-selector DATA or effect-dispatch sites.
- W in 1..319 has decremented selector 0..318. It makes the first DATA
  request through 2C5F:0003 and reads byte zero's low two bits. Exactly one
  enters the reset/effect-dispatch path. Any other property returns when
  the subsequently read bonus remains nonnegative.

Thus, for retained canonical W and bonus through the relevant reads, the
second DATA acquisition is not reachable. No canonical W enters the signed-
negative selector alternative identified by FND-PARTY-254 for large runtime
words. A source bound does not make the first DATA pointer valid or guarantee
its property byte, status result, reset success or dispatched effect.

On the surviving low-selector dispatch path, FND-PARTY-254's order remains:
reset item, assign DS:6174 one, request the effect with owner in both object
arguments, then assign DS:6174 zero on return. This does not preserve an
incoming nonzero flag value, and the source census supplies no new consumer
or preservation guarantee for that flag.

### Scope conditions

W is captured before the status query; its register/frame value must survive
that call. The bonus is reread after the first DATA request, so a canonical
initial bonus alone does not exclude a negative later read if that child or
an alias changes the record. Require retained record/owner/index identity,
source values through each deciding read, returning callees, admitted DATA
storage and disjoint valid frames/tables for this source-preserving class.
Other runtime writers, initialization paths and independent entries remain
outside it. Neither hook writes W directly (FND-PARTY-254).

## Interpretation

This removes canonical high signed-selector overflow and repeated-DATA
acquisition from the remaining hook alternatives. The source-backed branch
classes are finite and explicit; the existing helper finding remains the
complete local branch description. Q-PARTY-109 retains actual caller/value
and record preservation, status/DATA admission, rebuild/reset/dispatch effects
and DS:6174 producers/consumers. Q-ITEM-003 retains their placement/ownership
and storage admission. No original execution, native effect outcome, rule
confidence, parity or complete-reading promotion is claimed.

## Alternatives

Applying this bound after a runtime mutation would omit the deciding read's
lifetime. Treating every nonzero W as a spell ignores the separate special
rebuild branch and the status/property gates. Treating source frequencies as
invocation frequencies or a joint W/bonus distribution would claim evidence
the census does not contain.

## How to reproduce

Regenerate rdff-look-chain-census exactly as FND-ITEM-043 describes, retaining
its source hash, kind-count controls and first-end-header scope. In its
census.itemInputs, select field word10, require the value-count sum 1931,
and sum counts for W=0, 1<=W<=319, 320<=W<=327 and the complement. Select
field bonus16, independently require sum 1931 and sum counts below zero.
Retain high word values separately. This is an exhaustive partition of the
existing controlled histograms, not a second proprietary fixture.

The verified input and metadata-only placement-hook-source-summary.json stay
under GAME_DIR/analysis/item-removal-20261104/. The retained
placement-equipment-hooks.txt repeats descriptor 189 range 77C1A..77D26,
read through overlay_listing.py from the fingerprinted installed DSUN.EXE
(length 634416, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b). Reuse
FND-PARTY-254 for all local branch widths, return gates and child arguments.
No original code, record fixture or resource payload is committed.
