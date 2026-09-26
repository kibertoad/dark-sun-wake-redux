---
id: RULE-EXPLORE-001
title: Holding the pointer at an edge of the map view scrolls the view toward that edge until the pointer leaves it or the map ends
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-REGION-001]
---

## Summary

While exploring, the player scrolls the map by moving the pointer to an edge of the view. The view
keeps moving in that direction while the pointer stays at the edge, and stops when the pointer
leaves the edge or the view reaches the edge of the map.

## When it runs

While the map is shown and no menu or screen is open, for as long as the pointer is at an edge of
the view [SRC-MANUAL-1994].

## Parameters

`px` and `py`, the pointer's position in the view in pixels. `view_width` and `view_height`, the
size of the view in pixels. `edge`, the width in pixels of the band along each edge that scrolls,
which is not known.

## Inputs

None beyond the parameters.

## Procedure

```text
let dx = 0
let dy = 0
if px < edge:
    dx = -1
else if px >= view_width - edge:
    dx = 1
if py < edge:
    dy = -1
else if py >= view_height - edge:
    dy = 1
if dx != 0 or dy != 0:
    emit ViewScrolled(dx, dy)
```

## Outputs

`ViewScrolled` with the direction, -1, 0 or 1 on each axis. The view moves that way by a step that
is not known, and not past the edge of the map.

## Edge cases

At a corner of the view both axes scroll together. Once the view is at the map's edge on an axis,
the pointer at that edge moves nothing on that axis.

## What the sources say

SRC-MANUAL-1994, page 4: to scroll the screen, move the mouse cursor in the direction the screen
should move; the screen scrolls in that direction until the cursor moves away from the screen's
edge or the edge of the map is reached. Page 14 describes the Game Menu's Center on Leader, which
centres the screen on the party's leader,
and page 77 gives the key `H` for the same.

## Differences between builds

None known.

## Open questions

- How wide the scrolling band is, how far each step moves the view and how often a step is taken,
  and whether the band is the edge of the view or of the whole screen (Q-EXPLORE-001).
- Where Center on Leader and the key `H` put the leader in the view (Q-EXPLORE-001).
