---
id: FND-CONFIG-204
title: Initial format bytes distinguish a decimal frame caller from the fixed text wrapper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:291E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0F6A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3150:01FF
tool: Python 3.14.7 bounded initial-data reads and Capstone 5.0.7 bounded formatter selector/argument reading
environment: null
---

## Observation

FND-SCRIPT-005 identifies the program data segment as 57E0.
Using the approved executable's MZ header and load segment 1000,
its initial bytes at 291E map to file 0x0004F91E and contain
`%d` followed immediately by zero. Initial bytes at 0F6A map to
file 0x0004DF6A and contain `%C%C%C%s` followed immediately
by zero. Reads were bounded to 24 bytes each and only those two
short terminated formats were recorded, without neighboring data.

FND-CONFIG-202 supplies current DS:291E to 3150:000E for
its six-byte frame destination. FND-CONFIG-201 supplies current
DS:0F6A to the separate 401B:0172 text routine. These are different
format consumers. Initial-file contents alone do not establish their
current contents, DS at the call, absence of later writes or the
semantics of identically spelled conversions in both consumers.

For the first consumer, the fourteen-entry selector table at file
0x00026D75 and target table at 0x00026D91 route lowercase d
to 3150:01FF, file 0x000268FF. The local selector loop compares
word characters before the computed branch. This bounded reading
checks the d target's argument acquisition and sign handling, not
the full parser or decimal-output helpers.

At that target, nonzero private byte BP-22A advances the argument
cursor by four and loads a double word. Zero advances it by two
and sign-extends the word read through DS:[BX-2] into a double
word. The cursor was initialized from numeric BP+0E in the entry
read by FND-CONFIG-203; its later BX dereference uses DS, not
SS merely because the cursor originated at BP. Private BP-22B
can subsequently narrow/sign-extend the retained value again.
The dword's signed-negative branch negates it and sets a minus
marker; a nonnegative branch can set a plus marker when private
BP-22F is nonzero. These private fields' parser assignments and
the subsequent digit/padding output remain to be completed.

FND-CONFIG-202's caller has an earlier signed-negative first-word
argument rejection and later supplies that word to the formatter.
That earlier gate does not alone prove the formatter reads the same
word through its DS-based cursor, or that intervening calls and aliases
leave it unchanged. The usual five-decimal-digit upper bound for a
nonnegative signed word therefore remains an input/conversion reading
to confirm, not an established six-byte output-fit claim here.

## Interpretation

The initial decimal format provides a concrete lead for closing the
frame-buffer question, rather than requiring every conversion to
be understood first. The fixed text wrapper's separate format accompanies
its six extra words and far string pointer, but those conversion
semantics remain specific to its different consumer. Current DS,
format writes and DS/SS argument identity still matter in both paths.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain current format/DS provenance,
format writers, parser flags and decimal digit/padding output, accepted
arguments and DS/SS aliases. One reading preserves the initial formats
and reads the admitted nonnegative word through matching segments;
another observes changed formats or different argument storage. Full
producer and selected conversion readings would separate those cases.
No native fit, overwrite, original-game defect or emulated result is
claimed.

The reading that both callers use the same format consumer is ruled
out by their different call targets. A reading that a BP-derived cursor
necessarily reads SS storage is ruled out by the BX-based acquisition.
Initial short format strings do not prove current runtime identity.

## How to reproduce

Use the MZ header and mapped data segment from FND-SCRIPT-005
to read only the two bounded zero-terminated formats at 57E0:291E
and 0F6A. Compare the call targets and argument maps in
FND-CONFIG-202 and FND-CONFIG-201. Read the formatter's
fourteen-entry selector/target tables, select d, and inspect code
01FF through its signed value/marker branches. Verify cursor widths,
DS on the BX dereference and sign extension. Keep the full parser,
output conversion and current-state provenance open.
