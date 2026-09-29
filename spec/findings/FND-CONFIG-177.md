---
id: FND-CONFIG-177
title: Region wrappers stage outputs and pair expansion can reach the append capacity error
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:0C8A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:0D3F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:0E7C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:0BB5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4072:021E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4400:00BA
tool: Python 3.14.7 and Capstone 5.0.7 complete local 16-bit entry readings and declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-173 calls resident 4237:0C8A, 0D3F and
0E7C with input/output aliases and either checks or
ignores their AX. Their complete local spans are
`0x000381FA..0x000382AF`,
`0x000382AF..0x0003835C` and
`0x000383EC..0x00038480` respectively. Each has a
runtime stack-limit guard through 1000:2E48. Parameters
below are the first input at BP+6, second input at
BP+0A and output at BP+0E, all far pointers.

0C8A initializes a private 138-byte region through local
0003. It tests AX FFFF even though that callee's normal
return is zero (FND-CONFIG-176). It then visits every
pair of records, using word indices and unsigned comparisons
against the first and second inputs' counts, re-read at
each respective comparison. Record addresses are their
supplied segment and wrapped word offset input+0A plus
eight times the index. There is no own input-count cap
or pointer-null check before those reads.

Each pair calls 4400:00BA with the two eight-byte records
and a private eight-byte output. Returned zero skips
append; nonzero calls local 00A6 on the private region
with that output record. Returned FFFF from append
immediately returns FFFF from 0C8A, skipping its final
output copy. Other results advance to the next pair.
A finished scan calls 4400:015C on the private region,
then local 0076 to copy it to the supplied output,
ignores both AX values and explicitly returns zero.

The complete 4400:00BA span is
`0x000392BA..0x00039339`. It compares each word pair
at offsets 0/4 and 2/6 using signed comparisons. Strictly
separated bounds return zero. Equality is admitted.
For an admitted pair, the output lower words are maxima
of the corresponding lower words, and upper words are
minima of the upper words; it returns one. It writes
the first pair before testing the second, so a zero
return on the latter can leave those earlier output
words changed. The named 0C8A caller skips append on
that zero. The helper restores DS, SI and DI; it has
no own null, capacity or validity checks. Aliased inputs
and outputs require separate ordering analysis.

FND-CONFIG-176's append rejects a private count at least
16 before another eight-byte copy. Under valid stable
inputs, private initialization and preserved indices,
16 accepted pairs can append; a seventeenth nonzero
pair result reaches the concrete FFFF return. Bounding
each input to 16 records does not bound the number of
accepted pairs to 16. For example, counts four and five
with every record containing words 1,1,2,2 produce
20 locally admitted pairs: the seventeenth append is
rejected. This is a conditional static branch example,
not an emulated result or evidence that original producers
permit duplicated records in an ordinary invocation.

0D3F first calls local 0BB5 on its second input. A
returned one copies the first input directly to output
through 0076 and returns zero. The complete 0BB5 span,
`0x00038125..0x00038168`, visits the second input's
records using unsigned count tests. It calls 4072:021E
on each and returns zero on the first returned zero;
otherwise it returns one, including count zero. The
complete 021E body at `0x00035B3E..0x00035B73` returns
one exactly when all four supplied words are zero,
and zero otherwise, after its own stack guard. It has
no pointer-null test. No broader empty-region identity
is inferred from this local sentinel test.

Other 0BB5 results make 0D3F copy the first input to a
private 138-byte region, then call local 0180 once per
second-input record, supplying the private region as
both first input and output and that record as the
second input. FFFF immediately returns FFFF without
the final caller-output copy. Finishing the scan calls
4400:015C on the private region, then local 0DEC with
that same region as both arguments, then copies it to
output with 0076 and returns zero. Those three results
are ignored. Only 0180's selected entry, bypass and
append routes were read in this batch; its complete
branch effects and 0DEC remain dependencies rather than
an assumed subtraction or compaction contract.

0E7C tests its second input's four-word sentinel through
4072:021E. Returned one copies first input directly to
output and returns zero. Other results copy first input
to a private 138-byte region, call local 0180 with it
as both first input and output and the second input as
the other argument, and return FFFF if that call does.
Other results append the second input through 00A6;
FFFF also returns immediately. Continuing results call
4400:015C on the private region, copy it to output
through 0076, then return zero without checking those
last results. The copy's null-pointer bypass returns
zero without a copy (FND-CONFIG-175), so zero here does
not prove a populated or semantically valid output.

All external segments above were verified through declared
MZ relocations. Private outputs and delayed final copies
are the wrappers' own ordering. No general rollback,
input immutability, buffer initialization on a skipped
copy, absence of aliases or transitive side effects is
proved. DS, guard outcomes and valid stack/input state
remain explicit conditions.

## Interpretation

One wrapper has a directly traced FFFF origin in append
capacity, unlike its initializer-result test. Three
wrappers delay a caller-output copy until their local
continuation or take a direct-copy sentinel bypass.
That ordering bounds which copy a failure skips; it is
not a transactionality or success guarantee. Conditional
pair expansion can exceed output capacity even with
individually small input counts. Actual native records
and semantic region operations still need provenance.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain input counts, record
bounds and sentinel producers, duplicate/overlap invariants,
all callers, valid capacities and stack state, aliasing,
DS/index preservation, complete 0180/0DEC readings and
guard or native outcomes. One reading supplies at most
16 admitted pairs; another supplies more and reaches
the append rejection. The local cases separate their
branch effects but do not establish ordinary reachability.

Private valid nonaliasing storage can prevent an append
failure from executing the wrapper's final output copy;
other aliases or unread transitive effects can change
observable memory earlier. Complete producer/callee
coverage would distinguish their effects. The explicit
checked returns do not settle the actual shared list
helper's inputs or return. Resident cases remain in
Q-SCRIPT-007 after supported layouts and the harness
exist; no native or emulated observation is claimed.

## How to reproduce

Read 4237:0C8A through 0D3E, 0D3F through 0DEB and
0E7C through 0F0F from their entries. Retain parameter
order, private outputs, every count/return predicate and
which final copy each exit skips. Read 4400:00BA through
0138, 4237:0BB5 through 0BF7 and 4072:021E through
0252. Check signed inclusive bounds, partial output writes,
unsigned counts and the four-zero sentinel. Compare append
capacity in FND-CONFIG-176 and copy bypass/preservation in
FND-CONFIG-175. Follow 0180 only as far as this stated
reading; retain its complete branches and 0DEC for later
research. Resolve declared MZ operands and keep conditional
case derivation separate from native or emulated execution.
