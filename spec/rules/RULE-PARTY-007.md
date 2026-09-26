---
id: RULE-PARTY-007
title: Which classes each origin may take, and to which level
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-README-1.1, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

Each origin may take only some classes, and a non-human can rise only to a set level in most of
them. Humans may take every class with no limit, and every origin may be a psionicist with no
limit. No character rises above level 15. A non-human with a high prime requisite may go one to
four levels past the limit.

## When it runs

`class_level_limit` when the generation screen decides whether an origin may take a class
(RULE-PARTY-002), and `maximum_level` whenever a character could gain a level.

## Parameters

`origin`, the character's `origin` code, and `character_class`, a `character_class` code
(RULE-PARTY-002). `prime`, the character's score in the class's prime requisite.

## Inputs

None beyond the parameters.

## Procedure

```text
define class_level_limit(origin, character_class):
    # Eight limits per class, in the order of the origin codes. 0: the origin cannot take the
    # class. 255: no limit.
    let limit: UINT8[64] = [255, 12, 15, 16, 12, 12, 10, 12, 255, 0, 0, 14, 0, 12, 12, 16, 255, 16, 14, 255, 16, 12, 255, 16, 255, 255, 10, 255, 14, 12, 255, 15, 255, 0, 15, 12, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 16, 14, 8, 16, 0, 12, 255, 12, 12, 12, 0, 16, 12, 0]
    return limit[character_class * 8 + origin]

define maximum_level(origin, character_class, prime):
    let base = class_level_limit(origin, character_class)
    if base == 0:
        return 0
    let bonus = 0
    if prime >= 19:
        bonus = 4
    else if prime >= 18:
        bonus = 3
    else if prime >= 16:
        bonus = 2
    else if prime >= 14:
        bonus = 1
    return min(15, base + bonus)
```

## Outputs

`class_level_limit` returns the limit from the table: 0 when the origin cannot take the class,
255 for no limit. `maximum_level` returns the highest level the character can reach in the
class, 0 to 15. Neither changes state.

## Edge cases

The table stops at a prime requisite of 19, which gives 4 extra levels; the procedure gives 4 to
every higher score. A human's limits are all 255, so the extra levels never matter to a human,
and the level 15 cap applies to every character.

## What the sources say

SRC-README-1.1, table 3, gives the limits of the procedure's list, and says a human is never
limited and that a dash means the origin cannot belong to the class; its note gives level 15 as
the game's maximum for every character. Its section on exceeding level limits, table 8, adds 1
level for a prime requisite of 14 or 15, 2 for 16 or 17, 3 for 18 and 4 for 19, for demihumans.
Its sentence on characters with several prime requisites is cut off.

SRC-MANUAL-1994 gives the classes each origin may take twice, and the two lists disagree. The
origin descriptions (pages 17 and 18) allow a half-giant ranger and forbid a half-giant thief, a
mul druid, a thri-kreen druid and a thri-kreen thief. The class descriptions (pages 19 to 22)
forbid a half-giant ranger and allow the other four. README table 3 sides with the origin
descriptions on the half-giant ranger (limit 8), the half-giant thief and the thri-kreen thief,
and with the class descriptions on the mul druid (12) and the thri-kreen druid (16). No list
has been read from the executable.

## Differences between builds

None known.

## Open questions

- Which classes the generation screen offers for each origin, where the manual's two lists and
  the README disagree. The party START GAME supplies holds a thri-kreen fighter and druid
  (FND-PARTY-020), which fits the README and the class descriptions for that pair, but that
  character was not made on the generation screen (Q-PARTY-002).
- Whether the game applies these limits at all, and how it counts the prime requisite of a class
  with more than one (Q-PARTY-008).
