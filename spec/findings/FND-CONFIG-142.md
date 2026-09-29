---
id: FND-CONFIG-142
title: A bounded literal-writer query finds the setup slot assignment and no verified metadata-table write
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:00D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:60EB
tool: Python 3.14.7 and Capstone 5.0.7 bounded operand-candidate search and entry-based instruction checks
environment: null
---

## Observation

FND-CONFIG-140 and FND-CONFIG-141 retain the slot-index
and selector-metadata producers. A literal operand-candidate
search covered the resident MZ load image at
`0x00005200..0x00057570` and the declared code-and-data
ranges of all 49 FBOV overlays. It selected first memory
operands with displacement 60EB or 60ED, default DS or
explicit DS, and the stated write-like mnemonics below.
It returned five candidate starts, all containing 60EB.

The verified store is `0x00073CA1`, assigning word 520
to DS:60EB in overlay 188 entry 5702:00D4. Its containing
entry, zero-gate condition and later calls are read in
FND-CONFIG-144. The assignment is skipped by the entry's
nonzero DS:193E branch; it is not an unconditional startup
invariant. The word at DS:60EB in the shipped resident load
image is zero at file offset `0x000530EB`.

Four other locally decodable candidates do not start at verified
instruction boundaries. Each begins one byte before a real word
read of DS:60EB:

| Rejected candidate start | Verified read start | Entry used for the boundary check |
|---|---|---|
| 0x00024EEB | 0x00024EEC | Resident file offset 0x00024E8C |
| 0x00025055 | 0x00025056 | Resident file offset 0x00024E8C |
| 0x000733FC | 0x000733FD | Overlay 188 file offset 0x00073398 |
| 0x0007360B | 0x0007360C | Overlay 188 file offset 0x0007350E |

The second entry-based block also confirms a metadata read
at `0x00025043`, DS:60ED indexed by BX. It is a read,
not a metadata producer. This query found no selected literal
metadata-table writer; it does not prove that the table is
immutable or that every writer was covered.

The already-recorded selector-zero metadata byte is zero in
the shipped image (FND-CONFIG-141). Neither that byte nor
the slot word's initial value is an observation of state when
the rest caller executes.

## Interpretation

The known initial slot zero and guarded setup assignment 520
both lie within FND-CONFIG-140's named ordinary slot range.
They are concrete producers consistent with that range, but
not a complete range invariant. The bounded writer query
adds no verified metadata mutation and no additional slot
assignment. Rejected overlapping decodes are not evidence
of arithmetic writes to the slot word.

## Alternatives

FND-CONFIG-150 and FND-CONFIG-151 subsequently identify an
offered metadata buffer and a block-transfer path. FND-CONFIG-152
traces its bounded interrupt request. Those uncovered write forms
supply new evidence without making this literal query exhaustive
or proving successful metadata population.

Q-CONFIG-008 retains indirect, block, aliased and explicit-other-
segment writes, differently encoded uses, setup invocation and
state timing. Those can change either field even when this
literal query finds no further instruction. One reading is that
the shipped metadata remains at its file values; another is that
unread writers replace it. This inventory does not distinguish
them. No flag-preservation or table-immutability status is raised.
The query must not be repeated without new coverage, a new tool
or a new reading that reaches those remaining write forms.

## How to reproduce

Use FMT-EXE-001 through FMT-EXE-005 to bound the resident
image and every overlay code-and-data range. Search raw
little-endian 60EB and 60ED operands, locally decode candidate
starts up to five preceding bytes, and select first memory operands
with the stated displacement and default/explicit DS for mov,
inc, dec, add, sub, and, or, xor, xchg or pop. Keep these
as candidates until the instruction boundary is checked.
Use the named containing entries for the four rejected starts
and FND-CONFIG-144's exported entry for the verified store.
Read only the stated initial slot word using DS segment 57E0
from FND-SCRIPT-005. Do not turn absent selected candidates
into an exhaustive negative claim or a runtime invariant.
