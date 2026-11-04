"""Fixed native F1/Enter save in an owned private drive; no guest setup writes."""
import hashlib
import json
import os
from pathlib import Path
import re
import struct
import sys

from dinorefurb_dosbox_session import (
    Media, RUST_DOS_PINNED_SHA256, RustDosConfig, RustDosSession,
    RustDosSettings, SoundBlaster, Target, read_saved_state,
)

sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_drive import ROOT
from restore_opener import verify_checkpoint_executable
from regenerate_checkpoints import admit_state, mapping, snapshot, ready, checked_read
from load_entry import fields, map_ready, named_file, returned_frame, input_map_ready, observe_delivery
from checkpoint_rng import Recorder
from startup_window import wait_for_stop, checkpoint_summary
from getter_metadata import capture as capture_getter_metadata


def save_ready(state, ui):
    return (ready(state, 18500) and ui["load_mode"] == 0
            and ui["selected_row"] == 1 and ui["list_top"] == 0 and ui["exit_byte"] == 0)


def save_root(client, callback):
    seg, off = (int(x, 16) for x in callback.split(":"))
    jump = checked_read(client, seg, off, 5)
    if jump[0] != 0xEA or struct.unpack_from("<H", jump, 1)[0] != 0x785:
        raise ValueError("Save callback differs from the declared overlay entry")
    seg = struct.unpack_from("<H", jump, 3)[0]
    call = checked_read(client, seg, 0xBEF, 3)
    if call != struct.pack("<BH", 0xE8, (0x3FB - 0xBF2) & 0xFFFF):
        raise ValueError("Save call differs from the declared near call")
    return f"{seg:04X}:03FB"


def save_return(client, registers):
    frame = checked_read(client, int(registers["ss"], 16), int(registers["sp"], 16), 6)
    off, seg, row = struct.unpack("<HHH", frame)
    if off != 0xBF2 or row != 1:
        raise ValueError("Save frame differs from the declared row-one caller")
    return f"{seg:04X}:{off:04X}"


def main():
    if (len(sys.argv) not in (3, 4) or
            any(not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", x) for x in sys.argv[1:]) or
            (len(sys.argv) == 4 and sys.argv[3] != "verify")):
        raise SystemExit("usage: native_save.py <source-run> <new-run> [verify]")
    verifying = len(sys.argv) == 4
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    source = game / ("captures/rust-dos-native-save" if verifying else "captures/rust-dos-regenerated") / sys.argv[1]
    meta = json.loads((source / ("native-save.json" if verifying else "regeneration.json")).read_text(encoding="utf-8"))
    state = read_saved_state(source, "saved-map" if verifying else "play-ready")
    verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
    admit_state(state)
    delta = mapping(meta["load_mapping_stop"])
    rng = Recorder(delta)
    run = game / "captures/rust-dos-native-save" / sys.argv[2]
    if run.exists():
        raise ValueError("run already exists")
    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
        emulator_sha256=RUST_DOS_PINNED_SHA256, run_directory=run,
        target=Target("DSUN.EXE", ("-W0", "-L")), start_state=state,
        emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
            deterministic=True, cycles="15000", sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1),
            ultrasound=None), readiness_timeout=60.0)
    report = {"source": sys.argv[1], "load_mapping_stop": meta["load_mapping_stop"],
              "inputs": [], "writes": [], "draws": [], "relocations": [], "passed": False}
    with RustDosSession(settings) as session:
        def record():
            (run / "native-save.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        try:
            report["restored_stop"] = session.stop
            if (session.stop["reason"] != "load" or session.stop["cs_ip"] != state.stop["cs_ip"]
                    or session.stop["emu_ns"] != state.stop["emu_ns"]):
                raise ValueError("restored instruction/time differs")
            current = snapshot(session.client, delta)
            if not (input_map_ready(session.client, delta, current, fields(session.client, delta)) if verifying else map_ready(current)):
                raise ValueError("source is not the admitted map state")
            report["save_before"] = named_file(run, "SAVE02.SAV")
            if verifying:
                png = session.client.screenshot()
                if (current != meta["saved_state"] or report["save_before"] != meta["save_after"]
                        or hashlib.sha256(png).hexdigest() != meta["frame_sha256"]):
                    raise ValueError("restored map, native save or frame differs")
                saved = session.save_state("roundtrip", check_files=True)
                report["getter_metadata"] = capture_getter_metadata(session.client, delta)
                session.load_state(saved)
                if (snapshot(session.client, delta) != current or named_file(run, "SAVE02.SAV") != meta["save_after"]
                        or hashlib.sha256(session.client.screenshot()).hexdigest() != meta["frame_sha256"]):
                    raise ValueError("same-process roundtrip differs")
                report["passed"] = True
                report["same_process_roundtrip"] = True
                if capture_getter_metadata(session.client, delta) != report["getter_metadata"]:
                    report["passed"] = False
                    raise ValueError("same-process getter metadata differs")
                print(json.dumps({"verified": True}), flush=True)
                return
            if report["save_before"]["exists"]:
                raise ValueError("fixed native-save slot already exists")
            for off in (0x822, 0x811):
                session.client.set_breakpoint(rng.address(0x1000, off))
            key_name = "f1"
            key = session.send_input("key", {"key": key_name, "hold_ms": 100})
            delivered = None
            phase, previous = "window", None
            root = destination = watch = None
            builder_armed = False
            for index in range(1024):
                target = int(session.stop["emu_ns"]) // 1000000 + 100
                observed, exhausted = wait_for_stop(session, session.continue_(until_ms=target))
                stop = observed.stop
                report["last_stop"] = stop
                report["observations"] = index + 1
                if exhausted:
                    raise RuntimeError("operation observation budget exhausted")
                if stop["reason"] == "breakpoint" and stop["cs_ip"] == rng.address(0x1000, 0x822):
                    report["draws"].append(rng.record(session, stop))
                    record()
                    continue
                if stop["reason"] == "watchpoint" and watch is not None:
                    new = save_return(session.client, report["save_entry"]["registers"])
                    report["relocations"].append({"before": destination, "after": new, "stop": stop})
                    if new != destination:
                        session.client.remove_breakpoints(destination)
                        destination = new
                        session.client.set_breakpoint(destination, once=True)
                    record()
                    continue
                if root is not None and stop["reason"] == "breakpoint" and stop["cs_ip"] == root:
                    report["save_entry"] = stop
                    ui = fields(session.client, delta)
                    if ui["load_mode"] or ui["selected_row"] != 1 or ui["list_top"]:
                        raise ValueError("Save entry state differs from admitted row one/top zero")
                    destination = save_return(session.client, stop["registers"])
                    session.client.remove_breakpoints(root)
                    watch = f"{stop['registers']['ss']}:{int(stop['registers']['sp'],16)+2:04X}"
                    checked_read(session.client, int(stop["registers"]["ss"], 16), int(stop["registers"]["sp"], 16)+2, 2)
                    session.client.set_watchpoint(watch, 2)
                    session.client.set_breakpoint(destination, once=True)
                    phase = "saving"
                elif destination is not None and stop["reason"] == "breakpoint" and stop["cs_ip"] == destination:
                    if not returned_frame(report["save_entry"]["registers"], stop["registers"]):
                        raise ValueError("Save return belongs to another frame")
                    seg, off = (int(x, 16) for x in destination.split(":"))
                    if checked_read(session.client, seg, off, 1) != bytes([0x59]):
                        raise ValueError("Save caller cleanup differs")
                    report["save_return"] = stop
                    session.client.remove_watchpoints(watch)
                    watch = None
                    phase = "map"
                elif not (stop["reason"] == "time" or
                          (phase == "map" and stop["reason"] == "breakpoint" and stop["cs_ip"] == rng.address(0x39D1, 0x97D))):
                    raise RuntimeError("unexpected stop; continuation not admitted")
                delivered = observe_delivery(session, key, delivered)
                if not delivered.pending and delivered.state != "done":
                    raise RuntimeError("key delivery failed")
                current = snapshot(session.client, delta)
                ui = fields(session.client, delta)
                report["current"] = {"state": current, "fields": ui}
                input_ready = phase == "map" and input_map_ready(session.client, delta, current, ui)
                if input_ready and delivered.state == "done" and not builder_armed:
                    session.client.set_breakpoint(rng.address(0x39D1, 0x97D), once=True)
                    builder_armed = True
                if (phase == "window" and delivered.state == "done" and current == previous and save_ready(current, ui)):
                    report["inputs"].append({"key": key_name, "delivery": delivered.detail})
                    root = save_root(session.client, rng.address(0x5736, 0x2F))
                    session.client.set_breakpoint(root)
                    key_name = "enter"
                    key = session.send_input("key", {"key": key_name, "hold_ms": 100})
                    delivered = None
                    phase = "action"
                if (input_ready and delivered.state == "done"
                        and stop["reason"] == "breakpoint" and stop["cs_ip"] == rng.address(0x39D1, 0x97D)):
                    report["inputs"].append({"key": key_name, "delivery": delivered.detail})
                    report["save_after"] = named_file(run, "SAVE02.SAV")
                    if not report["save_after"].get("length", 0):
                        raise ValueError("native writer did not leave a nonempty save")
                    png = session.client.screenshot()
                    (run / "saved-map.png").write_bytes(png)
                    report["saved_state"] = current
                    report["frame_sha256"] = hashlib.sha256(png).hexdigest()
                    report["checkpoint"] = checkpoint_summary(session.save_state("saved-map", check_files=True))
                    report["passed"] = True
                    break
                previous = current
                record()
            else:
                raise RuntimeError("native-save state budget exhausted")
        except Exception as error:
            report["error"] = str(error)
            raise
        finally:
            record()
    print(json.dumps({"passed": report["passed"], "draws": len(report["draws"])}), flush=True)


if __name__ == "__main__":
    main()
