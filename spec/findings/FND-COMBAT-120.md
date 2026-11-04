---
id: FND-COMBAT-120
title: Numbered resource reads advance a local fallback cursor and restart from active selection on each call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0615..38FF:07B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1..38FF:08D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:08D7..38FF:09D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:0157..39A9:01A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:01A2..39A9:01BC
tool: tools/research/exec-census/resident_listing.py at 0e7cb66a; existing lookup and transfer contracts FND-CONFIG-038/040/151
environment: null
---

## Observation

Q-COMBAT-064 needs the archive supplying each numbered restore resource.
FND-CONFIG-038/040 describe lookup and traversal, and FND-CONFIG-151
reads the complete underlying transfer. This reading connects their
archive cursor and continuation behavior to the separate requests of
FND-COMBAT-035. All offsets below are hexadecimal.

### Fallback changes a local cursor

After its guard and signature call, 38FF:07B1 copies current far
DS:9D9F to SS:BP-0C. It checks current error state and its output
address, then at 0803 reads DS:9D9F again into that local before
starting type lookup. The lookup passes the local archive pointer
with the requested type to 39A9:0045.

A missing type, missing plain-number result, or zero AL from the
indexed helper 0615 reaches 0892. That branch passes the local
archive to 39A9:0157 and stores returned DX:AX back into BP-0C.
Nonzero repeats type lookup; zero leaves the search. Neither the
reader's own body nor traversal's own body assigns that cursor to
global DS:9D9F. The reader uses the eventual local record for archive
preparation and for the file handle at record +4 used by seek/read.

This is not a select-handle call. The global writers remain the
separate mechanisms of FND-CONFIG-068 and FND-COMBAT-096/110.
Guards, indexed-helper effects, aliases and other callees still need
their own admission; absence of an own store is not universal
preservation of the global pointer.

### Only particular lookup failures advance

The indexed helper's zero AL includes failure of its own index-pair
preparation, seek or eight-byte read, as FND-PARTY-042 and
FND-CONFIG-065 already record. Such a metadata I/O failure can therefore
reach fallback. The indexed helper's own offset/length output stores
occur only after its exact eight-byte read succeeds, immediately before
AL one. Its normal AL-zero exits do not themselves write those outputs;
child aliases and frame preservation remain separate conditions.

After a plain entry is found, its +8 length and +4 offset are copied
to local words. The common selected-record checks require signed
length greater than zero and signed offset at least zero. A failure
sets missing-type/number error from the accumulated type-found byte
and returns FFFF. It does not go back to 0892 to try another archive.

After an admitted record, count FFFFFFFF selects its complete length.
Allocation, when needed, precedes preparation and transfer. A rejected
preparation at 0962, unequal seek position or unequal returned read
count takes the common failure return. These paths likewise have no
own next-record search. Thus the local fallback is not a retry of
another archive after every kind of failure. FND-CONFIG-151
retains partial-buffer and allocation/result qualifications.

### Traversal consults current globals

39A9:0157 returns zero for a null input or mode DS:9D9B zero.
Mode one returns the current record's +0C link. Its other mode branch
compares a nonnull successor against current global DS:9D9F; with
no successor it compares current head DS:9DA3 against current
DS:9D9F and can return the head. These are reads at traversal time,
not comparisons against a saved start pointer from the reader frame.
The admitted setter's modes zero, one and two are FND-CONFIG-040;
the own else branch is not itself a range check on other stored values.

With stable admitted mode two, active pointer, list head and valid
links, this implements the previously recorded wrap without revisiting
the active record. Changing that state across lookup children or
providing invalid/cyclic links is outside that finite traversal claim.
The reader does not independently freeze the list or traversal anchor.

## Interpretation

Each 04AB numbered request makes a fresh 07B1 invocation and starts
from the then-current active pointer. A fallback match does not itself
become the next request's global starting record through these own
bodies. Even with an unchanged active pointer, SAVE/24 and SAVE/26
can have different supplying records if their lookup results differ
among admitted runtime archives. This is a conditional path property,
not evidence that a native save has mixed provenance.

Q-COMBAT-064 need not reread whether fallback locally selects its
record, whether every read error retries, or which pointer anchors
mode-two termination. It still needs the actual archive graph, mode,
directory entries, lengths and callee/global preservation at each
request. FND-COMBAT-112 excludes only unchanged shipped providers;
runtime-written/copied or other admitted archives remain necessary.
Q-COMBAT-065 retains destination storage and Q-COMBAT-066 later
effects. No native failure, successful load or evidence-status change
is established.

## Alternatives

- A local lookup cursor is not the globally selected archive.
- Stable active selection does not require every resource to have one provider.
- A found but invalid entry is not the same branch as a missing entry.
- Final resource seek/read failure does not retry another archive;
  indexed-pair acquisition failure can instead return to lookup fallback.
- Mode-two termination needs a preserved anchor and admitted graph.

## How to reproduce

Use DSUN.EXE resolved through tools/game-dir.mjs and locked evidence
Python. resident_listing.py verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read the five listed ranges,
omitting the established alignment gaps. Follow BP-0C through its
two initial loads, fallback replacement and later preparation/handle
uses. Distinguish search-miss edges from metadata/preparation/transfer
failures and traversal's global rereads from the local cursor.
Reuse FND-CONFIG-038/040/151 and FND-COMBAT-035 rather than
assuming admitted live record contents from this body alone.
Retain the listing only in GAME_DIR/analysis/hit-incoming-20261104/
restore-resource-fallback.txt. No original listing or payload is committed.
