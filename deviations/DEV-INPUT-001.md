# DEV-INPUT-001

- Departs from: RULE-INPUT-001, RULE-EXPLORE-001
- Reason: Holding the right mouse button and dragging pans the map, and Alt+Enter switches between a window and full screen.
- Setting: None
- Default: mandatory
- Justification: Both controls add to the interface without taking anything away. A right click that is released without moving still steps the pointer mode as RULE-INPUT-001 says, and edge scrolling works whenever no drag is in progress. The original has no drag gesture, and no key table the spec records binds Alt+Enter (FND-AI-004), so a player who never drags or presses the chord sees the original's controls, and a setting would switch off nothing that player uses.
- Dropped: no

The owner approved both controls on 2026-09-14.

A right-button press starts a drag once the pointer moves while the button is held. Each move pans
the map by 13 world pixels for every 10 logical pointer pixels, in the direction that drags the
world under the pointer, and the release then leaves the pointer mode unchanged. Leaving the
letterboxed canvas clears the drag's anchor, so coming back cannot jump the view, and leaving the
map view cancels the drag.

Either Alt key with Enter toggles full screen once each time the chord becomes complete. Full
screen uses the display's current mode, and leaving it restores the 960x600 window.
