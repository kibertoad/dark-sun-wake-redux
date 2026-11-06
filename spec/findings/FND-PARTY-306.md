---
id: FND-PARTY-306
title: The cast-window callback refreshes an active member before publishing its list count but skips an empty member
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00095E10..0x000960C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009785F..0x00097ACD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009620E..0x00096224
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00096E12..0x00096E1F
tool: overlay_listing.py at 34fdac1a; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; retained mode-caller211-first/second reports; FND-PARTY-279/280/303/304/305 and FND-UI-041
environment: null
---

## Observation

Q-PARTY-134 retains whether the list read by 211+10F4 actually belongs
to the selected member/mode and count. This checks the known refresh
callback's own active/empty split and publication ordering. Offsets are
hexadecimal. The licensed installed DSUN.EXE is size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, unchanged from FND-PARTY-305.

### Refresh registration is a window-presentation callback

211+02B5 pushes callback 57C1:0070 and window 55C9:0000 to
3A8E:0D0C at +040F. FND-EXE-634 identifies that trampoline as
211+0000; FND-UI-041 establishes that the setter conditionally writes
the supplied window's +F9 field. A null window bypasses that store.
The refresh routine calls +F9 after its child work, not before every
possible frame event. Its call, return and stored pointer remain
subject to FND-UI-041's explicit admission conditions.

The known main handler's shared +1002 path calls 3A8E:060D with
DS:1431 at +1007, then continues to control-state and other helpers.
This is an actual request for window refresh, not a direct call to
the list builder. Applying the callback registration requires that
DS:1431 still names the registered object and its +F9 still names
211+0000, and that refresh reaches the after-child callback.

### Empty members bypass the builder

After its four-member presentation loop, 211+0000 reloads current
4E71:0B44 at +01F5 and tests word +0E of that member's 66-byte
record through current DS:19C5. Zero at +0202 jumps to +0213,
bypassing both +020B's call to 211+1A4F and +0210's call to +1CBD.
It later repeats the member/record test for its name presentation.
Neither the empty branch nor the remaining own suffix writes the
list or count globals. The suffix still calls presentation and state
helpers; their effects are not assumed absent.

Thus invoking this callback is insufficient to prove a newly zeroed
count for an empty selected member. FND-PARTY-304's explicit clears
are in different caller branches; they cannot be inferred from this
callback's empty-member bypass. An actual stale positive count still
needs a prior publication and preserved state, plus admission of the
later handler input. No such native sequence is asserted here.

### Active-member build precedes publication

On the nonzero +0202 branch, +020B calls 211+1A4F. As recorded in
FND-PARTY-279, it clears DS:9C1E and reads current member/mode.
For modes one/two it calls the spell builder with DS:9C20, level and
member; for mode three it calls the discipline builder. Between the
own mode selection and outgoing argument construction there is no
child call. Actual member/mode/segment identity across external
effects and throughout each builder still requires preservation.

Each builder returns before +1AE9 assigns its returned count to
DS:9C1E and +1AEC/+1AEF copies that count to DS:9D79. Other modes
retain and publish the initialized zero. Consequently the own code
does not publish the new count and then start filling its prefix:
the complete builder call precedes publication. FND-PARTY-279/280
retain the builder's capacity, counter, source and backing conditions.

After publication, +1AF2 initializes a separate presentation index SI.
The suffix compares it signed with current DS:9C1E and also limits
its own iteration to 35. It reads list words, rereads current member
and mode, requests ICON resources, performs icon/predicate calls and
releases resources. Its own instructions contain no later list/count
store. Its mode rereads and reached children mean that publication
does not freeze the member/mode or list through the later handler.
The 35-iteration presentation limit does not truncate the published
count; FND-PARTY-305's intended callback range is a separate bound.

## Interpretation

The known callback's own refresh/publication order is answered. An
active-member builder completes before its returned count is exposed;
an empty selected member bypasses the builder rather than clearing
that count locally. The remaining freshness question is actual callback
admission, the active/empty member and record binding at that test,
prior-count retention on the empty path, and changes during subsequent
presentation/helpers or before the handler event. Alternate inputs
and source histories remain Q-PARTY-134; number/frame preservation
remains Q-PARTY-136.

This removes the unknown own ordering, not a whole client. It does
not establish all writer lifetimes, native reachability, a complete
reading or a queue closure. FMT-PARTY-007 remains supported.

## Alternatives

- Every invocation refreshes or clears the list count: the empty-member
  branch explicitly bypasses that work.
- The new count is published before the builder fills its output:
  contradicted by the call/return and subsequent stores.
- Every request for window refresh necessarily executes this callback:
  pointer identity, callback binding and refresh gates remain required.

## How to reproduce

Use overlay_listing.py for descriptor 211 file 95E10..960C5 and
9785F..97ACD, the complete inventoried bodies, retained as
list-refresh-caller211.txt and list-publication-complete211.txt in
GAME_DIR/analysis/handler-census-20261106/. The shorter
list-publication211.txt stops after count publication and alone
cannot support claims about the suffix. Reuse mode-caller211-first
and second for the callback setter and +1007 refresh request.
Keep FND-UI-041's setter and after-child callback contracts distinct
from the APFM event dispatcher in FND-PARTY-305. No original game,
emulator session or emulated call ran.
