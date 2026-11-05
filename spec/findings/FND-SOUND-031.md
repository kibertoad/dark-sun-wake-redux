---
id: FND-SOUND-031
title: Setup lower reader translates CR and end markers while retaining conditional DOS errors and position results
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:04CE..1000:0505
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:05CC..1000:05F5
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:06FF..1000:072E
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:27FC..1000:2873
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3720..1000:37F1
tool: tools/research/exec-census/resident_listing.py at 2914dbb9; Python 3.12.10, Capstone 5.0.7
environment: null
---

## Observation

This follows the actual lower reader and zero-read position checker from
FND-SOUND-030. Offsets, masks and error codes are hexadecimal. All table,
buffer and stack accesses below require valid retained storage. DOS
interrupt outcomes are parameters to this reading, not observations.

### Raw read and error return

1000:06FF indexes a word handle table at DS:DD3A by twice its handle.
Flag 0002 rejects the read through error helper 04CE with code 0005.
Otherwise it invokes interrupt 21 with AH=3F, BX=handle, CX=requested
word count and DS:DX=the supplied far buffer. It restores DS before
testing carry. Carry clear returns DOS's AX unchanged; carry set passes
the returned AX error to 04CE. This wrapper itself has no handle bound
check; the ordinary caller 3720 performs one first.

04CE always returns AX=FFFF and removes its own two-byte argument. For
a nonnegative signed input it caps values greater than 0058 to 0057,
stores the selected code at DS:DD66, and maps it through a signed-byte
table at DS:DD68 into word DS:007F. For negative inputs it negates the
word: a signed result greater than 0030 takes the same 0057 mapping;
otherwise DS:DD66 becomes FFFF and DS:007F receives that negated word.
The latter signed comparison also retains the wrapped 8000 case. The
mapping table's actual values and later consumers are not read here.

### Higher reader and translation

3720 rejects an unsigned handle at or above DS:DD38 through 04CE with
code 0006. It returns zero without reading when the requested count word
is zero or FFFF, or handle-table flag 0200 is set. Otherwise it calls
06FF. Results zero and FFFF return unchanged. Any other result returns
unchanged when handle-table flag 4000 is clear; when set, that result
becomes a word loop count over the supplied buffer.

The translation loop copies ordinary bytes in place, removes CR bytes,
and stops on byte 1A. Its source and destination offsets advance within
the same supplied segment; this is not normalized far-pointer movement.
Removing CR while more source bytes remain continues the loop. If the
last byte is CR, it calls 06FF again with count one into a local stack
byte and then copies that byte to the output. It does not test this
additional read's AX or reclassify the local byte before copying it.
Thus successful initial reading alone does not prove that appended byte
was freshly supplied or that it is neither CR nor 1A.

If translation emits no bytes after removing CR, it repeats the original
request rather than returning zero immediately. Otherwise it returns the
low-word difference between destination and original buffer offsets.
On byte 1A it requests a relative seek through 05CC by the negative
remaining loop count, then ORs 0200 into the handle table regardless of
the seek result. The marker and following input are not copied. The
requested rewind includes the marker itself; the return value counts
only bytes emitted before it. The next 3720 call therefore sees the
0200 zero-result gate unless something else changes that table state.

05CC clears handle-table flag 0200 before invoking interrupt 21 AH=42
with origin from its supplied low byte and signed displacement from its
two supplied words. Carry clear retains the returned DX:AX. Carry set
passes the error through 04CE and sign-extends FFFF into DX, returning
FFFF:FFFF. The marker caller drops both success and failure results
before setting 0200, so its local flag change is not proof of a rewind.

### Zero-read position checker

27FC uses the same unsigned handle bound and 0006 error path. With
handle-table flag 0200 set it returns one without an interrupt.
Otherwise it invokes interrupt 21 AX=4400. Carry set reports an error;
carry clear with DL bit 80 set returns zero. For other results it gets
the current position with AX=4201 and zero displacement, saves that
DX:AX, requests the end position with AX=4202 and zero displacement,
then restores the saved position with AX=4200. Each carry-set result
takes 04CE's FFFF return. A failed end-position request does not reach
the restore request. A failed restore does not establish retained
position. On successful restore it compares the returned position with
the saved end position as unsigned high/low words: below returns zero,
equal or above returns one. These are the exact results tested by
FND-SOUND-030's direct byte-read path.

## Interpretation

These bodies remove the local raw-read return, error-helper return,
CR/1A translation and zero-read position-test dispatch obligations from
Q-CONFIG-006. They do not establish what DOS returns, which flags the
installed setup stream actually receives, or whether its buffers and
table remain valid. In particular 06FF can return FFFF, while the
unbuffered byte reader's nonzero test still continues to its scratch
byte (FND-SOUND-030). A faithful account must retain that distinction
and the unchecked last-CR read rather than assuming successful delivery.

## Alternatives

An end marker is not evidence that the rewind succeeded. A zero raw
count is not automatically the position check's result one. The helper
does not guarantee stream-position restoration on every error path.
No effect of actual DOS/device behavior, error mapping values, native
stream initialization, flush child 292B, alias/lifetime or later table
writers is settled. These remain Q-CONFIG-006. This is static local
evidence only; no runtime, complete-reading or parity promotion is made.

## How to reproduce

Use approved installed SOUND_DS.EXE, length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Invoke resident_listing.py with -I
and the five location ranges above in order. Retain UTF-8 reports at
GAME_DIR/analysis/disc-adv-survey-20261104/setup-lower-read-results.txt
and setup-lower-read-results-repeat.txt. Both have SHA-256
2227b776770f1f4cbb1d1198ab57dce0e52ed1da9c8bd07e68d07e72e08150de.
Follow the reached far calls from 3720 and the byte reader's 27FC call,
track carry at each interrupt and both words of seek results, and keep
the last-CR call's ignored return separate from the initial read test.
The source-derived listings remain local; no original program is run.
