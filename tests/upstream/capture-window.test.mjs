import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, writeFileSync, rmSync, mkdirSync, existsSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';
test('worker times out a stalled renderer and captures physical client edges on recovery', t => {
  if (process.platform !== 'win32') return t.skip('Windows worker acceptance');
  const scratch = mkdtempSync(join(tmpdir(), 'capture-worker-'));
  t.after(() => rmSync(scratch, { recursive: true, force: true }));
  const result = spawnSync('powershell.exe', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File',
    resolve(import.meta.dirname, 'capture-worker.ps1'),
    resolve(import.meta.dirname, '../../tools/Capture-OriginalWindow.ps1'), scratch],
    { encoding: 'utf8', timeout: 30000 });
  assert.equal(result.status, 0, result.stdout + result.stderr);
});
test('direct capture reads an offscreen synthetic window and rejects invalid or blank results', t => {
  if (process.platform !== 'win32') return t.skip('Windows PrintWindow acceptance');
  const scratch = mkdtempSync(join(tmpdir(), 'window-capture-'));
  t.after(() => rmSync(scratch, { recursive: true, force: true }));
  const runner = join(scratch, 'acceptance.ps1');
  writeFileSync(runner, String.raw`
param($Source, $Output, [switch]$ForceUniformPositive)
$ErrorActionPreference = 'Stop'
$form=$null
$stage='setup'
function Write-CaptureDiagnostics($Failure) {
 $details=[ordered]@{stage=$stage;error=$Failure;handleCreated=$false}
 if($null -ne $form) {
  $details.handleCreated=$form.IsHandleCreated
  $details.visible=$form.Visible
  $details.clientWidth=$form.ClientSize.Width
  $details.clientHeight=$form.ClientSize.Height
  $details.uniform=$form.Uniform
  $details.readyVerified=$form.ReadyVerified
  $details.paintCount=$form.PaintCount
  $details.printCount=$form.PrintCount
  $details.printClientCount=$form.PrintClientCount
 }
 [Console]::Error.WriteLine('capture-fixture: '+($details | ConvertTo-Json -Compress))
}
try {
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
$sourceText = Get-Content -LiteralPath $Source -Raw
$native = [regex]::Match($sourceText, "(?s)Add-Type -TypeDefinition @'\r?\n(.*?)\r?\n'@").Groups[1].Value
Add-Type -TypeDefinition $native
$tokens=$null; $errors=$null
$ast=[Management.Automation.Language.Parser]::ParseInput($sourceText,[ref]$tokens,[ref]$errors)
if ($errors.Count) { throw 'Capture helper did not parse' }
$function=$ast.Find({param($n) $n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq 'Save-DirectScreenFrame'}, $true)
Invoke-Expression $function.Extent.Text
Add-Type -ReferencedAssemblies System.Windows.Forms,System.Drawing -TypeDefinition @'
using System; using System.Drawing; using System.Windows.Forms; using System.Runtime.InteropServices;
public class CaptureCanvas : Form {
 [DllImport("dwmapi.dll")] public static extern int DwmFlush();
 public bool Uniform, ReadyVerified;
 public int PaintCount, PrintCount, PrintClientCount;
 public CaptureCanvas(){FormBorderStyle=FormBorderStyle.None;AutoScaleMode=AutoScaleMode.None;ShowInTaskbar=false;ClientSize=new Size(160,96);StartPosition=FormStartPosition.Manual;Location=new Point(0,0);}
 protected override bool ShowWithoutActivation { get { return true; } }
 protected override void OnPaint(PaintEventArgs e) { PaintCount++; e.Graphics.Clear(Color.Red); if(!Uniform) e.Graphics.FillRectangle(Brushes.Lime,16,0,16,16); }
 protected override void WndProc(ref Message m) {
  if(m.Msg==0x317 || m.Msg==0x318) {
   if(m.Msg==0x317) PrintCount++; else PrintClientCount++;
   using(var g=Graphics.FromHdc(m.WParam)) {
    g.Clear(Color.Red); if(!Uniform) g.FillRectangle(Brushes.Lime,16,0,16,16);
   } m.Result=new IntPtr(1); return;
  } base.WndProc(ref m);
 }
}
'@
function Wait-FixtureReady($Canvas) {
 $stage='fixture-readiness'
 $deadline=[Diagnostics.Stopwatch]::StartNew()
 do {
  [Windows.Forms.Application]::DoEvents(); $Canvas.Refresh()
  if($Canvas.ClientSize.Width -ne 160 -or $Canvas.ClientSize.Height -ne 96) {throw 'Synthetic fixture client size differs from 160x96'}
  $probe=[Drawing.Bitmap]::new(160,96)
  try {
   $Canvas.DrawToBitmap($probe,[Drawing.Rectangle]::new(0,0,160,96))
   $right=if($Canvas.Uniform){[Drawing.Color]::Red}else{[Drawing.Color]::Lime}
   $ready=$probe.GetPixel(4,8).ToArgb() -eq [Drawing.Color]::Red.ToArgb() -and $probe.GetPixel(24,8).ToArgb() -eq $right.ToArgb()
  } finally {$probe.Dispose()}
  if($ready){
   if([CaptureCanvas]::DwmFlush() -ne 0){throw 'Synthetic compositor readiness flush failed'}
   $Canvas.ReadyVerified=$true
   return
  }
  Start-Sleep -Milliseconds 25
 } while($deadline.ElapsedMilliseconds -lt 2000)
 throw 'Synthetic source pixels were not ready within 2000ms'
}
$form=[CaptureCanvas]::new()
 $form.Uniform=[bool]$ForceUniformPositive
 $stage='show'
 $form.Show(); $form.Refresh()
 $stage='fixture-readiness'
 Wait-FixtureReady $form
 $form.Location=[Drawing.Point]::new(-10000,-10000)
 $handle=$form.Handle
 $bounds=[pscustomobject]@{Width=160;Height=96;X=0;Y=0}
 $path=Join-Path $Output 'valid.png'
 $form.Uniform=[bool]$ForceUniformPositive
 $stage='positive-capture'
 Save-DirectScreenFrame $handle $bounds $path
 $stage='positive-pixels'
 $bitmap=[Drawing.Bitmap]::new($path)
 try {
  if ($bitmap.GetPixel(4,8).ToArgb() -ne [Drawing.Color]::Red.ToArgb() -or
      $bitmap.GetPixel(24,8).ToArgb() -ne [Drawing.Color]::Lime.ToArgb()) {throw 'Captured pixels are not from the target renderer'}
 } finally {$bitmap.Dispose()}
 $form.Dispose()
 $form=[CaptureCanvas]::new(); $form.Uniform=$true
 $stage='blank-show'; $form.Show(); $form.Refresh()
 $stage='blank-readiness'; Wait-FixtureReady $form
 $form.Location=[Drawing.Point]::new(-10000,-10000)
 $handle=$form.Handle
 $stage='blank-rejection'
 try {Save-DirectScreenFrame $handle $bounds (Join-Path $Output 'blank.png');throw 'Blank result accepted'}
 catch {if($_.Exception.Message -notmatch 'uniform frame'){throw}}
 $stage='invalid-handle-rejection'
 try {Save-DirectScreenFrame ([IntPtr]0) $bounds (Join-Path $Output 'invalid.png');throw 'Invalid window accepted'}
 catch {if($_.Exception.Message -notmatch 'does not support direct capture'){throw}}
 $stage='rejected-output-check'
 if((Test-Path (Join-Path $Output 'blank.png')) -or (Test-Path (Join-Path $Output 'invalid.png'))){throw 'Rejected frame written'}
 $stage='complete'
 Write-CaptureDiagnostics $null
} catch {
 Write-CaptureDiagnostics $_.Exception.Message
 throw
} finally {if($null -ne $form){$form.Dispose()}}
`);
  const result = spawnSync('powershell.exe', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', runner,
    resolve(import.meta.dirname, '../../tools/Capture-OriginalWindow.ps1'), scratch], { encoding: 'utf8', timeout: 30000 });
  assert.equal(result.status, 0, result.stdout + result.stderr);
  const success = JSON.parse(result.stderr.split(/\r?\n/).find(line => line.startsWith('capture-fixture: ')).slice('capture-fixture: '.length));
  assert.equal(success.stage, 'complete');
  const rejectedOutput = join(scratch, 'diagnostic-control');
  mkdirSync(rejectedOutput);
  const rejected = spawnSync('powershell.exe', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', runner,
    resolve(import.meta.dirname, '../../tools/Capture-OriginalWindow.ps1'), rejectedOutput, '-ForceUniformPositive'],
    { encoding: 'utf8', timeout: 30000 });
  assert.equal(rejected.status, 1, rejected.stdout + rejected.stderr);
  const diagnostics = JSON.parse(rejected.stderr.split(/\r?\n/).find(line => line.startsWith('capture-fixture: ')).slice('capture-fixture: '.length));
  assert.equal(diagnostics.stage, 'positive-capture');
  assert.match(diagnostics.error, /uniform frame/);
  assert.equal(diagnostics.handleCreated, true);
  assert.equal(diagnostics.visible, true);
  assert.ok(diagnostics.clientWidth >= 32, 'client contains the sampled width');
  assert.ok(diagnostics.clientHeight >= 16, 'client contains the sampled height');
  assert.equal(diagnostics.uniform, true);
  assert.equal(diagnostics.readyVerified, true);
  assert.equal(diagnostics.clientWidth, 160);
  assert.equal(diagnostics.clientHeight, 96);
  assert.ok(Number.isInteger(diagnostics.printCount) && diagnostics.printCount >= 0);
  assert.ok(Number.isInteger(diagnostics.printClientCount) && diagnostics.printClientCount >= 0);
  assert.equal(existsSync(join(rejectedOutput, 'valid.png')), false);
});
