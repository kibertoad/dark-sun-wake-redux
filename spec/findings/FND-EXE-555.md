---
id: FND-EXE-555
title: Game general cleanup selects reverse priorities and begins with relocated no-op callbacks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0163..1000:0176
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0163..1000:0176
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0264..1000:02A5
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0264..1000:02A5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0387..1000:03EE
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0387..1000:03EE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050572..0x00050574
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00050456..0x00050458
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050676..0x00050682
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0005055A..0x00050566
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000509BE..0x000509C4
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x000508A2..0x000508A8
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-551's cleanup helper 0388 reads its third incoming word
into SI. Zero enters a callback loop; nonzero skips it. The loop
reloads count DS:3572 installed or DS:34E6 on disc each time,
exits on zero, otherwise decrements that memory word before reading it into
BX. It shifts BX left two at word width and calls the far
pointer at DS offset BX+A446 installed or BX+A390 on disc,
with word-width offset wrap. It then retests the current count. There
is no local table-extent or count upper-bound check, and callees can change
the reloaded count. The shipped count word is zero in both editions.
That value is not a census of runtime registrations or other writers.

After the zero-SI loop, it manufactures a far return into 0163
and then calls a stored far pointer at DS:3676 installed or
DS:35EA on disc. 0163 loads ES from CS:02C4,
saves SI/DI, sets SI 39BE and DI 39C4 installed or
SI 3932 and DI 3938 on disc, and calls near 0264.
It restores DI/SI and returns far. Actual ES/table and callee preservation
remain conditions.

0264 initializes priority AH zero, candidate DX to DI and scan
BX to SI. It advances BX by six until BX equals DI. ES
selector byte FF skips an entry. Otherwise byte one unsigned below AH
skips it; at least AH replaces both AH and candidate DX. Thus the
largest priority is selected, and later equal-priority entries replace earlier
ones. Priority zero can be selected. This differs from the strict smaller-
priority startup dispatcher in FND-EXE-510.

No candidate returns near. A candidate loads BX from DX, compares
selector byte zero and marks that byte FF before dispatch. The following
ES push preserves the comparison flags. Zero selector calls near through
ES word two; every other admitted selector calls far through ES words
two/four. It restores ES after either call and rescans from the beginning,
without testing the returned result or locally saving SI/DI around dispatch.
There is no local iteration bound beyond equality with DI; mutable bounds,
marks and targets prevent the shipped table alone proving termination.

The shipped six-byte cleanup table contains selector one, priority one,
offset 0193 installed or 0192 on disc, and relative segment
3AE5 or 3AD6. Segment-word MZ relocation index 4446 at
47E0:39C2 installed, or index 4433 at 47D7:3936 on
disc, gives modeled target 4AE5:0193 or 4AD6:0192 with
load segment 1000. That callee's body remains unread. The table lies
just before FND-EXE-509's zero-fill start; the fill does not erase
the shipped entry.

0388 next follows FND-EXE-551's direct vector/checksum helpers.
Its second incoming word nonzero skips the later termination path and
returns near with six-byte incoming cleanup. Zero continues; held SI
zero calls stored far pointers at DS:367A/367E installed or
DS:35EE/35F2 on disc before passing the first incoming word
to 019E. Nonzero SI skips those two calls. 03DF supplies
zero, zero and its incoming word to 0388, selecting these zero-SI
paths under intact frame/callee binding. General return and native nonreturn
limits remain as recorded by FND-EXE-551.

All three shipped stored callback pairs are offset 0387, relative segment
0000. Their segment words have MZ relocations:

| Callback | Installed index and operand | Disc index and operand |
| --- | --- | --- |
| First | 4456, 47E0:3678 | 4443, 47D7:35EC |
| Second | 4455, 47E0:367C | 4442, 47D7:35F0 |
| Third | 4454, 47E0:3680 | 4441, 47D7:35F4 |

With modeled load segment 1000 they point to 1000:0387,
whose sole instruction is a far return without cleanup. These are shipped
no-op identities, not a runtime callback census. FND-EXE-512 records
a later writer of both words of the first pair, so retaining its shipped
identity after that writer would be incorrect. Other writers, actual DS,
aliases and far-pointer lifetimes remain open.

## Interpretation

This resolves general cleanup's local callback-count dispatch, reverse-priority
selection and shipped far-pointer identities. Q-EXE-007 retains the cleanup
table callee 4AE5:0193/4AD6:0192, callback registration and all
pointer/count writers, actual bounds/segments, native contracts, other callers,
aliases and remaining startup dependencies. No complete cleanup or launch
exclusion is claimed.

## Alternatives

Using startup's first-tie rule contradicts cleanup's at-least replacement.
Treating relative segment zero as a null target ignores relocation.
Treating shipped no-ops as permanent ignores the recorded first-pair writer.
Bounding callback iterations by the shipped zero count ignores reloaded mutable
state and registration producers. Treating marks as successful dispatch ignores
their pre-call stores.

## How to reproduce

At revision 6b461cf1 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode resident
offsets in Locations in sixteen-bit mode. Read the shipped words/entries at
the file-data locations, with data bases 4D000/4CF70. Read the
MZ relocation table from header word 0018, count at 0006, and
four-byte offset/segment records; check the four indices and segment operands
above. Track count decrement/reload, wrapped index formation, both priority
comparisons, pre-call marks and paired targets, and 03DF's outgoing words.
Use FND-EXE-510/551 for contrasted dispatch/cleanup and FND-EXE-512
for the first pointer's later writer. Licensed bytes remain outside Git;
no original process, DOSBox or emulated call runs.
