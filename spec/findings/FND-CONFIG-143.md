---
id: FND-CONFIG-143
title: The setup traversal callee writes bounded output records but leaves helper effects and retry termination open
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2196
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit disassembly; MZ relocation and declared FBOV fixup mapping
environment: null
---

## Observation

Resident entry 2D40:2196 occupies file offsets
`0x00024796..0x00024A10`, ending with a far return at
`0x00024A0F`. It reads an input index at BP+6 and a far
output pointer at BP+8. FND-CONFIG-144 resolves the setup
call's inputs to index 523 and pointer 5072:0040; its earlier
helpers can change the selected slot before this call.

The routine initializes a signed output count to zero and retains
the input index in CX and SI. Its outer check at `0x000249C5`
finishes when unsigned CX is at least 9999 or signed count is
at least 79. Otherwise it reads the byte at
4F49:0C33 + three times CX. A zero byte or unsigned byte
above five writes CX to output offset six, resets count to zero
and enters the common finish path, without calling either
traversal helper.

For byte values one through five, it chooses an output kind:
one for the first accepted slot; otherwise four when the byte
at DS:60ED + SI is eight; otherwise three when CX equals
the word DS:60EB; otherwise two. The first-slot test uses a
local word initially zero. For kind three that local word retains
its prior value; other kinds replace it with the selected slot byte.

The output position is the supplied far pointer plus ten times
count. Before traversal calls, it writes these fields:

| Record offset | Width | Written value |
|---|---|---|
| 0 | Byte | Selected kind |
| 1 | Byte | Low byte of another local word, initially zero |
| 2 | Word | Current slot byte, zero-extended |
| 4 | Word | Paired word at 4F49:0C34 + three times CX |
| 6 | Word | SI |
| 8 | Word | DS:6167 indexed by twice the current slot byte |

At `0x000248CC` it calls 1AA0:0566, passing the current
index, a near pointer to local state, zero and the current output
ordinal. At `0x000248D9` it calls 1AA0:051C with that local
state pointer and replaces CX with returned AX. Neither helper
is read here. Their arguments, local-state writes, further calls
and effects cannot be inferred from this caller alone.

If returned CX is unsigned below 9999 and a metadata byte
selected using a local-state word is eight, the caller writes
CX into the paired table word for the slot named by DS:60EB
at `0x00024908`. It then sets CX to DS:60EB and writes the
low byte of its retained local type word into that slot's byte
at `0x0002491E`. These are direct conditional table writes,
not evidence that the routine is read-only.

The continuation accepts CX equal to 9999, or an initial output
word at offset two equal to five. Otherwise it rejects a paired
slot word equal to 9999 or negative, or a slot byte zero, six,
four or above five, by repeating the 1AA0:051C call. This
retry does not advance the output count. An accepted result
increments count at `0x000249A3`, updates the auxiliary local
word and SI from local-state arrays, and returns to the outer check.

The finish path resets count to zero if CX is unsigned below
9999 and its selected slot byte is above five. It writes byte
FF at output plus ten times count (`0x000249FC`), then the
low count byte at output base plus one (`0x00024A06`), and
returns the count in AX. The final count byte overwrites the
first record's earlier offset-one byte when records were emitted.
No gameplay identity is assigned to this output layout.

The relevant table segment operands, including those at
`0x000247C1`, `0x00024904`, `0x00024917` and
`0x000249E0`, are declared MZ relocation words: raw 3F49
maps to 4F49. The call segment operands at `0x000248CF`
and `0x000248DC` are declared MZ relocations: raw 0AA0
maps to 1AA0. DS-relative fields use mapped DS segment
57E0 from FND-SCRIPT-005.

## Interpretation

If slot 523's byte is still zero at the setup call, the complete
local path writes word 523 at output offset six, bytes FF and
zero at output offsets zero and one, and returns zero without
calling either traversal helper or writing the three-byte table.
The earlier clear in FND-CONFIG-144 does not prove that
slot 523 remains zero after its intervening helpers.

The outer ordinal bound alone does not prove termination:
the rejection path can repeat a helper without incrementing it.
The direct output writes for the named setup pointer do not
alias the flag at 4F49:000A. Other paths include table writes
and unread calls; this does not establish flag preservation,
valid indices, an adequate output buffer or reachable termination.

## Alternatives

FND-CONFIG-147 subsequently resolves the zero-slot condition for
FND-CONFIG-144's zero-gate setup call. That selected path does
not invoke either traversal helper; later and other inputs remain open.

FND-CONFIG-145 subsequently reads the traversal helpers and
conditional count-consumption argument. Valid shared state,
non-aliasing and the near-pointer segment relationship remain open.
Q-CONFIG-008 retains local-state
producers, metadata and slot ranges, setup-time contents,
incoming setup paths and later table changes. One possible
setup state takes the zero-slot path; another reaches traversal
after an earlier helper changes the slot. A helper may eventually
yield an accepted result or sentinel, but the local count check
does not settle repeated rejection. No status of a behavioral
rule is raised by this partial transitive reading.

## How to reproduce

Read the complete entry from `0x00024796` through its far
return, following every local branch. Check BP+6 and the
far-pointer loads from BP+8, the outer bound, invalid-slot
path, six record writes, two calls, conditional table writes,
rejection backedge and terminal writes. Confirm instruction
boundaries from the entry before interpreting narrower windows.
Verify the named MZ relocation memberships and apply load
segment 1000. Compare the declared pushed segment fixup
and setup call in FND-CONFIG-144. Use FND-CONFIG-145 for
the subsequent helper reading; do not execute the original or
infer helper effects from the caller's local buffers.
