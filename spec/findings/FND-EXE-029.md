---
id: FND-EXE-029
title: A dispatch helper searches sentinel-linked nodes and returns an unvalidated stored word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0058B2D0..0x0058B32A
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction and function reporters
environment: null
---

## Observation

FND-EXE-028's nonzero/default dispatch paths call `0x0058B2D0` with a
selected data pointer, then pass its returned word to a second helper.
The first callee reserves 24 local stack bytes after saving one register
and reads its original 32-bit input at stack offset 32. It reads the first
node word through `0x0240DB50`, using that same address as the sentinel.
An initial sentinel takes the fallback return without a comparison call.

Otherwise it saves the node pointer locally and calls `0x006BCC40` with
node plus eight as first argument and the original input word as second.
It tests the entire 32-bit result. Zero reloads the saved node and returns
its word at offset twelve without a local null, size or lifetime check.
The direct body does not copy that resource or transform its bytes.

Nonzero reloads the saved node and reads offset zero as the next pointer.
It saves that pointer and compares it with the sentinel. Non-sentinel
continues with the next node plus eight; sentinel returns fixed pointer
`0x00738FBE`. The fallback contents are not read or retained here. Both
return paths restore the local stack reservation and saved register.

There is no local node-null check, traversal count, visited-node set or
independent termination guard. The saved node is reloaded after the helper
before either offset-zero or offset-twelve access. Helper mutations, node
construction, shared-head writers, links, comparison semantics and returned
data lifetime remain unread. Neither the sentinel comparison nor apparent
string names proves an acyclic collection or exact string equality.

## Interpretation

This narrows FND-EXE-028's first helper boundary to a node search using a
full-width zero result as its selection predicate. It returns a stored word
or fixed fallback pointer, conditional on readable nodes and normal helper
completion. Message identity, encoding, format-string contracts and final
presentation remain open under Q-EXE-009; the second helper and collection
producers require reading. This is not a complete caller or writer reading.

## Alternatives

A low-byte-only result test, returning the node itself, scanning until null,
locally validating the stored word or copying its contents are ruled out by
the bounded instructions. Exact equality, normalized matching and other
zero-result predicates remain competing readings of the unread comparison
helper. A valid circular collection might terminate all admitted searches;
the local body alone cannot establish that producer invariant.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x0058B2D0` and
read 45 instructions from its entry, stopping at `0x0058B32A` and excluding
following functions. Track original input across calls, saved node, argument
last writers, full-width result test, link reread and sentinel exits.
Enumerate initial sentinel, zero-result selection, nonzero advance and later
sentinel conditional on valid producer state and normal helper behavior.
Do not infer a complete writer set or comparator identity from the analyzer.
Keep rich reports local and execute no interpreter or game.
