---
id: FND-SOUND-025
title: Setup's decimal count path converts the installed 13 and minus-one tokens without a sentinel shortcut
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0B26..1AF6:0BB3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0BF9..1AF6:0CDE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:072E..1000:08BE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0B43..1000:0B62
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0BF0..1000:0BF6
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0C22..1000:0DEF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3991..1000:39E7
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x00001FF6..0x00002022
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D12E..0x0001D134
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D52C..0x0001D5AC
tool: tools/research/exec-census/resident_listing.py at b5bd8e13 and tools/evidence/report.mjs table; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0
environment: null
---

## Observation

FND-SOUND-024 left the native conversion of FND-SOUND-023's installed
counts open. This reads the reached decimal scanner path for the two
short tokens 13 and -1, not the general formatted-input implementation.
Code and field offsets below are hexadecimal; ordinary counts are decimal.

### Line, token and conversion wrapper

1AF6:0BF9 seeks a left bracket through the file-byte child 1000:2F81,
then copies from that bracket into DS:EA7E until LF. After a semicolon it
continues consuming but stops copying. On LF it stores the copied length
in DS:1886 and appends NUL; a child result of -1 returns failure, including
before LF. A CR after the value is retained. This is the local line logic,
conditional on child byte delivery and adequate storage, not a complete
file/runtime contract.

For the count lines, 1AF6:0B26 skips non-digit, non-minus bytes and copies
digits, minus and x/X into a local twenty-byte token, appending NUL at the
first other byte. The existing classification report from FND-SOUND-020
sets digit bit 2 exactly for ASCII 0 through 9. Thus each measured count
line's short value yields token 13 or -1, assuming the delivered line and
classification table are retained. CR ends either token; it is not a
number or a sign. These two tokens fit the local buffer with terminator.

1AF6:0C8F selects DS:D9CE only for an initial 0x or 0X; otherwise it
selects DS:D9D1. Exact source measurements show percent-x-NUL at the
first location and percent-d-NUL at the second. Neither count token has
the hexadecimal prefix, so both take the decimal format. The wrapper
passes its local output word to 1000:39C4, ignores that child's assignment
count and returns the local word. 1AF6:0B26 stores that returned word
through its caller's output pointer and returns one.

### Reached scanner and callbacks

1000:39C4 calls near scanner 1000:072E with character callback 3991,
unread callback 39B9, the address of its input far pointer, the format
pointer and far variadic-output pointer list. Callback 3991 increments
the input offset and returns its unsigned byte, except NUL returns -1.
Callback 39B9 decrements that input offset without writing source bytes.

On percent the scanner initializes flags to 20 and width to FFFF.
The controlled format-class table at DS:DDCC maps d to class 7 and x
to class 18. Its twenty-two-word CS:0BF6 dispatch table maps those
classes to 0859 (base ten) and 085E (base sixteen). The independent
percent control maps class 3 to 0808, the literal-character arm.
For d, the path preserves the far-output flag 20, leaves suppression
and double-word flags clear, and passes width 7FFF and base ten to
numeric worker 0C4D.

The worker skips classified whitespace, recognizes a leading minus and
records its sign separately. The retained classification table's whitespace
bit 1 is set exactly at ASCII 9 through 13 and 32. The decimal digit
helper 0C22 admits the token digits with carry clear. The worker accumulates
the first digit and subsequent base-ten digits in a low/high pair, using
the sign only at the end. The small values here never reach its overflow
branch. Reading the terminating NUL returns -1; this fails the digit
test, invokes the unread callback and ends the number successfully.

At 0DAF..0DBF the worker returns magnitude 13 unchanged, or negates
magnitude one as a double word, yielding low word FFFF for -1. It stores
successful numeric status one. At 0898..08B3 the scanner increments its
assignment count, resolves the supplied far output pointer and stores
exactly AX as one word. With the double-word flag clear it writes no
second word. The format's final NUL returns assignment count one through
0B5B and the common epilogue. Consequently the wrapper's output word is
initialized by these two successful paths before it is read back.

## Interpretation

For the delivered installed count lines under the stated storage/table
conditions, the local parser produces words 000D, 000D and FFFF for
MainGroup, DigitalGroup and MusicOnlyGroup. The alternative that this
decimal conversion interprets -1 as zero or as an absent-group directive
is excluded. Combining this with FND-SOUND-024 gives low-word group and
label requests 0xFF02 and 0xFFB0 for the negative count, if the respective
call sites are reached and that state is retained. It does not prove those
allocations succeed, that setup completes, or that an empty chooser runs.

## Alternatives

File-byte delivery, table/state mutation, allocation failure and later
child effects remain independent alternatives to a native successful
setup continuation. A missing numeric assignment is possible for other
tokens because the wrapper ignores the scanner result; no outcome for
those tokens is claimed here. Hexadecimal, long, repeated-sign, truncated,
overflow and general malformed/file-error cases remain Q-CONFIG-006.
This bounded decimal reading does not establish the other scanner format
branches or promote a complete reading or parity status.

## How to reproduce

Use SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57 and resident_listing.py's fixed load
model. Retain listings only under GAME_DIR/analysis/disc-adv-survey-20261104/.
setup-number-conversion.txt, setup-scan-wrapper.txt and setup-scan-core.txt
contain the conversion, scanner and callback entries. Complete the numeric
worker, digit helper and epilogues with setup-scan-number-tail.txt and
setup-number-complete-branches.txt. Table bytes linearly decoded after
0BF5 are data, not instructions in this reading. Reuse setup-value-helpers.txt
and setup-case.json with FND-SOUND-020's identity/controls/repeat.

Run report.mjs table twice for setup-number-formats-controls.json,
setup-scan-classes-controls.json and setup-scan-jumps-controls.json.
The respective report/repeat pairs compare exactly; their SHA-256 values are:

- a51a2d884abfff59e60a18ea2859f617b8cfbdd6d0fea250b615ae588e8246d0
- 659cec34718e6afb964cbabbbff747c76dd09e725b1d657a4e85ff257bf21f05
- e194e446bf8e9a25e740c030b4103d2bb4557337774ea48dbd48f8087788bbc3

Trace the two short decimal tokens through the recorded branch conditions,
callbacks, sign handling and single-word store; do not substitute a host
language's general numeric parser for that reading. No original program was run.
