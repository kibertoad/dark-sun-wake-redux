---
id: FND-CONFIG-205
title: Plain decimal word conversion needs at most six bytes for admitted nonnegative inputs
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3150:0293
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452
tool: Python 3.14.7 and Capstone 5.0.7 bounded selected decimal parser/output reading and complete private-field copy-helper reading
environment: null
---

## Observation

FND-CONFIG-204 identifies the initial format `%d` and its word
argument path. This reading follows that exact format, conditional on
valid unchanged format/source/destination storage and correct DS-based
argument identity. It does not assign other formatting conversions.

The entry's ten zero bytes and two zero words supply the default
fourteen-byte formatting record. The percent branch copies that record
into its active fields through 1000:0452 with CX=14. The complete
helper at file 0x00005652..0x0000566B loads its two supplied far
pointers, clears DF, copies CX bytes using word copies and a possible
last byte, restores DS/SI/DI/BP and far-returns while removing eight
argument bytes. It does not restore DF or test storage capacity.
For this even count it copies seven words. Correct private storage
and pointer/frame aliases remain conditions.

For plain d after percent, the parser bypasses sign/space/hash/zero
flags, width digits/star, precision period and length modifiers. It
therefore retains default zero width, zero precision, no explicit
precision and no sign marker. The selected d branch consumes a
signed word as in FND-CONFIG-204. A negative value is negated
in the double-word working value and sets a minus marker. Otherwise
no marker is added on the plain path.

The temporary far cursor is initialized to SS:BP-14, decremented
once and assigned a zero byte there before dispatch. At code 0293,
file 0x00026993, absent explicit precision and zero-padding flags
raise the minimum digit count to one. The digit loop repeatedly
uses unsigned double-word division by ten, stores remainder plus
ASCII zero before the temporary cursor, replaces the working value
with its quotient and increments DI. Zero magnitude skips this loop,
then the minimum-count loop inserts one zero digit. Other magnitudes
produce their decimal digits in reverse construction order, yielding
forward order when copied from the final cursor.

A minus marker, when present, is prepended and counted in DI.
The common output at file 0x00026CE7 skips field padding under
plain zero width, copies temporary bytes until their zero terminator,
and increments output count for each byte. The following format
pointer increment reaches its zero byte. FND-CONFIG-203's final
path then adds the destination terminator and returns the count
excluding that terminator. All temporary and destination offsets
are word-sized; no-wrap and valid nonaliasing storage are assumptions.

| Selected signed word | Output bytes before terminator | Required storage bytes |
|---:|---:|---:|
| 0 | 1 | 2 |
| 9 | 1 | 2 |
| 10 | 2 | 3 |
| 9999 | 4 | 5 |
| 10000 | 5 | 6 |
| 32767 | 5 | 6 |
| -32768 | 6, including minus | 7 |

These are static deductions for the selected path, not native or
emulated tests. Every nonnegative signed word from zero through
32767 needs at most six bytes including termination. A negative
word can require seven, so the bound is conditional on input sign.

FND-CONFIG-202's six-byte caller has an earlier signed-negative
argument rejection. If that admitted argument is unchanged and is
the word actually read through the formatter's DS-based cursor,
and the initial plain format remains current, the selected decimal
output fits that six-byte area. The earlier guard alone does not
prove those segment/state conditions. No general formatter-capacity
check or fit for another format is claimed.

## Interpretation

The selected decimal-output dependency is now bounded for the
nonnegative signed-word case. Remaining native caller-fit questions
concern current format, argument identity/preservation and aliases;
they no longer require guessing the decimal digit count. The same
six-byte area is insufficient for the most negative signed word.
No original-game overwrite or accepted negative call is established.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain current format writers,
DS/SS argument identity, intervening caller effects and storage aliases.
One reading keeps the admitted nonnegative word and initial plain
format; another changes those inputs or supplies different storage.
Complete callers/producers would distinguish those conditions.
Other formats, padding and length states require separate readings.
No native or emulated result is claimed.

The reading that zero magnitude produces no character under plain
formatting is ruled out by minimum precision one. A reading that
six bytes suffices for every signed word is ruled out by the minus
marker and five-digit magnitude at -32768.

## How to reproduce

Follow the percent branch's fourteen-byte copy, plain-d modifier
bypasses, selected argument/sign path, temporary cursor setup and
code 0293 through 032F. Check both unsigned divides, the zero
minimum-count case and sign insertion. Follow the common temporary
copy/padding blocks and final terminator from FND-CONFIG-203.
Read 1000:0452 through its far return and compare the conditional
input cases above. Keep current format, segment and caller-state
conditions separate from the selected arithmetic/output contract.
