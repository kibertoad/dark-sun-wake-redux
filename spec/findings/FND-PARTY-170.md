---
id: FND-PARTY-170
title: Leaving the Create Characters screen by its return button or Esc goes back to the start window while DS:0DAB is 0, so play begins only from START GAME, which loads characters 40 to 43 when nothing has been placed
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078B42..0x00078B68
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007908D..0x000790C9
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078E2E..0x00078E97
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079C93..0x00079D0A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079259..0x0007930C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079396..0x00079479
tool: Capstone 5.0.9 (bounded listings); tools/research/exec-census/trampoline_target.py
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`, where overlay 190's code starts at `0x78340`. A far call's segment word in overlay code
carries an FBOV fixup and is eight times a segment table row; the rows used here are 22 (`28C9`),
44 (`3D72`), 172 (`566A`), 171 (`5664`), 182 (`56BD`) and 194 (`574B`).

**The buttons.** The Create Characters screen's handler, overlay 212 `+0967`, passes every button
other than `0x283C` and the four character boxes to overlay 190 `+0802` with the far pointer
`57C9:002A`, overlay 212 `+08E3` (FND-PARTY-045). `+0802` looks the button up among the 15 words at
`+0D4D` (`mov cx, 0xF` at `+080E`) and jumps through the 15 words after them (`+0824`); a button not
in the table goes to `+0D3B`:

| Button | Branch |
| --- | --- |
| `0x0FA3` | `+0828` |
| `0x283C`, `0x2C28`, `0x2C29`, `0x2C2A` | `+0B57` |
| `0x2844` (`BUTN/10308`) | `+0AEE` |
| `0x2C2C` | `+0A95` |
| `0x2C2D` to `0x2C30` | `+08EB` |
| `0x2C31` to `0x2C34` | `+09DA` |

**The return button.** `+0AEE` calls the far pointer it was given, sets the byte at `+0x14` of
each of the four `0x31`-byte slot records at `DS:19C9` whose word at `+0x06` is nonzero and whose
`+0x14` is 0, and calls `28C9:2522` (`+0B30`), which returns the word at `DS:0DAB`
(FND-PARTY-046). When that is 0 it calls overlay 194 `+037B` (trampoline `574B:0020`) with 0
(`+0B3B`), which builds the start window again (FND-PARTY-045). Otherwise it calls overlay 182
`+19F8` (trampoline `56BD:00BB`), `3D72:0D83` and `28C9:2A81` with 1. Both paths leave through
`+0D3B`. These are the instructions of Esc's branch `+1953..+19C7` (FND-PARTY-046) apart from that
branch's test that the pointer is nonzero and the key is `0x011B`.

**DROP.** For a box that holds a character, overlay 190 `+0D89` offers `EDIT` (or `EDIT NAME`),
`DROP` (left out while the word at `4C10:0019` is nonzero) and `CANCEL` (or `DUAL`) through overlay
172 `+018F` (`+0FAC`); choice 2 goes to `+1056`. When the box number at `4E71:0B44` equals the
leader's slot at `4C13:0369` (FND-COMBAT-023), `+1070..+10B4` looks for another slot whose
`0x42`-byte record at `DS:19C5` has a nonzero word at `+0x0E` and, for the first one found, calls
overlay 182 `+1D3E` (trampoline `56BD:0020`) with that slot and 0. If the box is still the
leader's, it shows `LAST ONE: CAN'T DROP` (`+10DD`) and drops nothing. Otherwise it asks
`DROP THIS CHARACTER?` with `YES` and `NO` (`+10FC`), and on `YES` calls overlay 171 `+0D15`
(trampoline `5664:0025`) with 0. A box that is not the leader's is dropped after the question
whether or not another box holds a character.

The word at `DS:0DAB` is `00 00` in the load image (file `0x0004DDAB`).

## Interpretation

Neither exit of the Create Characters screen begins play. With `DS:0DAB` 0, both bring back the
start window, and the game begins only from its START GAME (or a load). START GAME runs the loader
of characters 40 to 43 only while the placed-object count at `DS:264E` is 0 (FND-PARTY-021), and
only `ADD` is known to raise the count on that screen (FND-PARTY-046). So a player who leaves the
screen without adding a character and chooses START GAME begins with the supplied party, not with
an empty one; the game shows no message about the empty party.

The file stores to the count only increments and stores of 0 (FND-PARTY-022), so `DROP` does not
lower it, and `DROP` refuses only the leader's box when no other box holds a character.

## Alternatives

- `DS:0DAB` is nonzero during party creation, so the exits take the other path: neither run of
  FND-PARTY-046 reaches its setters `2B1B:000A`, overlay 182 `+15D7` or `+1167`, but those runs
  leave unresolved transfers, so this is not ruled out.
- A party that has added a character and then dropped every box that is not the leader's, while
  the leader's slot is an empty box, reaches START GAME with the count nonzero and no character in
  a box: whether the leader's slot can name an empty box then, and what overlay 171 `+0D15` does,
  are not read here (Q-PARTY-006).
- `NEW` raises the count too (FND-PARTY-046's third alternative, Q-PARTY-017).

## How to reproduce

From the commit that adds this finding, with Capstone 5.0.9 on the installed `DSUN.EXE`: list
overlay 190 `+0802..+0828`, `+0AEE..+0B57`, `+1953..+19CA`, `+0F19..+0FCC` and `+1056..+1139` from file offset
`0x78340` plus the overlay offset, and read the 15 words at `0x7908D` and the 15 after them.
`trampoline_target.py <dsun> 574B:0020 56BD:00BB 56BD:0020 5664:0025` names the overlay targets.
The strings are at `DS:1DEA`, `1DF4`, `1DF9`, `1DFE`, `1E03`, `1E1D`, `1E32` and `1E36` (file
`0x4D000` plus the offset).
