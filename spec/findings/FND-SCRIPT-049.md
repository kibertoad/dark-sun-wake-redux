---
id: FND-SCRIPT-049
title: GPL37's early stop reads combat state through system variable32 and its later branch uses a shared global flag
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4C13:0000..4C13:0014
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:239F..172C:242A
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: report.mjs table and far_pointer_words.py at a9a5d3d0; retained script_decode.py index from FND-SOUND-077; FND-SCRIPT-010, FND-PARTY-241 and FND-COMBAT-023 interface reuse
environment: null
---

## Observation

This assessment connects the two state tests before GPL/37's postamble
to their existing native bindings and known script producers. It does
not infer their live values from the resource or registration alone.

FND-SCRIPT-010's kind-nine reader admits variable numbers 20..2E
hexadecimal and indexes four-byte far pointers at 4C13:0000. It reads
numbers 29 and 2A as double words and the other admitted numbers as
sign-extended words. Thus decimal number 32 selects the first pointer
and takes the word-reading branch. FND-PARTY-241 already identifies
the first five table destinations as 4C10:0019, 001B, 001D, 001F
and 0021, respectively.

A fingerprinted bounded table read independently recovers offsets
19, 1B, 1D, 1F and 21 hexadecimal with raw segment 3C10 in each
row. The existing far-pointer reporter confirms MZ-relocated pointers
at file 41330, 41334 and 41338 for target 4C10:0019 and the two
coordinate controls 4C10:001B/001D. This distinguishes raw segment
words from loaded addresses. DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b.

FND-COMBAT-023 and the combat_state glossary identify 4C10:0019 as
the combat-state word. Its zero/nonzero use is evidenced by the save,
rest and leader-change guards; it is not the script-ready state at
4C0E:000B. FND-PARTY-087 supplies specific known native start/clear
stores and their limited operand-census scope. The script kind-nine
read does not create another combat-state variable.

Consequently, under this table and storage binding, GPL/37's test at
0A77 is a combat-state test. A nonzero word selects the early stop
at 0AF9 described by FND-SCRIPT-043. Zero continues to the global
flag-24 test and its direct-skip or dialogue paths. Sign extension
does not change this zero/nonzero distinction. This does not prove
that an actual TALK entry has zero combat state, or that callbacks
and other accesses preserve the pointer and state until that read.

### Global flag24 is shared across scripts

The retained decoded index supplies these top-level direct opcode16
assignments with literal destination global flag24:

| Site | Assigned value |
| --- | --- |
| GPL/5 at 05BC | 0 |
| GPL/17 at 0063 | 1 |
| GPL/17 at 007D | 0 |
| GPL/19 at 03B7 | 0 |
| GPL/21 at 07A0 | 0 |
| GPL/27 at 014D | 0 |
| GPL/37 at 0D1B | 1 |
| GPL/249 at 06CF | 1 |

Reads in this same literal query occur at GPL/17 0027, GPL/37 0AFB
and GPL/249 06C6. In particular GPL/37 is not the flag's only script
producer, and a later invocation cannot assume its own prior set is
retained. FND-SCRIPT-009 supplies the assignment's bit operation:
flag24 is bit zero of byte three of the global flag block. The global
initialization/clear and save/restore interfaces remain FND-SCRIPT-026
and RULE-SCRIPT-005; they are additional whole-block mechanisms.

These positive decoded sites are not an exhaustive writer exclusion.
Computed destinations, nested instructions not selected by this query,
other writing opcodes, native aliases, restoration and the GPL/37 decode
gap remain outside it. Their actual ordering is not inferred from the
order in the listing. Source identities and decode limits are those of
FND-SOUND-077.

## Interpretation

Q-SCRIPT-024 can use the named combat-state source rather than an
unidentified kind-nine input. What remains is its actual zero-state
admission and preservation, alongside the global flag's producer/restore
history and FND-SCRIPT-043's menu/continuation conditions. Neither a
fresh local frame nor this script's own earlier flag writes determine
that shared global history. No native malformed execution, queue closure
or whole-entry complete reading is established.

## Alternatives

Equating system variable32 with the script-ready state uses the wrong
pointer. Treating global flag24 as a dialogue-local flag ignores its
global addressing and other producers. Conversely, known pointer bytes
and positive writer sites do not establish complete live-state coverage.

## How to reproduce

Run report.mjs table with the installed DSUN.EXE identity above, start
41330 hexadecimal, count five, stride four, unsigned word fields at
zero and two. Retain gpl37-system-pointer-config.json and
gpl37-system-pointer-report.json under
GAME_DIR/analysis/handler-census-20261106/. Require the two coordinate
controls from FND-PARTY-241. Run far_pointer_words.py for 4C10:0019,
4C10:001B and 4C10:001D and retain
gpl37-system-pointer-relocations.txt; require the three exact relocated
table locations above. The report does not establish live immutability.

Reuse FND-SCRIPT-010's native reader and FND-COMBAT-023's state consumers.
In FND-SOUND-077's retained music-health-all-scripts.txt select the literal
varD[24] occurrences and read the assignments with their resource/site
context. Keep this query's exclusions above; reuse the setter and whole-
block mechanisms instead of counting only literal script stores. No
original game or emulated call was run, and no derived output is committed.
