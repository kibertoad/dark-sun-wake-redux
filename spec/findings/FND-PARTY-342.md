---
id: FND-PARTY-342
title: A divide error runs the C run time's signal catcher, which calls the game's floating-point signal handler; that handler restores the screen, prints Math Err and ends the program with exit code 1
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2AF9..1000:2BDB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2C56..1000:2E36
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2A07..1000:2A88
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:04A3..1000:04B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:023F..277B:026C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007464C..0x00074682
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067A47..0x00067A83
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4448:002C..4448:0048
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:06E1..2D40:06E6
tool: Retained original Python 3.14.7 / Capstone 5.0.9 / xxhash 4.0.1 survey; connected rereading with pinned Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9), resident_listing.py and overlay_listing.py at 035caf90
environment: null
---

## Observation

This replaces FND-PARTY-105 in full. Its original survey is retained from
848844230331aa79e3c3cb6e325f0c175642bee7. The connected rereading corrects
the signal-4 handler segment and the shared vector-setter site's value domain;
the fatal divide-error/reporting reading is retained. No native exception or
callback was executed for this replacement.

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The start-up code points interrupt 0 at `1000:01A7`, which writes `Divide error` to
handle 2 and ends the program with code 3 (FND-EXE-270, FND-EXE-551). The level drain can divide
by 0 with `idiv` (FND-PARTY-082).

**The game's handler.** The routine `277D:0004` makes three far calls of `1000:2C77` at
`277B:0247`, `277B:0257` and `277B:0267`, with 8 and `5702:00F2`, 4 and `2D40:400C`, and 11 and
`5702:00F7` (FND-EXE-567). `5702:00F2` enters overlay 188 `+17AC`.

**`1000:2C77`** takes a signal number and a far pointer. It finds the number's position among six
bytes at `DS:393D`, returning -1 when it has none, and stores the pointer at
`DS:3925 + 4 * position` (`+2C7F..+2CD9`). For 8 it calls `1000:04A3` with 0 and `1000:2AF9`, and
with 4 and `1000:2B6A` (`+2D1F..+2D42`, `+2D8D`). `1000:04A3` sets the interrupt vector numbered
by its first argument to the far pointer after it through interrupt `0x21` function `0x25`.

**`1000:2AF9`**, the interrupt handler, saves the registers, loads DS with `57E0` and reads the
far pointer at `DS:392D`, the entry of signal 8. When it is `0000:0001` it returns from the
interrupt. When it is 0 or `FFFF:FFFF` it calls `1000:03EE` with 1. Otherwise it stores 0 in the
entry and calls the pointer with 8, `0x7F` and the address of the saved registers, and then
returns from the interrupt (`+2AF9..+2B69`).

**Overlay 188 `+17AC`** calls overlay 180 `+0867` (trampoline `56B2:0034`) with `DS:1985`, the
text `Math Err` and a line feed, and then `2D40:06E1` with the word whose offset is its first
argument, which is 8 (`+17AC..+17C6`). `2D40:06E1` returns at once. Overlay 180 `+0867` tests the
word at `4E71:0001`, calls `1BF3:2973` with 3, twice and storing `0xFFFF` in the word between when
it is not `0xFFFF`, clears the byte at `DS:1462` and calls `4448:002C` with its argument
(`+086A..+08A2`), as its neighbour `+08A3` does with `4448:0002` (FND-PARTY-034). `4448:002C` calls
`1000:3603`, which writes the text through `1000:345B` to the stream at `DS:3692`, and then
`1000:03DF` with 1, which reaches the run time's cleanup and the DOS terminate request
(FND-CONFIG-224).

**Other vector setters.** A search of the file's bytes for `mov ax, 0x25nn` finds the
start-up request for vector 0 and the four that put back vectors 0, 4, 5 and 6 at exit
(FND-EXE-270, FND-EXE-551), and for `mov ah, 0x25` finds
`1000:04A3`, `44DE:046D` and `4AE5:015E`. `direct_callers.py` finds `1000:04A3` called near from
seven sites, whose selecting values are 0x23, 0x24, 0x24, 0x23, 0 (in `1000:2C77` for signal 8), 5 and, at the shared final site, 0x23, 4 or 6;
`44DE:046D` far from two sites, which pass 9; and `4AE5:015E` uses the vector at `[0x111]`, `0x3F`
(FND-EXE-562). It finds `1000:2C77` called only from the three sites above. The start-up setter
of vector 0 is the positive control for the byte search. A search for `xor r, r` or `mov ax, 0`
followed by a load of a segment register, and for `push 0` followed by `pop es` or `pop ds`,
finds no store to offsets 0 to 3 through a zero segment.

**Stored-pointer use.** The two header-relative pointers retained in
FND-EXE-567 are `5702:00F2` and `5702:00F7`, entering overlay 188
`+17AC` and `+17C7`. Registration first installs its own dispatch pointer
at `DS:A4CA/A4CC` once, before validating the requested code. The lookup
compares that code to six zero-extended bytes at `DS:393D`, returning the
first matching index or -1. Invalid registration sets `DS:0094` to 0x13
and returns `FFFF:FFFF`; it does not store the supplied callback. A valid
registration snapshots the old pair, copies the incoming pair into
`DS:3925 + 4 * index`, then performs any vector work and returns the old
pair in AX/DX. It does not dereference the incoming callback as data.
The selected branches do not test vector-setter success or undo that copy.
The shared setter at `1000:2D90` receives vector 4 from `+2D3F` or
vector 6 from `+2D8A` (and 0x23 from the signal-2 branch).

The generic raise routine `1000:2DA2` uses the same lookup and snapshots
the pair. An unknown code returns 1; `0000:0001` is ignored. For a
non-null, non-ignored pair it clears both table words before calling
the saved far pointer at `+2E29`, with the signal code and the
zero-extended byte at `DS:3943 + index`. A normally returning callback
leads to result 0. The null default path reaches termination/interrupt
handling; that path does not use an incoming header pointer as data.
Unlike the native wrappers, this generic path does not separately reject
`FFFF:FFFF`; this reading does not assume every stored pair callable.

The native interrupt-0 and interrupt-4 wrappers use `DS:392D/392F`,
clear the callback before dispatch and pass signal 8, subtype 0x7F or
0x7E, and the saved-register address. The interrupt-5 wrapper first tests
the interrupted instruction for opcode 0x62; otherwise it chains to the
saved native vector. Its selected callback path uses `DS:3931/3933`,
clears the pair and passes signal 11, subtype 0x0A and saved registers.
These are callback invocations, not loads of resident-header fields
through the saved pair. Actual table contents, reached vector behavior,
interrupt frames and asynchronous mutation remain separate admissions.

Both overlay-188 callbacks pass their respective message to overlay 180
`+0867`. Their encoded continuation, only if that reporter returns, reads
a word through DS at the first argument's offset and passes it to
`2D40:06E1`; that callee has an empty stack-framed body. This continuation
is not a read through the original stored trampoline pointer. The fatal
reporter behavior above must not be mistaken for an observed return.
Calling either trampoline still reaches overlay-manager machinery and
the reporter's effects: this classification does not prove absence of
transitive header writes or settle arbitrary copied/aliased pointers.

## Interpretation

Once `277D:0004` has run, a divide error ends the game: the screen is put back by `1BF3:2973`,
`Math Err` is printed, and the program exits with code 1 through the run time's normal exit,
which runs its cleanup. The run time's own `Divide error` handler of start-up is replaced and
does not run. The handler does not return to the faulting instruction.

## Alternatives

- A vector number computed at run time, a vector table write through a segment register loaded
  from memory, and a call of `1000:2C77` through a pointer are outside the searches.
- What `1BF3:2973` does with 3 was not read here; FND-CONFIG-158 and FND-CONFIG-159 read it.

## How to reproduce

For the retained original survey, use revision 848844230331aa79e3c3cb6e325f0c175642bee7. From that revision, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `resident_listing.py <dsun>
1000:2AF9..1000:2B6A 1000:2C56..1000:2DA2 1000:01A7..1000:0220 1000:03EE..1000:0420
4448:002C..4448:0048 2D40:06E1..2D40:06E6 1000:3603..1000:3640 4AE5:0140..4AE5:0193`;
`overlay_listing.py <dsun> 188 7464C 74682` and `180 67A47 67A83`; `trampoline_target.py <dsun>
5702:00F2`; `direct_callers.py <dsun> 1000:04A3 44DE:046D 4AE5:0140 1000:2C77`, reading the pushes
before each call; and search the file for the bytes `B8 ?? 25` and `B4 25`, and for the zero
segment patterns above, decoding the instructions after each hit.

For the connected rereading, use the scripts at 035caf90 and the pinned
evidence Python. Run `resident_listing.py <dsun>` with ranges
`1000:2C56..1000:2E36`, `277B:023F..277B:0272`,
`1000:2A07..1000:2A88`, `1000:2AF9..1000:2BDB` and
`2D40:06E1..2D40:06E6`; run `overlay_listing.py <dsun> 188 7464C 74682`.
The readers verify the licensed input fingerprint before decoding. Follow
both incoming edges to the shared setter and distinguish copying a far
pointer from dereferencing it as header data. Preserve FND-EXE-567's
computed-pointer exclusions; this is not a replacement whole-file census.
