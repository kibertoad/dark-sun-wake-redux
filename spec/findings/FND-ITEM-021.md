---
id: FND-ITEM-021
title: Forwarding hit callers distinguish a mask-one follow-up from forwarded and overwritten returns
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F4F0..0x0007F539
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F58E..0x0007F598
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082C38..0x00082C45
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D0CE..0x0008D0E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:252A..28C9:2535
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at f8d8757e; locked evidence Python and Capstone
environment: null
---

## Observation

FND-ITEM-018 identifies overlay 179 +0C7F as a wrapper saving the hit
byte across queue processing. Reading its downstream continuations answers
part of Q-ITEM-009 without treating every outer return as an effect Boolean.
All return statements below assume an ordinary returning child and an intact
caller frame; Q-ITEM-011 retains admission and preservation obligations.

At overlay 193 +1200, the caller removes the arguments and tests AL mask
01. When set, it requests overlay 179 +2A05 with arguments SI, -1, -1,
1000. Both branches join +121D, where AL is overwritten from the caller's
own byte argument before another wrapper call is prepared. This gate does
not test mask 02. FND-PARTY-102 identifies DATA 100 for this call; this is
not evidence that any particular item-removal case takes this path or that
the requested follow-up completes.

Two later sites forward the returned low byte without interpreting it:
overlay 193 +123D cleans the arguments, restores DI and SI, and returns;
overlay 193 +129E cleans the arguments and returns. Neither normalizes AH.
Their containing functions begin at +1119 and +1249 respectively. The
consumers of those outer returns remain to be established.

Overlay 195 +14C8 instead cleans the arguments and sets AL to one before
returning, consistent with FND-PARTY-168. That one is independent of the
hit byte and does not prove successful execution of an item effect.

Overlay 204 +130E cleans the arguments, passes its saved mode word to
28C9:252A, and returns without another AX write. The complete eleven-byte
setter loads that argument into AX, writes the mode word at DS:0DAB, and
returns without children. Consequently this continuation returns the saved
mode's low byte rather than the hit byte. FND-CONFIG-117 supplies the
mode-saving context; this observation adds the setter's register effect.

## Interpretation

The mask-one request and these four ending continuations have different
contracts. A generic success Boolean, or an assumption that all wrappers
forward the hit byte, would erase observable differences in local control
flow. These readings remove their immediate post-call handling from the
unread portion of Q-ITEM-009. They do not establish complete callers,
native input admission, presentation or a complete reading of an item rule.

## Alternatives

- Mask 02 alone does not select the +1200 follow-up.
- The two forwarding epilogues do not themselves classify the result.
- The unconditional one and the restored mode are not the hit flags.
- Call-site coverage alone cannot prove result consumption on unread paths.
- DATA 100's gate does not establish any of the studied item DATA cases.

## How to reproduce

Resolve the installed DSUN.EXE through tools/game-dir.mjs. The bounded
readers verify its supported fingerprint: size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. With the locked evidence Python, run:

```text
python -I tools/research/exec-census/overlay_listing.py <dsun> 193 7F4F0 7F539
python -I tools/research/exec-census/overlay_listing.py <dsun> 193 7F58E 7F598
python -I tools/research/exec-census/overlay_listing.py <dsun> 195 82C38 82C45
python -I tools/research/exec-census/overlay_listing.py <dsun> 204 8D0CE 8D0E2
python -I tools/research/exec-census/resident_listing.py <dsun> 28C9:252A..28C9:2535
```

Retained reports in GAME_DIR/analysis/hit-incoming-20261104/ are
wrapper-damage-gate.txt (the +1200 gate through preparation at +123C),
wrapper-window-7f52d.txt, wrapper-window-7f58e.txt,
wrapper-window-82c38.txt, wrapper-window-8d0ce.txt and
mode-setter-return.txt. The latter's SHA-256 is
562047224906ea43b8c725424a989166603f667d78110a233a25e1275162ed69;
the gate report's is
e4f5eb101a223eb97ae1faa2e4692f750a010fd64ae6177c96adbaa24d8c66e7.
The first reproduction query combines the gate and first forwarding ending.
Original reports remain local; no listing or source bytes are committed.
