"""Fixed saved-map F3/Quit observation; no guest writes or arbitrary targets."""
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
from load_entry import fields, input_map_ready, observe_delivery
from checkpoint_rng import Recorder
from startup_window import wait_for_stop
from shutdown_snapshot import capture
from shutdown_calls import slot_call, slot_entry, slot_return, stream_use


class QuitFlow:
    def __init__(self):
        self.phase = "choice"

    def poll(self, state, delivery_done):
        number = state["window"]["number"] if state["window"] else None
        if number not in (None, 19501, 14000, 14001):
            raise ValueError("unexpected window during fixed Quit flow")
        if self.phase == "choice" and delivery_done and any(ready(state, n) for n in (14000, 14001)):
            self.phase = "exit"
            return "q"
        return None

    def exit_entry(self, delivery_done):
        if self.phase != "exit" or not delivery_done:
            raise ValueError("exit reached before verified Quit input")
        self.phase = "shutdown"


def file_states(run):
    result = {}
    drive = run / "drive-c"
    for name in ("DARKRUN.GFF", "CHARSAVE.GFF", "SAVE02.SAV", "RGN032.GFF"):
        path = drive / name
        if any(p.is_symlink() or p.is_junction() for p in (run, drive, path)):
            raise ValueError("shutdown private file path contains a link")
        if not path.exists():
            result[name] = {"exists": False}
            continue
        if not path.is_file() or path.stat().st_size > 128 * 1024 * 1024:
            raise ValueError("unsupported shutdown private file")
        digest, count = hashlib.sha256(), 0
        with path.open("rb") as stream:
            while block := stream.read(1024 * 1024):
                count += len(block)
                if count > 128 * 1024 * 1024:
                    raise ValueError("shutdown private file grew beyond bound")
                digest.update(block)
        result[name] = {"exists": True, "length": count, "sha256": digest.hexdigest()}
    return result


def write_request(registers):
    values = {name: int(registers[name], 16) for name in ("ax", "bx", "cx", "ds", "dx")}
    if any(not 0 <= value <= 0xFFFF for value in values.values()):
        raise ValueError("write stop has invalid word register")
    if values["ax"] >> 8 != 0x40:
        raise ValueError("write stop does not select AH40")
    return {"handle": int(registers["bx"], 16), "count": int(registers["cx"], 16),
            "source": f'{int(registers["ds"], 16):04X}:{int(registers["dx"], 16):04X}'}


def main():
    if len(sys.argv) != 3 or any(not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", x) for x in sys.argv[1:]):
        raise SystemExit("usage: shutdown_run.py <native-save-source-run> <new-run-name>")
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    source = game / "captures/rust-dos-native-save" / sys.argv[1]
    meta = json.loads((source / "native-save.json").read_text(encoding="utf-8"))
    state = read_saved_state(source, "saved-map")
    verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
    admit_state(state)
    delta = mapping(meta["load_mapping_stop"])
    rng = Recorder(delta)
    run = game / "captures/rust-dos-shutdown" / sys.argv[2]
    if run.exists():
        raise ValueError("shutdown run directory exists")
    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe", emulator_sha256=RUST_DOS_PINNED_SHA256,
        run_directory=run, target=Target("DSUN.EXE", ("-W0", "-L")), start_state=state,
        emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
            deterministic=True, cycles="15000", sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1),
            ultrasound=None), readiness_timeout=60.0)
    report = {"source": sys.argv[1], "load_mapping_stop": meta["load_mapping_stop"],
              "inputs": [], "draws": [], "writes": [], "events": [], "passed": False,
              "limits": "fixed saved-map Quit; observed requests/results only; stops before DOS termination"}
    with RustDosSession(settings) as session:
        def record():
            (run / "shutdown.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        try:
            stop = session.stop
            if (stop["reason"] != "load" or stop["cs_ip"] != state.stop["cs_ip"]
                    or stop["emu_ns"] != state.stop["emu_ns"]):
                raise ValueError("shutdown restore instruction/time differs")
            current = snapshot(session.client, delta)
            if (not input_map_ready(session.client, delta, current, fields(session.client, delta))
                    or current != meta["saved_state"]
                    or hashlib.sha256(session.client.screenshot()).hexdigest() != meta["frame_sha256"]):
                raise ValueError("shutdown source differs from admitted saved-map")
            report["restored_stop"], report["initial_seed"] = stop, current["rng_state"]
            report["files_before"] = file_states(run)
            # FND-COMBAT-051's choice loop fetches events here; the builder
            # stop used by native_save does not recur while the modal waits.
            poll = rng.address(0x39D1, 0x854)
            exit_root = rng.address(0x1000, 0x388)
            generator, setter = rng.address(0x1000, 0x822), rng.address(0x1000, 0x811)
            for address in (poll, exit_root, generator, setter):
                session.client.set_breakpoint(address)
            flow = QuitFlow()
            key_name = "f3"
            key = session.send_input("key", {"key": key_name, "hold_ms": 100})
            delivery = None
            requests = {rng.address(seg, off): (seg, off) for seg, off in
                        ((0x44DE, 0x264), (0x44DE, 0x285), (0x1000, 0x3EFB), (0x15F3, 0x1A4))}
            close_all, prep = rng.address(0x38FF, 0x2B5), rng.address(0x37FC, 0x8FB)
            terminate = rng.address(0x1000, 0x1A5)
            pending_write = None
            slot_before, slot_after = rng.address(0x1425, 0x16AB), rng.address(0x1425, 0x16AF)
            cleanup_entry = rng.address(0x1425, 0x0454)
            stream_stops = (rng.address(0x1000, 0x3F1E), rng.address(0x1000, 0x2F30))
            pending_slot = None
            report["cleanup_calls"], report["stream_uses"] = [], []
            for index in range(4096):
                observed, exhausted = wait_for_stop(session, session.continue_(), attempts=2)
                stop = observed.stop
                report["last_stop"], report["observations"] = stop, index + 1
                if exhausted or stop.get("reason") != "breakpoint":
                    raise RuntimeError("shutdown continuation did not reach a declared breakpoint")
                address = stop["cs_ip"]
                delivery = observe_delivery(session, key, delivery)
                report["last_delivery"] = {"key": key_name, "state": delivery.state}
                if address == generator:
                    report["draws"].append(rng.record(session, stop))
                elif address == setter:
                    raise ValueError("unadmitted seed setter during Quit")
                elif pending_write is not None and address == pending_write[0]:
                    item = report["write_requests"][pending_write[1]]
                    item["result"] = {"ax": stop["registers"]["ax"],
                                      "carry": bool(int(stop["registers"]["flags"], 16) & 1)}
                    session.client.remove_breakpoints(address)
                    pending_write = None
                elif address == exit_root:
                    flow.exit_entry(delivery.state == "done")
                    report["inputs"].append({"key": key_name, "delivery": delivery.detail})
                    report["entry"] = capture(session.client, stop, delta)
                    if report["entry"]["status"] != 0:
                        raise ValueError("Quit reached a nonzero-status exit")
                    report["write_requests"] = []
                    for target in (*requests, close_all, prep, terminate, slot_before, slot_after, cleanup_entry, *stream_stops):
                        session.client.set_breakpoint(target)
                    session.client.remove_breakpoints(poll)
                elif flow.phase == "shutdown" and address == slot_before:
                    if pending_slot is not None:
                        raise ValueError("nested slot call outside fixed pairing")
                    report["cleanup_calls"].append(slot_call(session.client, stop, delta))
                    pending_slot = len(report["cleanup_calls"]) - 1
                    if report["cleanup_calls"][pending_slot]["target"] != cleanup_entry:
                        raise ValueError("slot target outside fixed type-three observation")
                elif flow.phase == "shutdown" and address == cleanup_entry:
                    if pending_slot is None or "callee_entry" in report["cleanup_calls"][pending_slot]:
                        raise ValueError("cleanup entry lacks a unique observed slot call")
                    item = report["cleanup_calls"][pending_slot]
                    item["callee_entry"] = slot_entry(session.client, stop, delta, item)
                elif flow.phase == "shutdown" and address == slot_after:
                    if pending_slot is None:
                        raise ValueError("slot return lacks observed call")
                    item = report["cleanup_calls"][pending_slot]
                    if "callee_entry" not in item:
                        raise ValueError("slot return lacks fixed callee-entry observation")
                    item["result"] = slot_return(stop, delta, item)
                    pending_slot = None
                elif flow.phase == "shutdown" and address in stream_stops:
                    report["stream_uses"].append(stream_use(session.client, stop, delta))
                elif flow.phase == "shutdown" and address in requests:
                    if pending_write is not None:
                        raise ValueError("nested write before previous interrupt result")
                    item = write_request(stop["registers"])
                    item["stop"] = stop
                    report["write_requests"].append(item)
                    seg, off = requests[address]
                    after = rng.address(seg, off + 2)
                    session.client.set_breakpoint(after)
                    pending_write = after, len(report["write_requests"]) - 1
                elif flow.phase == "shutdown" and address in (close_all, prep):
                    r = stop["registers"]
                    body = checked_read(session.client, int(r["ss"], 16), int(r["sp"], 16), 8)
                    ret, caller, argument = struct.unpack("<HHI", body)
                    item = {"entry": address, "return": f"{caller:04X}:{ret:04X}", "argument": argument}
                    if address == prep:
                        off, seg = argument & 0xFFFF, argument >> 16
                        item["record"] = {"io_handle": struct.unpack("<H", checked_read(session.client, seg, off + 4, 2))[0],
                                          "dirty": checked_read(session.client, seg, off + 0x2C, 1)[0]}
                    report["events"].append(item)
                elif flow.phase == "shutdown" and address == terminate:
                    if (pending_write is not None or pending_slot is not None
                            or int(stop["registers"]["ax"], 16) >> 8 != 0x4C):
                        raise ValueError("incomplete write result or wrong termination service")
                    report["files_after"] = file_states(run)
                    report["passed"] = True
                    break
                elif address == poll and flow.phase != "shutdown":
                    current = snapshot(session.client, delta)
                    report["ui_state"] = current
                    next_key = flow.poll(current, delivery.state == "done")
                    if next_key:
                        report["inputs"].append({"key": key_name, "delivery": delivery.detail})
                        key_name = next_key
                        key = session.send_input("key", {"key": key_name, "hold_ms": 100})
                        delivery = None
                else:
                    raise ValueError("unadmitted shutdown stop or phase")
                record()
            else:
                raise RuntimeError("shutdown state-observation budget exhausted")
        except Exception as error:
            report["error"] = str(error)
            raise
        finally:
            record()
    print(json.dumps({"passed": report["passed"], "draws": len(report["draws"])}), flush=True)


if __name__ == "__main__":
    main()
