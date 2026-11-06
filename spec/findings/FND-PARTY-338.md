---
id: FND-PARTY-338
title: An admitted overlay header-return trap restores its own interrupt frame through the first-match rewrite
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
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
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:03E8..4AE5:04F4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:055A..4AE5:07C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:09CB..4AE5:0AB5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0C22..4AE5:0D27
tool: tools/research/exec-census/resident_listing.py at 8f183c45, 7d00e02e and 027bacb1; locked Python 3.12.10 with Capstone distribution 5.0.9; existing overlay interfaces in FND-EXE-226/234/562/566 and queue continuation in FND-PARTY-337
environment: null
---

## Observation

This replaces FND-PARTY-333. Its original record at 0a211fad is
preserved under that ID with a supersession link. This entry carries the
later connected readings and their recorded tool revisions, which had
been added in place to the old record. The replacement repairs that
evidence-trail error; it does not add a new run, status promotion or
complete-reading declaration. Unresolved conditions remain as recorded.

Q-PARTY-137 retains overlay-return admission in FND-PARTY-337's
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

### A sufficient own-write contract for the frame walk

The leaf and wrapper admit a concrete sufficient graph contract; this
is not a claim that every actual frame chain has that shape. Let S be
SP on entry to +075F and b0 its incoming BP. Require unchanged SS and
code, no outside writes, readable nonwrapping words, S at least four,
and S+2 at most b0. Let every reachable nonterminal link, with its low
bit cleared, advance to an even next node at least six bytes higher.
All nodes are at most FFFA, and a terminal link is zero or one.
Physical SS addressing must not wrap at the real-mode address boundary.

For a node b, its link word occupies b..b+2. The only leaf rewrite is
b+4..b+6; it is skipped for an odd original link. With the six-byte
spacing, that rewrite cannot touch the current link, any earlier link,
or any later node's link. The wrapper's one exchange at the first
matched node plus two likewise touches no link. Thus own rewrites do
not change the graph on which the traversal depends. Strict upward
progress in the bounded sixteen-bit offset domain also prevents a
cycle; under these link conditions the walk must reach its sentinel.
These are sufficient conditions, not a required shape for every safe
odd-link or terminal-node case.

The two initial pushes save CX at SS:(S-2) and BP at SS:(S-4). The near
return word is at SS:S. All three words lie below b0 under the stated
ordering and do not overlap the rewrites at b+4 or the later exchange
at b+2. The pushes themselves do not overwrite a link. No other leaf
instruction adjusts SP. Consequently its own writes preserve these
save/return words, and ordinary exit restores the input BP and CX.
For a wrapper invocation, its near return is the following stack word;
the exchange is above that word as well. Since b0 is nonzero under
this ordering, a matching node does not collide with the zero BX sentinel.

The exclusions are consequential. A mere even readable link is not
enough. With a four-byte successor gap, the current b+4 return-segment
word is the next node's link word. If it matches DX, storing AX there
can replace that link after the current successor has already been
captured in CX, changing the rest of the walk. Likewise, if b+4 equals
S-4, an admitted matching store overwrites the saved BP word: the final
POP BP then restores the replacement. Balanced pushes and pops do not
exclude either case. These are conditional alias counterexamples, not
observed native defects.

For Q-PARTY-137 this replaces an unspecified own-write hazard with
checkable stack ordering, node spacing and successor-bound conditions.
Actual handler BP/SP, initial links, outside/native changes, different
chains and rewritten caller storage remain to be admitted. The paused
chain in FND-PARTY-340 does not by itself establish the leaf's entry S
or the later handler's graph. Its source-level caller and saved-word
conditions cannot be dropped merely because this sufficient case is proved.

### BP through the connected load continuation

A connected reread follows +05A4 through allocation, wrap/move,
append, jump/trap rewriting and probation, together with the file-backed
load/fixup path and both directions of the EMS transfer interface.
For these named paths, the internal routines that explicitly replace BP
are the frame walker and cache allocator. Their normal restoration
sources are concrete, not a general callee-register convention:

| Routine | Own BP use | Normal restoration source |
|---|---|---|
| +075F frame walker | +0767 replaces BP with the next cleared-low-bit link | +0762 saves BP, +0782 pops it; for entry SP S the saved word is SS:(S-4), after the preceding CX push |
| +0C2E cache allocator | +0C53 loads its cache procedure word; +0C94 substitutes the file-loader offset for a chain update | +0C31 saves BP after DS, +0CA8 pops it; for entry SP S the saved word is SS:(S-4) |

The allocator's +0C22, +0CD5 and +0D04 children do not explicitly
assign BP. The latter two traverse cache-header links; they can fail
to terminate and write header fields, so ordinary return and separation
from the saved word remain conditions. The allocator restores BP before
its remaining +0D04 call and own final return. It does not intentionally
return its temporary loader-selector value as the caller's BP.

The named allocation/move/append/probation and rewrite routines do not
introduce another own BP replacement beyond the walker calls. This
includes wrap-around's stack of header segments and the file load/fixup
helpers. Their memory writes still need the admitted graph, source and
stack backing; absence of an explicit register assignment does not
exclude overwriting a saved word or a return frame. The walker's
SS-relative rewrite stores and wrapper exchange retain FND-EXE-234's
alias and first-match conditions.

The remaining native boundaries are specific. File-backed loading at
+04C6 reaches DOS seek/read requests through +03E8 (INT21 at +03F0
and +040C). EMS write and read paths reach INT67 at +09F7/+0A19/
+0A40 and +0A51/+0A85/+0AAA respectively. These routines provide no
surrounding BP save/restore of their own for those native requests.
The connected BP result therefore requires native BP preservation,
correct SS/SP and saved storage, and ordinary returns at those sites;
IRET or a carry/status gate alone supplies no such proof.

The selected default notification is the already-read RETF leaf. The
near hook is covered here only when it selects the EMS branch or the
no-copy return under its recorded flags. The selected header's own
near load target at +18 is a separate binding: this reread covers
+04C6 file loading and +0A4B EMS loading. Extended-memory loading,
a changed +18/+80/+86 target, changed mode, nonlocal failure or an
asynchronous handler is not silently assigned the same BP contract.

Thus a successful selected header-return load preserves the handler's
BP through these connected ordinary paths if the two named save slots,
native BP and target/stack conditions hold. This supplies the previously
unspecified restoration mechanisms for FND-PARTY-338's H argument;
it does not establish their actual reflection-time admission.

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

For the connected BP continuation, use resident_listing.py at 7d00e02e
with `4AE5:03E8..4AE5:04F4`, `4AE5:04F4..4AE5:055A`,
`4AE5:055A..4AE5:07C5`, `4AE5:09CB..4AE5:0AB5` and
`4AE5:0C22..4AE5:0D27`. Reuse FND-EXE-234/239/240/241/566
and FND-PARTY-339 for the connected interfaces. Separate a restored BP
register from its saved word's preservation and from native BP effects.
No original game ran for this reread.

For the sufficient frame-write case, reread the full wrapper/leaf
`4AE5:0753..4AE5:0785` with resident_listing.py at 027bacb1.
Track the link read before the +4 store and the clear-low-bit successor.
Compare each two-byte write with the ordered node/link/save intervals;
retain the four-byte-gap and saved-BP overlap counterexamples. This is
static reasoning over the recorded instructions, not an emulated call
or original run and not proof of actual frame admission.
