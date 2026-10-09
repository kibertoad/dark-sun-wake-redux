---
id: FND-EXE-478
title: Sound utility collects Dma and MIDI fields with different mapped-result checks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:0347..158E:0496
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000FD5C..0x0000FD73
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x000003A2..0x000003A6
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-476's stored one-byte conversion continues at 158E:0347.
The caller passes the retained second record pair and current DS:05FC
to 1BD4:02AA. Full returned AX zero selects the recorded AX-zero
common exit; any nonzero value continues. It removes eight argument
bytes. It then calls 2F81 with that record pair, removes four bytes,
stores AL at SS:BP-0A and exits on byte FF. Full AX is not tested.

The continuing byte is copied to BP-1C, followed by zero at BP-1B.
SS:(BP-1C) is passed to 2713; after four-byte argument removal the
caller holds only returned AX in DI, with no result or range test. This
is the local last writer of DI before the following marker and field calls,
not proof that every reached native path preserves it. FND-EXE-477
reads the converter's decimal-prefix and modular arithmetic contract.

The caller next matches current DS:0601 and then current DS:0608
against the same retained record pair. Each full AX-zero result exits;
each nonzero result continues after eight-byte cleanup. There is no position
reset between these searches. It then reads three bytes with consecutive
2F81 calls, storing them at BP-26, BP-25 and BP-24. Each is also
stored at BP-0A and tested there for byte FF. The caller writes a zero
terminator at BP-23, converts SS:(BP-26) through 2713 and passes
only returned AX to 164C:0042.

After two-byte mapper-argument cleanup it stores returned AX at BP-30.
FFFF selects the common exit; every other word continues. FND-EXE-476
reads the mapper's complete twelve-value equality mapping and default FFFF.
This second call's encoded segment 064C at shipped offset 710E is
targeted by MZ relocation record 217, zero-based, at shipped 03A2.
Modeled load segment 1000 binds it to native 164C:0042 independently
of a flat analyzer alias.

The caller then matches DS:060E against the same record, with the
same full AX-zero exit and eight-byte cleanup. It reads one byte through
2F81, removes four bytes, stores AL at BP-0A and exits on byte FF.
Otherwise it copies the byte to BP-2A, terminates at BP-29, converts
that string through 2713 and stores returned AX at BP-32 without a
result or range test. Returned DX is ignored for all these conversions.

Finally it calls 2873 with the retained second record pair, removes four
argument bytes and ignores the result. FND-EXE-358 reads that cleanup
root. It continues at 158E:0496, outside this reading. No local failure
after field collection rolls back the earlier frame stores. A matching
or read exit uses the same previously recorded common exit.

Under FND-CONFIG-213's segment 1E36 and MZ header size 1400,
the bounded shipped strings at offsets 05FC, 0601, 0608 and 060E
are respectively Dma=, [MIDI], Port= and Irq=, each zero-terminated.
Those source identities are file-data; applying them at the caller requires
actual current DS. FND-EXE-377 records the matcher's ordinary progression
and FND-EXE-379 the reader's unsigned bytes versus full-word failure.
The local field buffers establish neither actual digits nor later settings.

## Interpretation

This follows the configuration consumer through its remaining recorded
field strings. The one-byte Dma and Irq conversions are retained without
local numeric-result validation, while the three-byte MIDI Port conversion
must pass the fixed mapper. Sequential searches consume the same mutable
record state, and discarded cleanup does not certify a successful outcome.

Q-EXE-007 retains continuation from 0496 and all retained-word consumers,
DI and native preservation, actual DS/source/frame admission, constructed
string extents, record and table producers, aliases and lifetime/re-entry.
No hardware operation, complete reading or launch exclusion is claimed.

## Alternatives

Treating every field as range-validated overlooks the one-byte stores and
DI assignment. Treating the mapper as a conversion validator adds checks
beyond its fixed word equality tests. Treating later DI as necessarily this
conversion result assumes all intervening native preservation. Treating
section and field searches as independent resets ignores the shared record.
Treating cleanup as tested success ignores the discarded returned word.

## How to reproduce

At revision d50f8e0 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00007027..0x00007176 at IP 0347,
modeled CS 158E, with locked Capstone 5.0.7 in sixteen-bit mode.
Read file-data only over 0x0000FD5C..0x0000FD73, binding offsets
05FC, 0601, 0608 and 060E under segment 1E36 and header size
1400. For the mapping call, use the MZ relocation table at 003E
with 958 records: record 217 at 03A2 targets load-image offset 5D0E,
the segment word at shipped 710E. Apply load segment 1000 to 064C.
Use FND-EXE-360's other native call bindings. Track sequential matcher
results, each byte-width FF test, terminators, conversion low words,
DI assignment, mapper rejection and discarded cleanup result. Original
bytes stay outside Git; no original process, DOSBox or emulated call runs.
