---
id: FND-PARTY-333
title: An admitted overlay header-return trap restores its own interrupt frame through the first-match rewrite
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-PARTY-338]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:04F4..4AE5:055A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0672..4AE5:06B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0753..4AE5:0785
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:02FC..1000:02FD
tool: tools/research/exec-census/resident_listing.py at 8f183c45; locked Python 3.12.10 with Capstone distribution 5.0.9; existing overlay interfaces in FND-EXE-226/234/562/566 and queue continuation in FND-PARTY-332
environment: null
---

## Observation

Q-PARTY-137 retains overlay-return admission in FND-PARTY-332's
reflection-to-first-drain interval. FND-EXE-562 describes the manager's
return-address rewriting but leaves the first selected frame unresolved.
This reading applies FND-EXE-234's exact link semantics to the two
interrupt-frame layouts; it does not assert an actual live trap or load.

Let H be the handler BP immediately after its initial push BP and
assignment from SP. Under the admitted conventional interrupt frame,
SS:H holds incoming BP, H+2 the interrupted IP, H+4 CS and H+6 flags.
The handler rejects odd H. It saves registers, enables interrupts and
subtracts two from the saved IP. Zero selects the header-return path;
nonzero selects the trampoline path. Other vector/frame layouts are not
substituted for this admitted one.

### Trampoline entry exposes the caller's far return

For a trampoline entered by a far call, the caller's return IP and CS
lie at H+8 and H+0A. The handler adds six to BP and exchanges the words
at H and H+6. The value at the new BP is therefore the original incoming
BP, and its +2/+4 words are the caller's far-return IP/CS. This is the
frame presented to the loader's rewrite walk. The reverse exchange after
the loader restores the original saved-BP and flags words; it does not
undo a rewrite at H+8 or H+0A. Preserved saved storage and ordinary
callee return are required for that reversal.

### Header-return entry exposes the interrupted return itself

On a return to an unloaded overlay's header offset zero, the INT3F saved
IP is two. The handler subtracts two to produce zero and takes the
direct loader call without the six-byte BP adjustment. With BP retained through the loading continuation, incoming
walker BP remains H. FND-EXE-562 supplies the admitted unloaded-header
state: a nonzero saved return offset in header word +2, loaded code
segment at +10 after successful load, a nonzero trampoline count and
trap-form trampoline bytes before jump-form rewriting.

The jump-form routine then calls the frame wrapper with AX equal to the
new code segment, DX equal to the header segment and CX equal to the
saved return offset. The wrapper's leaf starts from incoming BP rather
than creating a new frame. It first reads SS:H, shifts that link right
one, and compares the current frame's CS at H+4 only if the shifted
link is nonzero and its original low bit was clear (FND-EXE-234).

If H is nonzero and its incoming BP link is nonzero even, the current
frame is considered before any parent. Its CS equals the trapping header
segment, so this is the first match: H+4 is replaced by the loaded code
segment and BX becomes H. Provided the remaining walk terminates and its
writes do not corrupt saved storage, the leaf restores CX. The wrapper
then exchanges that saved return offset with SS:H+2. The interrupted
return frame now contains the saved offset and loaded code segment.
Ordinary restoration and IRET resume that pair, rather than offset zero
in the header or a different parent frame.

This first-match identity needs no assumption that every parent frame
also returns to this overlay. It does need the explicit nonzero H/link,
finite valid link graph, unchanged frame words, successful loader return
and stack separation. A zero/one link terminates before the CS comparison;
an odd nonterminating link skips it; a matching H of zero does not set a
nonzero BX sentinel. Those cases cannot be assigned the same rejoin
from this argument.

### Notification and write scope

After loading, the handler calls through manager-data word pair +0086.
FND-EXE-562 identifies its default relocated target as 1000:02FC. A
bounded source read confirms that target is exactly one RETF instruction.
With that unchanged pointer and intact call frame, it has no explicit
data write, child call or native request. Its body is no longer an
unspecified default callback. This says nothing about other installed
pointer values or writers to the pointer.

The frame wrapper/leaf's explicit rewrite destinations are +4 of each
matched frame and +2 of the first nonzero matched frame. The handler
also adjusts its own saved IP and exchanges the two named BP/flags words
on the trampoline path. Register saves/call frames and the manager's
header/trampoline writes have their own backing obligations. These are
not arbitrary payload writes, but their physical separation from the
cast queue/index still requires actual SS, graph and header/buffer bounds.
FND-EXE-566 retains allocation, movement, file-read and cache-hook effects;
this reading does not establish that those effects preserve the queue.

## Interpretation

The normal return-trap mechanism has a concrete conditional frame rejoin
for the selected first-drain audit. Under its admitted frame and loader
conditions the interrupted return frame, not an unspecified outer frame,
receives the restored pair. The default notification target contributes
no additional explicit write mechanism. No successful live reload,
immutable callback, valid actual BP chain or complete reading is claimed.

Q-PARTY-137 retains actual frame/SS/chain and queue separation, BP preservation through
loading, header
state and saved-offset provenance, loader/cache/native outcomes, changed
notification targets, asynchronous writes and other return roots. Reuse
this conditional frame contract rather than asking again which frame
would be first under these exact conditions.

## Alternatives

- The walker first creates a frame from SP: contradicted by its initial
  use of incoming BP in FND-EXE-234.
- The trampoline adjustment and header path expose identical words:
  the former exposes the caller's far-return pair; the latter keeps the
  current interrupt-return pair at H+2/H+4.
- Every even handler BP guarantees repair: zero-offset sentinels, link
  zero/one/odd cases, invalid chains and aliasing defeat that inference.
- The default notification is an unread game callback: its selected body
  is a RETF leaf. A changed callback remains a separate actual target.

## How to reproduce

Use licensed DSUN.EXE size 634416 and XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`, with the locked evidence Python and
`tools/research/exec-census/resident_listing.py`. Read
`4AE5:04F4..4AE5:055A`, `4AE5:0672..4AE5:06B1`,
`4AE5:0753..4AE5:0785` and `1000:02FC..1000:02FD`.
The existing reporter checks identity and applies MZ relocation.

Reuse FND-EXE-562 for trap/jump header states and initial callback target,
FND-EXE-234 for full link/first-match/termination semantics, and
FND-EXE-566 for allocation and load dependencies. Track H, H+6, the
saved IP/CS and both exchanges by field role; check zero/one/odd links
and the nonzero first-match condition. This is a static frame derivation,
not a seeded execution or observed runtime restore. No original game ran
and no source-derived listing or memory dump is committed.
