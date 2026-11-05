---
id: FND-SOUND-034
title: Installed setup input spans fit local buffers but thirty-two hexadecimal alternate addresses lose their suffix before scanning
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND.INI
    offset: 0x00000000..0x0000D58D
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0B26..1AF6:0BB3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0C8F..1AF6:0CDE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0843..1000:08BE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0C22..1000:0DEF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3991..1000:39E7
tool: tools/research/exec-census/sound_ini_profile.py --inputs and resident_listing.py at be882b15; Python 3.12.10, xxhash 4.0.1, Capstone 5.0.7
environment: null
---

## Observation

This measures Q-CONFIG-006's remaining installed input spans and follows
the hexadecimal counterpart of FND-SOUND-025's decimal reading. The
approved SOUND.INI has length 54669 and XXH3-128
aecc7469098e0c188e0e3d056c5fd530. No numeric source line or device label
is reproduced here. Ordinary counts and lengths below are decimal;
addresses and word results explicitly marked as such are hexadecimal.

### Source geometry and numeric spellings

The installed source has 1500 LF-delimited physical lines and ends in
LF. Maximum physical width excluding CRLF is 91 bytes. There are 1008
bracket-bearing lines. None belongs to a line ignored by the source
profiler as a comment or separator, so that particular source/native
bracket-search discrepancy is absent in this file. No unexpected ASCII
control byte appears after removing the terminal CR of each line;
tabs are admitted by this measurement.

The largest span from the first left bracket to a semicolon or LF is
85 bytes, including a retained CR where present. The maximum first tag
length excluding brackets is 24. These require at most 86 bytes with
the line terminator and 25 bytes with the tag terminator. They fit the
256-byte gap from DS:EA7E to EB7E and the forty-byte gap from DS:EA56
to EA7E in the static line/tag helpers of FND-SOUND-020/025/030.
These are comparisons with mapped field gaps, not a claim that the
native helpers enforce those bounds or that storage remains valid.

There are 720 source values under the fixed numeric key set: 535
decimal and 185 hexadecimal spellings. Every spelling is at most five
bytes, including any sign or base prefix. Decimal magnitudes here are
at most 131 and the only negative value is minus one. All source
values fit in a word. SamplePlaybackMode has no source occurrence;
this is not evidence of its live value independently of record clearing.

Of the 185 hexadecimal spellings, 153 contain only digits and the
base-prefix letters. Their largest source value is 920. Thirty-two
others contain a hexadecimal letter outside the native extractor's
digit/minus/x alphabet: sixteen MidiAltAddress and sixteen
DigitalAltAddress occurrences. All thirty-two have source length five
and initial extractor-alphabet prefix length three. Eight have first
hexadecimal digit two and twenty-four have first hexadecimal digit
three. These are measured spelling properties, not conversions by the
profiler. FND-SOUND-029's count and missing/repeated-primary exceptions
remain unchanged.

### Native token and hexadecimal assignment

FND-SOUND-025 establishes the digit classification table and the actual
numeric wrapper. 1AF6:0B26 starts copying at the first classified digit
or minus in its retained line. It continues only through classified
digits, minus and lowercase/uppercase x. It appends NUL at the first
other byte; it does not include hexadecimal letters a through f or
A through F. For the measured source keys there is no digit in the tag
before the numeric value. A CR or separating space stops the token.
At most five copied bytes plus NUL fit its twenty-byte local array.

For the thirty-two exceptional source values the token is therefore
the base prefix followed by the single digit two or three, then NUL.
1AF6:0C8F selects the percent-x format from the initial zero/x pair,
using the format table and scanner dispatch already controlled in
FND-SOUND-025. Its far wrapper 1000:39C4 supplies the same character
and unread callbacks 3991/39B9. The x arm at 085E selects base sixteen
and calls numeric worker 0C4D with width 7FFF. The plain format leaves
suppression and double-word output flags clear.

The worker accepts initial zero under base sixteen, consumes the x
prefix, then admits the single digit through 0C22. Its NUL callback
returns FFFF; the digit helper rejects that result, the unread callback
backs the input offset up once, and the numeric path completes with
status one and magnitude two or three. There is no sign or overflow
on either case. The scanner increments its assignment count and stores
one AX word through the supplied far output pointer, with no DX word
store. The format NUL then completes the scan. The outer wrapper reads
its now-assigned local word, and 0B26 writes that word to its caller's
output pointer and returns one. It neither rejects nor resumes at the
lost source suffix.

Consequently, under intact line/classification/format/stack/output
storage and the source line's admission in the proper record context,
these thirty-two alternate-field assignments receive word 0002 eight
times and word 0003 twenty-four times, rather than the complete source
hexadecimal magnitudes. The native scanner's ability to accept hex
letters cannot recover letters the preceding extractor never passes.
The other 688 source numeric spellings survive this alphabet filter;
their decimal/base-sixteen small-value paths do not reach the worker's
overflow branch. This does not establish that every source field is
reached, retained, selected or sent to a driver.

## Interpretation

The installed line/tag/numeric spelling widths are removed from
Q-CONFIG-006's remaining uncertainty. The reading also rules out
unchanged hexadecimal conversion for the thirty-two measured alternate
values under the stated native conditions. Source lengths fitting a
record or token buffer never implied numeric identity; FND-SOUND-029
explicitly left conversion separate. No prior finding is superseded.

Remaining obligations are nested context/order and span/alias retention,
parser partial-store/failure continuation and numeric inputs outside this
exact small installed set. Q-CONFIG-011 retains native stream/heap and
DOS outcomes; Q-CONFIG-012 retains downstream field effects. No observed
hardware behavior, complete reading or parity validation is claimed.

## Alternatives

Assuming the scanner receives the whole hexadecimal spelling skips the
extractor's earlier termination. Treating the thirty-two cases as missing
assignments contradicts the successful single-digit scan. Conversely,
reporting an actual device setting or a successful setup run would omit
source delivery, record context, storage and later consumers. Arbitrary
long, repeated-sign, malformed or uninitialized-output cases remain open.

## How to reproduce

Run Python with -I and committed sound_ini_profile.py on the approved
installed SOUND.INI with --inputs. Retain setup-source-inputs.json and
setup-source-inputs-repeat.json under
GAME_DIR/analysis/disc-adv-survey-20261104/. Their SHA-256 is
574a936387490d329f2b66ee0b5100b851655ad93c9680a5e12a01181e40901c.
Aggregate numericRows by sourceBase and field, retaining rows whose
wholeSpellingInExtractorAlphabet is false; compare their sourceBytes
and extractorAlphabetPrefixBytes. The profiler validates exact source
identity and emits no token strings or device names.

Use approved SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Run resident_listing.py with the five
code ranges above in order, retaining setup-installed-hex-prefix.txt and
its -repeat pair in the same local directory. Both have SHA-256
8ad0b92e1e585e162e54871e25a6e4ace818fd9f40771e0da1927d391dd396e5.
Reuse FND-SOUND-025's classification, format and dispatch controls;
follow the actual zero/x/digit/NUL route, assignment width and caller
output store. Original files and derived reports remain local only.
