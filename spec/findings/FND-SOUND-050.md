---
id: FND-SOUND-050
title: Resource playback dispatch uses type-specific gates and treats 6000 and 6001 as record-default selectors
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4654:056A..4654:09A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4654:0A96..4654:0CF7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49E9:02ED..49E9:0497
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0D48..4842:0D4E
tool: resident_listing.py at def2e405; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with ReportDecompileMatches.java; FND-SOUND-007/016/049 and FND-PARTY-317/321
environment: null
---

## Observation

Q-SOUND-002 retains the meaning of the two words forwarded in
FND-SOUND-049. Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses and field offsets
below are hex; 6000 and 6001 name the caller's decimal constants.

### Dispatch classification and repeated type reads

4654:056A reads a six-byte entry at 50C3:(0D2D + 6*index).
Its byte +1 selects a type and its far field +2 supplies a record.
It initially clears three local pointer slots. Types 5/6 populate one,
type 4 another, and types 7/8 a third. Later branch tests reload
the table type. Actual type/pointer stability is necessary before
assuming the selected branch has the corresponding nonnull local;
the body does not validate that correspondence or bound its index.

Its second and third arguments are held in SI/DI and passed to
three children on selected paths. There is no local interpretation
of those words as a rate or volume. Retention across earlier children
and saved local frames is a prerequisite to applying their input
contracts; the body does not reload SI/DI from its formals before
every child call.

| Type under unchanged table binding | Local admission and effects |
|---|---|
| 4 | Requires entry byte +0 zero and configuration word +32 unequal to 0, 3 and 4. It compares configuration word +34 with active count 4E71:0B54 signed; when not greater, it calls 0D74 then 0CF7. It requests a value through 4842:0D66, stores its low byte at record +2, calls 0A96 with the three held words, increments the active count and marks the entry active. Those publications follow the child return; no returned success flag is tested. |
| 5/6 | Requires configuration word +14 bit 0 and DS:3434 zero. It publishes the low index byte at 4E71:0B6A, checks/rechecks state through 4842:0C82, can reject a lower unsigned record +8 priority, and can stop through 0C8E. Changed identity can call 47E5:02B6; FFFF takes an error-call/return route. A selected record marker can update a stream through 0C7C. Depending on configuration byte +3A and the 0D06/4E71:0C1F gates, it invokes digital child 49E9:02ED or skips it. Some continuations then publish record +8 at 0B6C and current identity at 0B6B. Publication is not proof the child ran or succeeded. |
| 7 | Requires an inactive entry, then configuration word +32 equal to zero followed immediately by equal to three. Later code tests record +0D, existing sequence state, requests a sequence through 47B9:0002 and stores DS:3442. It can acquire a value through 0D66, write record +2, call 0C2C with the held words, then start the sequence through 4842:0CF4. |
| 8 | Has the same successive zero/three configuration tests and record +0D/sequence-state conditions. Its later sequence-request path joins the common 0CF4 start without the type-7 0C2C parameter child. |
| Other unchanged types | Reach the common return without one of those type paths. |

The type-7/8 zero and three tests address the same held ES:BX+32,
with no intervening local write or child call. Their continuing routes
are impossible if that field stays unchanged. A changed field/binding
or alternate entry remains a separate admission question; this is not
an observed native bug or an unconditional whole-path exclusion.

For a stable configuration byte +3A, zero invokes the digital child;
one invokes it unless 0D06 returns one and 4E71:0C1F is zero,
which skips the child but still reaches identity/priority publication.
Other values return without those later cache publications. The actual
code reloads +3A on the non-one route; stability across those reads
is a condition, not a value cached by this body.

The type-5/6 load-failure call supplies error code 0B and selector
006A to 49DE:000F, then returns locally. FND-PARTY-321 shows
that this error routine's teardown/exit branch selects 0069, not
006A. The call must not be silently assigned the fatal-error
contract. Earlier identity publication has no local rollback.

### 6000 and 6001 select defaults in the reached children

Both 4654:0A96 and 0C2C load the indexed entry's record pointer.
They compare second argument with 1770, decimal 6000, and third
with 1771, decimal 6001:

| Child | Second word equals 6000 | Third word equals 6001 |
|---|---|---|
| 4654:0A96 | Takes record byte +7. | Takes record byte +8. |
| 4654:0C2C | Takes record byte +7. | Takes record byte +8. |
| 49E9:02ED | Takes record byte +0C, adds current word 4E71:0B68 at word width, then clamps the signed sum to decimal 15..127. | Takes record byte +0D, with the cached-value behavior below. |

The first two children zero-extend their selected bytes and pass them
to 4842:0D48. The second parameter uses selector seven. For the
default third parameter, both use selector ten. The status word is
B0 OR (zero-extended record byte +2 minus one). 0D48 selects
driver function BA and tail-jumps to the shared dispatcher. Driver
interpretation and actual output remain unproved by these wrappers.

Their explicit-value branches are distinct: arguments above unsigned
sixteen are overwritten with sixteen in their incoming stack slots.
Scaling is byte-width eight-times-value minus one, so zero wraps
to FF rather than becoming zero. The second parameter additionally
has an unsigned floor of 30 hex. The explicit third parameter uses
selector ten in 0A96, but selector seven in 0C2C, unlike its
default third branch. Preserve that encoded difference; do not normalize
it to a preferred controller interpretation without driver evidence.

0A96 also performs record-derived setup calls and invokes
47B9:01F5 if the timbre query returns zero. FND-SOUND-016
already records that helper's file path. Its child outcomes and
record preservation remain conditions, not fixed success results.

The digital child's default second result is compared with cached
byte 4E71:0B6E. A difference requests 4842:0CA0 with the
value and DS:3436, then publishes its low byte even though it
does not test the returned result. Default third similarly compares
record +0D with 0B6D, requests 0CAC when different and reloads
the record byte afterward for its cache store. The requested value
and later stored byte therefore need not match without preservation.

Its explicit second branch uses a signed upper comparison, word-width
scaling and mixed unsigned/signed clamps, unlike the byte algorithm
above. Explicit third uses an unsigned upper clamp and word-width
scaling followed by subtraction from 127; it requests 0CAC without
the default branch's cache update. Those explicit-value arithmetic
paths and actual caller domains need their own complete application
before assigning physical units or normalizing edge cases.

Finally the digital child selects DS:3424 for unchanged type six
and DS:3420 otherwise, passes that pointer with record word +2
to 4842:0C76, then requests 0C88 with DS:3436. These requests
are not native playback confirmation. The local returns do not
provide a common success flag.

## Interpretation

The caller's 6000/6001 words are default selectors, not literal
sample-rate/volume measurements at these interfaces. Q-SOUND-002
can remove that ambiguity and the dispatch body's unspecified local
type/gate behavior. Actual type/card/mode/record history, saved
register/frame retention, explicit-argument domains, driver selectors,
physical parameter meanings and original output remain open. No whole
queue closure, complete reading or native playback result follows.
RULE-SOUND-001 remains supported; no reachable contradictory state
is inferred from the conditional gate observations alone.

## Alternatives

- The two constants are passed through unchanged to a device as units:
  all three parameter children recognize them and select record defaults.
- A return from a child proves playback: gates can skip invocation and
  later publications do not test a success result.
- Type-7/8 configuration gates admit one stable value: they require the
  same unchanged word to equal both zero and three.
- Default and explicit third-parameter paths use the same selector in
  0C2C: one uses ten, the other seven.

## How to reproduce

Reuse FND-SOUND-049's input binding and FND-PARTY-321's error
selector. Read resident_listing.py for all four locations, tracking
the three zeroed pointer slots, repeated type/field reads, outgoing
push order, publications, arithmetic widths and common return paths.
Use bounded Ghidra matches at 4654:056A for param_2; verify
flattened pointer expressions and inferred types against instructions.

Retain sound-record-playback-contract.txt,
sound-record-parameter-children.txt, sound-digital-parameter-child.txt,
sound-controller-dispatch-selector.txt and sound-record-playback-ghidra
reports under GAME_DIR/analysis/handler-census-20261106/. No original
game, emulator session or emulated call supplied evidence here.
