---
id: FND-SCRIPT-047
title: Key minus99 selects different TALK entries under regional registration and replacement
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: retained script_decode.py index from FND-SOUND-077; script_reach.py at d454d213; FND-SCRIPT-015/028/038 interface composition
environment: null
---

## Observation

The contract assessed here is which literal TALK registrations for key -99
the retained decoded index supplies, and how the evidenced registration,
regional pruning and event-consumer interfaces distinguish their entries.
It is not an assertion that any of these registrations runs natively.

The retained index has three top-level opcode 6E instructions with literal
third argument -99:

| Registration site | Entry offset | GPL resource |
| --- | --- | --- |
| MAS/1 at 0135 | 0A41 | 37 |
| MAS/54 at 024F | 03DD | 239 |
| GPL/241 at 159F | 03DD | 239 |

The first is the independently identified positive control FND-SCRIPT-045.
This is a query over decoded instruction lines, not a complete census of
computed operands, nested registrations, changed scripts, native or restored
records. The GPL/37 rejected-atom gap remains excluded. Source provenance is
FND-SOUND-077: GPLDATA.GFF size 2191945, XXH3-128
2bd1d2b989eef2dcb0545feb95975fca; OBJEX.GFF size 6816516,
XXH3-128 e08d3cac153c547f6804ca27b155915d.

The committed graph reader finds paths from MAS/1 offset zero and MAS/54
offset zero to their respective registration sites. The MAS/54 prefix
through 024F has no intervening script branch or cross-script call, but
its native opcode children still need to return with the stream intact.
Its later cross-script call at 02E7 is after this registration.

From the separately declared local root GPL/241 offset 156A, request 48's
result is assigned to local number zero; the comparison at 1579 and branch
at 157F skip to 15E3 when that value is not positive. The positive path
requests 49 at 1582, requests 9 at 1590 and registers the GPL/239 pair at
159F. The graph takes both conditional outcomes and does not prove the
request result, child preservation, or admission to this local root.

FND-SCRIPT-038 binds each opcode 6E to the same TALK head and seven-byte
record builder. FND-SCRIPT-015's equal-key insertion reuses the existing
node and replaces its prefix. Thus, under valid stable storage and an
admitted equal-key insertion, the GPL/239 registration replaces the
GPL/37 pair rather than adding a second equal-key alternative to choose
later. This conditional replacement needs no new free node; it does not
settle allocation failure or malformed-list behavior for other paths.

FND-SCRIPT-028 supplies the complementary lifetime boundary. A ready-state
region change prunes origin-byte-zero TALK records before calling the new
region's MAS script. On MAS/1 and MAS/54's own intact current-script
bindings, neither is MAS/99, so their builder origin byte is zero.
Consequently a normal admitted region change followed by MAS/54 does not
preserve MAS/1's regional registration as a competing permanent record.
GPL/241's builder byte instead depends on its actual current-script
binding; no complete registration history is inferred from its resource.

## Interpretation

An observed or statically computed key -99 is insufficient to identify
GPL/37 as the selected TALK entry. It must be paired with the registration
history and region-state boundary. The two known later-pair producers
select GPL/239 at 03DD, not the malformed GPL/37 route. This removes the
need to treat equal-key replacement semantics or these literal alternatives
as unidentified mechanisms; actual ordering and retention remain open in
Q-SCRIPT-024. FND-SCRIPT-046's source placement remains a separate binding.
No queue item or whole-entry complete reading is closed by this assessment.

## Alternatives

Treating -99 as a globally unique script address drops the stored entry
pair. Treating regional records as permanently coexisting drops pruning;
treating equal-key insertion as appending a second choice drops replacement.
Conversely, these conditional interfaces do not prove that a native region
transition, GPL/241 branch or TALK event actually occurs.

## How to reproduce

Reuse music-health-all-scripts.txt under
GAME_DIR/analysis/handler-census-20261106/ with the source identities above.
Select top-level decoded opcode 6E lines whose third operand is literal
-99, requiring the MAS/1 control and retaining both other rows. Read the
GPL/241 sequence 156A..15E3 and MAS/54 prefix 0000..024F together.
Use script_reach.py LISTING 6E MAS/1@0 MAS/54@0 GPL/241@5482;
retain key99-registration-routes.txt in that directory. Its edges assume
returning local calls and intact instruction continuations, and unresolved
transfers remain limits. Reuse RULE-SCRIPT-003's branch semantics and the
registration/pruning/consumer findings cited above. No original game or
emulated call was run, and no original-derived output is committed.
