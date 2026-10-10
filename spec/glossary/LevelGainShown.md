# LevelGainShown

An event: the game shows that a character gained a level. It carries `name: char[]`, the
character's name, `level`, the new level, and `code`, the stored class code. RULE-PARTY-013 emits
it. No rule handles it yet; the original formats `%Fs is %d%s level %Fs`, the string at
`57E0:2B68` in BLD-GOG-EN-1.1, with the name, the level, the suffix `th` for a level above 3,
`rd` for 3 and `nd` otherwise (`57E0:2B7E`, `57E0:2B81`, `57E0:2B84`) and the class name through
the far pointer at `57E0:1483` plus 4 times the code; when that text is longer than 29
characters it formats `%Fs gains a level` (`57E0:2B87`) with the name instead. It passes the text
through `3150:000E` to the far routine `566A:002A` [FND-PARTY-081, FND-PARTY-057].
