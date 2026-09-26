# SOUND

Next ID: Q-SOUND-003

## Static

- Q-SOUND-002. (entries pending migration): Which code plays the VOC files and
  music, with what codec, sample rate and timing? Settles it: the reader of
  the VOC files and the calls into the sound helper. Blocks: slice 7.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-SOUND-001. (entries pending migration): Does the game audibly change when
  Music or Sound Effects is toggled at a stable screen, and is continuous
  audio present at the start screen and after START GAME? Settles it: the
  audio-presence live session. Tried: searches for a complete VOC header, the
  VOC extension, the sound helper's name and a DOS EXEC near those literals,
  which locate no audio consumer or playback schedule. Blocks: slice 7.

## Source

None.

## Blocked

None.
