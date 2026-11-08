"""Fixed Tab/Preferences getter/store observation for Q-CONFIG-014.

FND-CONFIG-235/236 give the getter, caller/store, gates and named library fields.
FND-INPUT-012, FND-UI-055 and SCR-UI-006 give the menu input route.
FND-UI-010/054 give the key-event builder; FND-PARTY-031 covers main.
FND-RNG-001/010 give RNG guards.
"""
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
from preferences_history import ValueHistory


def input_poll(client, delta):
    """Admit the key-event builder through main's actual relocated far call."""
    trace = Readback(delta)
    call = checked_read(client, 0x277B + delta, 0x0541, 5)
    if call[0] != 0x9A or struct.unpack_from("<HH", call, 1) != (0x097D, 0x39D1 + delta):
        raise ValueError("main does not call the declared key-event builder")
    return trace.address(0x39D1, 0x097D)


class MenuFlow:
    def __init__(self):
        self.phase = "menu"

    def poll(self, state, delivery_done):
        number = state["window"]["number"] if state["window"] else None
        allowed = (None, 19501, 10500) if self.phase == "menu" else (10500,) if self.phase == "move" else (None, 10500)
        if number not in allowed:
            raise ValueError("unexpected window during fixed Preferences route")
        if self.phase in ("menu", "move") and delivery_done and ready(state, 10500):
            action = "move" if self.phase == "menu" else "click"
            if action == "click" and state.get("selected_control") != {"tag": "BUTN", "number": 10303}:
                return None
            self.phase = "move" if action == "move" else "getter"
            return action
        return None


def select_preferences(session, poll):
    """The admitted click needs only getter/RNG stops, not repeated UI polls."""
    session.client.remove_breakpoints(poll)
    return session.send_input("mouse", {"action": "click", "button": "left", "hold_ms": 100})


class Readback:
    def __init__(self, delta):
        if type(delta) is not int or not -0x1000 <= delta <= 0:
            raise ValueError("unsupported Preferences relocation")
        self.delta, self.phase = delta, "entry"
        self.caller = self.stack = self.result = None

    def address(self, segment, offset):
        return f"{segment + self.delta:04X}:{offset:04X}"

    def named(self, client, stop, expected):
        r = stop.get("registers", {})
        watch = stop.get("watch", {})
        try:
            store_watch = (stop.get("reason") == "watchpoint" and self.phase == "store"
                           and self.caller is not None and expected == f"{self.caller:04X}:0041"
                           and int(watch.get("addr", "-1"), 16) == (0x57E0 + self.delta) * 16 + 0x26B4
                           and type(watch.get("len")) is int and watch["len"] == 1
                           and int(watch["old"], 16) != int(watch["new"], 16))
        except (TypeError, ValueError, KeyError):
            store_watch = False
        if ((stop.get("reason") != "breakpoint" and not store_watch) or stop.get("cs_ip", "").upper() != expected
                or r.get("system", {}).get("mode") != "real"):
            raise ValueError("Preferences readback requires its declared real-mode stop")
        try:
            ds, ss, sp, ax = (int(r[key], 16) for key in ("ds", "ss", "sp", "ax"))
        except (KeyError, ValueError, TypeError) as error:
            raise ValueError("missing Preferences readback registers") from error
        if ds != 0x57E0 + self.delta or ss != ds or not 0 <= ax <= 0xFFFF or not 0 <= sp <= 0xFFFF:
            raise ValueError("Preferences readback DS/SS or word registers differ")
        def value(offset, size):
            return int.from_bytes(checked_read(client, ds, offset, size), "little")
        cfg_off, cfg_seg = struct.unpack("<HH", checked_read(client, ds, 0x3411, 4))
        if cfg_off == cfg_seg == 0:
            raise ValueError("Preferences configuration pointer is null")
        cfg = checked_read(client, cfg_seg, cfg_off, 59)
        item = {"cs_ip": stop["cs_ip"], "ss": ss, "sp": sp, "ax": ax,
                "master_sound": value(0x13F7, 1), "music_on": value(0x1436, 1),
                "music_level": value(0x26B4, 1), "music_slot": value(0x343E, 2),
                "getter_handle": value(0x3464, 2), "configuration_pointer": f"{cfg_seg:04X}:{cfg_off:04X}",
                "configuration": cfg.hex(), "card_id": struct.unpack_from("<H", cfg, 8)[0],
                "library_mode": int.from_bytes(checked_read(client, 0x4E71 + self.delta, 0x0B51, 2), "little")}
        if store_watch and (not 0 <= int(watch["old"], 16) <= 255
                            or int(watch["new"], 16) != item["music_level"]):
            raise ValueError("Preferences store watch differs from the named byte")
        return item

    def trampoline(self, client, segment):
        # FND-INPUT-012: Preferences entry is trampoline8, not the header prefix.
        s, o = (int(v, 16) for v in self.address(0x5782, 0x0048).split(":"))
        jump = checked_read(client, s, o, 5)
        if jump[0] != 0xEA or struct.unpack_from("<HH", jump, 1) != (0, segment):
            raise ValueError("Preferences trampoline does not name the admitted caller segment")

    def entry(self, client, stop):
        if self.phase != "entry":
            raise ValueError("Preferences getter reentry or wrong phase")
        item = self.named(client, stop, self.address(0x4A6E, 0x011F))
        off, seg = struct.unpack("<HH", checked_read(client, item["ss"], item["sp"], 4))
        if off != 0x003E or item["sp"] + 4 > 0xFFFF:
            raise ValueError("Preferences getter return frame differs from the known caller")
        self.trampoline(client, seg)
        call = checked_read(client, seg, 0x0039, 5)
        target_seg = 0x4A6E + self.delta
        if call[0] != 0x9A or struct.unpack_from("<HH", call, 1) != (0x011F, target_seg):
            raise ValueError("Preferences caller does not call the known resident getter")
        store = checked_read(client, seg, 0x003E, 3)
        if store[0] != 0xA2 or struct.unpack_from("<H", store, 1)[0] != 0x26B4:
            raise ValueError("Preferences continuation is not the declared AL store")
        if not item["master_sound"] or not item["music_on"]:
            raise ValueError("Preferences getter entry has an unadmitted enable gate")
        self.caller, self.stack = seg, (item["ss"], item["sp"] + 4)
        self.phase = "return"
        return item

    def returned(self, client, stop):
        if self.phase != "return":
            raise ValueError("unpaired Preferences getter return")
        item = self.named(client, stop, f"{self.caller:04X}:003E")
        self.trampoline(client, self.caller)
        if (item["ss"], item["sp"]) != self.stack:
            raise ValueError("Preferences getter return stack differs")
        self.result, self.phase = item["ax"], "store"
        return item

    def stored(self, client, stop):
        if self.phase != "store":
            raise ValueError("unpaired Preferences byte store")
        item = self.named(client, stop, f"{self.caller:04X}:0041")
        self.trampoline(client, self.caller)
        if (item["ss"], item["sp"]) != self.stack or item["ax"] != self.result:
            raise ValueError("Preferences store frame/result changed")
        if item["music_level"] != self.result & 0xFF:
            raise ValueError("Preferences stored byte differs from the observed AL")
        self.phase = "done"
        return item


def observe_readback(session, delta, report, retain, history=None, seed_expected=None, parent=None, keyboard=None):
    """Fixed Tab/getter/store route, shared by restored and fresh observers."""
    rng, trace, flow = Recorder(delta), Readback(delta), MenuFlow()
    generator, setter = rng.address(0x1000, 0x0822), rng.address(0x1000, 0x0811)
    poll, getter = input_poll(session.client, delta), rng.address(0x4A6E, 0x011F)
    for site in (generator, setter, poll, getter):
        session.client.set_breakpoint(site)
    operation = session.send_input("key", {"key": "tab", "hold_ms": 100})
    delivery, input_name, recorded = None, "tab", False
    for index in range(512):
        sampling = getattr(keyboard, "EXPECT_REJECTION", False) and keyboard.phase == "delivery"
        continuation = (session.continue_(until_ms=session.stop["emu_ns"] // 1000000 + 1)
                        if sampling else session.continue_())
        ob, exhausted = wait_for_stop(session, continuation, attempts=2)
        stop = ob.stop
        report["last_stop"], report["stops"] = stop, index + 1
        retain()
        allowed = (("breakpoint", "watchpoint", "time") if sampling else ("breakpoint", "watchpoint")) if history else ("breakpoint",)
        if exhausted or stop.get("reason") not in allowed:
            raise ValueError("Preferences did not reach a declared instruction stop")
        if stop.get("cs_ip") == setter or (stop.get("cs_ip") == generator and stop.get("reason") != "breakpoint"):
            raise ValueError("unadmitted Preferences RNG/seed boundary")
        if getattr(keyboard, "EXPECT_REJECTION", False) and stop.get("cs_ip") == getter:
            raise ValueError("rejection reached forbidden getter")
        if seed_expected is not None and int.from_bytes(checked_read(session.client, 0x57E0 + delta, 0x384A, 4), "little") != seed_expected:
            raise ValueError("Preferences seed differs from recorded chain")
        if history:
            if stop["reason"] == "watchpoint":
                history.changed(session.client, stop)
            else:
                history._stop(stop)
                history.check(session.client)
                history.sequence, history.time = stop["seq"], stop["emu_ns"]
        if parent and parent.is_site(stop):
            parent.boundary(session.client, stop)
            retain()
            continue
        if keyboard and keyboard.is_site(stop):
            keyboard.boundary(session.client, stop)
            retain()
            continue
        delivery = observe_delivery(session, operation, delivery)
        report["last_delivery"] = {"input": input_name, "state": delivery.state}
        if not recorded and delivery.state == "done":
            report["inputs"].append({"input": input_name, "delivery": delivery.detail})
            recorded = True
        site = stop["cs_ip"]
        if history and stop["reason"] == "watchpoint":
            if trace.caller is None or trace.phase != "store" or site != f"{trace.caller:04X}:0041":
                continue
        if stop["reason"] == "breakpoint" and site == generator:
            draw = rng.record(session, stop)
            if seed_expected is not None:
                if draw["seed_before"] != seed_expected:
                    raise ValueError("Preferences draw seed does not join previous state")
                seed_expected = draw["seed_after"]
            report["draws"].append(draw)
            if history:
                history._stop(session.stop)
                history.check(session.client)
                history.sequence, history.time = session.stop["seq"], session.stop["emu_ns"]
        elif site == setter:
            raise ValueError("unadmitted Preferences reseeding")
        elif site == poll or sampling and stop["reason"] == "time":
            current = snapshot(session.client, delta)
            report["ui_state"] = current
            if getattr(keyboard, "EXPECT_REJECTION", False) and keyboard.phase == "delivery":
                if keyboard.endpoint(session.client, stop, current, delivery.state == "done"):
                    if report["readback"]: raise ValueError("rejection reached a readback")
                    history.finish(session.client)
                    history.close(session.client)
                    history.complete()
                    keyboard.complete()
                    report["result"], report["passed"] = "keyboard-turn-rejected", True
                    break
                retain()
                continue
            action = flow.poll(current, delivery.state == "done")
            if action:
                if keyboard:
                    if action != "move": raise ValueError("keyboard route reached an unexpected UI action")
                    keyboard.arm(session.client)
                    session.client.remove_breakpoints(poll)
                    operation = session.send_input("key", {"key": "p", "hold_ms": 1})
                    input_name, flow.phase, delivery, recorded = "p", "getter", None, False
                    continue
                input_name = action
                if parent and action == "click":
                    parent.arm(session.client)
                operation = (session.send_input("mouse", {"action": "move", "x": 160, "y": 101, "coords": "virtual"})
                             if action == "move" else select_preferences(session, poll))
                delivery, recorded = None, False
                print(action, flush=True)
        elif site == getter:
            if getattr(keyboard, "EXPECT_REJECTION", False): raise ValueError("rejection reached forbidden getter")
            if flow.phase != "getter":
                raise ValueError("Preferences getter reached before its fixed input route")
            report["readback"].append({"phase": "entry", **trace.entry(session.client, stop)})
            if parent:
                parent.nested(session.client, stop, "getter")
            if keyboard: keyboard.nested(session.client, stop, "getter")
            for offset in ((0x003E,) if history else (0x003E, 0x0041)):
                session.client.set_breakpoint(f"{trace.caller:04X}:{offset:04X}", once=True)
            print("getter-entry", flush=True)
        elif trace.caller is not None and site == f"{trace.caller:04X}:003E":
            report["readback"].append({"phase": "return", **trace.returned(session.client, stop)})
            if history and report["readback"][-1]["music_level"] == trace.result & 0xFF:
                session.client.set_breakpoint(f"{trace.caller:04X}:0041", once=True)
        elif trace.caller is not None and site == f"{trace.caller:04X}:0041":
            report["readback"].append({"phase": "stored", **trace.stored(session.client, stop)})
            if parent:
                parent.nested(session.client, stop, "store")
            if keyboard: keyboard.nested(session.client, stop, "store")
            if delivery.state != "done":
                raise ValueError("Preferences click delivery incomplete at endpoint")
            if history:
                history.finish(session.client)
                history.close(session.client)
                history.complete()
            if parent:
                parent.complete()
            if keyboard: keyboard.complete()
            report["passed"] = True
            break
        else:
            raise ValueError("unadmitted Preferences stop")
        retain()
    else:
        raise ValueError("Preferences exceeds512 named stops")


def main():
    if (len(sys.argv) not in (3, 4)
            or any(not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", x) for x in sys.argv[1:3])
            or len(sys.argv) == 4 and sys.argv[3] not in ("history", "parent", "dispatch", "keyboard", "keyboard-rejection")):
        raise SystemExit("usage: preferences_readback.py <play-ready-source-run> <new-run-name> [history|parent|dispatch|keyboard|keyboard-rejection]")
    history_mode = len(sys.argv) == 4
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    source = game / "captures/rust-dos-regenerated" / sys.argv[1]
    meta = json.loads((source / "regeneration.json").read_text(encoding="utf-8"))
    controls = [c for c in meta["checkpoints"] if c["name"] == "play-ready" and c["checked"]]
    if len(controls) != 1:
        raise ValueError("source lacks a unique checked play-ready checkpoint")
    control = controls[0]
    state = read_saved_state(source, "play-ready")
    verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
    admit_state(state)
    delta = mapping(meta["load_mapping_stop"])
    run = game / "captures/rust-dos-preferences-readback" / sys.argv[2]
    if run.exists():
        raise ValueError("Preferences readback run already exists")
    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe", emulator_sha256=RUST_DOS_PINNED_SHA256,
        run_directory=run, target=Target("DSUN.EXE", ("-W0", "-L")), start_state=state,
        emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
            deterministic=True, cycles="15000", sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1),
            ultrasound=None), readiness_timeout=60.0)
    report = {"source": sys.argv[1], "passed": False, "inputs": [], "draws": [], "writes": [], "readback": [],
              "limits": "fixed restored-map getter/store; no continuous startup preservation, later UI, audible output or other caller proof"}
    history = ValueHistory(delta) if history_mode else None
    parent = None
    keyboard = None
    if len(sys.argv) == 4 and sys.argv[3] == "keyboard-rejection":
        from preferences_rejection import RejectionTrace, RejectionHistory
        keyboard, history = RejectionTrace(delta), RejectionHistory(delta)
        report["keyboard"] = keyboard.report
        report["limits"] = "fixed completed P turn rejection; no callback/reload/getter/store admission, positive mutation delivery or other routes"
    if len(sys.argv) == 4 and sys.argv[3] == "keyboard":
        from preferences_key import KeyTrace
        keyboard = KeyTrace(delta)
        report["keyboard"] = keyboard.report
    if len(sys.argv) == 4 and sys.argv[3] in ("parent", "dispatch"):
        from preferences_parent import ParentTrace, DispatchTrace
        parent = (DispatchTrace if sys.argv[3] == "dispatch" else ParentTrace)(delta)
        report["parent"] = parent.report
    if history:
        report["value_history"] = history.report
    with RustDosSession(settings) as session:
        def retain():
            (run / "preferences-readback.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        try:
            stop = session.stop
            if stop["reason"] != "load" or stop["cs_ip"] != state.stop["cs_ip"] or stop["emu_ns"] != state.stop["emu_ns"]:
                raise ValueError("Preferences source restore instruction/time differs")
            current = snapshot(session.client, delta)
            if (current != control["state"]
                    or hashlib.sha256(session.client.screenshot()).hexdigest() != control["frame_sha256"]
                    or not input_map_ready(session.client, delta, current, fields(session.client, delta))):
                raise ValueError("Preferences source differs from the admitted input map")
            report["restored_stop"], report["initial_seed"] = stop, current["rng_state"]
            if history:
                history.arm(session.client, stop)
            if getattr(keyboard, "EXPECT_REJECTION", False): keyboard.baseline(session.client, stop)
            observe_readback(session, delta, report, retain, history, parent=parent, keyboard=keyboard)
        except Exception as error:
            report["error"] = str(error)
            raise
        finally:
            try:
                if history:
                    history.close(session.client)
            finally:
                retain()
    print(json.dumps({"passed": report["passed"], "draws": len(report["draws"])}), flush=True)


if __name__ == "__main__":
    main()
