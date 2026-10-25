# AI

Next ID: Q-AI-006

## Static

- Q-AI-001. RULE-AI-002: Which routine chooses a computer-controlled combatant's target,
  movement, action and the end of its turn, and do hostile creatures and party members under
  computer control share it? Settles it: a reading of the readers of `computer_control`
  (FND-AI-003), starting with `28C9:0605`, which skips the rest of its routine for a combatant
  under computer control, and of the combat entry that Q-COMBAT-001 locates. Tried: the static
  paths from the first hostile's object, `MONR`, `ETAB`, `RDFF`, coordinate input and the random
  number generator (FND-AI-001), and the computer-control strings and button (FND-AI-002,
  FND-AI-003). Blocks: slice 4.
- Q-AI-003. RULE-AI-002: When does a non-player character flee, fight back or call for help, and is that decided by combat or scripts? Settles it: the behavior-producing combat and script consumers for those outcomes, read with their callers. Blocks: completing RULE-AI-002.
- Q-AI-004. RULE-AI-003: What do a combatant's kind byte and `g_50C2_0000` hold, and what does
  overlay 198 `+0149` pick for the kind? Settles it: the writers of combatant record byte `+0x0E`
  and of `50C2:0000`, and a reading of overlay 198 `+0149` for mode 0. Tried: FND-RNG-035 reads
  the spell-use test and its caller, and FND-RNG-038 the routine that returns `g_50C2_0000`.
  Blocks: slices 2-7.
## Emulated call

None.

## Agent run

- Q-AI-005. RULE-AI-001, FMT-COMBAT-001: Does the end of an active effect on a party member clear a
  computer-control setting the player turned on with the button, as overlay 195 `+1000`'s tail
  reads? Settles it: a run that turns computer control on for a member with an active effect other
  than the five locking ones, lets the effect end, and reads byte `+0x18` of the member's combatant
  record before and after. Tried: FND-AI-005 reads the tail statically. Blocks: none.

## Live session

None.

## Source

None.

## Blocked

None.
