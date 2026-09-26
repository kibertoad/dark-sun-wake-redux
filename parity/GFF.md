# GFF

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-GFF-001` | GFF resource container | supported | partial | None | None | supported | `GffArchive` checks `signature`, `version` and `header_size` and reads `directory_offset`. It does not read `directory_size`, `unk_14` or `unk_18`, and does not check that the bytes after the directory are covered by nothing. It rejects two resources whose byte ranges overlap without being identical. Its tests use synthetic files. |
| `FMT-GFF-002` | GFF directory | supported | partial | None | None | supported | `GffArchive` reads `tag_list_offset` and `tag_list_end` without checking them, then `tag_count` and the tag tables. It stops before `gap_count` and never reads `gaps`. |
| `FMT-GFF-003` | GFF tag table | supported | complete | None | None | implemented | `GffArchive` reads plain and indexed tables, requires `indexed_count` to equal `entry_count` and the expanded ranges to give that many numbers, and rejects a repeated tag or number. |
| `FMT-GFF-004` | GFF resource entry in a plain tag table | supported | complete | None | None | implemented | `GffArchive` reads all three fields and bounds each range by the file size. |
| `FMT-GFF-005` | GFF byte range | supported | complete | None | None | implemented | `GffArchive` reads both fields in `GFFI` resources. It does not read the gap list, which uses the same record (`FMT-GFF-002`). |
| `FMT-GFF-006` | GFF numbering range in an indexed tag table | supported | complete | None | None | implemented | `GffArchive` reads both fields and rejects a range of length 0. |
| `FMT-GFF-007` | GFFI index resource | supported | complete | None | None | implemented | `GffArchive` requires `entry_count` to match the table and the resource to hold every entry. |
