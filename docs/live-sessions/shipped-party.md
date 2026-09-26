# shipped-party

Status: requested
<!-- or: accepted, YYYY-MM-DD / declined: the owner's reason / held, YYYY-MM-DD -->

- Build: BLD-GOG-EN-1.1, the owner's GOG installation under its own DOSBox launcher.
- Settles: Q-PARTY-001 (queue/PARTY.md, Live session).
- Blocks: slice 2.
- Length: about 10 minutes.

Confirms which four characters START GAME supplies. A routine of the game
loads characters 40 to 43 into the four party slots (FND-PARTY-013), and
earlier captures taken after some play match 40, 41 or 53, 42, and 33 or 43
(FND-PARTY-020); captures taken before any play tell 41 from 53 and 43 from 33.
Start a fresh normal launch. Do not choose CREATE CHARACTERS, load a save, or
enter the character-transfer utility, and do not move, fight or rest before
the captures.

## Script

1. S0, Q-PARTY-001. At the initial start screen, choose START GAME once.
   Captures: the first fully populated party-overview screen after the
   transition finishes.
2. S1, Q-PARTY-001. If the overview exposes a native member-select or View
   Character action, open the first visible member and then return. Captures:
   the member screen and the returned overview. Record the exact input used to
   open and to return.
3. S2, Q-PARTY-001. Repeat S1 for each other distinct visible member, in
   on-screen order. Captures: each member screen and returned overview. Do not
   click an unknown blank or application area.

Agent, after confirmation: compare each member screen's visible values
(RULE-PARTY-006) with `CHAR` records 40 to 43 and with 53 and 33, and record
which record each matches, or that none does, as dynamic findings with each
capture's `xxh3`.
