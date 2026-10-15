---
id: FND-RNG-022
title: Request 27 narrows packed offset-loop arguments to signed bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
tool: tools/evidence FBOV bounded reader; tools/research direct_callers.py; report.mjs table
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF29..0x0008BF4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C487..0x0008C4F1
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C2AF..0x0008C2C2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DCC6..0x0008DD05
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E6F5..0x0007E70B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089F17..0x00089F30
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089FC8..0x00089FD8
---

## Observation

The adjacent dispatcher, exported through 5787:0066, starts at overlay
204 +0169. It accepts request numbers 1 through 53 and selects a word
target table. Request 27 selects +04EF and forwards its last two
doubleword arguments to the wrapper at +1F06, exported as 5787:0084.
The known request-1 target +02B1 supplies an independent table control.
FND-VIDEO-010 describes the script instruction 0x22 dispatcher route;
this finding does not census shipped script instances of request 27.

The wrapper divides its first signed doubleword argument by ten,
retaining the low byte of the quotient and remainder. It retains only
the low byte of its other argument. Each byte is sign-extended to a
word before calling the root in FND-RNG-021: quotient supplies magnitude,
remainder supplies mode, and the other byte supplies iteration count.
The 16-bit sign-extension instruction is interpreted in its actual
default operand size, rather than the misleading wider mnemonic in
one reader's presentation.

For example, packed value 14 supplies magnitude 1 and mode 4; value 4
supplies magnitude zero and mode 4. Packed value 1284 narrows its
quotient to magnitude -128. A count argument with low byte 128 becomes
-128, while 256 becomes zero. Both nonpositive counts still reach the
root's reseeding and setup calls. These are local width consequences,
not evidence that scripts admit those values.

The direct-call candidate search also recovers three qualified far
call instructions to the root. Overlay 193 +0413 passes magnitude 4,
mode 4 and count 4, conditional on DI equaling 293 in the read window.
Overlay 201 +075F and +0810 each pass magnitude 2, mode 4 and count 1.
The first is followed by an external result test that can repeat the
call. The second lies after a local SI-equals-7 and nonzero-byte guard.
Those windows prove the immediate argument words and calls; they do
not establish the complete upstream branch admission or what those
state values mean. The wrapper's near call is separately qualified
by its pushed caller segment and far return contract.

## Interpretation

Other indirect, aliased or noncanonical transfers remain outside this
direct-call query. No absence claim, complete caller reading, native
admission, pixel effect or rule status change is made. Clock-buffer
identity and the full seed-writer census remain Q-RNG-003; complete
input/service and storage contracts remain Q-RNG-004 and Q-RNG-005.

## Alternatives

Treating forwarded doublewords as unrestricted root arguments conflicts
with the wrapper's byte stores and sign extension. Whether shipped scripts
use only a smaller domain remains open and requires their own census.

## How to reproduce

Use the licensed DSUN.EXE with XXH3 e296af55ba2ecde7e77f555c90f33d0b
and project evidence Python. Resolve the two exported trampolines,
read the listed dispatcher and wrapper ranges, and use report.mjs table
at file 0x8C487 with count/limit 53, stride two and a word target field.
Qualify request 27's selected branch and the known request-1 control.
Run direct_callers.py for 204+0000 and 204+1F06, then inspect each
listed caller window rather than treating byte matches as calls.
Keep all original-derived reports under GAME_DIR/analysis/startup-seed/.
