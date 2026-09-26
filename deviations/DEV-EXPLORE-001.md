# DEV-EXPLORE-001

- Departs from: RULE-REGION-001, RULE-ACTOR-001, RULE-EXPLORE-001
- Reason: On a display whose shape differs from the original's 320x200 screen, the map view in travel and combat grows to fill the display and shows more of the map around the same centre.
- Setting: None
- Default: mandatory
- Justification: Everything the original's view shows is still shown, at the same scale and around the same centre, and the view still stops at the map's edges. The only change is the extra map around it, which the original leaves to black bars on such a display. A player who wants the original's framing gets it on a 16:10 display, where the view is 320x200.
- Dropped: no

The owner approved the wider view on 2026-09-14.

The view's logical width and height follow from the physical aspect ratio, within bounds. Menus,
the other fixed screens and the overlays on the map keep the original's 320x200 layout, centred and
letterboxed.
