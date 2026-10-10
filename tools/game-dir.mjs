// needs: GAME_DIR
// The licensed installation this project reads. The machine-wide GAME_DIR is ignored: other
// restorations on the same machine set it to their own games (template issue 81).
import {existsSync,readFileSync} from 'node:fs';
import {basename,dirname,join,resolve} from 'node:path';
import {sourceXxh3} from '@scientific-method/executable-reader';

export const VARIABLE = 'DARK_SUN_WAKE_REDUX_GAME_DIR';

// Returns the directory holding the analysis executable named in tools/project-config.json:
// the directory in DARK_SUN_WAKE_REDUX_GAME_DIR when set, otherwise the one in the config.
// Returns null when the config's directory has no copy, so licensed tests skip; throws when
// the chosen copy differs from the recorded length or xxh3, or the override has no copy.
export function gameDir(env = process.env) {
  const config = JSON.parse(readFileSync(new URL('./project-config.json', import.meta.url), 'utf8').replace(/^﻿/, ''));
  const exe = config.original.analysisExecutable;
  const override = env[VARIABLE];
  const dir = override ? resolve(override) : dirname(exe.path);
  const file = join(dir, basename(exe.path));
  if (!existsSync(file)) {
    if (override) throw new Error(`${VARIABLE} is ${dir}, which has no ${basename(exe.path)}`);
    return null;
  }
  const bytes = readFileSync(file);
  if (bytes.length !== exe.byteLength || sourceXxh3(bytes) !== exe.xxh3_128) {
    throw new Error(`${file} differs from the analysis executable recorded in tools/project-config.json`);
  }
  return dir;
}
