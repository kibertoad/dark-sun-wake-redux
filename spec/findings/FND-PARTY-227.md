---
id: FND-PARTY-227
title: The current-item image helper returns 19103 without a selection and otherwise resolves only a zero cache
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007C1F0..0x0007C25B
tool: tools/research/exec-census/overlay_listing.py, trampoline_target.py and direct_callers.py; Python 3.12.10, Capstone wheel 5.0.9 (module version 5.0.7), xxhash 4.0.1
environment: null
---

## Observation

Trampolines 572F:0025 and 572F:0020 enter descriptor 191 at
+0000 and +0013 respectively. Their complete local bodies occupy
7C1F0..7C203 and 7C203..7C25B, matching the committed inventory.

The first helper has no arguments or children. It compares word
current DS:1A34 with FFFF, returning AX zero for equality and one
otherwise. It does not test graph length, the selected entry's kind,
record bounds or the record table pointer. This is an exact local
selection-word predicate, not proof that the selected item is valid.

The second helper calls that predicate. Zero AL selects result
19103 (4A9F hexadecimal) without reading the record table. Nonzero
AL instead reads the word at mapped segment 5072 offset 0044,
multiplies it by 23 at word width, and adds the low word to the
offset of the far record table at current DS:19C1. It reads the
selected record's word at +0C as its candidate result.

A nonzero candidate is returned unchanged. For zero, it rereads
the mapped 5072:0044 word, passes it to 571F:00C5 and retains
returned AX in DX. FND-PARTY-225 describes that resolver's local
OJFF request and zero-on-failure behavior. After the child returns,
this helper again reads 5072:0044, recomputes the 23-byte record
address using current DS:19C1, stores DX at +0C and returns DX in
AX. A zero resolver result is therefore stored and returned as zero;
this path does not substitute 19103 for a resolver failure.

These bodies do not themselves write DS:1A34 or 5072:0044. They
do not prove those words and the selected table refer to the same
current object across external work. In particular, the resolver's
global/register preservation and valid table storage remain input
obligations. The post-child reload is not a preservation check.

FND-CONFIG-186 independently records the resident pointer-selection
consumer: its argument-three branch calls the predicate, uses this
helper's AX with local resource-kind byte zero when AL was nonzero,
and passes that selection toward 56BD:006B. FND-PARTY-036 identifies
that byte-zero resource kind as BMP. These findings establish a
concrete consumer connection; this helper itself acquires no image,
draws no pixels and does not invoke the pointer backend.

Static controls distinguish its local cases: FFFF selection returns
19103 without a cache read; any other selection word admits the
cache path; a nonzero cache, including FFFF or 9999, is returned
unchanged; a zero cache calls the resolver and stores/returns its
word, including zero. No control was executed in the original.
Other callers' additional gates are outside these complete bodies.

## Interpretation

The current-item number helper is another zero-triggered cache
consumer/producer, with an explicit no-selection fallback. Its
predicate is weaker than record validity. The local fallback and
cache store narrow Q-PARTY-087's later-consumer obligation without
establishing all cache writers, invalidation or admitted storage.

## Alternatives

- Treating any nonzero predicate result as a valid record adds checks
  absent from the predicate.
- Treating 19103 as a universal failed-image fallback conflates the
  no-selection branch with the zero-resolver-result branch.
- Treating 9999 as a resolver sentinel in this helper is contradicted
  by its exact zero test. Other callers' gates remain separate.
- Treating the returned number as an acquired image pointer or a
  rendered cursor omits the later resource and graphics services.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and locked evidence Python.
Resolve both trampolines with trampoline_target.py. Read descriptor
191 using overlay_listing.py, 7C1F0..7C25B hexadecimal and half-open.
Retained report: GAME_DIR/analysis/party-record-heads/drawing-20261018/current-item-bitmap.txt.

Compare both complete bodies with the committed function inventory,
then read FND-PARTY-225's resolver contract and the independent
FND-CONFIG-186/FND-PARTY-036 pointer consumer descriptions. A
direct_callers.py query for 191+0013 finds the relocated resident
candidate at file 224DA as one of its callers; that candidate is
qualified by FND-CONFIG-186's complete resident reading. Other
query matches remain candidates until their instruction paths are read.
