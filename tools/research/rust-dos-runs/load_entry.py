"""Fixed F2 entry observation from an admitted map checkpoint; no guest writes."""
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
from checkpoint_rng import Recorder
from startup_window import wait_for_stop, checkpoint_summary


def fields(client, delta):
    def read(seg, off, size):
        return checked_read(client, seg + delta, off, size)
    def word(seg, off):
        return struct.unpack("<H", read(seg, off, 2))[0]
    def pointer(raw):
        off, seg = struct.unpack("<HH", raw)
        return f"{seg:04X}:{off:04X}"
    target = read(0x57E0, 0xA0FD, 4)
    off, seg = struct.unpack("<HH", target)
    window = None
    if off or seg:
        body = checked_read(client, seg, off, 0xF9)
        if body[:4] != b"WIND":
            raise ValueError("event target lacks the evidenced window tag")
        window = {"number": struct.unpack_from("<I", body, 8)[0],
                  "callback": pointer(body[0xF5:0xF9])}
    return {
        "count": word(0x57E0, 0xA103),
        "head": pointer(read(0x57E0, 0xA105, 4)),
        "event_target": pointer(target), "event_window": window,
        "global_handler": pointer(read(0x57E0, 0xA0F1, 4)),
        "kind_three_handler": pointer(read(0x57E0, 0xA0F5, 4)),
        "saved_handler": pointer(read(0x4C4D, 0x000F, 4)),
        "handler_mirror": pointer(read(0x57E0, 0x61A2, 4)),
        "selected_row": word(0x4C4C, 0), "list_top": word(0x4C4C, 2),
        "save_load_window": pointer(read(0x50AF, 0, 4)),
        "load_mode": read(0x50AF, 4, 1)[0],
        "list_pointer": pointer(read(0x57E0, 0x6278, 4)),
        "exit_byte": read(0x57E0, 0x627C, 1)[0],
    }


def map_ready(state):
    return (ready(state, 19501) and state["mode"] == 1
            and state["slot_kinds"] == [0, 2, 0, 2] and state["leader"] == 1)


def load_ready(state, ui):
    return ready(state, 18500) and ui["load_mode"] == 1


def event_words(client, registers, bias):
    if bias not in (0, 4):
        raise ValueError("unsupported event frame")
    data = checked_read(client, int(registers["ss"], 16),
                        int(registers["sp"], 16) + bias, 24)
    return list(struct.unpack("<12H", data))


def navigation_matches(call, entry, return_frame, expected_return, callback):
    return (call is not None and call["event_words"] == entry
            and entry[0] == 6 and entry[6] == 0x5000
            and call["fields"]["global_handler"] == callback
            and return_frame == expected_return)


def main():
    if (len(sys.argv) not in (3, 4) or
            any(not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", x) for x in sys.argv[1:]) or
            (len(sys.argv) == 4 and sys.argv[3] != "down")):
        raise SystemExit("usage: load_entry.py <play-ready-source-run> <new-run-name> [down]")
    navigation = len(sys.argv) == 4
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    source = game / "captures/rust-dos-regenerated" / sys.argv[1]
    meta = json.loads((source / "regeneration.json").read_text(encoding="utf-8"))
    state = read_saved_state(source, "play-ready")
    verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
    admit_state(state)
    delta = mapping(meta["load_mapping_stop"])
    rng = Recorder(delta)
    run = game / "captures/rust-dos-load-entry" / sys.argv[2]
    if run.exists():
        raise ValueError("run already exists")
    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
        emulator_sha256=RUST_DOS_PINNED_SHA256, run_directory=run,
        target=Target("DSUN.EXE", ("-W0", "-L")), start_state=state,
        emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
            deterministic=True, cycles="15000",
            sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1), ultrasound=None),
        readiness_timeout=60.0)
    report = {"source": sys.argv[1], "writes": [], "draws": [],
              "dispatches": [], "calls": [], "observations": [], "passed": False}
    with RustDosSession(settings) as session:
        def record():
            (run / "load-entry.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        try:
            report["restored_stop"] = session.stop
            if (session.stop["reason"] != "load" or session.stop["cs_ip"] != state.stop["cs_ip"]
                    or session.stop["emu_ns"] != state.stop["emu_ns"]):
                raise ValueError("restored instruction/time differs")
            initial = snapshot(session.client, delta)
            report["initial"] = {"state": initial, "fields": fields(session.client, delta)}
            if not map_ready(initial):
                raise ValueError("source is not the admitted two-character map state")
            dispatch = rng.address(0x39D1, 0x0752)
            global_call = rng.address(0x39D1, 0x083B)
            callback = rng.address(0x5736, 0x002F)
            for address in (dispatch, global_call, rng.address(0x1000, 0x0822), rng.address(0x1000, 0x0811)):
                session.client.set_breakpoint(address)
            key = session.send_input("key", {"key": "f2", "hold_ms": 100})
            key_name = "f2"
            previous = None
            for index in range(512):
                target = int(session.stop["emu_ns"]) // 1000000 + 100
                observed, exhausted = wait_for_stop(session, session.continue_(until_ms=target))
                report["last_stop"] = observed.stop
                if exhausted:
                    raise RuntimeError("operation observation budget exhausted")
                stop = observed.stop
                if stop["reason"] == "breakpoint" and stop["cs_ip"] == rng.address(0x1000, 0x0822):
                    report["draws"].append(rng.record(session, stop))
                    record()
                    continue
                if stop["reason"] == "breakpoint" and stop["cs_ip"] == dispatch:
                    report["dispatches"].append({"stop": stop, "event_words": event_words(session.client, stop["registers"], 4),
                                                  "fields": fields(session.client, delta)})
                elif stop["reason"] == "breakpoint" and stop["cs_ip"] == global_call:
                    report["calls"].append({"stop": stop, "event_words": event_words(session.client, stop["registers"], 0),
                                             "fields": fields(session.client, delta)})
                elif stop["reason"] == "breakpoint" and stop["cs_ip"] == callback and key_name == "down":
                    r = stop["registers"]
                    entry = event_words(session.client, r, 4)
                    frame = checked_read(session.client, int(r["ss"], 16), int(r["sp"], 16), 4)
                    off, seg = struct.unpack("<HH", frame)
                    ret = f"{seg:04X}:{off:04X}"
                    report["callback_entry"] = {"stop": stop, "event_words": entry,
                        "return": ret, "fields": fields(session.client, delta)}
                    delivery = session.observe(key, timeout=0)
                    report["navigation_input"] = {"key": "down", "state": delivery.state, "detail": delivery.detail}
                    if not navigation_matches(report["calls"][-1] if report["calls"] else None,
                            entry, ret, rng.address(0x39D1, 0x083F), callback):
                        raise RuntimeError("navigation callback/frame differs from the observed global call")
                    report["passed"] = True
                    break
                elif stop["reason"] != "time":
                    raise RuntimeError("unexpected stop; no continuation admitted")
                delivery = session.observe(key, timeout=0)
                report["input" if key_name == "f2" else "navigation_input"] = {"key": key_name, "state": delivery.state, "detail": delivery.detail}
                current = {"state": snapshot(session.client, delta), "fields": fields(session.client, delta)}
                if current != previous:
                    report["observations"].append(current)
                report["polls"] = index + 1
                record()
                if key_name == "f2" and delivery.state == "done" and current == previous and load_ready(current["state"], current["fields"]):
                    (run / "load.png").write_bytes(session.client.screenshot())
                    report["checkpoint"] = checkpoint_summary(session.save_state("load-entry", check_files=True))
                    if not navigation:
                        report["passed"] = True
                        break
                    if current["fields"]["global_handler"] != callback:
                        raise RuntimeError("Load hook differs from the declared callback")
                    session.client.remove_breakpoints(dispatch)
                    session.client.set_breakpoint(callback)
                    key = session.send_input("key", {"key": "down", "hold_ms": 100})
                    key_name = "down"
                    previous = None
                    record()
                    continue
                if not delivery.pending and delivery.state != "done":
                    raise RuntimeError("key delivery failed")
                previous = current
            else:
                raise RuntimeError("Load-entry state budget exhausted")
        except Exception as error:
            report["error"] = str(error)
            raise
        finally:
            record()
        print(json.dumps({"passed": report["passed"], "dispatches": len(report["dispatches"]),
                          "draws": len(report["draws"])}), flush=True)


if __name__ == "__main__":
    main()
