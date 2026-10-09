---
id: FND-EXE-363
title: Sound utility preliminary cleanup mutates its record before a flag-dependent result comparison
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:292B..1000:29F8
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-358 calls 292B when record word six is nonzero and record
word zero is signed negative. The helper saves BP/SI. An all-zero incoming
record pair calls 1000:29F8, then selects AX=0000 without testing that
callee's result. For a nonzero pair it compares record word eighteen with
the incoming offset only; mismatch selects AX=FFFF. No segment comparison
or record-extent check occurs locally.

For signed nonnegative record word zero, it first tests bit eight in word
two. With that bit clear, a pair at words twelve/fourteen different from
the incoming segment and low-word record offset plus five returns zero
without the following stores. Otherwise it clears word zero, then compares
that pair with the same default pair. A difference returns zero; equality
copies words eight/ten into twelve/fourteen before returning zero. Both
default-offset computations are low-word additions without segment carry.

For signed negative record word zero it forms AX as word six plus word
zero plus one, all at word width, and copies AX to SI. It subtracts SI
from record word zero before any downstream result. It pushes the count,
copies words eight/ten into twelve/fourteen, pushes that segment/offset,
sign-extends byte four to a word and pushes it, then calls 1000:3B03.
It removes eight argument bytes and compares returned AX with current SI.
There is no local count-capacity check, signed-byte admission or rollback.

Equality selects AX=0000. On a mismatch it reloads the record pointer
and tests bit 0200 in word two. A set bit also selects AX=0000 despite
the mismatch. A clear bit sets bit 0010 in word two and selects AX=FFFF.
The preceding word-zero update and pointer-pair reset remain on that local
failure path. The two return suffixes restore SI/BP and far-return without
incoming argument cleanup; neither establishes a DX result pair.

For any negative initial word-zero value W and initial word-six value L,
the encoded requested count is (L + W + 1) modulo 65536. Its immediately
following word-zero store is consequently (-L - 1) modulo 65536, independent
of W. That algebra describes the local word arithmetic, not an admitted
buffer length or the quantity a native operation actually consumes. Later
aliases or downstream writes are not excluded by this local equation.

FND-EXE-358's caller tests the helper's full returned AX and can leave
before its later record clearing when it is nonzero. FND-EXE-355 in turn
ignores that outer cleanup return before returning a zero pair. These are
distinct result decisions at different callers.

## Interpretation

The preliminary helper's failure can follow record mutation, and its zero
return can follow an unequal downstream result when bit 0200 is set.
Neither result is a general unchanged-state or successful-release guarantee.
The requested count's low-word arithmetic is separate from buffer capacity
and from the returned-word comparison.

Q-EXE-007 retains 1000:3B03's count/result and register contracts,
1000:29F8's iteration and callers, flag/state writers, actual segment and
record admission, extents, aliases and lifetime. The local SI restoration
does not establish every intervening callee's behavior. No complete-reading
promotion, native outcome or launch exclusion follows.

## Alternatives

Treating every mismatch as FFFF ignores the bit-0200 path. Treating FFFF
as pre-mutation rejection ignores the count subtraction and pointer reset.
Treating the wrapped sum as a validated length confuses arithmetic with
capacity admission. Treating the zero-pair iteration result as propagated
ignores the unconditional zero suffix after 29F8.

## How to reproduce

At revision 5d2e1c0 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00003D2B..0x00003DF8 at IP 292B, modeled CS 1000,
MZ header size 1400. Follow the zero-pair and offset-only guards, signed
record-word test, both default-pair comparisons, ordered count/pointer stores,
downstream arguments and full-word result comparison. Track the bit-0200
exception separately from the bit-0010 failure store. The byte extension
at IP 29D2 is the sixteen-bit instruction despite Capstone's wider mnemonic.
Compare FND-EXE-358 and FND-EXE-355's separate caller-result handling.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.
