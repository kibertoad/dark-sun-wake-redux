# SAVE

Next ID: Q-SAVE-008

## Static

- Q-SAVE-007. RULE-SAVE-001: Can the Quit shutdown route write a saved
  game or working archive through a reached callback or callee?
  Settles it: census the file-write mechanisms and selecting operands
  reachable from the recorded exit route, with a positive control from
  an explicit Save path, then resolve remaining computed callback/target
  and archive-write branches. Preserve unsearched streams and external
  effects; a shutdown call list alone does not settle a negative claim.
  Tried: FND-SAVE-027 retains the local exit sequence, corrects the
  bulk interrupt-table copy ABI and withdraws FND-SAVE-019's unconditional
  no-save conclusion. No reached save operation is established either.
  Blocks: completing RULE-SAVE-001's exit-side reading.

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
