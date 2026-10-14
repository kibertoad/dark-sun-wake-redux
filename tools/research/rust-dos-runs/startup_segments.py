"""Fixed DSUN.EXE probe: observe segments at program entry and game-main entry.

Run from the repository root with the locked evidence Python. Output is confined
to GAME_DIR/captures/rust-dos-startup-segments/<new run name>. No input, writes,
screenshots, fixed-time advancement or RNG draw is admitted. FND-PARTY-031
identifies main; FND-RNG-001 identifies the generator and its state.
"""
import json
import os
from pathlib import Path
import re
import shutil
import struct
import sys

import xxhash
from dinorefurb_dosbox_session import (
    Media, RUST_DOS_PINNED_SHA256, RustDosConfig, RustDosSession,
    RustDosSettings, SoundBlaster, Target,
)

ROOT = Path(__file__).resolve().parents[3]


def main():
    if len(sys.argv) != 2 or not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", sys.argv[1]):
        raise SystemExit("usage: startup_segments.py <new-run-name: letters/digits/_/->")
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    expected = config["original"]["analysisExecutable"]
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR", Path(expected["path"]).parent)).resolve()
    source = game / "DSUN.EXE"
    body = source.read_bytes()
    if len(body) != expected["byteLength"] or xxhash.xxh3_128_hexdigest(body) != expected["xxh3_128"]:
        raise SystemExit("DSUN.EXE differs from the approved installed build")
    run = game / "captures/rust-dos-startup-segments" / sys.argv[1]
    if run.exists():
        raise SystemExit("run directory exists; choose a new name")

    def prepare(destination):
        # This probe stops before main's first instruction; it needs no assets
        # or save directories on C:. Copy only the verified target.
        shutil.copy2(source, destination / "DSUN.EXE")

    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
        emulator_sha256=RUST_DOS_PINNED_SHA256,
        run_directory=run,
        target=Target("DSUN.EXE", ("-W0", "-L")),
        prepare_drive=prepare,
        emulator_config=RustDosConfig(
            media=(Media("D", game / "game.ins", "iso"),),
            deterministic=True, cycles="15000",
            sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1),
            ultrasound=None,
        ),
        readiness_timeout=60.0,
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

        generator = address(0x1000, 0x0822)
        main_entry = address(0x277B, 0x0024)
        seed_address = address(0x57E0, 0x384A)

        def seed():
            return struct.unpack("<I", session.client.read_memory(seed_address, 4).data)[0]

        report = {"target": "DSUN.EXE -W0 -L", "entry": entry,
                  "initial_rng_state": seed(), "draws": [],
                  "main_entry": main_entry, "generator_guard": generator,
                  "limits": "startup only; no later segment preservation or cleanup claim"}
        session.client.set_breakpoint(generator)
        operation = session.continue_(until=main_entry)
        while True:
            observed = session.observe(operation, timeout=30)
            if not observed.pending:
                break
            print("pending", operation.op, flush=True)
        report["stop"] = observed.stop
        report["main_rng_state"] = seed()
        (run / "startup-segments.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        if observed.stop.get("reason") != "breakpoint" or observed.stop.get("cs_ip") != main_entry:
            raise RuntimeError("main not reached; no draw or different stop may be continued by this probe")
        registers = observed.stop["registers"]
        print(json.dumps({"entry_cs_ip": entry["cs_ip"],
                          "entry_ds": entry["registers"]["ds"],
                          "entry_ss": entry["registers"]["ss"],
                          "main_cs_ip": main_entry, "main_ds": registers["ds"],
                          "main_ss": registers["ss"], "main_sp": registers["sp"],
                          "rng_initial": report["initial_rng_state"],
                          "rng_main": report["main_rng_state"], "draws": []}), flush=True)


if __name__ == "__main__":
    main()
