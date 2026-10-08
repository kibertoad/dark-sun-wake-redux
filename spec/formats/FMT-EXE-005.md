---
id: FMT-EXE-005
title: FBOV overlay code block with its fixup list
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DSUN.EXE", "CD:DSUN.EXE"]
byte_order: little
size: null
text: false
definition: fmt_exe_005.ksy
evidence: [FND-EXE-003, FND-EXE-005]
conflicting: []
split_with: []
related: []
---

## Layout

One overlay's block in FMT-EXE-001's `payload`, at the `payload_offset` its FMT-EXE-003 header
gives, with that header's `code_size` and `fixup_size`. Overlay code is located in the spec by its
file offset: overlay 169's code starts at `DSUN.EXE+0x00057580` [FND-EXE-003].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | `code_size` | `BYTE[code_size]` | `code` | The overlay's code and data. A trampoline's `target` is an offset in it. | supported | FND-EXE-003 |
| | `fixup_size` | `UINT16LE[fixup_size / 2]` | `fixups` | Offsets in `code` of 16-bit words that each hold a segment-table index shifted left by three, with the low three bits 0. Every offset leaves room for its word inside `code`. | supported | FND-EXE-005 |
| | | | | Total size `code_size + fixup_size` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 49 blocks of the installed `DSUN.EXE`, with 8,262 fixups, and the sizes of all 49 of the
disc's [FND-EXE-003, FND-EXE-005]. FND-EXE-173 additionally validates both sources'
fixup-table dimensions, operand bounds and descriptor-index admission through the
bounded source reader; this does not establish native loader behavior.

## Open questions

- Whether the loader replaces each fixup word with the segment its descriptor names, and which
  segment that is for a descriptor of an overlay (FND-EXE-007, Q-EXE-001).
  FND-EXE-175 supplies a bounded resident candidate; its caller admission and
  state/segment producers remain unread, so it does not settle the loader.
  FND-EXE-176 resolves its initial state segment and vector pointer; their
  writers and native admission still require reading. FND-EXE-177 adds a
  gated vector-exchange consumer with two unresolved cleanup callbacks.
  FND-EXE-178 separates their shipped defaults from concrete later writer leads.
  FND-EXE-179 records intervening callee restoration and external dependencies.
  FND-EXE-180 follows the paired external-target producer; live preservation
  and provider correspondence remain unresolved. FND-EXE-182 supplies a
  bounded shipped-interpreter match whose field/registration admission is open.
  FND-EXE-183 traces its packed-pointer producer and bounded counter return;
  counter admission, failure behavior and callback setup remain unread.
  FND-EXE-184 follows setup ordering and result consumption; callee effects,
  state admission, metadata extents and dispatch remain unresolved.
  FND-EXE-185 bounds the selected builder arm and its return; backing memory,
  admitted destination extent and callback dispatch remain unread.

- Which analyzer-owned body fragments are native code under the original CS
  bindings (Q-EXE-010)? FND-EXE-173 separates their physical source regions
  from their valid overlay entries. FND-EXE-174 shows that fresh analysis after
  relocation-pair repair still assigns non-code fragments to overlay bodies.
  A descriptor-base dispatch reading keeps
  eleven targets inside code; the analyzer-alias reading places most outside.
  Native segment producers and every admitted target must settle those competing
  interpretations before the affected boundaries or denominator are accepted.