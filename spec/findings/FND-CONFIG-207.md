---
id: FND-CONFIG-207
title: A resident path installs a record whose lookup pointer comes from a FONT selector request
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:00F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:5945
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:599E
tool: Python 3.14.7 and Capstone 5.0.7 bounded resident producer/call reading with operand widths and MZ relocation checks
environment: null
---

## Observation

A declared MZ far-call inventory identifies eight encoded resident
calls to 1BF3:5945, including file 0x0002F04E. FND-CONFIG-175
already reads that setter's complete local pointer stores. This
reading follows the preceding producer at 39D1:00F1, file
0x0002F001, without establishing its complete incoming paths.

The path pushes three double words, in reverse argument order:
zero, 100 decimal and the four-byte tag FONT. It calls 38FF:0438
and removes twelve argument bytes. The first parameter is therefore
the tag, the second a double-word selector 100 and the third a
double-word zero. These widths were checked at instruction operands,
not inferred from the immediate values.

Returned DX:AX is stored at current DS:A0E9/A0EB. The body tests
AX OR DX; zero enters an alternate helper/error path and skips the
following pointer stores at this branch. Nonzero reaches file
0x0002F03E, reads the stored double word and copies it to current
DS:A0ED and DS:A043. It then passes current DS:A03D by far
address to 1BF3:5945. The resource-helper and setter segment
operands at file 0x0002F010 and 0x0002F051 are declared MZ
relocations resolving to 38FF and 1BF3 respectively.

FND-CONFIG-175's setter installs the supplied record address at
CS:1052/1054. FND-CONFIG-206's lookup follows that pointer,
then reads the record's far field at +6. Under unchanged segment
and record state, A03D+6 is A043, so this path supplies that
field from the returned FONT-selector pointer. The installed pointer
is the address of the record, not the returned pointer itself.

The lookup's later table and near-offset reads still depend on valid
returned storage and its layout. The local AX OR DX test establishes
a nonzero encoded return only; it does not independently validate
resource length, type, table capacity or native readiness. This
bounded reading does not assign 38FF:0438's complete acquisition,
error, storage-lifetime or I/O outcomes. Later calls and writers may
replace the installed record or its field.

## Interpretation

One concrete producer connects the stored lookup pointer to an
explicit FONT-selector request. This narrows FND-CONFIG-206's
object+6 question without converting a nonzero returned pointer
into proof of a valid current font table. DS/state identity and
upstream reachability remain conditions on using this chain for
the formatted text caller.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain 38FF:0438's acquisition
contract, incoming resident paths, valid FONT storage, later A043
and CS:1052/1054 writers, aliases and native output. One reading
keeps the nonzero acquired pointer and installed record; another
returns through failure or later replaces that state. Complete
caller/callee and writer readings would separate those conditions.
No native or emulated result is claimed.

The reading that 5945 receives the acquired resource pointer directly
at this site is ruled out by the DS:A03D argument. A reading that
the selector and zero are word arguments is ruled out by the three
four-byte pushes and twelve-byte cleanup. The encoded call inventory
is not complete pointer/indirect-call coverage.

## How to reproduce

Select declared MZ relocated far calls with offset 5945 and relative
segment 0BF3. At file 0x0002F001, read the three push widths,
resource call, twelve-byte cleanup, returned-pointer stores, zero
branch and nonzero continuation through file 0x0002F056. Verify
both declared call-segment operands. Compare FND-CONFIG-175's
setter and FND-CONFIG-206's object+6 dereference, retaining
current DS, record aliases, acquisition validity and later-writer
conditions rather than asserting native table identity.
