---
id: FMT-EXE-003
title: FBOV overlay header in the resident image
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DSUN.EXE", "CD:DSUN.EXE"]
byte_order: little
size: null
text: false
definition: fmt_exe_003.ksy
evidence: [FND-EXE-002, FND-EXE-003, FND-EXE-004, FND-EXE-520, FND-EXE-561, FND-EXE-562, FND-EXE-563, FND-EXE-565, FND-EXE-566, FND-EXE-567, FND-EXE-582, FND-EXE-587, FND-EXE-588, FND-EXE-644, FND-EXE-645, FND-EXE-647, FND-EXE-648, FND-EXE-649, FND-EXE-650, FND-EXE-651, FND-EXE-652, FND-EXE-653, FND-EXE-654, FND-EXE-655, FND-CONFIG-234, FND-CONFIG-163, FND-PARTY-338, FND-PARTY-339, FND-PARTY-342]
conflicting: []
split_with: []
related: []
---

## Layout

The resident part of one overlaid segment, at the paragraph its FMT-EXE-002 descriptor's
`segment` names: `565C:0000` for overlay 169 to `57DD:0000` for overlay 217 in the installed
`DSUN.EXE` [FND-EXE-003]. The next overlay's header starts at the next paragraph boundary after
this one's trampolines.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `BYTE[2]` | `trap` | `CD 3F`, the instruction `INT 3Fh`, in every header. In the installed file, when the manager unloads the overlay, the innermost frame that would return into its code returns here instead, which runs the `INT 3Fh` handler. | supported | FND-EXE-003, FND-EXE-562 |
| `0x02` | 2 | `UINT16LE` | `unk_02` | 0 in every header of both files. In the installed file the manager stores there, when it unloads the overlay, the return offset it took from that frame, and gives it back when it loads the overlay again. | supported | FND-EXE-003, FND-EXE-562 |
| `0x04` | 4 | `UINT32LE` | `payload_offset` | Offset of this overlay's FMT-EXE-005 block from the start of FMT-EXE-001's `payload`; a multiple of 16. | supported | FND-EXE-003 |
| `0x08` | 2 | `UINT16LE` | `code_size` | Bytes of code in the block. | supported | FND-EXE-003 |
| `0x0A` | 2 | `UINT16LE` | `fixup_size` | Bytes of fixup list in the block after the code; always even. | supported | FND-EXE-003 |
| `0x0C` | 2 | `UINT16LE` | `trampoline_count` | Number of trampolines after the header, 1 to 53. | supported | FND-EXE-003, FND-EXE-004 |
| `0x0E` | 18 | `BYTE[18]` | `unk_0E` | All 0 in every header of both files. At run time the installed file's manager keeps, at these header offsets: the next header in an overlay cache's chain at `0x0E`, the load segment at `0x10`, the next kept header's segment at `0x12`, the position of the overlay's copy in the cache as a doubleword at `0x14`, the near address of the routine that loads it at `0x18` (`0x04C6` from the file, `0x0A4B` from EMS, `0x0BFE` from extended memory), state bits at byte `0x1A` (`0xFF` leaves the header out), at byte `0x1B` the number of extra passes the overlay gets at the head of the buffer queue before it is evicted, and at `0x1C` the next overlay in that queue of loaded overlays. No code found uses `0x1E`. | supported | FND-EXE-003, FND-EXE-520, FND-EXE-561, FND-EXE-562, FND-EXE-563, FND-EXE-566, FND-EXE-567 |
| `0x20` | `trampoline_count * 5` | `FMT-EXE-004[trampoline_count]` | `trampolines` | The overlay's resident entry points. | supported | FND-EXE-004 |
| | | | | Total size `32 + trampoline_count * 5` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 49 overlay headers of the installed `DSUN.EXE` and all 49 of the disc's. In both, every block
the headers name lies inside the pack, the blocks do not overlap, and every trampoline offset lies
inside its overlay's code [FND-EXE-003, FND-EXE-004].

## Open questions

- Which additional header-access mechanisms remain beyond the known manager
  routes (Q-EXE-033)? Reuse the shipped-record and field-use findings and
  FND-EXE-644/645/647's finite callback-pair classification. Their controls,
  unresolved transfers and excluded access mechanisms remain. FND-EXE-655
  qualifies the five-gap/four-contested cohort as an entry/data/conditional-
  exit boundary with an explicit inventory repair; it does not erase the
  baseline model's gap or return assumptions. Known local classifications
  are not a whole-program negative.
  Ownership is explicit in docs/research-closure-overlay-headers.json,
  pinned to coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv. Its assignments are metadata,
  not complete readings; its report retains every unassigned/shared function,
  inventory gap and out-of-scope row. Other editions' unassigned rows remain.
- Can callback-record destinations alias these resident headers (Q-EXE-035)?
  Reuse FND-EXE-587/588/648 for the window/application-frame stores and
  concrete publication/cleanup intervals. FND-CONFIG-234 distinguishes an
  output allocation from successful I/O. Actual record and cached-pointer
  histories, aliases and intervening writers remain in that queue item.
- Do the actual shared heap and input-buffer histories exclude these headers
  from their accesses (Q-EXE-036)? Reuse FND-EXE-649/650/651/652's explicit
  address contracts and exclusions. The wider heap service remains a
  dependency owned by CONFIG; unrelated span/backing or ring claims
  are not added to this entry's header-access question.
  The map shares the retained heap/wrapper functions with CONFIG and the
  buffer/normalizer/startup functions with INPUT where applicable. The
  inventoried 39D1:0009 body contains the retained 00F1 entry path; assigning
  that row does not claim its unreviewed prefix or other paths fully read.
- Which downstream signal/window/application-frame callback effects can
  select, alter or reinterpret these headers (Q-EXE-037)? Reuse
  FND-PARTY-342 and FND-EXE-582/587/588 for known arguments, setters and
  consumers. FND-EXE-653 reconciles all fifty current unresolved sites to
  retained local mechanism readings, including the added signal-2 root;
  this leaves actual target histories and header-relevant effects open,
  rather than fifty unidentified transfers. Preserve all writer/native
  and caller/construction exclusions of those readings.
  FND-PARTY-339 already supplies the nine outside-manager store roles and
  segment conditions. FND-EXE-654 adds a current-pin constructor-return
  point with a checked game-data root mirror; the other native/captured
  segment and continuous writer conditions remain. Do not reread those
  stores as unclassified equal-offset hits.
  FND-CONFIG-163's guard/native/cleanup conditions remain. The map does
  not assign every callback target or loader outside its listed functions;
  those unassigned rows and unresolved targets remain visible obligations.
- Under which actual caller/frame and loader conditions does the return
  trap implement the saved-offset/innermost-frame restoration described by
  +00/+02 (Q-EXE-034)? Reuse FND-PARTY-338/339's conditional frame/link,
  cache, target and copy contracts. They do not admit every handler graph,
  native BP/save-word binding, successful rejoin or asynchronous effect.
  The narrower reflected-handoff trial in FMT-PARTY-007 remains separate.

The whole-entry assessment at baseline 1bb1ae74 found no qualifying
undeclared complete reading. The subsequent scope split answers no question
and removes no unresolved claim. The entry remains supported, with no
complete-reading declaration; layout coverage, ownership assignments and
runtime return behavior require their own evidence. These obligations retain
all field claims together with their representation.
