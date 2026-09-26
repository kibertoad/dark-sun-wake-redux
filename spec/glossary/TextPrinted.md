# TextPrinted

An event: a script prints text. It carries `text: FARPTR<UINT8>`, the address of a NUL-terminated string, and `style: UINT8`, whose use is not known. RULE-SCRIPT-007 emits it. No rule handles it yet; the original passes it at once to the far routine `5702:004D` [FND-SCRIPT-012].
