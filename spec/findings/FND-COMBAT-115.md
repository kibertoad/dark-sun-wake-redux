---
id: FND-COMBAT-115
title: Save-list indexing uses a decimal prefix and the find wrappers do not save their query helper's returned pointer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D3F2..0x0007D52B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2E86..1000:2EF6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:37B6..1000:37F2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:381E..1000:384D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:0321..44DE:040F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45CD:001E..45CD:0038
tool: tools/research/exec-census/resident_listing.py and overlay_listing.py at ffc4325a; locked evidence Python and Capstone
environment: null
---

## Observation

Q-COMBAT-063 needs the source pathname supplied by the selected save
record. FND-EXE-621 already reads overlay 192 +00F2's complete builder,
including its missing upper index test; this finding reuses that body
and resolves the numeric-parser and DOS find-helper assumptions left in
its Alternatives. It does not repeat the builder as a new discovery.

### The filename supplies a decimal prefix, not a strict two-digit field

The builder obtains a name from its explicit find buffer at SS:BP-8A
plus 1E, copies it to BP-6C through near helper 1000:37B6, writes zero
at BP-58, and passes near BP-5A to 1000:2E86. Those latter offsets
are positions six and four in the find-buffer name, respectively.
Thus under admitted DS/SS storage identity and a preserved find record,
the parsed input is the two characters following the initial four
filename characters, terminated before the extension. The separate copy
at BP-6C retains the full name for the later path append.

2E86 reads its near input under DS. It starts DX:AX at zero, skips
bytes marked with mask one in the current DS:3575 classification table,
accepts an optional plus or minus, and accumulates decimal digits until
the first nondigit. It returns the signed result in DX:AX without an
end-pointer output or a requirement to consume the whole input.
For two ASCII decimal digits, with those digits not classified as
skippable and valid stable storage, the result is their value 0..99.
An input beginning with digit one followed by a nondigit returns one;
it is not rejected for the unconsumed second character. These are
conditional local arithmetic cases, not observed filenames.

The builder decrements only AX, copies it to SI and tests signed SI
for negativity. It does not inspect returned DX or require two consumed
digits. As already recorded in FND-EXE-621, it does not compare a
nonnegative SI with its incoming record count. That count bounds the
initial clearing loop only. An admitted parsed value one selects row
zero; ten selects nine; zero becomes FFFF and skips the row write.
The absence of an upper test remains the earlier finding's observation,
not evidence of an actual out-of-range file in this installation.

1000:37B6 copies a near DS string, including its scanned terminator,
to a near destination under ES=DS; 37D8 obtains near-string length;
381E searches backward from that length for the requested byte.
Neither those bodies nor the decimal parser establishes DS=SS for the
builder's near stack-derived arguments. FND-CONFIG-181/182's string
and segment distinctions remain relevant to the actual path admission.

### Find-buffer setup and the saved pointer differ

44DE:0340 and 03AC call local 0321 before selecting the supplied far
find buffer. 0321 saves ES and BX, requests INT 21h/AH 2F, copies
the resulting ES:BX into DX:AX, restores the saved ES/BX and returns.
Immediately after that call, each outer wrapper stores ES and BX in
its saved local pointer; it does not store the returned DX and AX.
The locals therefore have the restored pre-call ES/BX values, not
necessarily the queried pointer returned in DX:AX.

Each wrapper then calls 45CD:001E with the supplied buffer and tests
carry after argument cleanup. That helper tests whether the far pointer
is null, returning AX zero with carry set for null, or AX one otherwise.
The caller's intervening ADD SP,4 changes carry, so that subsequent
carry branch cannot simply be attributed to the helper's pointer test.
This is another local flag-lifetime limit, not a native null-buffer run.

The continuing path calls 032E with the supplied buffer; that helper
requests AH 1A with DS:DX and preserves AX, DX and DS. 0340 then
requests AH 4E with its pattern and masked attribute word; 03AC
requests AH 4F. They map the request's carry to zero or FFFF and
then call 032E with the saved local pointer before returning.
That last 032E preserves the mapped AX, but the saved pointer's
provenance above does not prove restoration of the queried original
DOS transfer address. Equality would require separate admitted state.

## Interpretation

The builder's source-row mapping is more specific than treating the
wildcard as validation of a two-digit save number. Numeric-prefix
parsing, near/far storage identity, external filename bytes, actual
selected-slot state and table capacity must be admitted separately.
The copy at BP-6C and parsed substring are distinct storage operations.

The DOS find request is explicitly routed through the supplied far
buffer, but its surrounding save/restore sequence cannot be assumed to
restore the queried transfer address. Q-COMBAT-063 retains the actual
DOS outputs, ES/BX and DS/SS state, buffer/table lifetime and other
pathname writers. FND-COMBAT-097 separately records the row reader's
description and archive-close behavior, and FND-COMBAT-081 records
the later event-selected load caller. No native directory operation,
extra save file, path mutation or visible failure was created or observed.
No rule or parity status changes.

## Alternatives

- A wildcard match is not the parser's proof of two decimal digits.
- The builder's clearing count is not its parsed-row upper bound.
- A query helper's output registers differ from the registers it restores.
- A branch after flag-changing argument cleanup does not directly test
  the callee's earlier carry result.
- The local pointer mismatch alone does not establish a native DOS outcome.

## How to reproduce

Use the locked evidence Python and DSUN.EXE resolved by tools/game-dir.mjs.
The readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read descriptor 192 file range
7D3F2 7D52B with overlay_listing.py, and the resident locations above
with resident_listing.py. Follow the find-buffer field, copied name,
termination store, parsed substring, AX-only decrement and signed gate
separately. At 0321 track ES/BX before save, after the interrupt and
after restore, then inspect which registers its callers store.
Retain reports only under GAME_DIR/analysis/hit-incoming-20261104/:
restore-save-list-producer.txt and restore-save-list-helpers.txt.
No original listing or filename payload is committed.
