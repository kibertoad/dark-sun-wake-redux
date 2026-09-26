---
id: RULE-ITEM-002
title: Splitting a bundle of grouped items
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-ITEM-001]
---

## Summary

A bundle of grouped items, such as arrows, can be split in half on the inventory screen. The
split needs an empty backpack slot for the second half.

## When it runs

When the player picks up a grouped item and clicks SPLIT on the inventory screen (SCR-UI-008,
SRC-MANUAL-1994, page 12).

## Parameters

`bundle_size`, the number of items in the bundle. `items_in_backpack`, as RULE-ITEM-001 gives it.

## Inputs

None beyond the parameters.

## Procedure

```text
define split_bundle(bundle_size, items_in_backpack):
    if not backpack_has_room(items_in_backpack):
        return 0
    return bundle_size / 2
```

## Outputs

The number of items in the new bundle, which goes in an empty backpack slot; the bundle keeps
`bundle_size` less that number. 0 means the split is refused.

## Edge cases

The manual does not say how an odd bundle is halved: the procedure gives the smaller half to the
new bundle, and that is a guess. Whether a bundle of one can be split is not known.

## What the sources say

SRC-MANUAL-1994, page 12: to split a bundle of grouped items (such as arrows) in half, select it
and click SPLIT; this works only with an empty slot in the backpack to hold the other half.

## Differences between builds

None known.

## Open questions

- How an odd bundle is halved, which half stays on the pointer, and what the game does with a
  bundle of one (Q-ITEM-002).
