"""Agent run of DSUN.EXE under rust-dos, deterministic and muted, driven by a plan.

usage: run2.py <run-dir> <plan.json>
plan: {"start": null | "<earlier run dir>:<state>", "steps": [{"keys": [...], "until_ms": N,
       "shot": "name", "save": "state-name"}, ...]}
"""
import json, os, shutil, sys
from pathlib import Path
from dinorefurb_dosbox_session import (Media, RustDosConfig, RustDosSession, RustDosSettings, Target,
                                       RUST_DOS_PINNED_SHA256, read_saved_state)

game = Path(os.environ["GAME_DIR"])  # the installed BLD-GOG-EN-1.1
run = Path(sys.argv[1])
plan = json.loads(Path(sys.argv[2]).read_text())
skip = {"DOSBOX", "analysis", "capture", "game.gog", "game.ins", "cloud_saves", "__support"}

def prepare(c: Path) -> None:
    for p in game.iterdir():
        if p.name in skip or (p.suffix.lower() in (".pdf", ".lnk", ".txt", ".conf", ".ico", ".dll", ".exe",
                                                   ".zip", ".hashdb", ".info", ".script", ".dat", ".msg", ".ini")
                              and p.name.upper() not in ("DSUN.EXE", "SOUND.INI", "DJ.DAT")):
            continue
        if p.is_dir():
            shutil.copytree(p, c / p.name)
        else:
            shutil.copy2(p, c / p.name)
    cfg = bytearray((c / "SOUND.CFG").read_bytes())
    # The sound setup's file names IRQ 5 for the music and the digital driver (words at 2 and 12);
    # the session's rust-dos has its Sound Blaster at IRQ 7 and takes no configuration.
    assert cfg[2:4] == bytes([5, 0]) and cfg[12:14] == bytes([5, 0]), bytes(cfg[:16]).hex()
    cfg[2:4] = cfg[12:14] = bytes([7, 0])
    (c / "SOUND.CFG").write_bytes(bytes(cfg))

config = RustDosConfig(media=(Media("D", game / "game.ins", "iso"),), deterministic=True, cycles="15000")
start = None
if plan.get("start"):
    d, name = plan["start"].rsplit(":", 1)
    start = read_saved_state(Path(d), name)
settings = RustDosSettings(
    emulator=Path("artifacts/rust-dos/rust-dos.exe"),
    emulator_sha256=RUST_DOS_PINNED_SHA256,
    run_directory=run,
    target=Target("DSUN.EXE", ("-W0", "-L")),
    prepare_drive=None if start else prepare,
    emulator_config=config,
    readiness_timeout=60.0,
    start_state=start,
)

def wait(s, op):
    ob = s.observe(op, timeout=600)
    while ob.pending:
        ob = s.observe(op, timeout=600)
    return ob

with RustDosSession(settings) as s:
    for k, step in enumerate(plan["steps"]):
        # A deterministic machine takes input only while paused and delivers it once resumed, so
        # queue the input, run to the step's time, and only then see each input delivered.
        inputs = [s.send_input("key", {"key": key}) for key in step.get("keys", [])]
        if "type" in step:
            inputs.append(s.send_input("type", {"text": step["type"]}))
        if "mouse" in step:
            inputs.append(s.send_input("mouse", step["mouse"]))
        if "until_ms" in step:
            ob = wait(s, s.continue_(until_ms=step["until_ms"]))
            print(k, step["until_ms"], ob.stop.get("reason"), ob.stop.get("cs_ip"), flush=True)
        for op in inputs:
            ob = s.observe(op, timeout=5)
            if ob.pending:
                raise SystemExit(f"input of step {k} not delivered by {step.get('until_ms')}")
        if step.get("status"):
            print("  mouse", s.client.status().get("mouse"))
        for addr, n in step.get("read", []):
            print("  mem", addr, s.client.read_memory(addr, n).data.hex())
        if "shot" in step:
            (run / f"{step['shot']}.png").write_bytes(s.client.screenshot())
        if "save" in step:
            st = s.save_state(step["save"], check_files=True)
            print("  saved", step["save"], st.checkpoint)
