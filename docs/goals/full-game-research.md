# full-game-research

## Condition

Every parity row's Spec status is `supported`, `established` or `validated`,
or its row's Notes names an item outside Static that holds the rest; no item
under Static in `queue/` names a slice in its `Blocks:` line; the
documentation check and `./tools/Test.ps1` pass on the last commit; and each
batch ended with a status block. Stop earlier only for a reason
`docs/goals/README.md` lists. The owner set this goal on 2026-10-18 ("execute
protocol until full game is researched and mapped") and narrowed it the same
day to what slices 2 to 7 need.

Progress (both print nothing when the condition's two parts hold):

```sh
awk -F'|' 'NF>8 && $2 ~ /`/ {s=$4; gsub(/ /,"",s); if (s!="supported" && s!="established" && s!="validated") print FILENAME, $2, s}' parity/*.md
awk '/^## /{s=$2} /^- Q-/{inq=(s=="Static"); id=$2} inq && /Blocks: *slices? *[0-9]/{print FILENAME, id}' queue/*.md
```

Take items in this order: items blocking the lowest-numbered open slice, then
items that raise a parity row below `supported`, then other slice-blocking
items. An item that cannot close statically moves to the section of the
evidence that would settle it rather than splitting again.

## Scope

Side: research. Areas: ACTOR, AI, COMBAT, CONFIG, EXE, EXPLORE, GFF, IMAGE,
INPUT, ITEM, MAGIC, PARTY, QUEST, REGION, RNG, SAVE, SCRIPT, SOUND, TALK, TEXT,
TIME, UI, VIDEO. Batches: research only. Queue sections: Static first, then Emulated
call.

## Must not touch

`src/` and `tests/`. Shared tooling owned by upstream-gap-resolution.

## Dead ends

- Q-CONFIG-008: do not repeat the bounded incoming inventories
  (FND-CONFIG-114), the literal slot/metadata writer query (FND-CONFIG-142),
  the literal selector-base query (FND-CONFIG-148), the literal setup-gate
  writer query (FND-CONFIG-149), or the MZ, same-segment and FBOV incoming
  queries (FND-CONFIG-196, FND-CONFIG-198) without new pointer or
  registration coverage.

## Handover

- Stage: Slices.
- Last gate: 2026-10-18, documentation check passed (1372 entries, 169
  parity rows, five deviations).
- Unfinished: none.
- Blockers: none known.
- Next: Q-UI-001, Q-PARTY-087, Q-PARTY-088/089, Q-PARTY-086,
  Q-COMBAT-014/015/016 (from docs/HANDOVER.md).
