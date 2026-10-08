---
id: FND-EXE-078
title: Shared guard and pool words occupy virtual-only BSS and have no overlapping declared base-relocation sites
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
    offset: 0x00000080..0x00000177
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00000178..0x000001C7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00000218..0x0000023F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0036F000..0x003A04EB
tool: bounded physical PE header and base-relocation-table reading
environment: null
---

## Observation

FND-EXE-025/073/074's pool branches and wrappers use the shared guard at
`0x0242C910`, bitmap at `0x02427E40`, counter at `0x02427E50` and adjacent
word at `0x02427E54`. FND-EXE-076/077's literal/reference reading does not
establish their initial state or all writers. Physical header and relocation
reading provides a different, explicitly bounded initialization model.

The verified PE's section table begins at shipped offset `0x00000178` and
has nine forty-byte entries. Its `.bss` header at `0x00000218` gives RVA
`0x0035B000`, VirtualSize 30230144, SizeOfRawData zero, PointerToRawData zero
and characteristics `0xC0000080`. At FND-EXE-011's preferred base this is the
half-open virtual interval `0x0075B000..0x0242F680`. The four full words above
fit wholly inside that virtual interval. None has physically backed bytes
in this section, and the zero raw pointer is not a source offset from which
to read their initial values.

For comparison, FND-EXE-075's once words at `0x0071B170` and plus four are
inside physically backed `.data`. That section's header gives RVA
`0x002F0000`, VirtualSize 176836, SizeOfRawData 177152 and PointerToRawData
`0x002EEC00`. Physical mapping reproduces FND-EXE-075's offsets
`0x00319D70..0x00319D77`. The declared entry at `0x00401210` is also physically
backed, in `.text` with RVA `0x00001000` and raw pointer `0x00000400`.
These positive mappings distinguish virtual-only storage from a failed
mapping algorithm or a missing section.

The PE header declares entry RVA `0x00001210`, preferred base `0x00400000`
and SizeOfImage 34058240. Its COFF symbol pointer and symbol count are both
zero. No symbol-table declaration is available here to identify the guard
as a named weak import or a constant. The TLS and load-configuration directory
RVA/size pairs are both zero; that is absence from these declared directories,
not absence of callbacks or initialization through other routes.

### Declared base-relocation sites

The base-relocation directory has RVA `0x02049000`, size 201964 and physical
start `0x0036F000`, ending inclusively at `0x003A04EB`. A complete bounded
parse of that declared directory finds only relocation types zero and three.
For this i386 table, type zero is padding and type three is the four-byte
HIGHLOW relocation form. Every block fits inside the directory and every
site lies inside the declared image. No unsupported type is silently given
a width or interpreted as padding.

Two independently known source-pointer sites are positive controls:

| Relocation site at preferred base | Type | Relocation entry's shipped offset | Source control |
|---|---|---|---|
| `0x005FCD57` | 3 | `0x00393C7C` | FND-EXE-075's pushed pool-input address |
| `0x00716CD4` | 3 | `0x00398C32` | FND-EXE-011's command record name pointer |

No declared type-three four-byte site intersects any byte of the four guard
or pool words listed above. The comparison covers overlaps, not only a site
whose starting address equals the word's start. Thus this declared relocation
model supplies no direct adjustment of those words, while recognizing known
relocated instruction and record pointers elsewhere.

The relocation-site result is not a simulation of a loaded process. It
covers this directory and its observed types only, excluding imported code,
indirect stores, generated state, startup callbacks, aliases, custom loaders
and external writers. Relocation of an instruction operand pointing at a
word is separate from relocation of the word's own storage. The literal
reads in FND-EXE-077 therefore do not constitute direct relocation sites
for the guard itself.

## Interpretation

The guard and pool words have no shipped physical initializer to quote, and
no overlapping site in this verified image's declared base-relocation table.
This rules out explaining a later nonzero guard merely by reading a stored
file word or by a direct relocation of that storage. It does not establish
what the platform loader puts there, what startup code later writes, whether
the guard remains zero, or a complete pool lifetime. Those dependencies
remain Q-EXE-009; FND-EXE-171 follows the declared startup entry separately.

## Alternatives

Reading file offset zero because the section's raw pointer is zero, treating
VirtualSize as a raw source span, omitting partial overlaps, or treating every
relocated pointer operand as a relocation of its target's contents is ruled
out. A zero-filled ordinary load is a possible initial-state model, not a
memory observation or proof that later guards cannot change.

## How to reproduce

Verify FND-EXE-011's source length and XXH3 identity. Read the PE pointer from
the DOS header and validate signature, i386 machine, PE32 optional header,
optional-header size at least 224 and the complete nine-entry section table.
Read the actual entry, base, image-size, COFF symbol and directory fields;
map the exact full words `0x0242C910`, `0x02427E40`, `0x02427E50`,
`0x02427E54` and controls `0x0071B170`, `0x00401210`. Check complete
four-byte virtual and raw bounds independently and reject raw spans outside
the file. Never convert a virtual-only mapping into a read from file zero.

Map the entire declared base-relocation directory to contiguous physically
backed bytes. Parse every block with size at least eight, even byte length
and an end within the directory; decode all two-byte entries with a hard
150000-entry cap. Require every site inside SizeOfImage and reject types
other than zero and three. Bound results to 64. Test type-three intervals
against all four queried full words and require the two named positive
sites above. The scan finishes below both caps. Treat padding as no write,
not a four-byte site, and reject any unsupported type before claiming complete
width coverage. Keep reports local and execute no original program or loader.
