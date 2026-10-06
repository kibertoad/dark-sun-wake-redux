# Research queue

The open research questions about the original, one file per spec area,
named after the area: `queue/COMBAT.md`. The
[work protocol](../vendor/upstream/work-protocol.md#the-queue) (lines 84-123) defines the
format; this file is a short reminder of it and stays in place when the area
files arrive.

An area file opens with the area as a `#` heading and a line giving the ID the
next new item takes, `Next ID: Q-COMBAT-013`, followed by these `##` sections
in this order, each holding list items or `None.`:

1. `Static`: a reading of the executable or data files settles it.
2. `Emulated call`: calling one function of the original in an emulator
   harness settles it. The harness in `tools/emu/` does not exist yet
   (`docs/RUNTIME.md`), so items here wait until a tooling batch builds it.
3. `Agent run`: a run of the original an agent makes alone. Coding agents never
   launch or control DOSBox here (`AGENTS.md`), so this section stays `None.`
4. `Live session`: a run the repository owner performs, requested in
   `docs/live-sessions/`.
5. `Source`: a document that has to be found or read.
6. `Blocked`: stopped until something else changes; the item says what.

One item:

```markdown
- Q-COMBAT-012. RULE-COMBAT-012, FMT-SAVE-001: Which of two actors attacking
  each other rolls first? Settles it: the order of the two calls at the
  resolver's entry, and one owner capture of a fight where both attack.
  Blocks: slice 4.
```

Every item has an ID, names the spec entries it concerns (behaviour with no
entry gets an `unknown` entry first), asks one question, says what would
settle it, and names the slice it blocks or `none`. It goes in the file of the
area of the first entry it names. The ID comes from the file's `Next ID:`
line, which then goes up by one; it is never reused, and it stays with the
item when the item moves. Everything outside the queue (handovers, live
session requests, `Spec gap:` notes) names items by ID. An item already worked
on adds `Tried:`, saying what was examined and why it did not settle the
question; anything the attempt learned about the original goes in `spec/`
first and `Tried:` names the finding. An item under `Blocked` adds
`Waiting on:`.

Static analysis comes first, and runs are the last resort for each question:
a `Live session` item is taken up only after its own static attempt is
recorded under `Tried:`, or when it asks for the run that confirms a static
reading.

Close an item by recording the answer in `spec/` and deleting the item in the
same commit, which names it in a `Queue:` trailer so that it can still be
found. An open reading of an entry, in its Open questions section, always has
an item, and is cited by the item's ID. Content no item can settle yet explicitly ends with `(No item: <why>)`; every exemption supplies a reason. A complete static reading makes its
entries `established` with no run. A reading that is not complete yet leaves
them `supported`, and the same commit adds a `Static` item for what it still
has to cover, and an `Emulated call` item where the harness can reach the
functions the reading covers. Every rule the code decides gets an `Emulated
call` item, since its fixture is what the row's tests replay. Only an entry
that depends on something the code does not decide (interrupts, uninitialised
memory, timing, the operating system) gets a `Live session` item for the owner
capture that would confirm it. An item with a `Tried:` note is taken up again
only with something the first attempt did not have: new evidence, a new tool,
or a reading nobody has tried. If that second attempt ends in the same place,
move the item, with what was tried, to the section of the evidence that would
change the outcome (`Emulated call` or `Live session` for a run, `Source` for
a document), and to `Blocked` only when that evidence is out of reach for now.

A file that would pass 1,000 lines becomes a directory of the same name, with
a `README.md` holding the heading and the `Next ID:` line, and one file per
section that has items, named after the section in lower case with a hyphen
for the space: `queue/COMBAT/static.md`, `queue/COMBAT/live-session.md`. Each
opens with `# COMBAT: Static`. A section file that would still pass is split
by the kind of the first entry each item names: `queue/COMBAT/static/RULE.md`.

Run `node tools/Check-ResearchTracking.mjs` to validate area queues, stable IDs and active Open questions references. The canonical gate runs it; structural success does not prove research complete.
