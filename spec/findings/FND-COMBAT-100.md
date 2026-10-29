---
id: FND-COMBAT-100
title: Decoded literal region-change selectors are nonzero while eleven variable-selector sites remain
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: tools/research/exec-census/script_decode.py at 33d4aa51; SHA256 97ade9fa2f64f8d016bc102804e03bd39e2ca81726073b07bed54a1f1c2a3a44
environment: null
---

## Observation

The retained decode named by FND-UI-051 was absent on this machine.
This reading regenerates it with the unchanged script_decode.py and the
same supported GPLDATA.GFF and OBJEX.GFF inputs. The sweep reports
350 resources, 349 decoded without a stop, 26,003 literal local code
targets and 797 literal script-call targets checked, with no target
outside an instruction start. GPL/37 retains FND-SCRIPT-024's stop
at 0x0D39 on expression byte B3, resuming at literal target 0x0D58.
The gap is not decoded instruction coverage or proof of unreachability.

Among the decoded instruction starts, 912 have opcode 0x5E. Of these,
218 have the exact literal first operand 32766, reproducing FND-UI-051's
positive control. Another 588 have a different simple integer first
operand, and 106 have a first operand that is not a simple integer.
These are decoded expression categories, not execution counts.

Of the 218 literal-sentinel instructions, 207 also have a simple integer
second operand. Their minimum is 1 and maximum 69; none has a zero
low word. The other eleven second operands are:

| Script | Instruction offset | Selector expression |
|---|---|---|
| GPL/4 | 0x02A3 | local number 11 |
| GPL/9 | 0x1675 | kind-9 variable 36 |
| GPL/15 | 0x0496, 0x04C9 | kind-9 variable 36 |
| GPL/16 | 0x014D, 0x0B81 | kind-9 variable 36 |
| GPL/18 | 0x0772 | kind-9 variable 36 |
| GPL/119 | 0x0547 | local number 0 |
| GPL/137 | 0x1069 | local number 5 |
| GPL/163 | 0x027A | local number 0 |
| GPL/300 | 0x0411 | kind-9 variable 36 |

RULE-SCRIPT-004 and FND-SCRIPT-010 describe local-number reads and
kind-9 reads. FND-PARTY-241 binds kind-9 number 36 (0x24) to the
word at 4C10:0021 through the initial named-pointer table.
FND-SCRIPT-028 records 1695:000B storing its region argument there.
That word is distinct from the current-region word DS:140C used by
the equality gate in FND-COMBAT-099. Equality of their actual values
is not established by the shared region terminology.

## Interpretation

Combined with FND-COMBAT-099, the 207 literal-selector sites cannot
select the reader's zero-selector 61-source branch by their own decoded
arguments. The eleven remaining literal-sentinel sites reduce the
selector-producer question to four local-number sites and seven reads
of the named context word, subject to actual script admission and
preservation of their parameter values. No statement here determines
whether one of these sites executes with selector zero.

The 106 nonliteral first operands remain a separate obligation: the
literal-sentinel census does not decide whether their runtime value
becomes 32766, or what selector accompanies it. The undecoded gap and
independent executable incoming routes also remain visible.

## Alternatives

- A variable selector or a nonliteral first operand may admit a route the
  simple integer groups do not. Q-COMBAT-056 and Q-COMBAT-057 retain
  these separate producer questions; source presence does not prove execution.
- The named context pointer could be replaced or its target written by
  another route. Existing local producer evidence does not settle its full
  lifetime or equality with DS:140C.
- The decoder reads in file order, resumes after its known gap and uses
  OBJEX.GFF's FNFO/2 field types as FND-SCRIPT-024 describes. Its target
  controls do not prove arbitrary computed transfers or runtime type state.
  Q-COMBAT-055 retains executable admission and preparation obligations.

## How to reproduce

Use locked evidence Python with GPLDATA.GFF, size 2191945, XXH3
2bd1d2b989eef2dcb0545feb95975fca, and OBJEX.GFF, size 6816516,
XXH3 e08d3cac153c547f6804ca27b155915d. Run script_decode.py with
these two paths and --sweep, then --all. Retain output only under
GAME_DIR/analysis/reader-admission-20261029/, not in Git. Select
complete output lines matching a resource name, hexadecimal instruction
offset and opcode 5E. Classify the first operand as a simple signed
decimal integer only when it ends at the first operand separator;
repeat that test on the second operand of exact first operand 32766.
Count the groups and compare the eleven variable sites above. Check
the positive control of 218 and retain the GPL/37 stop explicitly.
The committed decoder is the regeneration procedure; no decoded script
listing or original bytes are committed.
