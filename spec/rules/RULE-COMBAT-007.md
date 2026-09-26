---
id: RULE-COMBAT-007
title: What the difficulty setting changes in combat
status: unknown
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: []
conflicting: []
split_with: []
related: [RULE-CONFIG-002]
---

## Summary

The difficulty setting (RULE-CONFIG-002) makes combat easier or harder. How it does so is not
known.

## When it runs

Not known. A player's report places it when a hostile creature appears.

## Parameters

None known.

## Inputs

`difficulty`.

## Procedure

Not known.

## Outputs

Not known.

## Edge cases

None known.

## What the sources say

SRC-MANUAL-1994, page 15: difficulty controls the level of difficulty in combat. SRC-GAMEFAQS-81038,
section 2.8: a new game begins on Balanced; Easy seems to give hostile creatures about half the hit
points they have on Balanced and Hideous about twice as many, and the change applies when a
creature appears, not to creatures already present. It says nothing of Hard, of rounding, or of the
party.

## Differences between builds

None known.

## Open questions

- Whether the difficulty scales the hit points of hostile creatures when they appear, by how much
  on each setting, with what rounding, and whether it changes anything else. The FAQ's report is
  the only lead, and no code that reads `difficulty` is known. Settling it needs the same hostile's
  hit points on each setting from equivalent saves (Q-COMBAT-007).
