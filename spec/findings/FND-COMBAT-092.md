---
id: FND-COMBAT-092
title: The APFM mode setter looks up the first matching child-table pointer without verifying its payload tag or hit eligibility
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:049C..39D1:05E1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0755..3F96:07B3
tool: tools/research/exec-census/resident_listing.py at d63747b7; Capstone 5.0.9
environment: null
---

## Observation

FND-COMBAT-087 identifies resident 3F96:0755 as one producer of
DS:A15B. It copies its first far argument into a local window pointer,
then calls 39D1:049C with that pointer, the APFM tag double word and
its identifier word at BP+0A. The caller removes ten argument bytes.
It retains returned DX:AX, tests both words for null and returns FFFF
on null before the preliminary service, A15B assignment or selector calls.

39D1:049C has its own stack-limit guard. Its first far argument must
be nonzero. The APFM tag takes its non-WIND branch: this branch does
not search the registered-window list at DS:A105 or test its count.
It instead scans the supplied window's child table in ascending index
order. Child i uses the original window segment and a 16-bit offset
equal to window offset plus zero-extended byte +F2 plus 30*i plus 105.
Its count test uses the unsigned word at window+F3.

For each entry, the lookup first compares the entire tag double word
at entry+4 with its tag argument. On equality it zero-extends the
identifier word into a double word and compares that with entry+8.
The first match immediately returns entry+0 as DX:AX. It does not
continue after a matching entry whose pointer is zero. Thus a stored
number with nonzero high word cannot match this word-identifier call,
and a later duplicate does not override the first match.

This APFM branch does not dereference the returned payload, verify its
tag, test the child-entry flags, test window visibility or call the hit
search. It has no own store through the window or child pointer. The
MENU-only nested lookup is bypassed for APFM. A null window returns
zero directly; exhausting the APFM entries sets diagnostic word DS:9DB0
to 10, formats a message through 1000:374E and reaches the zero-return
tail if that callee returns. Its effects and the stack-limit guard are
not assumed harmless.

The mode setter retains this first matching pointer across its
3D72:0B5F call, then writes it to A15B and its mode argument to A141.
FND-COMBAT-087 records how the following 3D72:0635 can replace that
pointer from current A125 or leave it in place. FND-UI-068 independently
corrects the hit-search continuation and output writes. Those two lookup
mechanisms do not impose the same eligibility tests or freeze the same
record pointer.

## Interpretation

The setter's successful local lookup binds its saved pointer to the
first tag-and-number match in the caller-supplied child table. It does
not establish membership in the current registered graph, a valid APFM
payload, callback zeros or subsequent storage preservation. Those claims
need the table and pointer producers, not merely a nonnull lookup result.

Q-COMBAT-051 can reuse these local selection rules and the hit-search
output mapping in FND-UI-068. Its remaining work is the graph/table
producers, payload identity, lifetime, aliases and later writers; actual
resource acquisition and service-mode admission remain Q-COMBAT-050
and Q-COMBAT-052. No nonzero callback target or whole-load exclusion
is established here.

## Alternatives

FND-UI-008 reads the separate WIND lookup branch, while FND-UI-045
identifies several child lookups by tag and number. Neither makes the
APFM branch's caller-supplied window into an independently registered
window. Valid disjoint storage, nonwrapping address formation and admitted
guard/callee returns remain conditions. A matching directory entry can
name a modified, stale or aliased record; this reading does not prove
that such a state is reached by the original.

## How to reproduce

Run resident_listing.py on the licensed installed DSUN.EXE with
39D1:049C..39D1:05E1 and 3F96:0755..3F96:07B3. The tool verifies
the build length and XXH3-128 and applies MZ relocations at segment
1000. Follow the APFM tag from the caller into the non-WIND branch,
both full-width comparisons, the first-match return and the exhaustion
tail. Keep the word identifier separate from the stored double-word
number. Check that the MENU call is unreachable for this tag and that
the caller's null test precedes all later mode/service effects. Reuse
FND-COMBAT-087 for the selector; do not substitute the hit search's
region/flag gates for the lookup's actual tests.
