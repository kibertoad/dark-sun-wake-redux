# MusicRequested

An event: a script asks for music. It carries `music: UINT16`. RULE-SCRIPT-007 emits it. It has no handlers: the original passes it to the far routine `2D40:0B0F`, which returns at once [FND-SCRIPT-012, FND-SOUND-009].
