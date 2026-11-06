---
id: FND-SAVE-027
title: The recorded shutdown route restores callbacks and interrupt tables but does not exclude writes behind unresolved callees
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388..1000:03EE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0163..1000:01F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0140..4AE5:01F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:0605..44DE:0641
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067321..0x00067354
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067942..0x00067A28
tool: retained FND-SAVE-019 procedure and observations; resident_listing.py, overlay_listing.py and direct_callers.py at 31f5bc42; FND-CONFIG-224/233 and FND-PARTY-322 interfaces
environment: null
---

## Observation

This supersedes FND-SAVE-019 to withdraw its unconditional absence-of-save
claim and correct the argument interpretation of44DE:0623. Its listed
exit sequence remains useful bounded evidence, retained below. Earlier
callee-head readings and callback observations are not recast as a fresh
exhaustive walk. New instruction checks use installed DSUN.EXE,634416
bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.

### Retained exit sequence

At modeled load segment1000, overlay180 code begins at file671E0.
FND-SAVE-012 identifies main-loop return into1000:03DF, which calls
0388(status,0,0). The recorded local sequence with both flags zero is:

1. While DS:3572 is nonzero, decrement it and invoke the far pointer
   at DS:A446+4*DS:3572, the callback table filled by030B.
2. Call0163 to run the exit-procedure records from39BE to39C4.
   The recorded far record is4AE5:0193, which restores its saved vector,
   closes its DOS handle and calls two near pointers of its own.
3. Call the far pointer at DS:3676.
4. Call01F3, restoring the saved vectors for interrupts0,4,5 and6.
5. Call0176, which checks the first47 bytes at DS:0000 and writes
   its runtime null-pointer message to handle2 on a checksum mismatch.
6. Call the far pointers at DS:367A and367E.
7. Call019E to request DOS termination with AH4C and the status in AL.

The shipped3676/367A/367E pointers name1000:0387, a return.
136E:0004 publishes3676 when streams are used. The earlier direct-call
report identifies030B calls at resident fileBAD0, registering167B:0029,
and overlay180+014F, registering180+0762; +015D supplies180+0848
to0361. Registration capacity, later table writers and actual runtime
targets remain conditions. FND-PARTY-322 retains the known registration
and exact sound-mode selector; FND-CONFIG-224 retains the ordinary
nonreturning exit route without assuming a returned DOS interrupt rejoins
the original caller.

The recorded180+0762 prefix calls4A6E:0187 when13F7 is nonzero,
200+05C5,25AF:0036,188+03BA,1000:0481,1000:0873,
1BF3:4FEB(0),188+0491 and167B:0029. It conditionally closes13FE
and13FC through2D40:3BD1, and writesFFFF to13FE. Nonzero
4DE7:0001 selects graphics cleanup39D1:0326. Its return is followed
by clearing that byte. FND-CONFIG-233 supplies the FONT release and
saved-record restoration, rather than assuming cleanup clears pointers.

If4E71:0001 differs fromFFFF, the continuation requests1BF3:2973(3).
Exact13F7==1 calls180+0722; other byte values call4842:071A(0).
It then calls38FF:02B5 with double-wordFFFFFFFF, invokes44DE:0623
as detailed below, clears DS:1462 and returns far. Earlier helper effects
and indirect/native transfers are not excluded by this local sequence.

### One far pointer and a bulk low-memory copy

Startup+0165 pushes a segment fixup naming descriptor103, segment4DE7,
then word0002 and calls44DE:0605 at+016B. Shutdown+0833/+0836
push the same segment and offset into44DE:0623 at+0839. These are
one far pointer4DE7:0002, not separate handle/count arguments or a
pointer whose offset is the raw descriptor word0338.

0605 saves BP,DS,SI,ES,DI and CX, loads its supplied far destination
into ES:DI, sets DS:SI to0000:0000 and CX to512 words. It pushes
flags, disables maskable interrupts, performs the word copy, restores
flags and saved registers, and returns far without argument cleanup.

0623 reverses the memory roles: ES:DI starts at0000:0000 and DS:SI
is its supplied pointer, again with512 words. It uses the same saved
flags, CLI, repeated word copy and restoration order. With DF clear,
the declared calls save and restore1024 bytes between0000:0000..0400
and4DE7:0002..0402, half-open. That low-memory range is the real-mode
interrupt-vector table. The copying body makes no software interrupt or
child call; it does not render text or write a file.

Neither body clears DF. CLI does not establish a forward copy. With DF
set the offsets decrement by two at word width and wrap; the forward
intervals above do not apply. Both restore their incoming flags on an
ordinary intact-stack return. Actual source contents, DF, backing and
intervening vector changes are not admitted by the copy instructions alone.
This bulk vector restore occurs after FONT cleanup in the known caller,
so a lifetime analysis must retain its callback/vector effects and subsequent
exit callbacks rather than treating the continuation as an inert return.

## Interpretation

The recorded route requests shutdown and DOS termination, and includes a
concrete interrupt-table restore with the corrected pointer ABI. Its local
sequence does not show an explicit save operation, but that is insufficient
to prove no resource, save or working archive can be written transitively.
FND-SAVE-019 itself left computed callees unexamined; its title and the
unconditional conclusion in RULE-SAVE-001 exceeded that scope.

Q-SAVE-007 now holds the missing mechanism/target audit for the no-save
claim. Q-CONFIG-008 retains actual FONT lifetime and later-reader admission
through shutdown's helpers, vectors and exit callbacks. No native read after
release, saved-file write, complete reading or queue closure is established.

## Alternatives

Treating a local shutdown call list as an exhaustive no-write proof ignores
unresolved computed callees. Treating the shipped return pointers as all
runtime callback targets ignores later publications. Treating0623 as a
file-output wrapper contradicts its explicit memory-copy instructions.
Treating CLI as CLD ignores the separate interrupt and direction flags.

## How to reproduce

Retain FND-SAVE-019's historical entry-based procedure for the exit and
registration observations: resident_listing.py ranges1000:0100..0400,
167B:0029..0070,136E:0000..0010 and4AE5:0140..01F0;
overlay180 ranges67300..67350 and67942..67A40;
direct_callers.py targets1000:030B/0361; immediate_search.py for
3676,367A,367E and3572. Their unresolved paths remain unresolved.

For the new copy contract use resident_listing.py44DE:0605..0641,
overlay_listing.py descriptor180,file67321..67354 and67942..67A28,
and direct_callers.py targets44DE:0605/0623. Verify both established
caller entries, fixup-derived segment, argument width, CX512, DS/ES roles,
DF-sensitive REP MOVSW and saved flags/registers. Reports in
GAME_DIR/analysis/handler-census-20261106/ are
shutdown-interrupt-table-copy-pair.txt, shutdown-interrupt-copy-callers.txt,
shutdown-interrupt-table-save-call.txt and
font-cleanup-established-owner.txt. Some reports include neighboring
context, which adds no entry. No original-game or emulated call was run;
all original-derived reports stay local.
