---
id: FND-EXE-601
title: Ghidra leaves call targets inside an instruction out of DSUN.EXE function bodies; with those bytes filled and one more overlay 204 table jump declared, every walk instruction that can execute lies in a row, and nothing enters overlay 191 +0B0A
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0A0F..4842:0A23
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0871..15F3:087E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4420:0200..4420:020D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2E48..1000:2E5A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388..1000:03DF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:019E..1000:01A7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E49F..0x0008E500
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E5C5..0x0008E5D7
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007CCFA..0x0007CD7C
tool: Ghidra 12.1.3 with the scripts and order of FND-EXE-597, tools/ghidra/FillInstructionBodies.java, tools/ghidra/dsun-declared-entries.json and dsun-declared-flows.json at the commit that adds this finding; tools/evidence/join-overlay-views.mjs; tools/research/exec-census/resident_listing.py, overlay_listing.py, direct_callers.py, far_pointer_words.py and walk_inventory_coverage.py; standard-checker standard-coverage; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

The inventory checked is the one of FND-EXE-599 (snapshots `INST-single-20261021q` and
`INST-overlay-20261021q`). FND-EXE-597 left in it 14 walk instructions outside every range and 17
partly outside.

**Bytes left out of bodies.** In both views Ghidra 12.1.3 computes a function's body without the
target of a call, also where that target is a byte inside an instruction the body holds; the
instruction's other bytes stay in the body. Twenty such bytes are left out of rows:

| Byte | Row | Reference that names it |
| --- | --- | --- |
| `4842:050F`, `0562`, `0592`, `05BA`, `06C5`, `0710`, `0888`, `08A7`, `08CE`, `0987`, `09C8`, `0ABA`, `0C20` | the row holding the `or bh, 0` before each | the near call after `push cs` that enters the one-byte `iret` routine at the byte (FND-EXE-594) |
| `4842:0A19` | `4842:0A0F` | the near call at `4842:0A1C`, after `push cs`, into the `CF` of the `or bh, 0` at `4842:0A18`: a sixteenth routine of that form, which FND-RNG-058's walk does not reach |
| `15F3:0878` | `15F3:0851` | the near call at `15F3:0873`, whose return address `15F3:0876` names the bytes `0D 0A`; its target is the `pop dx` after them, and Ghidra decodes `15F3:0876..15F3:0879` as one `or ax, 0x5A0A` |
| `25AF:075E` | `25AF:0742` | a computed reference from the near `call word ptr [di + 0x61B8]` at `1BF3:6199`, outside that call's `CS` |
| `3F96:07AB` | `3F96:0755` | a computed reference from the near `call bx` at `45CD:00BB`, outside that call's `CS` |
| `455B:0230` | `455B:0001` | a computed reference from the near `call word ptr [0x82]` at `4AE5:01AE`, outside that call's `CS` |
| overlay 173 `+1BB3` (`0x5C4D3`) | `0x0005C3ED` | in the overlay view, computed references from the far calls through `DS:0E1C` at `4842:13FB` and `4842:14EA`; the four bytes the file holds there are the text `c\ga`, read as a far pointer |
| overlay 199 `+0D0F` (`0x8860F`) | `0x00088521` | in the overlay view, a far call that Ghidra decoded at mapped `7F7B:378C`, past the 13,706 bytes of overlay 189's code, in no function |

`tools/ghidra/FillInstructionBodies.java` adds to each body the bytes of the instructions that
start in it and that no other function holds. Run on the resident view after the second
`CorrectNearTransfers.java` and on the overlay view after its second `CorrectNearTransfers.java`,
before `ClipBodiesToEntryRegion.java`, it adds one byte to each of 18 resident rows and 2 overlay
rows of the exported views, the bytes above.

**`4420:020B`.** At `4420:0204` the `jb` skips to `4420:027A` when the word at `0x9C` is below
`SP`; otherwise `4420:0206` far-calls `1000:2E48`, and the `jmp` at `4420:020B` follows the call.
`1000:2E48` writes the message at `CS:2E36` through `INT 21h` function 9 and jumps to `1000:03EE`,
which calls `1000:0388` with the exit status, 0 and 1. With its second argument 0, `1000:0388`
does not take the return at `1000:03C3`; it calls `1000:01F3` and `1000:0176`, and with its third
argument nonzero it skips the far calls at `1000:03C9` and `1000:03CD` and calls `1000:019E`, which
ends the program through `INT 21h` function `4Ch`. No path from `1000:2E48` returns, so nothing
reaches `4420:020B`.

**Overlay 204 `+259D`.** Its three `cs` tables are switches on the double word argument at
`[bp + 8]`: each loads a count and the offset of that many double word keys, compares the argument
with each key in turn (`loop`), goes to `+27D7` (result 0) when none matches, and on a match at
index `i` jumps through the word `2 * i` after the keys. The loop is the only bound, so each table
has exactly its count of targets:

| Jump | Keys | Count | Key values | Targets |
| --- | --- | --- | --- | --- |
| `+2610` (`0x8E3D0`) | `+2817` | 9 | 0, `0x0D`, `0x0E`, `0x12`, `0x13`, `0x17`, `0x19`, `0x20`, `0x21` | `+263D`, `+2614`, `+2628`, `+2649`, `+265E`, `+2673`, `+267F`, `+2694`, `+26C7` |
| `+2703` (`0x8E4C3`) | `+2805` | 3 | 0, `0x19`, `0x21` | `+2707`, `+2713`, `+2728` |
| `+2763` (`0x8E523`) | `+27E1` | 6 | 0, 1, 3, 7, 8, `0x0A` | `+2767`, `+2773`, `+2787`, `+279B`, `+27AF`, `+27C3` |

The first and third were declared from FND-RNG-058's walk report already, and the views followed
them. The walk report has no declaration for `+2703`; its three targets are the bytes
`0x8E4C7..0x8E4DC` and `0x8E4E8..0x8E4FD` the row left out. `dsun-declared-flows.json` now declares
it, with this finding as evidence. The `jmp` instructions at `+26DC` (`0x8E49C`) and `+273D`
(`0x8E4FD`) each follow an unconditional `jmp` and are no target of any table, so nothing reaches
them.

**Overlay 191 `+0B0A`.** Every segment word in the file that names overlay 191's header segment
`572F` (61 of them: 11 relocated words in the load image, 50 fixup words in overlay code) is the
segment of a far call, with the offsets `0x20`, `0x25`, `0x2A`, `0x2F`, `0x34`, `0x39`, `0x3E`,
`0x43`, `0x48`, `0x4D`, `0x52`, `0x61`, `0x66` and `0x6B`, except the load-image word at file offset
`0x4B678`, which follows the word 0. None names the trampoline `572F:0057` that enters `+0B0A`.
Overlay 191's code holds no near call to `+0B0A`, no indirect call or jump, and no word `0x0B0A`.

**Restage.** The procedure of FND-EXE-597 with the fill step and the new declaration (snapshots
`INST-single-20261021r` and `INST-overlay-20261021r`, each exported twice with identical results)
gives `ENTRIES_COMPLETE` with the counts of FND-EXE-599, `FLOWS_COMPLETE` in the overlay view with
105 jumps and 799 references (104 and 796 before), no start without an instruction, and the same
2,356 resident and 877 overlay rows. Against the inventory of FND-EXE-599:

| Check | FND-EXE-599 | Restaged |
|---|---|---|
| Rows | 3,233 | 3,233 |
| Body bytes (mapped bytes of the 264 regions) | 484,251 (81.4%) | 484,313 (81.4%) |
| Starts without an instruction | 0 | 0 |
| Walk instructions (of 165,558) outside every range | 14 | 1 |
| Walk instructions partly outside | 17 | 0 |

Twenty-one rows change, each only in size and ranges: the 20 above gain their byte, and overlay
204 `+259D` gains `0x8E4C7..0x8E4DC` and `0x8E4E8..0x8E4FD` (532 to 574 bytes). The one walk
instruction outside is the `jmp` at `4420:020B`.

## Interpretation

The holes were an artefact of how Ghidra builds a body: a call target is kept out as the possible
entry of another function even when it lies inside an instruction. Filled, each row holds every
byte of its instructions. The `jmp` at `4420:020B` and the two `jmp` instructions after the
overlay 204 cases are dead code a compiler leaves after a call that does not return or a case that
jumps away; they stay outside the rows, and every walk instruction that can execute lies in a row.
Overlay 191 `+0B0A` keeps its row, since its trampoline is an entry, but no code reaches it, so
its near `ret` never runs.

## Alternatives

- The fill keeps the misdecode at `15F3:0876`: the row holds `15F3:0876..15F3:0879` whole, two
  bytes of text and the `pop dx`, decoded as one instruction.
- The wrong computed references that cut the holes at `25AF:075E`, `3F96:07AB`, `455B:0230` and in
  overlays 173 and 199 stay in the views; they name no function and change no row other than by the
  byte they cut.
- Overlay 191 `+0B0A` could be entered through a far pointer built at run time from a segment and
  an offset that no relocated word or fixup gives; none was searched for. A far entry would end at
  its near `ret` with the caller's segment still on the stack.
- An `INT 21h` function `4Ch` that returned would reach `1000:01A7`, not `4420:020B`.

## How to reproduce

Follow the DSUN reconciliation path in `docs/EVIDENCE-TOOLS.md` with `FillInstructionBodies.java`
run as it says; the logs, exports, the join configuration and the staged files are kept under
`GAME_DIR/analysis/exe-inventory-20261021/restage/r18/`. The references come from
`ReportReferences.java` on the views of FND-EXE-599 and of the restage (read-only). With the locked
evidence Python, in `tools/research/exec-census/`: `resident_listing.py <dsun> 4842:0A0F..4842:0A23
15F3:0870..15F3:0880 4420:01F8..4420:0212 1000:2E48..1000:2E5A 1000:0387..1000:03EE
1000:019E..1000:01B0`; `overlay_listing.py <dsun> 204 0x8E35D 0x8E5A1` and `191 0x7C1F0 0x7D212`;
`direct_callers.py <dsun> 191+0B0A` with the control `173+2E1B`; `far_pointer_words.py <dsun>
572F:0057`; a scan of the MZ relocation words and of each overlay's fixup words for the value of
`572F` (load image) and `0x5F8` (descriptor 191 times 8, overlay code), printing the bytes on
either side; and `walk_inventory_coverage.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
<FND-RNG-058's report>`. Nothing was executed.
