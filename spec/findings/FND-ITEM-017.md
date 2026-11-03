---
id: FND-ITEM-017
title: The hit wrapper tests effect mask 02 against damage mask 01 before presentation requests and returns the flags byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000653F5..0x000654BC
tool: tools/research/exec-census/overlay_listing.py at c72222e8; Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9)
environment: null
---

## Observation

This answers Q-ITEM-009's local hit-wrapper consumer question. Reuse
FND-ITEM-010 for the item-case mask 02 store, FND-RNG-044 for the current
effect-switch reading, and FND-MAGIC-021 for the already-read switch tail
and cure-specific wrapper continuation. This reading checks the exact
pointer producer, numeric masks, complete returning continuation and request
gates rather than reading the whole switch again.

The source is installed DSUN.EXE, length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The reader verifies these, derives overlay
179's code base 642E0 and resolves far-call fixups. No original code ran.

### Pointer and local consumer

At overlay 179 +1120 the hit wrapper forms the near offset of SS:[BP-7]
and pushes it for the +1373 call at +1134. The separately passed block
pointer has an explicit SS word; the flags pointer does not. FND-ITEM-010
records the switch's store through that near flags pointer. Identity with
the caller's SS local therefore needs the actual segment relationship,
not merely matching offsets. The conclusions below describe the caller's
own byte and the item-store connection under admitted near-pointer identity.

After the switch returns and its arguments are removed, +113A tests
mask 02 of SS:[BP-7]. With that mask set, +1145 tests mask 01. Mask 02
set with mask 01 clear goes directly to +1168. All other combinations
look up the current DI object's metadata record number at 4F49:0C34 plus
three times DI, then read word zero of its 49-byte record through DS:19C9.
Zero also selects +1168; nonzero goes to the return at +11D5.

For stable, valid record/frame storage, the complete local gate is:

| Flags mask 02 | Flags mask 01 | Combatant word zero | Select +1168 |
|---|---|---|---|
| Set | Clear | Not read on this route | Yes |
| Clear | Either | Zero | Yes |
| Set | Set | Zero | Yes |
| Clear | Either | Nonzero | No |
| Set | Set | Nonzero | No |

FND-MAGIC-018 independently records the wrapper setting mask 01 on its
damage route. This is a distinction between two masks in this result byte,
not a universal interpretation of every combat flags field.

### Request continuation and return

At +1168 the body requests DATA for its current SI number through
2C5F:0003, takes byte +16 of the returned record and passes it to
2C5F:0A22. This happens before the following mode gates. It does not test
the success of either request locally.

When DS:0DAB equals one, it requests overlay 206 +1073 only if local word
SS:[BP-0E] is not -1 and bit 80 of the selected 37-byte slot's byte zero
is clear. Arguments are that local word, the slot record's words +0A and
+0C, and one for signed DI below four or zero otherwise. A rejected gate
returns without that request. The local word's complete producer/semantic
contract is not established by this continuation.

When DS:0DAB is not one, a value of five returns without an overlay 206
request; any other value requests +1121 with DI. The branch with mode one
never falls through to +1121.

Every local route joins +11D5, which reloads SS:[BP-7] into AL, restores
DI/SI and the frame and returns at +11DB. There is no own flags-byte store
in this continuation. This is an eight-bit returned value; AH is not
cleared there. The body makes the named requests, not a proof of successful
text, image or other visible presentation, nor whole-callee preservation.

## Interpretation

An item test's mask 02 is locally consumed as a presentation-path gate,
in combination with damage mask 01 and the combatant-word fallback. It is
also part of the reloaded return byte. Consequently DATA 235's test returning
one does not establish actual item removal, and its resulting mask does
not by itself prove visible feedback or the ultimate caller's result.

This removes the unread local consumer/gates from Q-ITEM-009. Near-pointer
identity and frame/record admission, child effects/preservation, and complete
returned-byte caller/consumer coverage remain. FND-MAGIC-018 already records
one caller ORing the returned byte into an aggregate; reuse that evidence
before tracing its later use. No complete_reading or status promotion follows.

## Alternatives

- Mask 02 alone is not a universal item-removal success result: the lowest
  item test returns one even without selecting an item (FND-RNG-035).
- A simultaneous mask 01 prevents the direct mask-02 presentation route,
  but the zero combatant word can still select the same continuation.
- The mode gates do not suppress the preceding DATA-byte request.
- Numeric mask 02 here is not mask 04 or an unrelated effect-state bit.
- Matching near offsets alone do not establish that the switch changed
  the caller's SS-local byte on every admitted path.

## How to reproduce

With the locked evidence Python and verified installed <dsun>, run:

```text
python -I tools/research/exec-census/overlay_listing.py <dsun> 179 653F5 6541A
python -I tools/research/exec-census/overlay_listing.py <dsun> 179 6541A 654BC
```

The ranges lie in the inventory-declared hit-wrapper body ending at 654BC.
Retain UTF-8 output only in GAME_DIR/analysis/item-removal-20261104/:
hit-result-pointer-control.txt SHA-256
8853dbf5f47101bcb63657af2a3f6e739568dd0c518d72b371ba90c4a5983e25;
hit-result-continuation.txt SHA-256
83f8654a3bf9ca42b081399e0a6d5cb1e20f436398d2a0b5a2f3162e19219cfa.
No original listing, bytes or extracted records are committed.
