"""Fixed, read-only native probe of main's start-window opener call site."""
import json
import os
from pathlib import Path
import re
import struct
import sys

from dinorefurb_dosbox_session import (
    Media, RUST_DOS_PINNED_SHA256, RustDosConfig, RustDosSession,
    RustDosSettings, SoundBlaster, Target,
)

sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_drive import prepare_drive, ROOT


def wait_for_stop(session, operation, attempts=10):
    for attempt in range(attempts):
        observed = session.observe(operation, timeout=30)
        if not observed.pending:
            return observed, False
        print("pending", operation.op, attempt + 1, flush=True)
    session.pause()
    # pause returns a reply mapping; observe the interrupted continuation.
    while True:
        observed = session.observe(operation, timeout=30)
        if not observed.pending:
            return observed, True


def main():
    if len(sys.argv) != 2 or not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", sys.argv[1]):
        raise SystemExit("usage: startup_window.py <new-run-name>")
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                              Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    run = game / "captures/rust-dos-startup-window" / sys.argv[1]
    if run.exists():
        raise SystemExit("run directory exists; choose a new name")
    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
        emulator_sha256=RUST_DOS_PINNED_SHA256, run_directory=run,
        target=Target("DSUN.EXE", ("-W0", "-L")),
        prepare_drive=lambda destination: prepare_drive(game, destination),
        emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
                                     deterministic=True, cycles="15000",
                                     sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1),
                                     ultrasound=None), readiness_timeout=60.0,
    )
    with RustDosSession(settings) as session:
        entry = session.entry
        if entry["cs_ip"].split(":")[1].upper() != "0000":
            raise RuntimeError("unexpected DSUN entry offset")
        loaded = int(entry["registers"]["cs"], 16)

        def address(segment, offset):
            actual = segment - 0x1000 + loaded
            if not 0 <= actual <= 0xFFFF:
                raise RuntimeError("translated segment outside real mode")
            return f"{actual:04X}:{offset:04X}"

        seed_address = address(0x57E0, 0x384A)
        def seed():
            return struct.unpack("<I", session.client.read_memory(seed_address, 4).data)[0]

        destination = address(0x277B, 0x02E4)
        guard = address(0x1000, 0x0822)
        report = {"entry": entry, "initial_rng_state": seed(), "draws": [],
                  "destination": destination, "generator_guard": guard,
                  "target": "DSUN.EXE -W0 -L", "inputs": [], "writes": []}
        session.client.set_breakpoint(guard)
        operation = session.continue_(until=destination)
        observed, exhausted = wait_for_stop(session, operation)
        if exhausted:
            report["limit"] = "wall-clock observation limit; no destination claim"
        report["stop"] = observed.stop
        report["final_rng_state"] = seed()
        reached = (observed.stop.get("reason") == "breakpoint"
                   and observed.stop.get("cs_ip") == destination)
        if reached:
            registers = observed.stop["registers"]
            report["opener_argument_word"] = struct.unpack(
                "<H", session.client.read_memory(f"{registers['ss']}:{registers['sp']}", 2).data)[0]
        (run / "startup-window.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        print(json.dumps({"reached": reached, "stop": observed.stop.get("cs_ip"),
                          "reason": observed.stop.get("reason"),
                          "initial_seed": report["initial_rng_state"],
                          "final_seed": report["final_rng_state"],
                          "argument": report.get("opener_argument_word")}), flush=True)
        if not reached:
            raise RuntimeError("start-window call not reached; stop is not admitted for continuation")


if __name__ == "__main__":
    main()
