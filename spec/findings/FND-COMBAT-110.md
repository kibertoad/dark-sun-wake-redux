---
id: FND-COMBAT-110
title: Restore archive preparation selects a newly opened working record only on its open branch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071A17..0x00071A56
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068850..0x000688A6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0066..38FF:02B5
tool: tools/research/exec-census/trampoline_target.py, overlay_listing.py and resident_listing.py at 7981aadc; locked evidence Python and Capstone
environment: null
---

## Observation

Q-COMBAT-018 retains the archive used by the numbered restore request.
FND-COMBAT-043 reads the callers' shared helper +1BE7, while
FND-CONFIG-037/038/040/068 identify the implicit reader selection and
fallback. This connects the helper's successful open to that selection;
it does not assume a saved numeric handle is the active archive pointer.
Offsets below are hexadecimal unless stated otherwise.

Overlay 187 +1BE7 first tests byte DS:167A. Nonzero returns AL zero
without calling an opener or selector. Zero requests opening the loaded
near filename darkrun.GFF, passes far output address DS:144E and the
two following word arguments 0002 and 04D4, then tests the returned AL.
A zero result calls a diagnostic before the helper's encoded flag-setting
continuation. This diagnostic's return or termination is not established.
A nonzero result skips it, sets DS:167A to one and returns AL one.

The far call resolves through 56BD:002F, descriptor 182 trampoline three,
to overlay 182 +0000 at file 68850. Its full inventoried body ends at
688A6, including the far return. It copies the current directory prefix
DS:44F2 into SS:BP-52, appends the near name with limit word 50 and
requests resident 38FF:0066 with that path, the forwarded output address,
the option word and the zero-extended final word. FND-CONFIG-182 supplies
the append's signed length/truncation qualifications; stable names, prefix,
segments, buffers and an untruncated resulting working path remain
prerequisites to naming the file actually opened.

On return the wrapper maps AX equal to FFFF to AL zero and any other
AX to AL one, without another archive call. The resident successful path
already read in FND-CONFIG-037 links the newly allocated record and
assigns its far pointer to DS:9DA3 and DS:9D9F. The final selected-record
store is at 38FF:027F. The body then writes the numeric handle through
its nonnull output argument, advances its counters and returns AX zero.
There is no later child call on this ending. For the supplied valid
DS:144E output, that numeric handle is separate from the selected-record
pointer at DS:9D9F.

Thus, under admitted path construction, successful resident opening and
preserved ordinary storage, +1BE7's newly opened branch returns with the
working record selected. Its reuse branch does not establish selection:
DS:167A being nonzero only skips the local open, and does not reselect
the saved handle. The flag's producers and active-pointer lifetime still
need evidence at the later numbered request.

## Interpretation

The shared preparation helper's two selection effects are now explicit.
Q-COMBAT-018 need not reread the generic open wrapper to connect an actual
successful new open to selection. It still needs path/flag admission,
selection changes between preparation and SAVE/24 or SAVE/26, and the
reader's fallback record that supplies each resource. FND-CONFIG-040's
mode-dependent traversal means the selected record alone does not prove
which open archive supplies a matching resource. Restored contents,
destination backing and row/count consistency remain separate obligations.
No successful native load, global archive identity or complete reading is
claimed, and no rule status changes.

## Alternatives

- A saved handle and an active record pointer are different state.
- A reuse flag is not a select-handle operation.
- The encoded continuation after an open diagnostic does not prove that
  failed opening returns normally or selected a record.
- An open success does not prove selection is unchanged until the reader.
- First selection and the archive eventually supplying a resource differ
  when fallback traversal is enabled.

## How to reproduce

Use installed DSUN.EXE through tools/game-dir.mjs and locked evidence
Python. Readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run:

```text
trampoline_target.py <dsun> 56BD:002F
overlay_listing.py <dsun> 187 71A17 71A56
overlay_listing.py <dsun> 182 68850 688A6
resident_listing.py <dsun> 38FF:0066..38FF:02B5
```

The inventory at 7981aadc gives the generic wrapper 86 bytes. Preserve
the source-derived trampoline/fixup mapping, explicit output-pointer push
order and the return byte/word distinctions. Reuse FND-CONFIG-182 for
string append and FND-CONFIG-040/068 for fallback and selection writers;
this reading makes no new exhaustive writer claim.

Keep original-derived reports only in GAME_DIR/analysis/hit-incoming-20261104/:
restore-working-archive-helpers.txt, restore-open-wrapper.txt and
restore-resident-open.txt. The first also includes the already-read close
helper; the claimed preparation ends at 71A56. No original listing, bytes
or save-resource content is committed.
