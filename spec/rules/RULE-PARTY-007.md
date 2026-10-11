---
id: RULE-PARTY-007
title: Which classes each origin may take, and to which level
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-160, FND-PARTY-081, FND-PARTY-065, FND-PARTY-068, SRC-README-1.1, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-PARTY-009]
---

## Summary

Each origin may take only some classes; humans and psionicists may take every one. Every
character rises to level 15 in each class it holds. The game applies none of the README's
per-origin level limits and none of its prime requisite bonus.

## When it runs

`class_level_limit` when DONE checks a new character (RULE-PARTY-002), and `maximum_level`
whenever a character could gain a level. The generation screen decides which classes it offers
from its own table (RULE-PARTY-009).

## Parameters

`origin`, the character's `origin` code, and `character_class`, a `character_class` code
(RULE-PARTY-002).

## Inputs

None beyond the parameters.

## Procedure

```text
define class_level_limit(origin, character_class):
    # Eight limits per class, in the order of the origin codes. 0: the origin cannot take the
    # class. 255: no limit.
    let limit: UINT8[64] = [255, 12, 15, 16, 12, 12, 10, 12, 255, 0, 0, 14, 0, 12, 12, 16, 255, 16, 14, 255, 16, 12, 255, 16, 255, 255, 10, 255, 14, 12, 255, 15, 255, 0, 15, 12, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 16, 14, 8, 16, 0, 12, 255, 12, 12, 12, 0, 16, 12, 0]
    return limit[character_class * 8 + origin]

define maximum_level(origin, character_class):
    # The game raises a level only below 15 and tests neither the origin nor a score
    # (FND-PARTY-160).
    if class_level_limit(origin, character_class) == 0:
        return 0
    return 15
```

## Outputs

`class_level_limit` returns the limit from README table 3: 0 when the origin cannot take the
class, 255 for no limit, otherwise a level the game does not use. `maximum_level` returns 0 when
the origin cannot take the class and 15 otherwise. Neither changes state.

## Edge cases

The class change of overlay 209 `+17DC` gives a new class level 1 and moves the earlier classes
up one position, with no limit test (FND-PARTY-160); which classes it may give is Q-PARTY-007's
question.

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
and with the class descriptions on the mul druid (12) and the thri-kreen druid (16).

The generation screen offers a class with no other class exactly where table 3 gives a limit,
from a table of its own (RULE-PARTY-009, FND-PARTY-068): a half-giant may be a ranger but not a
thief, a mul a druid, and a thri-kreen a druid but not a thief. That settles which classes each
origin may take.

The game does not apply table 3's level limits or table 8's bonus. Every routine that sets a
level byte either sets a starting level (FND-PARTY-065), a new class's level 1, or a drain, or
adds one level while the level is below 15 (FND-PARTY-081); none reads a score, and the origin
byte only limits a human to its current class. Table 3's rows and columns appear neither in
`DSUN.EXE` nor in the archives (FND-PARTY-160). A non-human therefore rises to 15 in every class,
and the README's question of how to count a class with more than one prime requisite does not
arise.

## Differences between builds

None known.

## Open questions

None.
