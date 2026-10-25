# SAVE

Next ID: Q-SAVE-007

## Static

- Q-SAVE-002. FMT-SAVE-001: Where does a character's `CACT` identifier come from, and what does
  its top bit mean? Settles it: the code that gives a new character its identifier. Tried:
  comparing every 16-bit window of both families with the `CHAR` resource numbers, which finds
  none (FND-SAVE-002). Blocks: slice 5.
  Tried: FND-SAVE-013 takes a census of the operands naming the `GREQ` globals: the four words are
  the subregion edges, the byte the overhead map's page and the tenth byte the defeat picture.
  The `GREQ` parts are removed from Settles it; the scripts' values move to Q-SAVE-003.

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
