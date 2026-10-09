---
id: FND-EXE-377
title: Sound utility pathname continuation matches bytes before unchecked position adjustments
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:008F..158E:011A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:0043..158E:0048
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1BD4:02AA..1BD4:0318
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0EC9..1000:0EF5
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-360's nonzero pathname-interface result continues at 158E:008F.
It passes the retained record pair and current DS:05D2 to far-returning
1BD4:02AA, removes eight argument bytes and tests full AX. Zero takes
the already recorded AX-zero common exit; nonzero continues. The pattern
identity and its admitted termination remain conditional on actual DS.

The matcher 02AA snapshots the incoming pattern far pair in a four-byte
frame and saves SI. A zero byte at the current pattern pointer returns one.
Otherwise it calls 1000:2F81 with the incoming record pair, removes four
bytes and holds full returned AX in SI. FFFF returns zero. Every other
word is passed to 0EC9; its result is held on the stack. The current pattern
byte is separately sign-extended and passed to the same mapper. It compares
the two mapped words. Equality increments the local pattern offset only;
inequality reloads the initial pattern pair. It then repeats the pattern
terminator check. It has no independent iteration cap, source/pattern extent
check or segment adjustment for offset wrap. A mismatch does not recompare
the consumed value against the restarted pattern's first byte.

The mapper 0EC9 returns FFFF for an incoming full word FFFF. Otherwise
it takes the incoming low byte as an unsigned index and tests bit 08 in
current DS's byte table at displacement DA9B. A clear bit returns that
unsigned byte; a set bit returns the byte minus 32 at word width. There
is no additional value-range check before subtraction. It restores BP and
returns far without incoming cleanup, and locally preserves SI but changes
BX/DX. The table writers, actual DS and semantic character mapping remain
open; this reading assigns no language or case-conversion contract.

After a nonzero matcher result, the pathname caller passes the record pair
to 2D48 and saves returned DX/AX in SS:BP-34/-36 without testing it.
It then calls 2CC8 with the same pair, that saved quantity and mode zero,
removes ten bytes and ignores the result. FND-EXE-376 reads 2CC8's
local adjustment, stores and full-pair result check. The caller decrements
its saved quantity by one with borrow, then calls 2F81 and stores AL
in SS:BP-0A. It does not test the full returned word here.

The frame word BP-38 was initialized to one at 158E:0043. If its current
value is nonzero, this continuation clears it and replaces the stored byte
with zero. Thus under admitted noninterfering frame conditions the first
read byte is suppressed before the delimiter tests. Later stored bytes
3A, FF or 0A end the local loop; any other byte repeats from 158E:00BB,
including another unchecked 2CC8 call and saved-quantity decrement.
There is no local independent count limit. FF or 0A select the AX-zero
common exit. Byte 3A continues at 158E:011A, outside this reading.

## Interpretation

This follows the actual stack-path caller beyond its interface result into
byte matching and position adjustment. Full-word matcher failure, byte-width
delimiter handling, unchecked position results and the suppressed first byte
are separate decisions. The matcher restarts its pattern on a consumed
mismatch; it is not a general overlapping-substring search contract.

Q-EXE-007 retains 2D48 and 2F81's complete contracts and producers,
later continuation from 011A, pattern/table identity and writers, actual DS,
frame and record admission, source extents, aliases and lifetime/re-entry.
No execution exclusion or complete reading follows from this bounded path.

## Alternatives

Treating every pathname use as execution ignores this local byte consumer.
Treating a nonzero matcher result as admitted data assumes the unread record
reader and pattern storage. Treating every FF delimiter as a full FFFF
result ignores AL-only storage. Treating the first observed byte as immediately
tested ignores its conditional replacement. Treating mismatch restart as
overlap-aware ignores the lack of a repeated comparison of the consumed byte.

## How to reproduce

At revision 6222cd5 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00006D6F..0x00006DFA at IP 008F and
0x00006D23..0x00006D28 at IP 0043, modeled CS 158E;
0x0000D3EA..0x0000D458 at IP 02AA, modeled CS 1BD4;
0x000022C9..0x000022F5 at IP 0EC9, modeled CS 1000.
Use MZ header size 1400 and FND-EXE-360's native call bindings.
Track pattern and record argument order, local pointer restart, full-word
and byte-width tests, mapper table addressing, saved quantity and marker,
loop re-entry and ignored results. The extension at 1BD4:02E7 is
sixteen-bit AL sign extension despite a wider decoder mnemonic. Original
bytes stay outside Git; no original process, DOSBox or emulated call runs.
