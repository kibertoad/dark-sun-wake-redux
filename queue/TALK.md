# TALK

Next ID: Q-TALK-002

## Static

- Q-TALK-001. RULE-TALK-001, SCR-UI-012: What do the two far routines of the menu instruction do: how
  `5702:0048` draws a row, why the title row is drawn in capitals, how rows past the five on
  screen are reached, and how `5702:0025` turns a click or a key into a row number? Settles it:
  reading the routines behind those entries of the resident header of overlay 188, and the
  `WIND/12501` scroll buttons they use. Tried: the menu handler `172C:2CF8`, which only passes
  rows to one and takes the row from the other (FND-TALK-001), and the capture `dsun_008.png`
  (FND-TALK-004). Blocks: slice 3.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

None.

## Blocked

None.
