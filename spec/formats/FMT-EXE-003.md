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
evidence: [FND-EXE-002, FND-EXE-003, FND-EXE-004, FND-EXE-520, FND-EXE-561, FND-EXE-562, FND-EXE-563, FND-EXE-565, FND-EXE-566, FND-EXE-567, FND-EXE-582, FND-EXE-587, FND-EXE-588, FND-EXE-644, FND-EXE-645, FND-EXE-647, FND-EXE-648, FND-EXE-649, FND-EXE-650, FND-EXE-651, FND-EXE-652, FND-CONFIG-234, FND-CONFIG-163, FND-PARTY-338, FND-PARTY-339, FND-PARTY-342]
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

- Which additional direct, computed or indirect readers/writers can interpret
  or alter these resident headers beyond the known manager routes
  (Q-EXE-033)? FND-EXE-003/004 covers all shipped records and trampolines
  in both editions; FND-EXE-520/561/562/563/566 supplies the known manager
  field uses. FND-EXE-567's relocated/fixup census explicitly excludes
  runtime-computed segments and does not prove all header accesses known.
  FND-PARTY-342 classifies its two retained signal-table pointers as stored
  callbacks: registration and immediate consumers do not dereference them
  as header data. Their downstream manager/reporter effects and other
  computed or aliased pointers remain within the census obligations.
  FND-EXE-587 already supplies the connected window/global callback
  setter, forwarder, argument and indirect-call reading. In particular,
  its three window setters store the callback value at record +F5/+F9/+FD;
  treating that callback argument as an unread header-data dereference
  would repeat resolved local work. This does not establish that every
  destination record is disjoint from a header, exclude adjusted-base or
  block-copy writers, or admit all saved/restored callback histories.
  Its stated exclusions remain. FND-CONFIG-163 retains the shared guard
  helper's termination/native/cleanup conditions: the callback-store
  continuation is not proof that a guard call returns normally.
  FND-EXE-588 supplies the application-frame setter/forwarder and callback
  census, retaining tag/record-construction, block-copy and computed-pointer
  exclusions. FND-EXE-644 resolves the nine first-jump candidates in the
  retained overlay pointer report as local callback argument forwarding;
  the branch joins themselves add no unread header-data dereference.
  Actual route admission, destination aliasing and reached callback effects
  remain distinct from that finite immediate-use result.
  FND-EXE-645 matches the other 144 first-call candidates one-to-one to
  the expected relocated callback arguments, including sixteen cases
  after a structure-copy helper. Together the findings classify this
  finite 153-pair set under their decoder/frame models; they do not prove
  every instruction start, predecessor, native frame or alias condition.
  Reuse that correspondence rather than repeating the local slot census.
  FND-EXE-647 supplies positive exact instruction-start matches for all
  306 source pushes and 160 consumer/helper calls in the declared-root
  model. Its six controls pass and its inside-call-byte control stays
  distinct. The model's 50 unresolved transfers, five gaps, four contested
  decodes and false negativeUsable remain; these positive matches are not
  actual predecessor/frame/native admission or an exhaustive access proof.
  FND-EXE-648 gives the concrete 4DA0:0000 destination's initial source
  value, two allocation-derived publication paths and close-before-clear
  interval. FND-CONFIG-234 shows reader failure can leave its published
  allocation returned by the wrapper. Remaining questions for that route
  concern the actual allocation/header address domain and concrete writers
  or callbacks across the named intervals, not an unnamed pointer producer.
  FND-EXE-649 composes the recorded direct break publishers into a
  conditional canonical current-pair/lower-bound invariant. Under its
  startup/data/frame and excluded-writer conditions, fresh allocation
  header stores lie above the resident overlay domain. A checked next
  break alone is insufficient; its noncanonical counterexample is not an
  admitted game state. Free-list returns, actual bindings, cached history
  and later header-relevant effects remain separate.
  FND-EXE-650 separates free-list address confinement from complete heap
  validity. The relevant remaining inputs are the actual root and followed
  link/release/predecessor segment domains and nonwrapping count/end
  producers. A high root alone does not exclude a foreign link; a high
  selected segment alone does not exclude a wrapped tail. Full reciprocity,
  termination or checker success is not an additional obligation unless it
  can change the described header access. Actual input histories and the
  excluded writer/alias mechanisms remain.
  FND-EXE-651 classifies the five reported direct release argument sources.
  These are forwarded/existing pointers, a saved original allocation, a
  computed remainder and post-callee input-buffer fields. The input flag's
  zero-start supplied-buffer history bypasses release under its stated
  bindings; retained or changed flag/pointer histories can differ. Actual
  source domains, copy-frame separation, remainder extent and implicit
  buffer-writer effects remain, not an unspecified release callee.
  FND-EXE-652 gives the packet writer's fixed-segment copy spans and
  sufficient whole-window separation conditions. Actual captured segments,
  mapping and preserved frame/code/shared-field histories remain; a complete
  ring-correctness proof is not independently required for that exclusion.
  Other excluded header-access mechanisms remain separate.
  FND-EXE-582 resolves retained direct target sources under its stated
  direct-store limits, leaving indexed, copied, string/block and other
  unresolved target/access mechanisms. A complete-reading declaration
  cannot treat these exclusions as evidence of no further readers/writers.
  Acquisition success and unrelated service behavior are not added to the
  representation claim; a service remains relevant where its effects can
  select a header, alter a field/extent or change a described interpretation.
- Under which actual caller/frame and loader conditions does the return
  trap realize the saved-offset/innermost-frame restoration described by
  the +00/+02 fields (Q-EXE-034)? FND-PARTY-338 gives the first eligible
  frame, zero/odd/sentinel cases, a sufficient upward-chain own-write
  contract and precise native BP/save-word conditions. FND-PARTY-339
  supplies bounded cache interfaces and scalar/link/target/copy domains.
  These conditional contracts do not establish every actual handler graph,
  successful rejoin, native binding or reached asynchronous effect.
  The narrower reflected-handoff trial remains separate; answering that
  one root alone would not settle this entry's general return behavior.
  Retain the runtime field claims together with their representation;
  moving their proof obligations here neither answers nor drops them.

The whole-entry evidence assessment at baseline 1bb1ae74 found no
qualifying undeclared complete reading. Layout coverage is separate from
code reader/writer completeness and the claimed runtime return behavior.
The entry remains supported; these obligations are not an ownership
assignment or a declaration of fully read manager functions.
