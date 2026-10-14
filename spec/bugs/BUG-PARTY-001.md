---
id: BUG-PARTY-001
title: The character sheet's next-level experience figure comes from the wrong class's table, or from past the table, for most class codes
status: disputed
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: presentation
intent: unintended
player_reliance: unknown
evidence: [FND-PARTY-058, FND-PARTY-083, FND-PARTY-073]
conflicting: [FND-PARTY-187, FND-PARTY-190]
split_with: []
related: [SCR-UI-002, FMT-PARTY-001]
---

## Symptom

After `EXP: ` and the character's experience, the character sheet prints in brackets the
experience at which the character's next level would start. For most characters the figure is
that of another class's level table, or a number unrelated to any table.

## Trigger conditions

Any character whose class codes (FMT-PARTY-001 `classes`) are not all ones whose code equals the
position of their class in the 8-name table: only code 1 (Cleric) reads its own class's row. The
figure is printed whenever the sheet shows the experience line (overlay 186 `+0421`).

## Mechanism

For each class whose level is not 15, the sheet reads the `DATA` 1000 threshold row numbered by
the class code less 1, multiplies the word at the level by 100 and prints the least of the
results, using the first class only for a human (FND-PARTY-058). The class bytes hold the 17-code
numbering, in the shipped records and in every character generation stores (FND-PARTY-073), and
the routines that apply experience in play turn a code into a row through the class bytes of the
pairs at `4E4F:009C` first (FND-PARTY-083). The sheet does not, so code 2 (a Cleric code) reads
the Druid row, code 5 (a Druid code) the Preserver row, and codes 9 to 17 read 320 to 640 bytes
into a resource of 320 bytes, whatever memory follows it.

## Frequency

Every time the sheet is drawn for a character with such a code. Of the 19 shipped characters, only
record 53 (code 1 alone) gets its own class's figure.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether the sheet ever reads a record whose class bytes hold the 17 codes. The mechanism above
  assumes it does. Against it: in play, View Character prints each supplied character's own
  classes' thresholds (Cleric 7 shows 110000, Gladiator 7 shows 150000), because overlay 184
  `+1DCE` has turned the codes back into 1 to 8 in the record at `4E4F:0029` that the sheet reads
  (FND-PARTY-187, FND-PARTY-073); character generation also keeps codes 1 to 8 there
  (FND-PARTY-073). Of the sheet's three callers, overlay 212's prints a record `+1DCE` has just
  converted from the same slot (FND-PARTY-188, FND-PARTY-080). Two routes stay open. The
  generation screen prints the record at `DS:1429`, which six routines outside generation point at
  a stored details record holding the 17 codes. Generation's own handler reaches them only as it
  closes the screen, after its last sheet draw (FND-PARTY-189), and opened by `NEW` from Create
  Characters or from play it runs no other handler and prints a level 7 Fighter's next threshold
  as 125000, Fighter's own (FND-PARTY-190); settled by the edit path into generation and the
  calls the reach left unresolved (Q-PARTY-076). The DUAL path prints
  the record `+1DCE` converted with the codes of the slot in `4E71:0B44`, which may differ from
  the record copied; settled by the stores to that word before overlay 209 `+14FF`
  (Q-PARTY-077).
