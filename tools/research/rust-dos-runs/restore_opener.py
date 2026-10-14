"""Restore an owned opener checkpoint and execute only its declared opener call."""
import json
import os
from pathlib import Path
import re
import struct
import sys
import xxhash

from dinorefurb_dosbox_session import (
    Media, RUST_DOS_PINNED_SHA256, RustDosConfig, RustDosSession,
    RustDosSettings, SoundBlaster, Target, read_saved_state,
)

sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_drive import ROOT
from startup_window import wait_for_stop, checkpoint_summary


def verify_checkpoint_executable(state, expected):
    source = state.directory / "drive-c/DSUN.EXE"
    size = expected["byteLength"]
    if type(size) is not int or not 0 < size <= 1024 * 1024:
        raise ValueError("invalid configured DSUN executable bound")
    if not source.is_file() or source.stat().st_size != size:
        raise ValueError("checkpoint DSUN.EXE differs from the approved installed build")
    with source.open("rb") as stream:
        body = stream.read(size + 1)
    if len(body) != size or xxhash.xxh3_128_hexdigest(body) != expected["xxh3_128"]:
        raise ValueError("checkpoint DSUN.EXE fingerprint differs from the approved installed build")


def opener_mapping(saved_stop, call):
    segment, offset = (int(part, 16) for part in saved_stop.split(":"))
    loaded = segment - 0x277B + 0x1000
    target = 0x574B - 0x1000 + loaded
    if offset != 0x02E4 or not 0 <= loaded <= 0xFFFF or not 0 <= target <= 0xFFFF:
        raise ValueError("checkpoint is not at the declared opener call")
    if call != struct.pack("<BHH", 0x9A, 0x0020, target):
        raise ValueError("checkpoint instruction is not the declared opener call")
    return loaded


def main():
    if len(sys.argv) != 3 or any(not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", name) for name in sys.argv[1:]):
        raise SystemExit("usage: restore_opener.py <source-startup-run-name> <new-run-name>")
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                              Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    source = game / "captures/rust-dos-startup-window" / sys.argv[1]
    state = read_saved_state(source, "opener")
    verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
    if state.stop is None or state.emulator_sha256 != RUST_DOS_PINNED_SHA256:
        raise SystemExit("source checkpoint lacks the approved saved stop or emulator identity")
    run = game / "captures/rust-dos-restore-opener" / sys.argv[2]
    if run.exists():
        raise SystemExit("run directory exists; choose a new name")
    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
        emulator_sha256=RUST_DOS_PINNED_SHA256, run_directory=run,
        target=Target("DSUN.EXE", ("-W0", "-L")), start_state=state,
        emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
                                     deterministic=True, cycles="15000",
                                     sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1),
                                     ultrasound=None), readiness_timeout=60.0,
    )
    with RustDosSession(settings) as session:
        restored = session.stop
        report = {"source_run": sys.argv[1], "source_state": "opener", "load_stop": restored,
                  "draws": [], "inputs": [], "writes": []}
        def record():
            (run / "restore-opener.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        record()
        if restored is None or restored.get("reason") != "load" or restored.get("cs_ip") != state.stop.get("cs_ip"):
            raise RuntimeError("source opener checkpoint did not restore at its declared stop")
        call = session.client.read_memory(restored["cs_ip"], 5).data
        loaded = opener_mapping(restored["cs_ip"], call)
        def address(segment, offset):
            mapped = segment - 0x1000 + loaded
            if not 0 <= mapped <= 0xFFFF:
                raise RuntimeError("translated segment outside real mode")
            return f"{mapped:04X}:{offset:04X}"
        def seed():
            return struct.unpack("<I", session.client.read_memory(address(0x57E0, 0x384A), 4).data)[0]
        report["initial_rng_state"] = seed()
        session.client.set_breakpoint(address(0x1000, 0x0822))
        session.client.set_breakpoint(address(0x1000, 0x0811))
        destination = address(0x277B, 0x02E9)
        operation = session.continue_(until=destination)
        observed, exhausted = wait_for_stop(session, operation)
        report["return_stop"] = observed.stop
        report["final_rng_state"] = seed()
        report["diagnostic_limit"] = exhausted
        record()
        if observed.stop.get("reason") != "breakpoint" or observed.stop.get("cs_ip") != destination:
            raise RuntimeError("opener return not reached; different stop is not admitted")
        if report["final_rng_state"] != report["initial_rng_state"]:
            raise RuntimeError("RNG state changed during opener call")
        saved = session.save_state("opened", check_files=True)
        report["checkpoint"] = checkpoint_summary(saved)
        record()
        print(json.dumps({"restored": restored["cs_ip"], "returned": destination,
                          "seed": report["final_rng_state"], "checked_save": saved.checkpoint}), flush=True)


if __name__ == "__main__":
    main()
