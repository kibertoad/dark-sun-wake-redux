---
id: FND-CONFIG-145
title: The setup traversal helpers queue indices and restore a saved special slot while consuming a pending count
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:051C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:0566
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:05DC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F5A
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit disassembly; declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-143's two traversal callees are resident
1AA0:051C (`0x0001011C..0x00010166`) and
1AA0:0566 (`0x00010166..0x000101DC`). The second
also calls local 1AA0:05DC
(`0x000101DC..0x0001023F`). All three bodies end with
far returns. Their near state pointers are dereferenced through
DS; the caller forms that pointer from a BP-relative local
address, whose ordinary BP accesses use SS. This reading does
not establish the caller's DS-to-SS relationship.

Entry 051C takes the state pointer. It initializes its return
index to 9999. When the unsigned state word at offset zero
is positive, it decrements that word and reads the index at
state offset two plus twice the decremented count. If the index
equals DS:60EB, it copies three bytes from state offset
0xC2 plus three times the decremented count to the slot at
4F49:0C33 plus three times that index. It then returns the
index. With zero count it makes no call, writes no stored
state and returns 9999. There is no local loop.

Entry 0566 takes the input index, state pointer, a mode word
and an output ordinal. It reads the current slot byte at
4F49:0C33 plus three times the input index. A signed-positive
mode uses starting selector position one. Otherwise it reads
a signed starting position from DS:5EEB plus 34 times
the slot byte. From that position down through zero it reads
a selector word at DS:5ECB plus 34 times the slot byte plus
twice the position. A zero selector is skipped. For a nonzero
selector it calls the complete shared getter 1AA0:0009
(FND-CONFIG-140), with the input index and selector. Returned
low word 9999 is skipped; every other low word is passed to
05DC with the state pointer, selector and output ordinal.
Each iteration decrements the signed position. A negative
starting position makes no iteration. The loop has no local
backedge that omits this decrement, but its selected addresses
and getter inputs have no bounds check here.

Entry 05DC takes the returned index, state pointer, selector
and ordinal. Before checking the count, it writes words at
state offsets 0x02, 0x42 and 0x82, each indexed by twice the
current count. The three words are the index, selector and
ordinal respectively. If the index equals DS:60EB, it copies
three bytes from that table slot into state offset 0xC2 plus
three times the current count. These saved bytes are the ones
051C later restores. It increments the state count only when
its unsigned value is below 31. Otherwise it writes word one
to DS:0423, leaving count unchanged. The count check follows
the indexed writes and optional copy; it is not a guard against
all writes for an arbitrary starting count.

Both copies call resident 1000:3F5A
(`0x0000915A..0x0000917E`). Its complete body loads the
first far pointer as destination and the second as source,
clears direction and copies count divided by two words followed
by a byte when the count is odd. It restores DS and returns
the destination pointer. With count three, the calls save or
restore exactly three forward bytes. It has no further call.
The pushed table segments at `0x00010155` and
`0x00010213`, table-load operand at `0x00010175`,
and copy-call segments at `0x0001015B` and
`0x00010228` are declared MZ relocation words. Raw
3F49 maps to 4F49 and raw zero maps to 1000.

The caller initializes its BP-relative state count to zero at
`0x000247B2`, passes the near pointer at
`0x000248C6..0x000248CC`, and later retries 051C
without repeating 0566 (FND-CONFIG-143). This is a
local call sequence, not yet proof that DS accesses and those
SS locals denote the same storage.

The five shipped starting-position words for slot bytes one
through five are all zero, at file offsets `0x00052F0D`,
`0x00052F2F`, `0x00052F51`, `0x00052F73` and
`0x00052F95`. These are initial image values, not observed
setup-time values or proof that no producer changes them.

## Interpretation

The rejection backedge in FND-CONFIG-143 consumes one
pending index per retry, then gets sentinel 9999 on an empty
count, provided its state storage is valid and the count is not
aliased by the table copy or other writes. Starting from zero,
05DC's increments keep that count at most 31 under the same
conditions. Thus the local retry is not inherently a repeated
read of one unchanged index. This conditional count argument
does not establish termination for the actual setup state.

The helper graph now has a bounded complete local reading:
0566 calls the getter in FND-CONFIG-140 and 05DC;
05DC and 051C call only the copy helper. The getter can
replace the special table slot, and these helpers save and
restore it around pending entries. They are not all read-only.
For DS:60EB in zero through 523, the three-byte slot copies
remain separate from 4F49:000A. Valid state buffers, selectors,
other table indices and segment relationships remain required;
no unconditional flag-preservation or gameplay claim follows.

## Alternatives

FND-CONFIG-150 subsequently identifies direct selector and starting-
position assignments in a post-setup initializer, and a metadata-buffer
argument to a resource call. Its unread callees and later states still
leave the reachable selector and metadata ranges unresolved.

FND-CONFIG-147 subsequently resolves the setup zero-gate call:
its slot remains zero and that call does not reach these helpers.
The conditions below still apply to other calls and later states.

Q-CONFIG-008 retains the near-pointer segment relationship,
non-aliasing and buffer validity, starting-position and selector
producers, actual setup-time slot contents, metadata and other
index ranges, setup invocation and subsequent changes. One
reading takes the named fields as a valid shared pending buffer,
which supports finite consumption; another permits malformed or
aliased inputs that invalidate that argument. The complete local
bodies rule out an unchanged-count retry for an ordinary positive
count, but do not establish which buffer state reaches them.
Initial zero starting positions do not distinguish an unchanged
shipped table from runtime population by unread producers.

## How to reproduce

Read the three complete helper bodies at the stated bounds and
follow the positive-count branch, both copy argument orders,
signed selector loop, shared getter call and writes preceding the
unsigned 31 check. Read the complete copy helper, checking its
carry-dependent odd-byte continuation. Verify the named MZ
relocations before applying load segment 1000. Compare the
caller initialization, near-pointer argument and rejection retry
in FND-CONFIG-143 and the getter's complete graph in
FND-CONFIG-140. Read only the five named initial words with
DS mapping 57E0 from FND-SCRIPT-005. Keep SS-relative and
DS-relative storage distinct until their relationship and input
ranges are evidenced; no original function is executed here.
