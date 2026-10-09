---
id: FND-EXE-502
title: Game diagnostic helpers map signed error words and clear a handle flag before positioning
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:06BA..1000:06F3
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:06BA..1000:06F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:07B0..1000:07D9
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:07B0..1000:07D9
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

In both game editions, FND-EXE-500's near error helper 06BA saves BP
and SI and reads the incoming word at SS:BP+4. It tests that word
signed. Nonnegative values through 0058 retain their value; larger values
become 0057. It stores that index at DS:37EE installed or DS:3762
on disc, reads the indexed byte at DS:37F0 or DS:3764, and
sign-extends the byte to the word held in SI.

Negative inputs are negated at word width. A signed result greater than
0030 joins the 0057 index and byte-mapping path. Every other result
stores FFFF in the index word and keeps the negated word in SI,
without reading the byte table. In particular, input 8000 negates to
8000 and its signed comparison takes this latter path. This is not
an absolute-value range check that rejects every magnitude above 48.

All paths store SI at DS:0094, put FFFF in AX, restore SI and
BP and near-return with two-byte incoming cleanup. There are no calls
or interrupts inside this helper. Actual DS, writable aliases, table
extent and byte producers remain unadmitted. The return word is fixed
independently of the stored mapped value.

The far helper 07B0 saves BP and reads its incoming handle word at
SS:BP+6. It doubles that word at word width and clears bit 0200
in the indexed word at DS:37C4 installed or DS:3738 on disc.
This store precedes the interrupt and is not rolled back locally on failure.
There is no local handle-bound check before the indexed read and write.

It requests interrupt 21 with AH 42, AL the low byte at SS:BP+12,
BX the full incoming handle, CX the word at SS:BP+10 and DX the
word at SS:BP+8. Carry clear goes directly to restoring BP and returning
far without incoming cleanup. It does not locally replace returned AX or DX.
Carry set pushes returned AX into near 06BA; that helper removes the
two-byte argument and returns AX FFFF. Opcode 99 then sign-extends
AX into DX, producing DX FFFF as well, before the same far return.
Native register and frame preservation are not established by these local
instructions.

FND-EXE-500's character routine supplies the sign-extended record byte
four as the handle, zero for both quantity words and mode two. It removes
eight argument bytes and ignores the returned pair. Therefore the caller
does not branch on positioning failure before continuing its byte-write path.
The ignored pair does not erase the earlier indexed-flag store or the error
helper's shared-state stores.

## Interpretation

This resolves two previously unread local helpers beneath the containing
diagnostic's output path. The selected request uses AH 42, not a
program-execution selector. Its local failure pair is FFFF:FFFF, while
the error helper's stored value depends on signed input and current table
bytes. Q-EXE-007 retains actual segment and state admission, table/record
writers, bounds, aliases and lifetime, native effects and preservation,
the flush helper, other counted-byte branches, surrounding callers and
whole-game launch-capability coverage. No complete output reading or
whole-game launch exclusion follows from these two helpers.

## Alternatives

Treating the positioning operation as a pure query ignores its flag clear
before the interrupt. Treating failure as leaving state unchanged ignores
that clear and the error-state stores. Treating the minimum signed word
as a positive magnitude ignores word-width negation and the signed
comparison. Treating the mapped byte as unsigned ignores opcode 98's
byte-to-word sign extension. Treating the fixed AX result as the mapped
error value confuses the return with the separate shared-state store.

## How to reproduce

At revision da42888 require both DSUN.EXE identities from FND-EXE-350.
Use Capstone 5.0.7 in sixteen-bit mode, header size 5200 and modeled
load segment 1000. Decode shipped 58BA..58F3 and 59B0..59D9 in
both editions at the corresponding 1000 addresses. Verify opcode 98
at 06D5 as byte-to-word sign extension and opcode 99 at 07D6
as word-to-pair sign extension. Track incoming frame offsets, the pre-request
indexed store, carry branches, near two-byte cleanup and far caller cleanup.
Compare signed cases 0000, 0058, 0059, FFD0, FFCF and 8000,
without executing the original. Keep edition-specific table offsets separate.
Licensed bytes stay outside Git; no game process, DOSBox or emulated call runs.
