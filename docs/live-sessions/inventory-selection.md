# inventory-selection

Status: requested
<!-- or: accepted, YYYY-MM-DD / declined: the owner's reason / held, YYYY-MM-DD -->

- Build: BLD-GOG-EN-1.1, the owner's GOG installation under its own DOSBox launcher.
- Settles: Q-ITEM-002 (queue/ITEM.md, Live session).
- Blocks: slice 5.
- Length: about 10 minutes.

Establishes the native inventory screen's item-label geometry and the first
result of selecting one observed label. It does not ask for equipping, using,
transferring, dropping, rearranging, saving, or inspecting an unknown control.
Start a fresh normal launch, use the shipped party, and keep the default window
size. The two labels named below are leads from an earlier capture; their
record identity, slot, quantity, owner, and behaviour are not established.

## Script

1. I0, Q-ITEM-002. From a stable exploration or party screen, press `I` once.
   Captures: the first settled inventory page. Record the exact input, the
   active member and screen as you would label them, and the rectangle of
   every clearly visible item label. Do not infer a slot or item from
   placement.
2. I1, Q-ITEM-002. If the longsword label is visible, click once at the centre
   of it. Captures: the first settled result. Record the pointer coordinate,
   every visible change, and whether a new panel or control appears. Do not
   click a newly revealed or unknown control; if a panel opens, stop after its
   capture.
3. I2, Q-ITEM-002. Only if I1 leaves the same inventory page stable with no new
   panel, restore the I0 state with the visible Back or Return action, then
   click once at the centre of the dagger label. Captures: the first settled
   result, with the pointer coordinate and every visible change. Do not use,
   equip, transfer, drop, or rearrange either item.

An unchanged result is valid evidence; do not repeat the click until it
appears to work.

Agent, after confirmation: measure the label rectangles and the changed
regions, and record them as dynamic findings with each capture's `xxh3`.
