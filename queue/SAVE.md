# SAVE

Next ID: Q-SAVE-007

## Static

- Q-SAVE-004. RULE-SAVE-002, SCR-UI-013, SCR-UI-014: Is `save_list_top` (`4C4C:0002`) always 0
  when the Load Game or Save Game screen opens? Settles it: every writer of the word, with the
  paths from each to the entries of overlay 192. Tried: FND-SAVE-020 shows overlay 192 never writes
  it; FND-UI-038 finds the stored-character list's writes from `DS:044C`.
  Blocks: slice 5.
- Q-SAVE-005. RULE-SAVE-002: What does the save writer's word replacement through `GPLI/1`
  (overlay 187 `+1615` and `+1829`) change, and does a load leave the replaced form in memory?
  Settles it: a reading of `GPLI/1`'s entries, of the 19-byte records' and trigger records' words
  they match, and of every reader of those words after a load. Tried: FND-SAVE-016 finds the reader
  does not call `+1829`.
  Blocks: slice 5.
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

- Q-SAVE-006. RULE-SAVE-001: After the player answers Save to the `F3` exit choice and leaves the
  Save Game screen with EXIT, does the next save or load end the game? Settles it: a run that does
  this and watches `DS:627C` and `DS:1462`. Tried: FND-SAVE-018 finds only stores of 1 and one test
  of `DS:627C` among direct operands.
  Blocks: slice 5.

## Live session

None.

## Source

None.

## Blocked

None.
