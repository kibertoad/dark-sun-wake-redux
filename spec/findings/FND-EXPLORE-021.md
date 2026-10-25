---
id: FND-EXPLORE-021
title: In the route search a wall follower that lands on its own node is closed, one that lands on an earlier node walks on remembering it, one that lands on a later node links the nodes and moves the search there, and a node with both followers closed sends the search back to its parent
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1B2E:01D5..1B2E:05C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BDF:0022..1BDF:006B
tool: Capstone 5.0.9 over resident code; tools/research/exec-census/resident_listing.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000. FND-EXPLORE-014 reads `1B2E:02D2`'s structure.
Its nodes are 18 bytes: `+0`, `+2` the cell, `+4` the heading, `+6` the turn of the follower that
linked it (0 for a straight step), `+8` the straight link, `+0x0A` and `+0x0C` the links of the
two followers, `+0x0E` the parent (0-based) and `+0x10` the next link. Links hold a 1-based node
number, -1 for closed and 0 for open with nothing found. Each follower `k` keeps a cell, a heading,
its turn (`DS:0446`, `DS:0448`), a remembered earlier node `e_k` (0 at the start) and the heading
it had when `e_k` was set. `1BDF:0022` returns the 1-based number of the node at a cell, or -1;
`1B2E:01D5`, one follower step, turns that -1 into 0 and returns -1 itself after four blocked
turns.

When a follower step returns `r`, stored as the current node's link for follower `k`, and `r` is
not 0, with `h = r - 1`:

1. `h` is the current node: the follower is closed, its link -1 (`1B2E:04F8`).
2. `h` is below the current node (`1B2E:0441..1B2E:0470`): when node `h`'s link for the same
   follower is -1 this follower is closed; otherwise `e_k` becomes `h`, the heading is saved and
   the follower walks on in the next round.
3. `h` is above the current node (`1B2E:0473..1B2E:04F6`). When `e_k` is set, node `e_k` gets `r`
   as its link for this follower, the saved heading in `+4`, -1 in its straight link and 0 in its
   other follower's link unless that is -1, and becomes the current node. In both cases node `h`
   gets the current node as parent, the current node gets this follower's turn in `+6` and `r` in
   `+0x10`, `h` becomes the current node, and the round is marked found. The other follower is
   still stepped in the same round.
4. After both followers have stepped (`1B2E:0515..1B2E:0579`), a node whose two follower links
   are -1 gets -1 in `+0x10`. When its parent is -1 or 0, the search fails (`1B2E:0579`). When the
   parent's `+6` is 0 it returns -1 (`1B2E:0545`). Otherwise every link of the parent naming this
   node becomes -1, the parent becomes the current node and the rounds end, so the outer loop
   (`1B2E:058D`) works the parent again, restarting its open follower from the parent's cell and
   heading (`1B2E:0354..1B2E:0398`).
5. The rounds go on while nothing is found and the search has not failed; round 500 marks it
   failed (`1B2E:03A7..1B2E:03B2`). The outer loop stops at the target cell, at a node whose x is
   negative (unused nodes, which `1BDF:0052` fills with -1), or on failure, and returns 0 or -1.

## Interpretation

Each obstacle is followed round both ways, and the first follower to come out on a later node of
the straight line joins the route there. A follower that circles back to where it started is
dropped. One that comes back to an earlier node first and then reaches a later one makes the
route leave from that earlier node instead, which cuts the loop out. A node where both followers
fail is given up, and the search tries again from the node before it.

## Alternatives

A follower boxed in on all four turns makes `r` -1 and `h` -2, and the code then reads a link of
the node before the array (`1B2E:0446`); what that reads was not followed. `1B2E:0761..1B2E:0798`
was listed for the node set-up only.

## How to reproduce

`resident_listing.py <dsun> 1B2E:01D5..1B2E:05C1` and `... 1BDF:0022..1BDF:006B`.
