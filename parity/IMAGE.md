# IMAGE

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-IMAGE-001` | Image resource with a list of frames | supported | partial | None | None | supported | `IndexedImage` checks `size` against the resource length and reads `frame_count` and `frame_offsets`, requiring the offsets to rise and stay inside the resource. `StartupAssetExtractor` pairs the title picture with `PAL/11011` and every other startup image with `PAL/1000`, which the spec leaves open, so it carries `PLACEHOLDER: FMT-IMAGE-001`. The packs `DSIX` and `DSOB` keep the decoded frames. Its tests use synthetic files. |
| `FMT-IMAGE-002` | Image frame | supported | complete | None | None | implemented | `IndexedImage` reads `width` and `height` and picks the encoding by the `0xFF` and the tag, as RULE-IMAGE-001 does. Its tests use synthetic files. |
| `FMT-IMAGE-003` | Palette resource | supported | complete | None | None | implemented | `IndexedPalette` requires exactly 768 bytes. Its tests use synthetic files. |
| `FMT-IMAGE-004` | Palette colour | supported | complete | None | None | implemented | `IndexedPalette` rejects a component above 63 and widens each to `(c << 2) \| (c >> 4)`. |
| `RULE-IMAGE-001` | Decoding an image frame, and the row encoding | supported | complete | None | None | implemented | `IndexedImage.ReadRows` follows the procedure, stops after `height` rows or at `0xFF`, and rejects a repeated row, a run past the width and a run whose codes do not give its pixel count. Its tests use synthetic files. |
| `RULE-IMAGE-002` | Decoding a planar image frame | supported | partial | None | None | supported | `IndexedImage.ReadPlanar` follows the procedure but draws nothing for a frame with a bit count of 0, where the procedure takes the dictionary entry, so it carries `PLACEHOLDER: RULE-IMAGE-002`. It rejects a bit count above 8 and a stream that ends early. Its tests use synthetic files. |
