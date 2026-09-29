---
id: FND-CONFIG-198
title: A fixed request-and-release loop counts qualifying handles rather than successful releases
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0E3B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:282D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:28C5
tool: Python 3.14.7 and Capstone 5.0.7 complete bounded resident 16-bit reading with operand widths and MZ relocation checks
environment: null
---

## Observation

FND-CONFIG-195's encoded release call at file 0x000337B7 belongs
to resident 3D72:0E3B. Its complete local body spans file
0x0003375B..0x000337D7 inclusive. It reserves 0200 hexadecimal
stack bytes, saves SI/DI and has the runtime stack-limit guard
through 1000:2E48 before its loops.

The first loop starts SI at zero and compares it signed-less-than
0100 hexadecimal after every increment. Each iteration calls
1BF3:282D with five words, in parameter order (0,0,0,1,1).
The pushes have widths four, four and two bytes: the first double
word supplies the two final ones, the next supplies two zeros,
and the final word supplies the first zero. The caller removes ten
argument bytes. AX is stored at numeric offset BP-0200+2*SI;
there is no result test or early exhaustion break in this loop.

That store uses BX with no segment override and therefore DS,
although BP and the reserved frame belong to SS. The second loop's
BX-based reads and writes also use DS. Valid local storage therefore
requires a suitable DS/SS relationship and alias conditions; reserving
512 stack bytes alone does not prove the addressed array is that frame.
The offset arithmetic is word-sized.

The second loop resets SI and DI to zero and again traverses 256
indices. A stored word signed-at-most one skips release. Otherwise
it increments DI before passing that word directly to 1BF3:28C5.
The returned AX is ignored. After return it writes FFFF to the same
indexed location and advances. It finally returns DI in AX and
restores saved DI/SI and the frame. Thus DI counts entries admitted
to the release call, not successful reclamations or rendered effects.
Under intact frame/index state, it ranges from zero through 256.

The runtime, request and release segment words at file
0x0003376D, 0x00033781 and 0x000337BA are declared MZ relocations.
FND-CONFIG-191's request preserves DS/SI/DI on normal return;
FND-CONFIG-194's release does likewise. Their normal preservation
supports the fixed loop bounds, conditional on valid memory, the
runtime guard returning and no conflicting alias state.

### Conditional initialized-state case

FND-CONFIG-193 installs roots zero and one with inclusive bounds
(0,0)..(319,199) and makes the other 254 slots free for the scan in
FND-CONFIG-183. If that initialized state is unchanged, DS/frame
storage is valid and nonaliasing, and no external state changes
intervene, each fixed request selects root zero with coordinates
(0,0)..(1,1). FND-CONFIG-191's comparisons admit those bounds.
It sets the selected slot flag to 0040, making it a reference slot.

The first 254 requests then consume the 254 eligible slots. The
last two return FFFF from slot-scan exhaustion. No release occurs
until all requests finish. The second loop admits the 254 handles
and skips the two FFFF words. Each reference-slot release marks its
slot free without lowering E4E or entering block compaction, as in
FND-CONFIG-194; it still touches VGA ports. The conditional final
AX is 254. This is a composition of local readings, not a native
observation or an emulated result, and does not establish that this
routine is called after the initializer under those assumptions.

### Bounded incoming inventories

A query over every declared MZ relocation found no far-call operand
with relative segment 2D72 and offset 0E3B. A separate query over
every declared FBOV overlay fixup found no far-call operand with
offset 0E3B whose segment token resolves to resident descriptor 44,
relative segment 2D72. Both select the far-call opcode three bytes
before the segment operand; they do not classify other uses of a
matching segment token or computed/unrelocated targets.

The FBOV segment table bounds this resident descriptor's interval
at file 0x00032920..0x00033DDF. Within that interval, a raw E8
operand query found no signed displacement targeting file
0x0003375B. That result is a bounded candidate inventory, not a
complete control-flow or incoming-reference analysis. Calls through
pointers, segment aliases, different encoded forms or registration
remain possible. None of these inventories establishes native
reachability or proves the body is unused.

## Interpretation

This caller supplies one concrete fixed request form and a signed
guard for its direct release route. Its final count cannot establish
successful release, ordinary pool reclamation or a native capacity
measurement. The initialized-state case distinguishes reference-slot
capacity from paragraph-buffer capacity without asserting startup
reachability or hardware outcomes.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain incoming callers, initial slot
state, DS/SS and frame aliases, guard outcomes and native VGA effects.
One reading enters after the unchanged initializer with valid shared
frame storage; another enters with occupied slots or different segment
state. Complete callers and state producers would separate those
code-decided conditions. Hardware output still needs owner evidence.

A reading that DI counts successful releases is ruled out by its
increment before the call and ignored result. A reading that exhaustion
stops the request loop is ruled out by its unconditional store and
index continuation. No native or emulated result is claimed.

## How to reproduce

Read 3D72:0E3B through far return 0EB7. Check the widened pushes,
argument cleanup, both signed 0100 loop tests, default segment of
each BX-based array access, pre-call DI increment and untested release
continuation. Verify all three declared far-call segment operands.
Compare FND-CONFIG-183's scan, FND-CONFIG-191's reference request,
FND-CONFIG-193's initial roots/free flags and FND-CONFIG-194's
reference release. Derive the initialized-state case only with its
segment, storage and unchanged-state assumptions explicitly retained.

For the incoming inventories, enumerate MZ relocation operands and
FBOV fixup operands, selecting only a far-call opcode three bytes
before the segment word and offset 0E3B. Resolve each FBOV token
through its descriptor before comparing resident segment 2D72.
Use descriptor 44 and the next greater declared segment to bound
the same-segment E8 candidate query. Keep these negative inventories
separate from pointer/registration coverage and native reachability.
