# REGION

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-REGION-001` | Region name resource | supported | complete | None | None | implemented | `GffRegion` requires one `RNME` resource and reads it as printable ASCII ending with a NUL, at most 64 bytes. `DSRG` keeps the name. Its tests use synthetic files. |
| `FMT-REGION-002` | Region terrain map | supported | partial | None | None | supported | `GffRegion` requires a `MAP ` resource of 12,544 bytes with the `RNME` number and checks that every byte names a tile of the file. It does not look for an `RMAP` resource first, as the original's loader does. `DSRG` keeps the map. Its tests use synthetic files. |
| `FMT-REGION-003` | Region cell flag map | supported | complete | None | None | implemented | `GffRegion` requires a `GMAP` resource of 12,544 bytes and keeps it as raw bytes. It does not clear bit 5 on loading. |
| `FMT-REGION-004` | Region cell flags | supported | complete | None | None | implemented | `GffRegion` keeps each cell's byte unchanged. `RegionTerrainGrid` reads `blocked` as a movement block (RULE-EXPLORE-003) and does not use `occupied`. |
| `FMT-REGION-005` | Region entity table | supported | complete | None | None | implemented | `GffRegion` requires the `ETAB` size to be a multiple of 8 and at most 16,384 records, and checks that each record names an `OJFF` of `OBJEX.GFF`. |
| `FMT-REGION-006` | Region entity record | supported | partial | None | None | supported | `GffRegion` reads all five fields. It uses the absolute value of `object`, and `RegionSceneRasterizer` mirrors the image when `unk_flags_bit_7` is set; both are readings the spec leaves open, so both carry `PLACEHOLDER: FMT-REGION-006`. |
| `RULE-REGION-001` | Drawing a region's terrain tiles | supported | complete | None | `DEV-EXPLORE-001` | implemented | `RegionSceneRasterizer.RasterizeTerrain` draws each view pixel from the tile its map cell names, skipping pixels the tile leaves undrawn. It then draws the `ETAB` objects over the tiles. Its tests use synthetic regions. |
