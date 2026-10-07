---
id: FND-EXE-076
title: Physical shared-guard literal search finds three candidates absent from the current decoded reference list
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x001FB945..0x001FB948
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x001FB9F7..0x001FB9FA
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x001FBB04..0x001FBB07
tool: Ghidra 12.1.3 PUBLIC reference/context reports and bounded physical literal search
environment: null
---

## Observation

FND-EXE-025/073's pool branches read shared word `0x0242C910` before
selecting their initialization and wait/signal calls. Its producer remains
open under Q-EXE-009. Two searches expose different domains for that question.
Neither executes the original, and neither establishes a complete writer set.

The saved Ghidra program's reference manager reports fourteen references
to that exact address, each labeled READ and shown as a full-word load.
It reports no WRITE, READ_WRITE or address-taking reference to that address.
This is the current analyzer reference list, not proof that the shipped
program never writes the word. A reference list can omit undisassembled
instructions, and computed/indirect accesses need not reference this literal.
FND-EXE-011 records that the original analysis stopped at a timeout.

The independently located bitmap at `0x02427E40` supplies a reference-kind
control: the same query finds the three reads and two direct writes recorded
by FND-EXE-025/073, including their distinct publication instructions. Thus
this query can report writes as well as reads; the guard's absent write label
is not explained by a read-only reporter filter. That control does not prove
that every possible guard writer would be in the analyzer's domain.

An independent physical scan searches all positions at which a four-byte
little-endian encoding of the guard address can fit in the verified shipped
file. It finds these seventeen offsets; the right column maps the literal
bytes themselves through the PE section table, not an assumed instruction
start or an analyzer function boundary:

| Shipped-file literal offset | Preferred-image literal address |
|---|---|
| `0x001FACB6` | `0x005FB8B6` |
| `0x001FAD63` | `0x005FB963` |
| `0x001FADA8` | `0x005FB9A8` |
| `0x001FAE30` | `0x005FBA30` |
| `0x001FAEE3` | `0x005FBAE3` |
| `0x001FAF28` | `0x005FBB28` |
| `0x001FAFB0` | `0x005FBBB0` |
| `0x001FB945` | `0x005FC545` |
| `0x001FB9F7` | `0x005FC5F7` |
| `0x001FBB04` | `0x005FC704` |
| `0x001FC1F2` | `0x005FCDF2` |
| `0x001FC21E` | `0x005FCE1E` |
| `0x001FC27D` | `0x005FCE7D` |
| `0x001FC33E` | `0x005FCF3E` |
| `0x001FC35F` | `0x005FCF5F` |
| `0x001FC7A2` | `0x005FD3A2` |
| `0x001FFC6C` | `0x0060086C` |

Fourteen literal positions fall inside the loads listed by the reference
query. The other three are the separately cited file-data ranges. Requests
for instruction context at their preferred-image literal addresses all
report no containing instruction. They are physically present and mapped;
the failure is missing decoded instruction ownership at those positions,
not an empty physical search or absent file mapping. No candidate is yet
classified as a read, write, pointer, embedded data or reachable instruction.
Their code provenance and incoming paths must be established before a
recovery or decoding result can support a producer claim.

The physical scan's bitmap control finds the five literal occurrences in
FND-EXE-025/073's known three reads and two writes. Its address-taking
control is `0x02427E50`: one of its five physical matches is the input
push in FND-EXE-075's initialization callback. The other four matches are
navigation candidates only in this scan. These controls independently
cover known memory-read, memory-write and immediate-address encodings;
they do not promote all pattern matches to instructions or references.

The physical model searches exact four-byte encodings across the whole
file, including code, headers and data, regardless of analyzer ownership.
It excludes computed addresses, aliases obtained through other pointers,
partial/relative encodings, loader-created data, generated code and any
external writer. A literal hit alone does not identify an access width,
reference kind, instruction boundary, caller or execution path. Conversely,
the absence of a decoded WRITE from the fourteen-reference list cannot
exclude a write in the three undecoded regions or an excluded access model.

## Interpretation

The guard producer search now has independent physical candidates beyond
the current analyzer list. The three undecoded ranges are the next static
leads under Q-EXE-009; the evidence rules out using that list alone as a
complete literal-reference census. No all-writers, never-written, constant
zero, disabled-pool or successful synchronization conclusion is established.
FND-EXE-074/075's local contracts remain conditional on actual admission,
storage and external effects.

## Alternatives

Equating an absent analyzer write with no possible writer, treating a failed
instruction-context lookup as unmapped bytes, or declaring every physical
literal occurrence a real instruction is unsupported. The three candidates
may be undisassembled accesses or non-instruction data; actual incoming code
provenance and bounded decoding settle that distinction, while alias and
startup writers remain separate dependencies of the guard lifecycle.

## How to reproduce

Verify FND-EXE-011's source length and XXH3 identity and use its preferred PE
base. Run ReportReferences with exact targets `0242C910` and control
`02427E40`; its per-target cap is 200 and neither report reaches it.
Record reference kinds rather than treating a function name as the access.
Independently scan every fitting four-byte position in the entire shipped
file for the little-endian values `0x0242C910`, `0x02427E40` and
`0x02427E50`, allowing overlapping hits, with a hard 128-match cap per
value that fails rather than truncates. Map each complete match only through
physically backed section bytes; reject a four-byte match crossing the
section's raw bounds as an address mapping. The three scans finish below
the cap. Use the known reads/writes in FND-EXE-025/073 and the known
address push in FND-EXE-075 as independent positive controls.
Request ReportInstructionContext at exactly `005FC545`, `005FC5F7` and
`005FC704`; preserve its three no-containing-instruction diagnostics.
Keep all reports in local analysis storage. Do not infer code from those
literal positions or recover a guessed entry without its code provenance.
Execute no original program and keep alias, generated and external writer
models excluded from any complete-search claim.
