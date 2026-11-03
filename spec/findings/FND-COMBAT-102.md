---
id: FND-COMBAT-102
title: The named context setter stores its argument before its state gate and its known callers use different argument sources
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:000B..1695:010F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069B4B..0x00069D75
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070F62..0x0007118B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00086D3B..0x00086D96
tool: tools/research/exec-census/segment_offset_refs.py, direct_callers.py, resident_listing.py and overlay_listing.py at b74f84b0; locked evidence Python and Capstone 5.0.9
environment: null
---

## Observation

FND-COMBAT-100 identifies seven literal-sentinel instructions whose selector
is kind-9 number 36. FND-PARTY-241 binds its initial named pointer to the
word at 4C10:0021, and FND-SCRIPT-028 identifies its setter. This reading
checks the setter's exact store ordering and its two known direct callers;
it does not establish all writes or admit the selector's runtime values.

### The script-state gate does not guard the context-word store

Starting at the established resident entry 1695:000B, the function loads its
word argument from the stack into SI, establishes ES as 4C10, and writes SI
to ES:0021 at 1695:0017. Only afterward does it load the state segment and
compare 4C0E:000B with 2, at 1695:0021. The equality branch enters the
trigger/script work of FND-SCRIPT-028; inequality jumps to the later exit.
Thus even a call made while the script state differs from 2 has already
written the context word. This does not establish that such a call occurs.

### The two direct caller sites supply different sources

The fixup-qualified direct-call candidate scan returns the two sites already
named in FND-SCRIPT-029. Reading their containing routines confirms them:

| Caller | Producer immediately before the call |
|---|---|
| Overlay 182 +142E, file 0x00069C7E | Pushes the current word DS:140C at +142A, when its local byte gate allows this call. |
| Overlay 187 +1306, file 0x00071136 | Pushes its incoming stack word at BP+6 at +1303; it does not reread DS:140C there. |

The latter routine is the +1132 region-change continuation of
FND-COMBAT-044. Earlier it reads that argument at +1213, writes it to
DS:140C at +1216 and calls the numbered reader with it at +121B. The later
setter call follows the read, postprocessing and setup calls. In particular,
the setup call at +12D0 and the local call at +12DD precede the +1303 push.
The direct argument read establishes the producer at this instruction, not
preservation of the stack or globals through all those children.

FND-SAVE-015 independently identifies SAVE resource 18's initial destination
as DS:140C and resource 45's as 4C10:0021. FND-COMBAT-035 supplies the
reader's zero-selector 61-source bound, nonzero 17-source bound and ascending
requests. Under those initial destination bindings, reaching both of these
requests is therefore a zero-selector possibility, with resource 18 requested
before 45. Child effects, replacement destinations, selected resource lengths,
successful completion and preservation remain conditions. These distinct
restore destinations cannot be replaced by an assumed permanent alias.

### The displacement candidates are not an exhaustive writer census

The immediate-segment/six-instruction candidate pass finds the known setter
store and three starts in overlay 197's routine at file 0x00086D3B. Reading
that routine from its entry confirms that the latter matches are comparisons
of the context word, including further comparisons reusing ES; they do not
write it. The setter is the independently known write control, and the
comparison routine checks that a displacement match need not be a writer.

This pass explicitly excludes computed offsets, indirect/bulk destinations,
other segment-formation methods and references beyond its six-instruction
window. It does not search for named-pointer-table replacement or prove that
no other instruction can write the context word. The saved-resource route
above is one reason its lone store candidate cannot be called the only writer.

## Interpretation

The known setter and its direct caller arguments now have a bounded source
and ordering description. The state-two check protects later script work,
not the preceding context assignment. One known caller samples current
DS:140C; the other rereads its original stack argument after intervening work.
Neither observation establishes that the two words are always equal when
one of the seven script instructions executes, or that either word is zero
on an admitted path.

The remaining named-context value, pointer and lifetime proof is independent
of the four local-number selector proofs. Q-COMBAT-058 holds that remaining
contract; Q-COMBAT-056 keeps local-number assignment coverage and lifetime.
No gameplay claim or evidence status is promoted by this reading.

## Alternatives

- A permanent equality between the context word and DS:140C is not proved
  by region terminology or the gate caller. The other caller's argument
  source, intervening calls and two distinct save destinations require
  their own preservation and admission evidence.
- A zero-context execution is also not proved. Distinct storage and a
  setter that accepts a word establish neither a zero argument source nor
  admission of the relevant callback/script while that value is live.
- The displacement pass's single write candidate cannot exclude indirect
  stores, bulk restoration or pointer replacement. No universal writer
  absence or complete-reading declaration follows from this candidate list.

## How to reproduce

Use the supported installed DSUN.EXE, size 634416 and XXH3
e296af55ba2ecde7e77f555c90f33d0b, with the locked evidence Python.
Run segment_offset_refs.py SOURCE 4C10 21 and direct_callers.py SOURCE
1695:000B. Read resident_listing.py SOURCE 1695:000B..1695:010F.
Read overlay_listing.py from the established entries, with descriptor and
half-open file ranges: 182 69B4B 69D75, 187 70F62 7118B, and
197 86D3B 86D96. Check segment formation, instruction boundaries and
argument pushes, rather than treating each raw candidate as a writer.
Reuse FND-SAVE-015 and FND-COMBAT-035 for the conditional save destinations
and loop; do not infer successful original-game transfers from this reading.

Reports remain in GAME_DIR/analysis/reader-context-20261103/:
context-displacement.txt, context-callers.txt, setter-complete.txt and
region-change-complete.txt, with gate-complete.txt for the gate caller.
context-comparisons.txt includes the checked comparison routine; reproduce
that routine from its entry as above. Earlier short slices include undecoded
prefixes and are not substitutes for the containing-routine readings.
No original bytes, listings or save resources are committed.
