# shipped-party

Status: requested
<!-- or: accepted, YYYY-MM-DD / declined: the owner's reason / held, YYYY-MM-DD -->

- Build: BLD-GOG-EN-1.1, the owner's GOG installation under its own DOSBox launcher.
- Settles: Q-PARTY-001 (queue/PARTY.md, Live session).
- Blocks: slice 2.
- Length: about 10 minutes.

Identifies the four characters START GAME supplies and captures the native
party-overview composition without substituting a created party. Start a
fresh normal launch. Do not choose CREATE CHARACTERS, load a save, or enter the
character-transfer utility.

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

Agent, after confirmation: compare each member screen's visible values with
the installed and disc `CHAR` candidates and record which candidate each
matches, or that none does, as dynamic findings with each capture's `xxh3`.
