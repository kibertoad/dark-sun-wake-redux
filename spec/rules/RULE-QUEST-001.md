---
id: RULE-QUEST-001
title: How the game's scripts advance the story from one quest step to the next
status: unknown
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: []
conflicting: []
split_with: []
related: [RULE-SCRIPT-004, RULE-SCRIPT-008, RULE-TALK-001]
---

## Summary

The party moves through the campaign from Tyr to the finale by completing quests, and what it has
done decides which conversations, regions and outcomes it reaches next. The game keeps that
progress in the script variables (RULE-SCRIPT-004) and runs scripts from triggers and
conversations (RULE-SCRIPT-008, RULE-TALK-001). Which variables stand for which step, which
triggers run which scripts, and how the steps connect are not known.

## When it runs

Not known. The scripts that change the story's state run from conversations and from the triggers
the scripts register.

## Parameters

None known.

## Inputs

`global_flags`, `local_flags` and the other script variables of RULE-SCRIPT-004.

## Procedure

Not known.

## Outputs

Not known.

## Edge cases

None known.

## What the sources say

SRC-GAMEFAQS-81038, sections 3.1 to 3.25: the route through the game from Tyr to the finale, the
branches of each quest, the outcomes the author saw for choices, and soft locks the author met.
The guide does not say which release it describes, and it describes outcomes, not the variables or
scripts that produce them.

## Differences between builds

None known.

## Open questions

- Which script variables record each quest step, which triggers and conversations change them, and
  what each step makes reachable. The trigger lists that decide when the game runs a region's
  scripts are not understood yet (Q-QUEST-001).
- Which of the route, branches and soft locks the guide reports hold in BLD-GOG-EN-1.1
  (Q-QUEST-002).
