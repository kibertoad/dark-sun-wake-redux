# ACTOR

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-ACTOR-001` | Object definition | supported | partial | None | None | supported | `GffObjectFrameCatalog` requires 16 bytes and a zero `unk_0E`, reads `x_offset`, `y_offset` and `image`, keeps the other words raw (bytes `0xA` and `0xB` as one word), and checks that `image` names a `BMP ` with frames. It does not put the image numbers 11,001 to 11,008 or 13,009 in place of `image` for the objects and slots `31E0:0EFF` does. `DSOB` keeps the decoded frames and the raw words. Its tests use synthetic files. |
| `FMT-ACTOR-002` | Object data resource | unknown | missing | None | None | unknown | The rebuild keeps `RDFF` resources as raw bytes in `DSOP` and has no reader. |
| `FMT-ACTOR-003` | MONR resource | unknown | missing | None | None | unknown | The rebuild keeps `MONR` as raw bytes in `DSOP`. `OpaqueRecordProfile` and `OpaqueRasterProfile` report statistics of it for local analysis only. |
| `RULE-ACTOR-001` | Drawing a region's placed objects | supported | partial | None | None | supported | `RegionSceneRasterizer` draws the first frame of each entity's image in stored order after the terrain. It subtracts the entity's `vertical_offset` where the spec subtracts the definition's, which is the same value in every shipped record, and it mirrors the image when the entity's bit 7 is set, which carries `PLACEHOLDER: RULE-ACTOR-001`. Its tests use synthetic regions. |
