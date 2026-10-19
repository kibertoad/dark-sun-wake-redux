"""Independent-process strict restore checks for regenerated local checkpoints."""
import hashlib
import json
import os
from pathlib import Path
import re
import sys
import struct
from dataclasses import replace

from dinorefurb_dosbox_session import (
    Media, RUST_DOS_PINNED_SHA256, RustDosConfig, RustDosSession,
    RustDosSettings, SoundBlaster, Target, read_saved_state,
)
sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_drive import ROOT
from regenerate_checkpoints import admit_state, mapping, snapshot
from restore_opener import verify_checkpoint_executable
from dinorefurb_dosbox_session.errors import ConfigurationRefused


def compare(expected, actual, png):
    if any(actual.get(key) != value for key, value in expected["state"].items()):
        raise ValueError("restored named state fields differ")
    if hashlib.sha256(png).hexdigest() != expected["frame_sha256"]:
        raise ValueError("restored frame differs")


def main():
    if len(sys.argv) < 4 or any(not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", x) for x in sys.argv[1:]):
        raise SystemExit("usage: verify_checkpoints.py <source-run> <new-run-prefix> <state> [...]")
    if any(x not in ("start", "added", "dropped", "afterdrop", "talk", "play", "play-ready", "create") for x in sys.argv[3:]):
        raise ValueError("unsupported checkpoint name")
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    source = game / "captures/rust-dos-regenerated" / sys.argv[1]
    report = json.loads((source / "regeneration.json").read_text(encoding="utf-8"))
    delta = mapping(report.get("load_mapping_stop", report["restored_stop"]))
    for name in sys.argv[3:]:
        expected = [x for x in report["checkpoints"] if x["name"] == name]
        if len(expected) != 1:
            raise ValueError("checkpoint lacks a unique expected state/frame")
        state = read_saved_state(source, name)
        verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
        admit_state(state)
        run = game / "captures/rust-dos-regenerated" / f"{sys.argv[2]}-{name}"
        if run.exists():
            raise ValueError("verification run already exists")
        settings = RustDosSettings(
            emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
            emulator_sha256=RUST_DOS_PINNED_SHA256, run_directory=run,
            target=Target("DSUN.EXE", ("-W0", "-L")), start_state=state,
            emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
                deterministic=True, cycles="15000",
                sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1), ultrasound=None),
            readiness_timeout=60.0)
        mismatch = replace(settings,
            run_directory=run.with_name(run.name + "-mismatch"),
            emulator_config=replace(settings.emulator_config,
                sound_blaster=SoundBlaster(base=0x220, irq=7, dma=1)))
        refused = False
        try:
            with RustDosSession(mismatch):
                raise RuntimeError("mismatched sound hardware unexpectedly admitted")
        except ConfigurationRefused as error:
            if "sound_blaster" not in str(error):
                raise
            refused = True
        with RustDosSession(settings) as s:
            result = {"source": sys.argv[1], "name": name, "restored_stop": s.stop,
                      "state": snapshot(s.client, delta), "passed": False,
                      "sound_irq_mismatch_refused_before_launch": refused}
            try:
                if (s.stop["reason"] != "load" or s.stop["cs_ip"] != state.stop["cs_ip"]
                        or s.stop["emu_ns"] != state.stop["emu_ns"]):
                    raise ValueError("restored instruction/time differs")
                png = s.client.screenshot()
                (run / "restored.png").write_bytes(png)
                compare(expected[0], result["state"], png)
                if name in ("play", "play-ready"):
                    leader_address = f"{0x4C13 + delta:04X}:0369"
                    leader = struct.unpack("<H", s.client.read_memory(leader_address, 2).data)[0]
                    result["leader"] = leader
                    if leader != 1 or result["state"]["slot_kinds"] != [0, 2, 0, 2]:
                        raise ValueError("two-character Play state differs from FND-PARTY-178")
                saved = s.save_state("roundtrip", check_files=True)
                s.load_state(saved)
                compare(expected[0], snapshot(s.client, delta), s.client.screenshot())
                result["same_process_roundtrip"] = True
                result["passed"] = True
            finally:
                (run / "restore-check.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
            print(json.dumps({"checkpoint": name, "passed": result["passed"]}), flush=True)


if __name__ == "__main__":
    main()
