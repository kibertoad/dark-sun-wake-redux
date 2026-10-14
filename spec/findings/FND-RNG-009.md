---
id: FND-RNG-009
title: Two overlay callers seed the generator from the low return word of a DOS-service-backed helper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
tool: tools/research/exec-census/direct_callers.py, overlay_listing.py, resident_listing.py and trampoline_target.py; installed executable fingerprints and relocation/fixup validation
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067835..0x00067844
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BDC0..0x0008BDE4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0811..1000:0822
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0963..1000:09B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:046E..1000:0494
---

## Observation

The installed-build relocation-aware direct-call query for 1000:0811 finds
two overlay candidates: overlay 180 at file 0x0006783E, code offset +065E,
and overlay 204 at file 0x0008BDDE, code offset +001E. Both are confirmed
instruction starts by bounded decoding from their established entries:
56B2:0020 resolves to overlay 180 +0141 at 0x00067321; 5787:006B resolves
to overlay 204 +0000 at 0x0008BDC0. Their far-call segment operands are
overlay fixups naming descriptor 0, whose resident segment is 1000.

Both caller sequences push a word zero, call 1000:0963, remove that word,
push the returned AX word, call 1000:0811 and remove the seed argument.
The source helper's DX return word is not pushed. The seed setter reads
the word at BP+6, clears the high state word and stores the argument in
the low state word, as FND-RNG-002 describes. Thus these callers truncate
the helper's returned DX:AX value to its low 16 bits for the seed.

At 1000:0963 the helper receives a near output pointer. It makes two
near-offset local buffers and calls 1000:046E and 1000:0481 through
push-CS/near-call far-return sequences. Those callees invoke INT 21 with
AH=2A and AH=2C respectively, then store the returned CX and DX words
through their near output pointer. The helper passes the buffer offsets
to 1000:386D, preserves that callee's DX:AX result in locals and returns
it unchanged. When its own output pointer is nonzero it also stores the
two result words through that pointer; the two seed callers pass zero,
so that optional output store is skipped. The converter's calendar,
global-state and service semantics are not completely read here.

FND-PARTY-031 identifies main's startup call to the overlay-180 entry.
FND-UI-039 observes a later startup RNG state change without reaching
the known generator entry. That observation is consistent with seeding,
but does not itself identify which write produced the observed 44976.
Overlay 204's caller has other entry paths; no claim is made here about
which game action reaches them or about how often it reseeds.

## Interpretation

The seed setter has recovered direct overlay callers. Their input is the
low word returned by an OS-service-backed conversion helper, rather than
an argument from a reducer or a generator draw. This removes the lack
of any recovered seed caller from Q-RNG-001. Q-RNG-003 retains the helper's
complete conversion, input-memory identity, service effects and reseeding
reachability; Q-RNG-001 retains rule/reducer callers and indirect state access.

FND-RNG-002's absence of resident loaded-image encodings remains a bounded
observation. It never excluded these overlay calls: before fixup their
segment word is a descriptor reference, not the loaded segment literal.

## Alternatives

- The query covers relocation-backed direct far calls, matching overlay
  fixups and same-segment near-call candidates. It excludes indirect calls,
  computed far transfers, jumps used as calls and noncanonical segment aliases.
  It does not prove these are the only seed writers or callers.
- The known generator was queried alongside the setter as a positive
  control. It recovers the documented resident reducer sites and script
  instruction site, plus additional overlay byte candidates. Those additional
  candidates need their own instruction, caller and rule readings; this finding
  makes no complete generator-caller census claim.
- The converter's complete arithmetic and environmental inputs remain open.
  Neither an epoch, timezone, exact seed formula nor replay portability is
  established. Near-buffer DS/SS identity must be checked on admitted paths.
- No experiment executes the seed setter or reproduces its helper's return
  in this batch. The startup observation is corroborating context, not proof
  that this static sequence produced its particular final value.

## How to reproduce

Use the locked evidence Python and the approved installed DSUN.EXE with
direct_callers.py targets 1000:0811 and 1000:0822. Resolve the two overlay
entry trampolines with trampoline_target.py. Decode overlay 180 from
0x00067321 through 0x00067844 and overlay 204 from 0x0008BDC0 through
0x0008BDE4, then read the listed resident ranges and the converter's entry.
Reports may be retained under GAME_DIR/analysis/startup-seed/, never Git.
The committed tools and the finding's stated result suffice to repeat the
bounded query without depending on a machine-local report citation.
