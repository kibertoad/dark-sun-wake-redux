# SoundRequested

An event: a script asks for a sound. It carries `sound: UINT16`. RULE-SCRIPT-007 emits it. Its handler is RULE-SOUND-001, run at once; the original passes the low byte of `sound` to the sound-effect routine through the far routine `2D40:0B00` [FND-SCRIPT-012, FND-SOUND-009].
