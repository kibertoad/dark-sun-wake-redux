---
id: FND-RNG-016
title: Clock conversion uses four-year calendar arithmetic and an April-to-October hour adjustment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:386D..1000:3986
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3CD7..1000:3DB6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:07D9..1000:07F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00050950..0x00050976
    kind: file-data
tool: tools/research/exec-census/resident_listing.py; tools/evidence/report.mjs table with installed-build XXH3 validation
environment: null
---

## Observation

The converter first calls the environment initializer. It reads the
resulting DS words at 397A and 397C as a low/high pair, adds 315532800,
and maintains a wrapping 32-bit local accumulator. Its near date
argument supplies a word year and signed byte day and month. Time
supplies unsigned hour, minute and second bytes at offsets 1, 0 and 3
(FND-RNG-015).

The bounded shipped data window corresponding to load-image 57E0:3950
contains twelve bytes: 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31.
The following thirteen words at 57E0:395C are 0, 31, 59, 90, 120, 151,
181, 212, 243, 273, 304, 334, 365. Those locations map to shipped offsets
50950 and 5095C with MZ header 5200 and load segment 1000. These are
bounded file-data observations; continued DS identity and absence of
other table writers remain reading obligations.

The converter subtracts 1980 from the year with word arithmetic. It
adds the sign-extended arithmetic-right-shift-by-two result times
126230400, then the low two bits times 31536000. If those low two bits
are nonzero it also adds 86400. Multiplication through 1000:07D9 keeps
the low 32 product bits. The month loop sums signed month-table bytes
before the requested month into a word day count, adds signed day
minus one, and adds one more when month is greater than two and the
year's low two bits are zero. No Gregorian century exception appears
in these branches. No date admission check precedes the table loop.

The converter forms word hours as day count times 24 plus the unsigned
hour byte. If DS:397E is nonzero, it calls the near daylight helper
with word arguments year-minus-1970, zero month, day count, and hour.
The helper removes its eight argument bytes. A nonzero return subtracts
one from word hours. The converter then adds the sign-extended word
hours times 3600, unsigned minute times 60 and unsigned second, retaining
the low 32 bits and returning them as DX:AX.

For ordinary synthetic dates within years 1980..2099, month 1..12,
valid day, hour 0..23 and minute/second 0..59, the intermediate day/hour
words do not overflow. With the listed tables and no intervening table
changes, the arithmetic is therefore the initialized global pair plus
315532800, the four-year/year contributions, and elapsed seconds within
the selected year, less one hour if the helper returns nonzero. This
domain is an arithmetic review condition, not evidence that every
caller or service admits only those values.

For its zero-month path, the daylight helper adjusts a leap-year day
count after day 58 and derives a month by walking the cumulative table.
Months before April or after October return zero; May through September
return one. In April and October it computes a Sunday boundary using
word arithmetic and unsigned remainder by seven. For year-minus-1970
greater than 16, April uses the first Sunday; otherwise April uses the
last Sunday. October uses the last Sunday. April returns one after the
boundary, or on it at hour 2 or later. October returns one before the
boundary, or on it through hour 1. Hour tests read only the argument's
low byte. The separately admitted nonzero-month path adds its preceding
cumulative-table value to the day argument and applies its own leap
adjustment; that path's callers and input admission remain unreviewed.

## Interpretation

The local converter arithmetic and daylight branches explain how a
calendar input can contribute to the seed-source return independently
of the environment initializer's selected global pair and enable word.
This answers those local-body obligations of Q-RNG-004 while leaving
the initializer, service domain, state/table writers and complete
caller admission open. It does not establish a general seed rule.

As a conditional arithmetic cross-check, interpreting FND-RNG-014's
stored words gives year 1995, month 4, day 11, hour 12, minute 34 and
second 56. With the listed tables, global pair 18000 and enable word
nonzero, the helper selects the April adjustment. The converter's sum
is 797618096, whose low word is 44976, matching that finding's return
and seed argument. The globals were not read by that probe; a matching
conditional calculation does not prove which initializer path ran.

## Alternatives

- A modern calendar/timezone library is not a substitute for these
  word widths, four-year branches and historical April boundary switch.
- An invalid signed month can suppress the loop or index beyond the
  twelve observed bytes. Neither this finding nor the local code proves
  the full service input domain or safety of arbitrary caller values.
- The zero-month helper scan requires a bounded day count. Its table
  values alone do not prove termination for arbitrary words.
- Environment lookup, character classification, numeric parsing, global
  and table writers, memory aliases and other callers remain Q-RNG-004
  and Q-RNG-003. No complete reading or native branch coverage is claimed.
- No helper was emulated and no source-derived fixture was committed.

## How to reproduce

Use the locked project evidence Python with resident_listing.py on the
approved installed DSUN.EXE and read the three address ranges above.
Check default word CWD/CBW, signed month/day tests, arithmetic shifts,
word accumulation and the multiplication helper's low-32-bit product.
Trace the daylight helper's zero and nonzero-month paths separately.

Use report.mjs table with source DSUN.EXE and XXH3
e296af55ba2ecde7e77f555c90f33d0b. Query start 0x50950, count/limit 12,
stride 1 and one width-1 field at offset 0; then start 0x5095C,
count/limit 13, stride 2 and one width-2 field at offset 0. Name the
bounded candidate windows as count evidence, retaining index admission
as a separate reading. Store configurations/results only in GAME_DIR.
