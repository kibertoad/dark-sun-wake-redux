---
id: FND-EXE-236
title: Cleanup near-callback default differs from two later decoded replacement stores
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004AF60..0x0004AF62
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:1256..4AE5:1257
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:09BF..4AE5:09C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0BC0..4AE5:0BC6
tool: Ghidra 12.1.3 PUBLIC and executable-reader 2.5.0
environment: null
---

## Observation

Under FND-EXE-176's candidate state mapping, DS-relative word `0x0080`
corresponds to shipped-file word `0x0004AF60`. That word initially holds
near offset `0x1256`. Under candidate CS `4AE5`, source decoding at
`4AE5:1256` gives one instruction and one byte: a near return without extra
argument cleanup. It ends exclusively at `4AE5:1257`, immediately before
FND-EXE-178's separate far-return candidate. The resident saved listing has
no instruction at the near-return start; its missing window is not evidence
that the candidate bytes are absent.

Two decoded instructions explicitly store `0x0D11` into current DS-relative
word `0x0080`, at `4AE5:09BF` and `4AE5:0BC0`. Their bounded surrounding
contexts are the callback-writer paths in FND-EXE-178. The first store follows
the word-`0x0084` replacement there; the second follows the word-`0x0082`
replacement. Neither native invocation nor the writers' live DS identity is
established by those local stores.

A decoded memory-operand search for scalar `0x0080` reports seven operands,
including FND-EXE-235's computed near call and these two stores. Other hits
are stores at `1000:01DD` and `15F3:074B`, a read at `1000:0218` and an indexed
ES-relative read at `409B:0C34`. Their effective storage identities remain
unread; equal displacement alone cannot classify them as accesses to this
callback word. This is a positive lead search, not an exhaustive writer or
caller claim. Undecoded code, computed addressing without that scalar,
aliased offsets and external or interrupt-time writes are not excluded.

## Interpretation

The shipped default is a near-return candidate under declared segment
bindings, while two later explicit stores supply a different offset. This
advances FND-EXE-235's target dependency but does not resolve its live callback.
The comparison literal in that caller is neither this default nor proof of
the later target's admission.

Q-EXE-001 and Q-EXE-010 retain writer DS and native caller/CS admission,
other hits' effective storage, every callback writer, the replacement target's
behavior, invocation order and interrupt-enabled changes. No complete_reading
or replacement inventory is established.

## Alternatives

An immutable no-op callback is unsupported by the later replacement stores.
A far return for the default conflates two adjacent but distinct starts and
is contradicted by source decoding of the near-return candidate. A missing
saved instruction window cannot establish absent source code. Treating all
seven displacement hits as one field would assume the missing segment and
effective-address identities.

## How to reproduce

At revision `876510c`, use FND-EXE-176's hash-guarded source table command:
start `0x0004AEE0`, count one, stride `0x0112`, limit one, countEvidence one
candidate state header, field callbackWord80 at offset `0x0080`, width two.
The result is the shipped unsigned word above, not a live-memory observation.

Use the same source hash and FND-EXE-226's x86-bounds settings, extending
the declared resident region to `0x00040050..0x00042050`, with segment `4AE5`,
IP zero and entry/sole entries value `0x000412A6` (`4AE5:1256`). Supply no
seeds or summaries. Inspect the one-byte near-return candidate; region
declaration and CFG completion do not admit its native invocation.

In the resident Ghidra snapshot, read-only with analysis disabled, run
ReportInstructionWindow at `4AE5:1256`, count two, retaining its explicit
undisassembled-start result. Run ReportScalarConstants with kind memory and
value `0x80`; its seven operands are emitted without the 300-match cap.
Use ReportInstructionContext at `4AE5:09B9` and `4AE5:0BBA` to inspect the
two positive stores. No negative search or complete writer set follows.
Sources, configurations and reports remain in GAME_DIR.
