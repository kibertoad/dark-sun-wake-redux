---
id: FND-SOUND-075
title: Command-line region selection stores a wrapping low word from an ordered gated suffix parse without a region bound
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0024..277B:02C9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3787..1000:37B6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0001CF51..0x0001CF85
    kind: file-data
tool: resident_listing.py and bounded table reporter at 42e5154f; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

The installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. This connected reading refines the
command-line region producer in FND-SOUND-010/071, reusing the converter
interface established by FND-RNG-018 and FND-COMBAT-115. Its boundary is
argument selection, key gating, conversion and publication before startup
initialization, not every later producer of the current-region word.

277B:0024 holds the incoming near argument-vector offset in DI and starts
SI at one. It compares SI with incoming signed word argc at each iteration.
For each admitted index it loads the near argument pointer from current
DS:(DI + low-word 2*SI), reads byte one, sign-extends it and calls
1000:09B0. The latter preserves FFFF as a special value; otherwise it
zero-extends the low byte and adds 32 only where the current DS:3575 table
has mask four set. FND-COMBAT-114 records this helper's interface.
The result minus ASCII a must be at most 25 unsigned to enter the 26-word
CS dispatch table at 05A1. The bounded table read confirms k selects 00EB
and r selects 0168. There is no test of argument byte zero for a hyphen.

The k branch compares the same argument's suffix beginning at byte two
with the near string at DS:06BD, recorded as terminated ASCII 911 in
FND-SOUND-010 and FND-PARTY-116. 1000:3787 sets ES=DS, scans the second
string through its terminator, compares that many bytes and returns the
unsigned-byte difference at the first mismatch, or zero for equality.
For the fixed terminated key this tests exact suffix equality, not a
prefix or a separate following argv element. The zero-result branch sets
DS:143C to one. The mismatch branch leaves that byte unchanged.

The r branch first tests current DS:143C. Zero skips conversion and the
region store. Nonzero passes that argument's suffix at byte two to
1000:2E86, then stores returned AX at DS:140C. It does not test DX,
consumed length, a success flag, or any minimum/maximum region value.
The numeric helper skips currently classified whitespace, accepts one
optional sign and consumes decimal digits until the first nondigit.
It retains the low 32 bits and returns zero when no digits occur
(FND-RNG-018/COMBAT-115); this caller retains only the low 16 bits.
There is no local failure diagnostic for this r branch.

Both branches join the SI increment and next argument. Thus a key admitted
later in the vector does not retroactively enable an earlier region
argument, and another admitted region argument replaces the prior value.
The parser supplies no own clear of DS:143C after the key succeeds. Actual
vector/frame/classification/segment retention across calls remains a
precondition, not an inference from a host command-line convention.

For admitted, terminated ASCII suffixes and the recorded classification
bytes, these synthetic arithmetic consequences follow; none is a native run:

| Region suffix | Published word |
|---|---|
| 50 | 0032 hex |
| -1 | FFFF hex |
| 65536 | 0000 hex |
| 12tail | 000C hex |
| empty or no initial digit after whitespace/sign | 0000 hex |

For example, with the key initially clear, two argv elements -K911 then
-R50 admit the store; reversing them leaves the earlier r branch skipped.
The suffix is attached to each element. This does not assert how a shell
or the original runtime constructs those elements.

After the loop, 023F..02C9 performs callback registration and startup calls.
There is no own further region store in that prefix, but the called startup
bodies are not thereby proved to preserve DS:140C. This contract does not
establish the eventual loaded region or music selection for a host invocation.

## Interpretation

Q-SOUND-004's known command-line region-store component has no local
accepted-region bound: it publishes a truncated numeric result. Native
region-domain claims must account for actual argv admission, startup effects,
SAVE restoration and subsequent transition callers rather than treating the
parser's presence as validation. No whole-entry complete reading follows.

## Alternatives

A range-checked region selector or a parser that requires complete numeric
consumption is contradicted by the direct store after conversion. Requiring
a hyphen is unsupported by the byte-one dispatch. Actual malformed or short
argv storage, changed classification/key bytes, nonreturning diagnostics,
other aliases and later writers remain outside these conditional examples.

## How to reproduce

Use tools/research/exec-census/resident_listing.py at 42e5154f for
277B:0024..277B:023F, 277B:023F..277B:02C9 and
1000:3787..1000:37B6. Reports are music-region-command-parser.txt,
music-command-startup-forwarding.txt and music-command-key-compare.txt
under GAME_DIR/analysis/handler-census-20261106/.

Reuse tools/evidence/report.mjs table with music-command-dispatch-config.json:
source DSUN.EXE, the fingerprint above, start 0x1CF51, count 26, stride two,
one unsigned width-two field target at offset zero. Count evidence is the
unsigned comparison with 25 at 005F and CS table access at 0069. Output
music-command-dispatch-report.json retains all rows; k and r independently
match the known FND-SOUND-010/PARTY-116 branch targets. FND-RNG-018 and
FND-COMBAT-115 give the numeric-helper and classification reproduction.
No original game, emulator session or emulated call supplied this finding.
