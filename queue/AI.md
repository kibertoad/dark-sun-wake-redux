# AI

Next ID: Q-AI-003

## Static

- Q-AI-001. RULE-AI-002: Which routine chooses a computer-controlled combatant's target,
  movement, action and the end of its turn, and do hostile creatures and party members under
  computer control share it? Settles it: a reading of the readers of `computer_control`
  (FND-AI-003), starting with `28C9:0605`, which skips the rest of its routine for a combatant
  under computer control, and of the combat entry that Q-COMBAT-001 locates. Tried: the static
  paths from the first hostile's object, `MONR`, `ETAB`, `RDFF`, coordinate input and the random
  number generator (FND-AI-001), and the computer-control strings and button (FND-AI-002,
  FND-AI-003). Blocks: slice 4.
- Q-AI-002. RULE-AI-001, FMT-COMBAT-001: Where does the game set `control_locked`, and which
  screens pass key events to the resident key routine at `28C9:0CFF`, so that Space turns
  computer control off? Settles it: a search for writes of the whole byte `0x18` of the
  combatant records, and for the far pointers or calls that reach `28C9:0CFF`. Tried: a byte
  search for instructions that set bit 6 (FND-AI-003) and for direct far calls to the key
  routine (FND-AI-004). Blocks: none.

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
