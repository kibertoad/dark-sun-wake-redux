---
id: FND-ITEM-022
title: Five wrapper continuations and the swing branches overwrite the returned hit register value
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D1C9..0x0005D207
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D2F5..0x0005D37D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FF85..0x0007FFA6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F460..0x0007F4F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F74E..0x0007F77E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000819B2..0x000819BD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081BDF..0x00081C12
tool: tools/research/exec-census/overlay_listing.py at 3bfd0a99; locked evidence Python and Capstone
environment: null
---

## Observation

These are further local consumers of overlay 179 +0C7F's forwarded hit
result, following FND-ITEM-021. FND-PARTY-102 supplies the known call
sites. This reading concerns the returned register value; it does not
exclude changes to memory, aliases or other effects of the hit routine.

Five continuations overwrite AX before interpreting the returned value:

| Wrapper call | First subsequent AX assignment |
|---|---|
| Overlay 193 +1170 | DI is set to 40, then copied to AX at +117B |
| Overlay 193 +11A0 | DI is set to 80, then copied to AX at +11AB |
| Overlay 193 +11D0 | DI is set to 20, then copied to AX at +11DB |
| Overlay 193 +145E | Metadata segment setup loads AX at +146B |
| Overlay 195 +0242 | An unconditional jump reaches +046F, whose metadata segment setup loads AX at +0474 |

No branch, store of AX/AL or child call precedes those first overwrites
after argument cleanup. The metadata segment values are resolved through
the overlay fixups; they are not hit-result masks.

The swing call at overlay 173 +28A9 first compares DI with its local
word and its own argument with one. These comparisons do not read AX.
Its optional +28C2 call requests overlay 193 +1C95 with the target word
and effect code 26. That query, also described by FND-MAGIC-015, reads
the code argument as a byte, calls resident 3143:0061, and explicitly
assigns AX one or zero after comparing the child's result with DS:2100.
The swing's following AX test therefore tests that separate query's result
on an ordinary return. The query's argument push retains an incidental AH,
but its code-byte read does not consume that high byte.

If the optional query is skipped, +28D7 tests SI. Nonnegative SI reaches
+28E2 and overwrites AX from SI. Negative SI branches to +29D5, whose
first instruction overwrites AX for metadata segment setup. Thus both
routes overwrite the hit register value before a later register test.
The +29D5 ending eventually returns its local word at +2A56; the local's
producers and the intervening loop are not established by this ending.

## Interpretation

These six caller sites no longer require a search for an immediate use of
the forwarded hit register value. The optional swing branch's Boolean is
an independent effect query, not a test of hit flags. This does not imply
that the hit had no gameplay effect or that the containing routines have
been completely read. Frame and field identity remain Q-ITEM-011.

## Alternatives

- Equal numerical masks in a local field and AL do not identify one field.
- A discarded return value does not imply discarded memory effects.
- The swing query's zero-or-one result is not the preceding hit byte.
- Reading the swing ending does not establish its local's whole lifetime.

## How to reproduce

Use the installed DSUN.EXE resolved by tools/game-dir.mjs and the locked
evidence Python. The reader verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py with these
descriptor/start/end arguments, all bounds hexadecimal and half-open:

```text
173 5D1C9 5D207
173 5D2F5 5D37D
193 7FF85 7FFA6
193 7F460 7F490
193 7F490 7F4C0
193 7F4C0 7F4F0
193 7F74E 7F77E
195 819B2 819BD
195 81BDF 81C12
```

Reports remain in GAME_DIR/analysis/hit-incoming-20261104/:
swing-first-result-overwrites.txt, swing-negative-slot-ending.txt,
swing-effect-query-return.txt, wrapper-window-7f460.txt,
wrapper-window-7f490.txt, wrapper-window-7f4c0.txt,
wrapper-window-7f74e.txt, wrapper-window-819b2.txt and
wrapper-shared-continuation.txt. The query-return report includes the next
function's initial instruction, and the +0242 window includes following
linear bytes; the claims use only the bounded ranges listed above.
No original listing or bytes are committed.
