---
id: FND-RNG-018
title: Clock environment parsing uses wrapping integer hours and stops at the first daylight-label candidate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3B56..1000:3CD7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:315A..1000:31B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2E86..1000:2EF6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:323F..1000:3280
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:37B6..1000:381E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:07D9..1000:07F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000504F5..0x00050675
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00050976..0x0005098B
    kind: file-data
tool: tools/research/exec-census/resident_listing.py and tools/evidence/report.mjs table; installed-build fingerprint, MZ relocation and default word-width checks
environment: null
---

## Observation

The clock initializer looks up the shipped name TZ. Its lookup helper
sets ES equal to DS, clears the direction flag and searches a near-pointer
array rooted at DS:3920. For a properly terminated name and array, it
requires an exact case-sensitive name followed by an equals sign and
returns the near pointer after that sign. A zero array root, zero member
pointer, empty member string or exhausted matching search returns zero.
The population and admission of that array are not read here.

The initializer's local branches have these effects for properly terminated
ASCII input strings, matching DS storage, unchanged classification data
and valid independent output buffers:

1. A zero lookup result, a value shorter than four bytes, a nonletter
   among its first three bytes, or an invalid offset start selects defaults.
   The offset start must be a decimal digit, or a plus/minus sign followed
   immediately by a decimal digit. Spaces at that position are rejected.
2. Defaults write offset 18000 as a low/high pair, enable the daylight
   helper and copy the four-byte terminated labels EST and EDT through
   the two stored near pointers.
3. The parsed branch first clears four bytes of the daylight label buffer,
   copies the first three value bytes into the standard label buffer,
   and explicitly terminates that label. It parses the substring beginning
   at byte three, multiplies the returned low 32 bits by 3600, stores the
   resulting low/high pair and clears the daylight-enable word.
4. It scans from byte three to the terminator, skipping bytes without an
   alphabetic classification bit. At the first classified letter it
   requires at least three remaining bytes and classified letters in the
   following two positions. A failure ends the initializer immediately;
   it does not resume scanning for a later candidate.
5. A successful candidate copies exactly three bytes into the daylight
   label buffer, terminates it and sets the enable word to one. Remaining
   suffix bytes are ignored. Label case is preserved. The label text does
   not set a second numeric offset; FND-RNG-016's converter subtracts one
   hour when its separately called daylight helper returns nonzero.

The integer parser clears the direction flag and skips bytes classified
as whitespace, recognizes one optional plus/minus sign and accumulates
only ASCII decimal digits until the first other byte. The TZ guards have
already excluded leading whitespace before that optional sign. Its short
accumulator path switches to a two-word path once the high word becomes
nonzero. Both paths retain the low 32 bits; overflow has no rejection or
saturation branch. A minus sign negates the pair with carry. No digits
produce zero, and no parse-success flag is tested by the initializer.
The multiplication helper also retains only the low 32 product bits.
The initializer therefore has neither a 24-hour offset bound nor a
colon-separated minute parser.

These synthetic ASCII examples follow the read branches; they are not
native observations or committed original-derived fixtures:

| Value | Offset pair interpreted as low-32-bit seconds | Daylight enabled | Reason |
|---|---|---|---|
| ABC+ | 18000 | 1 | Missing digit after sign selects defaults. |
| ABC5:30 | 18000 | 0 | Integer parser stops before the colon; scan finds no label. |
| ABC-5 | 4294949296 | 0 | Minus five hours retains its two's-complement bits. |
| ABC5DEF6 | 18000 | 1 | DEF is the first label; its trailing digit is ignored. |
| ABC5D-EFG | 18000 | 0 | The first candidate D fails; EFG is never retried. |
| ABC4294967296 | 0 | 0 | Integer accumulation wraps before multiplication. |
| ABC1193047 | 1904 | 0 | Multiplication wraps even though the parsed integer fits. |

### Classification widths and storage

Initializer classification sign-extends each input byte to a word and
indexes relative to DS:3575. Its full index domain is -128..127. The
numeric parser instead zero-extends its whitespace byte, with index
domain 0..255, then checks digit byte values directly. The finite union
of those accesses corresponds, for mapped DS 57E0, to shipped offsets
504F5..50675. This window was read with the fingerprinted table reader.
ASCII letters have an alphabetic bit, digits have the decimal bit, and
only bytes 09..0D and 20 have the numeric parser's whitespace bit.

Bytes above 7F in the initializer read before the nominal classification
base. The shipped byte selected by input 8B has both alphabetic and
decimal classification bits. Consequently a decimal-classified offset
start need not be an ASCII digit outside the preceding ASCII condition:
the integer parser itself rejects 8B as a digit and can return zero.
This records the differing indexes and shipped bits; it does not prove
that those preceding bytes remain unchanged on every admitted path.

The shipped near output pointers at DS:3976 and 3978 contain A4E8 and
A4E4. All label writes use their current values through DS. The copies
and fill helpers explicitly clear the direction flag and set ES to DS.
Buffer lifetime, independence, pointer writers, array population and
continued segment identity remain prerequisites. No unrestricted
arbitrary-pointer or string-admission claim follows from these local bodies.

## Interpretation

This settles the local initializer's ASCII guard, default, integer-offset
and first-candidate suffix branches, including parse/multiply wrap and
the distinction between signed classification and ASCII digit checking.
It removes those local-body obligations from Q-RNG-004. The full input
and service admission stays there; independent clock-state/table writer
and storage-identity obligations are separated into Q-RNG-005.

FND-RNG-017 corroborates only the actual zero-lookup default branch.
The parsed examples here are direct static arithmetic readings, not
native coverage or an established seed-source rule.

## Alternatives

- A modern timezone grammar with minute offsets, long labels or explicit
  daylight offsets does not describe these observed local branches.
- Proper terminators, bounded near offsets, pointer-array population and
  independent label buffers are review conditions, not proved caller
  admission. Aliases can change later reads after earlier writes.
- Extended input bytes cannot be treated as ordinary ASCII letters or
  digits merely because classification bits are set. Signed indexing reads
  a different part of storage from the numeric parser's unsigned indexing.
- Global/table writers, other initializer consumers, service effects and
  native parsed cases remain open. No complete reading or parity promotion
  is claimed. The delivered emulated-call harness has no admitted named
  environment/argument bindings for this case.

## How to reproduce

Use the locked project evidence Python and resident_listing.py on the
approved installed DSUN.EXE for the six listed address ranges. Read the
initializer through its far return, each guard before each copy and the
first-candidate early returns. Check CBW/CWD as default word operations,
the ASCII digit bounds, two-word carry/negation, caller cleanup, helper
direction flags and the converter's later use described by FND-RNG-016.

Use report.mjs table with source DSUN.EXE and XXH3
e296af55ba2ecde7e77f555c90f33d0b. Query start 0x504F5, count/limit 384,
stride 1 and a width-1 flags field at offset zero. State the signed/
unsigned byte-index union as count evidence; use ASCII letters/digits
and whitespace as positive controls. Separately query start 0x50976,
count/limit 21, stride 1 and a width-1 field at offset zero for the
pointer/global/name/label window. Keep all configurations and reports
in GAME_DIR. No native environment mutation was made in this reading.
