# NamedMessageShown

An event: the game shows a one-line message with a combatant's name in place of its `%Fs`. It carries `message: char[]`, the text with `%Fs`, and `name: char[]`. RULE-COMBAT-004 emits it. No rule handles it yet; the original formats the text through `3150:000E` and passes it to the far routine `566A:002A` [FND-COMBAT-025].
