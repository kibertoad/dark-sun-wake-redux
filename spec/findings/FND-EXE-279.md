---
id: FND-EXE-279
title: Updater far wrapper returns the interrupt bound after recording an error
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:298E..1000:29AA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:06BA..1000:06F3
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The far wrapper establishes BP, sets AH to 0x4A, loads BX from
SS-relative BP plus eight and ES from BP plus six, then reaches INT 21h.
A carry-clear continuation sets AX to 0xFFFF and takes the far-return
suffix. A carry-set continuation pushes the post-interrupt BX and AX,
in that order, then near-calls 1000:06BA. After the helper returns,
it pops AX before restoring BP and far-returning. The wrapper itself
removes no incoming argument bytes and explicitly saves neither DS nor SI.

The near helper establishes BP, saves SI and reads the argument word at
SS-relative BP plus four into SI. A nonnegative signed value through
0x58 is stored to DS-relative word 0x37EE and used to index the byte
table at DS-relative 0x37F0. Values above 0x58 are replaced with 0x57
before those accesses. The fetched byte is sign-extended from AL to AX
at sixteen-bit operand width and copied to SI.

A negative argument is negated at word width. If the resulting signed
word is greater than 0x30 it reaches the same 0x57 replacement and
table path. Otherwise the helper writes 0xFFFF to word 0x37EE and
retains that negated word in SI. The minimum signed word remains
negative after word-width negation and is not excluded by this signed
greater-than comparison. All paths write SI to DS-relative word 0x94,
set AX to 0xFFFF, restore SI and BP and near-return removing two
argument bytes. Table contents and initialized extent are unread.

The helper's two-byte cleanup removes the saved post-interrupt AX
argument. Consequently the wrapper's subsequent pop into AX consumes
the saved post-interrupt BX, replacing the helper's all-ones return.
The helper's ordinary paths preserve incoming DS and SI. Preservation
across the interrupt itself is not shown by these bodies.

## Interpretation

FND-EXE-278 supplies the wrapper's segment and count arguments via two
words and a pushed CS. Its two post-call pops match the wrapper's
ordinary caller-cleaned far return. Carry-clear reaches the updater's
all-ones success arm. Carry-set records error state and ordinarily
returns the post-interrupt BX; the updater then tests that word against
the same sentinel before publishing either cache/request state or a
new bound. The helper's return alone does not determine that branch.

Q-EXE-001 and Q-EXE-010 retain interrupt behavior, admitted post-interrupt
results, DS/SI preservation across it, table/state writers, physical aliases
and initialized extent. This reading does not establish that the interrupt
returns, succeeds, leaves storage unchanged on failure or supplies a usable
bound. No complete reading or native execution is claimed.

## Alternatives

Treating the helper's AX as the wrapper's failure result ignores the pop
that replaces it. Treating carry-clear as returning the interrupt's AX
ignores the literal all-ones publication. Treating the negative error
path as bounded to positive magnitudes through 0x30 overlooks the minimum
signed word. Ordinary helper preservation does not prove interrupt
preservation or absence of asynchronous changes.

## How to reproduce

At revision 732aafc, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. With the
locked Python interpreter, xxhash 4.0.1 and Capstone 5.0.7 in sixteen-bit
x86 mode, decode shipped half-open ranges 0x00007B8E..0x00007BAA
and 0x000058BA..0x000058F3 at initial IPs 0x298E and 0x06BA.
The MZ header is 0x5200 and model load segment 0x1000. Follow both carry
continuations, signed argument gates, effective byte sign extension and
every push/pop through the helper's two-byte cleanup. Cross-check
FND-EXE-278's outgoing frame and result test. Keep the interrupt and
table contents unresolved. No caller-completeness claim is made; source
bytes and analysis output stay outside Git.
