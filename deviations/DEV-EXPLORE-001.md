# DEV-EXPLORE-001

- Departs from: RULE-REGION-001, RULE-ACTOR-001, RULE-EXPLORE-001
- Reason: On a display whose shape differs from the original's 320x200 screen, the map view in travel and combat grows to fill the display and shows more of the map around the same centre.
- Setting: Wide map view
- Default: on
- Justification: The rebuild's view is strictly better. Everything the original's view shows is still shown, at the same scale and around the same centre, and the view still stops at the map's edges. The extra map around it is map the player could scroll to anyway, where the original leaves black bars on such a display. The owner chose on as the default on 2026-09-26. A player who wants the original's framing turns Wide map view off on the launch options screen (DEV-UI-001).
- Dropped: no

The owner approved the wider view on 2026-09-14.

With the setting on, the view's logical width and height follow from the physical aspect ratio,
within bounds. With it off, the map view keeps the original's 320x200 framing, centred and
letterboxed, and the camera, the mapping of the pointer to the map, edge scrolling, the cell a
click walks to and the pointer's shape all use that framing. Menus, the other fixed screens and the
overlays on the map, the F9 conversation preview of DEV-TALK-001 among them, keep the original's
320x200 layout, centred and letterboxed, whatever the setting.
