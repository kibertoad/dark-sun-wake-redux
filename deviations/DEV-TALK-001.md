# DEV-TALK-001

- Departs from: SCR-UI-012, RULE-TALK-001
- Reason: F9 opens a preview of the opening conversation over the map, because the rebuild does not yet start conversations from the map.
- Setting: None
- Default: mandatory
- Justification: Without the preview the rebuild offers no way to reach a conversation at all, so it adds to the interface. F9's action in the original goes to the handler at offset `0x0FDB` of the key routine in FND-AI-004, which is not read yet; the rebuild has no F9 action to give up. The preview is a stopgap, and this deviation is dropped when conversations start from the map.
- Dropped: no

The preview draws the conversation windows of SCR-UI-012 at the original's scale on the fixed
320x200 canvas over the map, with the portrait, scroll controls, response rows and the text of the
opening conversation in the game's font. It runs the choices RULE-TALK-001 offers on the opening
pages, stores the chosen choice's index and branch target, and applies only the effects of the
branches the rebuild has traced; any other variable fails closed. Closing it restores the wide map
view of DEV-EXPLORE-001. Line wrapping is the rebuild's own greedy wrap.
