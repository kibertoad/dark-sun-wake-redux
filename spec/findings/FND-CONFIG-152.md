---
id: FND-CONFIG-152
title: The bounded metadata transfer reaches one operating-system request at the normalized buffer address
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:01AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:0002
tool: Python 3.14.7 and Capstone 5.0.7 complete local 16-bit entry readings; effective operand-size and declared MZ checks
environment: null
---

## Observation

FND-CONFIG-151's resource reader calls resident 44DE:01AF,
file offsets `0x0003A18F..0x0003A20C`, ending with
far return at `0x0003A20B`. It receives a handle word,
far destination and double-word count. It retains DS, BX and
CX, initializes a two-word accumulated count to zero, and
normalizes the destination through local 44DE:0002.

That complete leaf, `0x00039FE2..0x00039FF4`, retains
only the low four bits of the offset and adds the offset's
unsigned quotient by sixteen to the segment, with word
arithmetic. It has no callee and no stored-data write. For
pointer 57E0:60ED it returns the same linear address as
5DEE:000D. The transfer stores this normalized pointer in
its argument slots before issuing requests.

When the count's high word is nonzero, another path issues
32768-byte requests using a word loop counter formed by
doubling that high word. It advances the segment by 0800
after each carry-clear request and accumulates returned AX
with carry into its high accumulated word. The bounded
metadata case below does not enter that path; no general
iteration-count or large-buffer validity is claimed here.

For a positive count N at most 98, high word zero skips
the chunk loop. The helper sets CX to N, sets AH to 3F,
loads DS:DX from the normalized pointer, and invokes
INT 21h at `0x0003A1E8`, with BX holding the handle.
It restores DS after the interrupt. Carry clear adds returned
AX to the accumulated count and returns that count as DX:AX.
Carry set stores returned AX in DS:33BE and returns
DX:AX as FFFFFFFF. The word extension on that error path
uses the default 16-bit operation; the printed conversion
mnemonic must not be interpreted as a wider register operation.
Count zero skips the remaining request and returns zero.

The complete local body has no allocation, explicit capacity
check or additional callee beyond pointer normalization. Its
only external transfer actions are the INT 21h requests.
FND-CONFIG-151's caller compares the returned double word
with N and treats a mismatch as failure. Thus a short,
carry-clear return is accumulated rather than retried here,
and becomes a reader error in the bounded one-request case.

The call operand at `0x0002EB95` is a declared MZ
relocation mapping raw 34DE to 44DE. The DS mapping and
metadata buffer in the caller are identified by FND-SCRIPT-005
and FND-CONFIG-150. No operating-system call or original-game
function was executed for this reading.

## Interpretation

For a stable positive selected length N at most 98, the caller
requests a transfer at the same linear address as DS:60ED,
covering at most DS:60ED..614E if the operating system
supplies the requested bytes. This reaches a concrete block
producer beyond the literal-writer inventory, but successful
I/O and the bytes produced remain unobserved. The buffer is
not proven populated merely because its pointer is passed.

The finite local path for that count does not confirm an
operating-system result or recovery after a partial request.
It also does not validate arbitrary counts, pointer wrapping,
caller capacity or other invocations of the helper. A harness
without interrupt behavior cannot confirm these outcomes.

## Alternatives

Q-CONFIG-008 retains operating-system transfer outcomes,
selected resource bytes and lengths, archive state, failed-read
cleanup, later writes and reachable selector metadata. Carry
clear with N returned and a shorter or carry-set response are
separate possible continuations. The code decides their local
handling, not which response the operating system supplies.
No gameplay or unconditional successful-buffer claim follows.

## How to reproduce

Read the complete transfer body and normalization leaf at the
stated bounds. Verify the incoming argument layout, pointer
normalization, high-word loop bypass for N in one through 98,
CX and DS:DX at the single remaining interrupt, DS restoration,
carry branches and accumulated DX:AX return. Check the default
operand size of the error extension at `0x0003A1FC`.
Compare the caller's returned-count check and named relocation
in FND-CONFIG-151. Keep this static request separate from an
observed operating-system transfer; do not run the original.
