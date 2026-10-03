import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, readFileSync, existsSync, rmSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { tmpdir } from 'node:os';
import { spawnSync } from 'node:child_process';
const root = resolve(import.meta.dirname, '../..');
const project = join(root, 'src/DarkSunWakeRedux.Game/DarkSunWakeRedux.Game.csproj');
const args = ['restore', project, '--runtime', 'win-x64', '-p:RuntimeIdentifier=win-x64', '-p:SelfContained=true', '-p:PackageLockProfile=win-x64-directory', '--locked-mode'];
const options = { encoding: 'utf8', timeout: 30000, cwd: root, env: { ...process.env, CI: 'true' } };
function restore(extra) {
  const commandArgs = [...args, ...extra];
  if (process.platform !== 'win32') return spawnSync('dotnet', commandArgs, options);
  // PowerShell honors a configured dotnet.cmd launcher; Node's direct lookup chooses dotnet.exe.
  const command = '& dotnet ' + commandArgs.map(value => "'" + value.replaceAll("'", "''") + "'").join(' ') + '; exit $LASTEXITCODE';
  return spawnSync('powershell', ['-NoProfile', '-EncodedCommand', Buffer.from(command, 'utf16le').toString('base64')], options);
}

test('locked packaging rejects a missing lock before generating a replacement', t => {
  const dir = mkdtempSync(join(tmpdir(), 'packaging-missing-'));
  t.after(() => rmSync(dir, { recursive: true, force: true }));
  const missing = join(dir, 'missing.json');
  const result = restore([`-p:NuGetLockFilePath=${missing}`]);
  assert.notEqual(result.status, 0);
  assert.match(result.stdout + result.stderr, /Committed packaging lock required/);
  assert.equal(existsSync(missing), false);
});

test('locked packaging rejects a single-file request against directory dependencies without changing the lock', () => {
  const file = join(root, 'packaging/locks/win-x64-directory/DarkSunWakeRedux.Game.packages.lock.json');
  const before = readFileSync(file);
  const result = restore(['-p:PublishSingleFile=true']);
  assert.notEqual(result.status, 0);
  assert.match(result.stdout + result.stderr, /NU1004/);
  assert(readFileSync(file).equals(before));
});

test('locked packaging admits the explicit cross-host runtime without changing its lock', () => {
  const file = join(root, 'packaging/locks/win-x64-directory/DarkSunWakeRedux.Game.packages.lock.json');
  const before = readFileSync(file);
  const result = restore([]);
  assert.equal(result.status, 0, result.error?.message || result.stdout + result.stderr);
  assert(readFileSync(file).equals(before));
});
