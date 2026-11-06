---
id: FND-PARTY-331
title: The two direct spell-reader callers supply a byte loop slot or a reloaded selected-slot word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068E8D..0x0006900C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058CB5..0x00058D72
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058336..0x00058347
tool: tools/research/exec-census/direct_callers.py and overlay_listing.py at 8cbb4659; locked Python 3.12.10 with Capstone distribution 5.0.9; existing FND-PARTY-006/013/046/048/049/330
environment: null
---

## Observation

Q-PARTY-137 retains FND-PARTY-330's actual SPST slot and selected-length
admission. A hash-checked direct-call pattern query for overlay 186
`+0282` resolves trampoline `56E9:0025` and returns two encoded far-call
sites: overlay 171 `+0B5A`, file 0x58D6A, and overlay 182 `+06CF`, file
0x68F1F. Both are qualified as calls on their known caller streams, not
accepted solely as byte-pattern hits. The latter independently matches
the argument-based caller in FND-PARTY-013; its formerly unresolved
second-call target is now explicitly resolved through the overlay fixup.
No near-call candidate was returned. This is a direct-call search;
indirect calls, jumps, bypass entries and changed mappings remain excluded.

### Supplied-party caller

The complete owned body at overlay 182 `+063D..+07BC` initializes DI to
40 and its SS-local byte at BP-1 to zero. In each iteration it first
attempts the CHAR load through 2D40:000A. The CHAR arguments are the
zero-extended local slot byte, the word sum of DI and that byte,
tag CHAR and flag seven. Full AX equal to FFFF skips the subsequent
spell-reader call.

Otherwise the caller reloads the local byte, clears AH, adds its value
to DI for the resource-number word, then reloads the byte and clears AH
again for the slot word. It calls the resolved spell reader immediately
after pushing these two words. There is no callee between these final
argument reads and the spell-reader call. The spell-reader result is
ignored: ordinary continuation proceeds to the placement/setup path.

At the loop tail it increments the local byte and branches back while
that byte is unsigned below four. With retained initialized counter state
and DI across the intervening returning calls, the supplied slots are
0..3 and resource numbers 40..43. This is FND-PARTY-013's normal loop,
now with the exact second target checked. Irrespective of whether the
counter retains that history, the own call-site conversion supplies a
slot in 0..255 rather than an arbitrary signed-negative word. A changed
counter byte could still supply the positive overlap witnesses from
FND-PARTY-330; the zero extension alone does not exclude them.

### Stored-character selection caller

The known prefix of overlay 171 `+0AA5`, from file 0x58CB5 through
0x58D72, computes a list entry as a word-size sum of the two words at
4C4C:0000 and :0002, multiplied by 51, plus the base at DS:40C8.
It reads that entry's word +2 as the CHAR resource number and the word
at 4E71:0B44 as the destination slot, then calls 2D40:000A. Full AX
FFFF skips the spell request.

On the other branch it independently recomputes the list pointer,
reloads entry word +2 and reloads the whole word at 4E71:0B44, then
calls the spell reader. No mask, byte conversion or range test bounds
that slot locally. The successful CHAR return is not a test that the
later slot or list entry remains equal to the earlier one. There is no
child between the final slot push and the spell-reader call, but the
preceding CHAR call separates the two observations.

FND-PARTY-046 identifies the ADD-window producer: overlay 171 +0126
stores its incoming word argument directly at 4E71:0B44. Its prefix
+0126..+0137 confirms that store and has no slot bound. The known UI
source is the box-number path in FND-PARTY-046; the field is also used
by generation and other character paths. Neither this store nor the
selected-load prefix establishes all writers, entry arguments, backing
or preservation through intervening calls. Those are actual admission
conditions, not an unknown callee target.

## Interpretation

The direct SPST caller component is now explicit. For the normal retained
supplied-party loop and the shipped numbered resources, FND-PARTY-006's
short counts and FND-PARTY-330's nonoverlap bound apply. That loop cannot
supply a negative slot through its own call-site conversion. To supply
its positive out-of-range slots requires changed counter history or an
alternate entry/frame; resource-number and selected-length changes remain
separate conditions.

The selected-load caller needs a stronger state binding: the current
4E71:0B44 value and recomputed list entry at the second call, including
changes during CHAR loading. It cannot be treated as automatically
bounded by the intended four boxes or by the earlier CHAR arguments.
Remaining indirect/bypass callers, input storage, selected archive/length
and queue/frame histories stay Q-PARTY-137. No actual overlapping SPST
request, complete-reading declaration or queue closure is claimed.

## Alternatives

- FND-PARTY-013's second call is merely inferred to be the spell reader:
  its actual fixed-up target resolves to the reader at +0282.
- Both direct callers pass arbitrary word slots: the supplied-party site
  zero-extends a byte; only the selected-load site reloads the full word.
- A successful CHAR result preserves the selected slot or list pointer:
  no such equivalence check appears in the second caller's prefix.
- Two encoded calls prove all incoming paths: the pattern reporter omits
  the named indirect, jump and bypass forms and establishes no absence
  proof for them.

## How to reproduce

Use licensed DSUN.EXE size 634416, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. With the locked evidence Python run
`tools/research/exec-census/direct_callers.py <dsun> 186+0282`.
Require trampoline 56E9:0025 and the two named far sites. Use
`overlay_listing.py <dsun> 182 68E8D 6900C`,
`overlay_listing.py <dsun> 171 58CB5 58D72` and
`overlay_listing.py <dsun> 171 58336 58347`. These existing readers check
the source fingerprint and resolve overlay fixups. Follow the initialized
counter, exact argument widths, both FFFF branches, independent selected
field reloads, producer store and loop tail. The committed inventory owns
the first body as 383 bytes and the second parent as 624 bytes beginning
at 0x58CB5; this finding reads only the named prefix of the second body.

Reuse FND-PARTY-006 for shipped lengths, FND-PARTY-046/048/049 for the
known load and UI interfaces, and FND-PARTY-330 for queue overlap arithmetic.
Do not infer state preservation from a successful CHAR call or turn the
direct-call candidate count into an all-entry census. No game was run or
original-derived listing committed.
