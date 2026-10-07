---
id: FND-EXE-034
title: Negative-path field storage copies a payload and rereads its source length for publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D5030..0x006D5096
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601CB0..0x00601CB6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003595DA..0x003595E1
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE import-table reading
environment: null
---

## Observation

FND-EXE-032's negative path calls `0x006D5030` with payload minus twelve,
a local address and zero in its first three outgoing slots. This callee's
direct body reads exactly three original 32-bit arguments: a source-header
pointer, the second supplied word, and an extra count. It does not read
the caller's later outgoing register word in its own body. The meanings
of the local address and that later slot are not inferred from decompiler
parameter guesses.

It reads source offset zero as a count and adds the extra count at 32-bit
width without a local overflow guard. It calls `0x006D50A0` with this sum,
the source word at offset four, and the second original argument. This
storage helper's units, header size, success conditions and exceptional
effects remain unread. After normal return it saves the returned base,
forms base plus twelve as the eventual result, and rereads source offset
zero to select the following branch. There is no local result-null check.

A zero reread count skips the byte-copy call and writes zero to the returned
base's first 32-bit word. It then rereads source offset zero again and
writes a zero byte at returned base plus twelve plus that reread value.
Thus even this branch's terminator address uses another read, rather than
a permanently cached zero. It returns the previously formed base-plus-twelve
pointer after normal frame restoration.

A nonzero reread count calls the thunk at `0x00601CB0` with destination
base plus twelve, source header plus twelve and that count. The thunk jumps
through slot `0x024319DC`; bounded physical PE import reading identifies
memcpy from msvcrt.dll, with the cited shipped-file name including NUL.
After normal copy completion the caller rereads source offset zero and
writes it as the returned base's first word. It rereads source offset zero
again for the terminator position and writes a zero byte at base plus
twelve plus that value. It returns the saved base-plus-twelve pointer.
The imported copy return is not used as the publication value.

The direct body has no local source capacity, destination capacity, overlap,
null or terminator-range guard. Counts and pointer arithmetic are 32-bit.
The sequence does not locally freeze source length across storage, copy,
header publication and terminator positioning. Aliases and unread callee
mutations remain conditional; the destination header write itself could
affect a later source read when their storage aliases.

## Interpretation

Under valid storage, stable truthful source length and normal helper behavior,
this path copies the source payload, publishes its length and appends a zero
byte, returning the pointer after a twelve-byte prefix. Those conditions are
not a complete allocation or ownership contract. FND-EXE-030 uses a length
word twelve bytes before a payload; this sequence supplies one producer for
that word without establishing every producer or lifetime. Q-EXE-009 still
needs storage-helper effects, caller ranges, aliases and cleanup behavior.

## Alternatives

Reading a fourth original argument in this direct body, copying on the zero
branch, returning the raw base, caching one source count for all later writes,
or using memcpy's return as the result are ruled out by the bounded instructions.
An independent owned copy is consistent with stable non-aliased storage,
but cannot be established until allocation, inputs and release paths are read.
No reachable malformed-count or overlap failure is inferred from absent guards.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x006D5030` and
read 45 instructions from its entry, limiting claims to the cited body.
Read one instruction from `0x00601CB0` and independently map its PE import
slot through bounded raw sections and import lookup thunks. Follow
FND-EXE-032's first three argument writers into this body. Verify count
addition width, storage-call arguments, result-pointer adjustment, each
source-count reread, both copy branches, destination header publication,
terminator positioning and saved return. Keep unknown helper semantics,
allocation bounds, aliases and exceptional returns conditional. Keep rich
reports local and execute no interpreter or game.
