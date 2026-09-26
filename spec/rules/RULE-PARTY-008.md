---
id: RULE-PARTY-008
title: The keys 1 to 4 choose the party leader
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-002]
---

## Summary

The party has a leader, who walks for the party and talks to other characters. Pressing 1, 2, 3
or 4 makes the character in that party slot the leader. The leader button beside each character
box of the View Character screen does the same.

## When it runs

`leader_for_key` runs when the player presses a key while the map is shown (SRC-MANUAL-1994,
page 77).

## Parameters

`key`, the character printed on the key the player pressed, as an ASCII code.

## Inputs

None beyond the parameters.

## Procedure

```text
define leader_for_key(key):
    if key >= 0x31 and key <= 0x34:
        leader = key - 0x31
```

## Outputs

No return value. Sets `leader` to the slot of the key, 0 to 3, when the key is 1 to 4, and
changes nothing for another key.

## Edge cases

The manual does not say what a key does when its slot is empty.

## What the sources say

SRC-MANUAL-1994, page 77, lists `1` to `4` as setting the corresponding character as leader.
Page 10 says the View Character screen has a leader button beside each character box, which
makes that character the one who leads when walking and talking to non-player characters, and
page 12 that the inventory screen has the same buttons. Keys 5 and 6 show all the characters or
the leader alone while moving (page 77).

## Differences between builds

None known.

## Open questions

- Where the game keeps `leader`, and what a key for an empty slot does. The loading routine of
  FND-PARTY-013 flags every party slot but the one equal to a word it compares with, which may be
  the leader's slot (Q-PARTY-009).
