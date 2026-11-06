---
id: FND-PARTY-287
title: The script-cast producer derives a signed remainder selector that cannot itself equal positive 1001
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D68D..0x0008D779
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E2F0..0x0007E32C
tool: tools/research/exec-census/overlay_listing.py at c1f036fd; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-MAGIC-020 and FND-PARTY-286
environment: null
---

## Observation

Q-PARTY-134 retains actual number domains for the three word-input
clients. FND-MAGIC-020 already identifies the script-cast entry in
overlay 204 +18CD. Installed DSUN.EXE is 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Offsets below are hexadecimal;
1001 is the positive decimal DATA selector.

### Intrinsic selector production

At +18DA the entry loads EBX with decimal 1000. After its earlier
coordinate calculations, +1901 loads the second double-word argument
at BP+0C into EAX. Operand-size-prefixed sign extension at +1905
forms signed EDX:EAX; +1907 performs signed 32-bit division by EBX.
No instruction changes EBX from 1000 before this division.
At +190A it saves low word DX of the remainder at SS:BP-2.

For every signed 32-bit dividend, division by positive 1000 yields
a remainder from -999 through 999, with the dividend's sign when
nonzero. The quotient fits signed 32 bits, including the minimum
dividend; this operation has neither a zero divisor nor the signed
minimum-divided-by-minus-one overflow case. The remainder fits signed
16 bits, so saving DX does not introduce an additional numeric value.
No divisor or dividend assumption about a particular script is needed
for this instruction-point bound. In particular, it cannot produce 1001.

Later divisions derive other fields and overwrite the incoming packed
argument, not intentionally the saved selector. The level calculation's
EBX=100 does not change the earlier selector's divisor. Signed remainder
is meant here; mathematical nonnegative modulo is not substituted for
negative packed values. This finding makes no claim about the full
domain of the separately narrowed level byte.

### Actual outgoing argument and scope of preservation

The continuation reads slot coordinates unless CX is negative. On its
admitted path, +19A8 pushes word SS:BP-2 as the third word argument
to overlay 193 +0000 at +19AF. There is no child call between the
selector's production and this push. Other writes to DS globals and
the stack frame still require distinct admitted backing: a physical
alias with the saved selector could change its later value. Thus the
arithmetic result is unconditional for the decoded division, while
its use at the call requires retained saved-word state and correct
frame/segment identity.

Overlay 193 +0000 forwards its BP+0A number word unchanged through
the push at +001E to +003C at +0029. Its earlier argument pushes
and local frame require valid distinct stack storage. It supplies no
replacement selector in this prefix. Reached calls and later state
inside +003C, the hit routine, queued/reflected casts and the clients
remain separate preservation and input obligations.

## Interpretation

The script producer's arithmetic number domain is answered: it is
signed -999..999, not an arbitrary word. Positive 1001 requires a
different producer or a change after that production; repeating the
remainder calculation is unnecessary. Q-PARTY-134 retains the explicit
saved-word/forwarding/later-call preservation and all other producers.
FND-MAGIC-020's source route is reused, not counted as new caller coverage.

No whole getter site is excluded. Q-PARTY-124's eighty other direct
sites and Q-PARTY-134's three remain 83 total, plus repeated-hook
preservation and indirect/bypass paths. FMT-PARTY-007 stays supported;
no queue item closes or complete reading is declared.

## Alternatives

- Negative packed inputs do not make positive remainder 1001: signed
  division retains remainder magnitude below 1000.
- A bounded generated number is not proof every later copy or shared
  register retains it. Those obligations are explicitly outside the
  intrinsic arithmetic bound.
- This producer is not the only cast path. An input domain established
  here cannot be applied to arbitrary clients or reflected/queued values.

## How to reproduce

Run overlay_listing.py on the fingerprinted executable for descriptor
204 at 8D68D..8D779 and descriptor 193 at 7E2F0..7E32C.
Bounds are hexadecimal and half-open. The first range ends before
the producer's exit suffix; it covers all instructions from entry
through selector production and the outgoing call, not a full-body
closure claim. Verify operand-size prefixes, EBX's reaching literal,
the saved DX word and its later push independently.
Reports script-cast-remainder.txt and script-cast-forwarder.txt remain
under GAME_DIR/analysis/class-table-closure-20261106/.
No original game ran.
