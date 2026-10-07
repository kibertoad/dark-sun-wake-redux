---
id: FND-EXE-099
title: Physical fallback slots select a shared byte transfer path and a fresh-table two-call word method
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    offset: 0x00355828..0x0035582F
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    offset: 0x00355EE0..0x00355EE7
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    offset: 0x00354F78..0x00354F7F
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00417900..0x00417916
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00417920..0x00417968
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0058F890..0x0058F918
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689470..0x00689555
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689C50..0x00689D35
tool: Verified physical PE word queries and Ghidra 12.1.3 PUBLIC explicit-entry recovery and bounded reading
environment: null
---

## Observation

FND-EXE-098 grounds first-word writes of three table addresses; FND-EXE-094
grounds virtual byte offset eight and word offset twelve. Reading precisely
those full slots from the verified shipped PE gives:

| Selected table | Byte slot / shipped offset / target | Word slot / shipped offset / target |
| --- | --- | --- |
| `0x00758620` | `0x00758628` / `0x00355828` / `0x00689470` | `0x0075862C` / `0x0035582C` / `0x00689C50` |
| `0x00758CD8` | `0x00758CE0` / `0x00355EE0` / `0x00417900` | `0x00758CE4` / `0x00355EE4` / `0x00417920` |
| `0x00757D70` | `0x00757D78` / `0x00354F78` / `0x00417900` | `0x00757D7C` / `0x00354F7C` / `0x00417920` |

These are file-backed little-endian words, not analyzer labels. The last two
tables share these slots; nothing here equates their remaining slots or
objects. FND-EXE-098 writes `0x00758620` or `0x00757D70` into the fixed
fallback object's first word on its respective admitted input routes.
Its `0x00758CD8` store is to the separate object at `0x01B7BB18`, not to
that same fallback object.

The recovered byte entry `0x00417900` reserves twelve stack bytes and reads
its second argument at current ESP plus twenty, corresponding to entry ESP
plus eight. It supplies fixed pointer `0x0071F380` first and that full
address value second to `0x0058F890`. The five decoded instructions end
at this call; there is no local source-byte load or return-value normalization
in that recovered body. No instruction was initially decoded there, although
the physical table slot grounds this exact entry. Recovery adds its start
and twenty-three owned body bytes to the inventory; this size does not
prove that a normally returning callee has a decoded safe continuation.

The callee `0x0058F890` reserves twelve bytes, takes its first argument
from current ESP plus sixteen and the address of its second-argument region
from current ESP plus twenty. It supplies shared buffer `0x0240DB80`,
that first argument and the argument-region address to `0x00601F70`.
On normal return it scans buffer words for a zero byte using the word-level
zero-detection arithmetic and refines the selected cursor using the low/high
half and carry gates. It writes the full sixteen-bit value ten through the
resulting cursor. This is not a bounded-buffer or imported-callee contract:
there is no local scan limit, and the target's external effect remains unread.

It then supplies full four to `0x005FCD70`, immediately writes the shared
buffer address through the returned pointer, and calls `0x005FAED0` with
that pointer first, `0x00755C20` second and zero third. The allocation
return is not tested before the store. FND-EXE-025 records those shared
allocation/transfer helpers' bounded contracts; this caller does not establish
allocation success, buffer ownership or native exceptional completion.
Its decoded body likewise ends at the transfer call. No normally returned
source byte is established by this route.

The word entry `0x00417920` reserves twenty-eight stack bytes and saves
EBX, ESI and EDI inside that reservation. Its first full argument at current
ESP plus thirty-two is retained as object O; the second at plus thirty-six
is retained as address A. It reads the first-word table through O and calls
its offset-eight target with O and A. Before that first call, the retained
address is incremented at thirty-two-bit width. On normal return the full
EAX result is saved. The method freshly reloads the first-word table through
O, then calls that current table's offset-eight target with O and A plus one
modulo thirty-two bits. It shifts this second full EAX left eight, ORs it
with the first full result, and returns that full combined value after restoring
its saved registers and stack reservation. It performs no AL/AX masking in
this direct body and has no local object/target null check.

The second target therefore need not equal the first if the first call or
another admitted effect changes O's first word. Its first full return is not
assumed to be a normalized byte. FND-EXE-094's single-word outer reader later
keeps only AX and zero-extends it; its boundary route independently invokes
byte methods and keeps AL at each read. Caller truncation does not retroactively
normalize the word method's intermediate full-value combination. Recovery
adds this exact entry and seventy-three owned body bytes to the inventory;
no existing row changes size.

The other physical pair, `0x00689470` and `0x00689C50`, begins by retaining
the second full argument as an address, shifting a copy right twelve and
testing byte `0x01B7BB14`. A nonzero mode enters a two-stage indexed-word
reading route from globals `0x0075B6C8` and `0x01D4A380`. Its second
word's low-bit test can admit a call to `0x00417CF0` with the retained
address, computed second-word offset and a full flag selected as zero or four
by globals `0x0075B140` and `0x0075B144`. After normal return it freshly
reloads `0x01D4A380` and the selected word before testing its low bit again.
The zero mode and later paths lie outside these prefix readings. These
physically selected entries are concrete, but their complete byte/word results,
mapping publication and exceptional routes remain unresolved.

## Interpretation

The previously fixed fallback address has mode-dependent method admission.
The reset is not itself proof of a successful zero-byte read. Two grounded
tables share a byte entry that forwards into shared message/allocation/transfer
machinery, and a word entry that conditionally calls the object's current byte
method twice. The larger table has separately resolved targets whose prefixes
read mutable mapping state. Complete effects and the producers that admit these
states remain Q-EXE-009 in FMT-EXE-006; no shell outcome or native execution
is established here.

## Alternatives

- Analyzer default names cannot supply these virtual targets; the explicit
  physical slots identify them, including the initially undecoded byte entry.
- Sharing two slots does not establish whole-table identity or interchangeability
  of the separately written objects in FND-EXE-098.
- A shared fallback pointer is not evidence of a successful source read. The
  small byte entry makes a call with the address; its return and external
  effects cannot be replaced by an invented byte value.
- The word method does not locally force each returned value to byte width,
  reuse the first table or normalize its full combined return. The outer
  reader's width is a separate contract.
- Recovered ownership sizes are inventory metadata, not proof of complete
  decoding, native return, all callers or all table writers.

## How to reproduce

Check the FND-EXE-011 shipped identity: length 3802624 and XXH3-128
`09861838aa3018346f9f15c9a4f5925c`. Map each of the six listed virtual
slots through its containing PE section's physical raw extent; require all
four bytes to map contiguously and remain inside the file. Read each as a
little-endian full word. Query only table offsets eight and twelve; use
FND-EXE-098's exact first-word writers and FND-EXE-094's call-site offsets
as independent selection controls. No whole-table or negative search is made.

Use the saved Ghidra program with -noanalysis. ReportInstructionWindow.java
queries all four resolved entries with a limit of 65 each. The initial
`0x00417900` request reports undisassembled .text memory; it is not an
empty mapping or proof of absence. Supply only `00417900` and `00417920`
to RecoverCitedFunctions, then read twelve instructions from `0x00417900`
and seventy from `0x0058F890`. Restrict claims to the locations above,
excluding subsequent functions and undecoded gaps. Use FND-EXE-025 for
its previously bounded allocation/transfer helpers. Track both word-call
argument writes, retained full return, fresh table load and final caller width.

Export function inventory after recovery and normalize only start and owned
body-size columns. The delta adds `0x00417900` size 23 and `0x00417920`
size 73, with no removed or changed old rows. Keep physical slot results,
instruction reports, exact-entry input and the saved database beneath
GAME_DIR/analysis/exe-batches; commit no bytes, listings or original text.