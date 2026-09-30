import test from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, readFileSync, writeFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";
import { copyWorkingTree } from "./copy-working-tree.mjs";
const root = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
test("game rule replacement retains historical declarations without active ownership", t => {
  // Gap 28: the retained predecessor of RULE-SCRIPT-010 is historical.
  const historicalId = ["RULE", "SCRIPT", "001"].join("-");
  const scratch = mkdtempSync(resolve(tmpdir(), "game-rule-ownership-"));
  t.after(() => rmSync(scratch, { recursive: true, force: true }));
  copyWorkingTree(root, scratch);
  const file = resolve(scratch, `spec/rules/${historicalId}.md`);
  const original = readFileSync(file, "utf8");
  assert.match(original, /status: superseded/);
  assert.match(original, /superseded_by: \[RULE-SCRIPT-010\]/);
  assert.match(original, /```historical-text\r?\ndefine load_script/);
  const retained = original.replace("```historical-text", "```text");
  writeFileSync(file, retained);
  const checked = spawnSync(process.execPath, [resolve(root, "vendor/check-documentation.mjs"),
    "--root", scratch, "--check"], { cwd: scratch, encoding: "utf8", timeout: 120000 });
  assert.equal(checked.status, 0, checked.error?.message ?? checked.stdout + checked.stderr);
  assert.equal(readFileSync(file, "utf8"), retained);
});
