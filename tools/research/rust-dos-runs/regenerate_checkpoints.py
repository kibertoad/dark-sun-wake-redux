"""Bounded pinned-runtime menu capability checks; all captures stay local.

Reads FMT-UI-001/FND-UI-045 current window and FND-PARTY-049 slot kinds.
No guest writes. Unexpected RNG stops fail closed rather than lose draws.
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
from startup_window import wait_for_stop, checkpoint_summary
from checkpoint_rng import Recorder


def checked_read(client, segment, offset, size):
    if not 0 <= segment <= 0xFFFF or not 0 <= offset <= 0xFFFF or not 0 < size <= 261:
        raise ValueError("unsupported named field read")
    if offset + size > 0x10000 or segment * 16 + offset + size > 0x100000:
        raise ValueError("named field crosses real-mode bound")
    data = client.read_memory(f"{segment:04X}:{offset:04X}", size).data
    if len(data) != size:
        raise ValueError("short named field read")
    return data


def mapping(stop):
    segment, offset = (int(x, 16) for x in stop["cs_ip"].split(":"))
    if offset != 0x02E9:
        raise ValueError("expected main opener return")
    loaded = segment - 0x277B + 0x1000
    if not 0 <= loaded <= 0x1000:
        raise ValueError("unsupported resident load mapping")
    return loaded - 0x1000


def admit_state(state):
    if state.emulator_sha256 != RUST_DOS_PINNED_SHA256 or state.stop is None:
        raise ValueError("checkpoint lacks pinned identity/stop")


def snapshot(client, delta):
    ds = 0x57E0 + delta
    raw = checked_read(client, ds, 0xA11D, 4)
    off, seg = struct.unpack("<HH", raw)
    window = None
    if seg or off:
        body = checked_read(client, seg, off, 0xA0)
        if body[:4] != b"WIND":
            raise ValueError("current window does not have FMT-UI-001 tag")
        window = {"number": struct.unpack_from("<I", body, 8)[0],
                  "flags": struct.unpack_from("<H", body, 0x9E)[0]}
    kinds = checked_read(client, 0x4F49 + delta, 0x0C33, 12)[::3]
    seed = struct.unpack("<I", checked_read(client, ds, 0x384A, 4))[0]
    hoff, hseg = struct.unpack("<HH", checked_read(client, ds, 0xA0F1, 4))
    boff, bseg = struct.unpack("<HH", checked_read(client, ds, 0x19C5, 4))
    coff, cseg = struct.unpack("<HH", checked_read(client, ds, 0xA125, 4))
    control = None
    if coff or cseg:
        child = checked_read(client, cseg, coff, 12)
        control = {"tag": child[:4].decode("ascii", errors="replace"),
                   "number": struct.unpack_from("<I", child, 8)[0]}
    return {"window": window, "slot_kinds": list(kinds), "rng_state": seed,
            "leader": struct.unpack("<H", checked_read(client, 0x4C13 + delta, 0x0369, 2))[0],
            "box_presence": [struct.unpack("<H", checked_read(client, bseg,
                boff + 0x42 * slot + 0x0E, 2))[0] for slot in range(4)] if bseg or boff else None,
            "selected_control": control,
            "mouse_enabled": checked_read(client, ds, 0x33D9, 1)[0],
            "handler": f"{hseg - delta:04X}:{hoff:04X}" if hseg else None,
            "mode": struct.unpack("<H", checked_read(client, ds, 0x0DAB, 2))[0]}


def ready(state, number):
    w = state["window"]
    return (w is not None and w["number"] == number and w["flags"] & 0x1F == 8
            and state["mouse_enabled"] == 1)


def main():
    if len(sys.argv) not in (3, 4) or any(not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", x) for x in sys.argv[1:]):
        raise SystemExit("usage: regenerate_checkpoints.py <new-run-name> <stage> [opened-source-run]")
    stage = sys.argv[2]
    if stage not in ("start", "create", "popup", "mouse-callback", "left-popup", "pointer-selection", "button-release", "party", "talk-entry", "play-entry", "talk", "play", "conversation-ready", "talk-resume", "play-resume"):
        raise SystemExit("unsupported capability stage")
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    source_name = sys.argv[3] if len(sys.argv) == 4 else "v175-regenerate-opened-20261019"
    resuming = stage in ("conversation-ready", "talk-resume", "play-resume")
    source = game / ("captures/rust-dos-regenerated" if resuming else "captures/rust-dos-restore-opener") / source_name
    state_name = "play" if stage == "play-resume" else "conversation-entry" if resuming else "opened"
    state = read_saved_state(source, state_name)
    verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
    admit_state(state)
    source_meta = json.loads((source / "regeneration.json").read_text(encoding="utf-8")) if resuming else None
    mapping_stop = source_meta.get("load_mapping_stop", source_meta["restored_stop"]) if resuming else state.stop
    delta = mapping(mapping_stop)
    rng = Recorder(delta)
    run = game / "captures/rust-dos-regenerated" / sys.argv[1]
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
    report = {"stage": stage, "source_run": source_name, "inputs": [], "observations": [], "checkpoints": [],
              "draws": [], "writes": [], "load_mapping_stop": mapping_stop}
    with RustDosSession(settings) as s:
        def record():
            (run / "regeneration.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        def observe():
            current = snapshot(s.client, delta)
            changed = not report["observations"] or current != report["observations"][-1]
            if changed:
                report["observations"].append(current)
            report["polls"] = report.get("polls", 0) + 1
            record()
            if changed:
                print(json.dumps(current), flush=True)
            return current
        def advance(until=None):
            target = int(s.stop["emu_ns"]) // 1000000 + 100
            def resume():
                return s.continue_(until=until) if until else s.continue_(until_ms=target)
            ob, exhausted = wait_for_stop(s, resume())
            while (not exhausted and ob.stop.get("reason") == "breakpoint"
                   and ob.stop.get("cs_ip") == rng.address(0x1000, 0x0822)):
                try:
                    draw = rng.record(s, ob.stop)
                except Exception as error:
                    report["unadmitted_rng"] = {"stop": s.stop, "error": str(error)}
                    record()
                    raise
                report["draws"].append(draw)
                record()
                ob, exhausted = wait_for_stop(s, resume())
            report["last_stop"] = ob.stop
            accepted = (ob.stop.get("reason") == "breakpoint" and ob.stop.get("cs_ip") == until) if until else ob.stop.get("reason") == "time"
            if exhausted or not accepted:
                report["unexpected_stop"] = ob.stop
                report["unexpected_state"] = snapshot(s.client, delta)
                report["button_match_fields"] = {
                    "current": checked_read(s.client, 0x57E0 + delta, 0xA125, 4).hex(),
                    "pressed": checked_read(s.client, 0x57E0 + delta, 0xA15F, 4).hex()}
                (run / "unexpected.png").write_bytes(s.client.screenshot())
                record()
                raise RuntimeError("bounded poll reached unexpected stop; no RNG draw admitted")
        def reach(number, empty_box=None):
            previous = None
            for _ in range(100):
                advance()
                current = observe()
                occupied = current["box_presence"]
                empty = empty_box is None or (occupied is not None and occupied[empty_box] == 0)
                if ready(current, number) and current == previous and empty:
                    return current
                previous = current
            raise RuntimeError(f"window {number} readiness budget exhausted")
        def save(name):
            before = observe()
            png = s.client.screenshot()
            (run / f"{name}.png").write_bytes(png)
            saved = s.save_state(name, check_files=True)
            report["checkpoints"].append({**checkpoint_summary(saved), "state": before,
                "frame_sha256": hashlib.sha256(png).hexdigest()})
            record()
        def click(x, y, button="left"):
            if (x, y) not in ((160, 93), (70, 107), (115, 129), (252, 47), (115, 117),
                             (157, 76), (121, 47), (158, 69), (121, 107), (158, 129), (125, 56), (100, 175)):
                raise ValueError("unsupported menu target")
            move = s.send_input("mouse", {"action": "move", "x": x, "y": y,
                                         "coords": "virtual"})
            for _ in range(100):
                advance()
                result = s.observe(move, timeout=0)
                current = observe()
                if result.state == "done" and current["mouse_enabled"] == 1:
                    break
            else:
                raise RuntimeError("pointer selection did not settle before click")
            report["inputs"].append({"action": "move", "x": x, "y": y,
                                      "operation": result.detail})
            op = s.send_input("mouse", {"action": "click", "button": button, "hold_ms": 500})
            for _ in range(10):
                advance()
                result = s.observe(op, timeout=0)
                if not result.pending:
                    break
            report["inputs"].append({"x": x, "y": y, "button": button,
                                      "operation": result.detail})
            record()
            if result.pending or result.state != "done":
                raise RuntimeError("input delivery not confirmed")
        def keypress(key):
            if key not in ("esc", "2"):
                raise ValueError("unsupported checkpoint key")
            op = s.send_input("key", {"key": key, "hold_ms": 100})
            for _ in range(10):
                advance()
                delivered = s.observe(op, timeout=0)
                if not delivered.pending:
                    break
            report["inputs"].append({"key": key, "operation": delivered.detail})
            record()
            if delivered.state != "done":
                raise RuntimeError("key input not delivered")
        report["restored_stop"] = s.stop
        record()
        if s.stop["cs_ip"] != state.stop["cs_ip"] or s.stop["reason"] != "load":
            raise RuntimeError("opened checkpoint restore differs")
        for segment, offset in ((0x1000, 0x0822), (0x1000, 0x0811)):
            s.client.set_breakpoint(f"{segment + delta:04X}:{offset:04X}")
        if not resuming:
            reach(19500)
            save("start")
        if stage not in ("start", "talk-entry", "talk", "conversation-ready", "talk-resume", "play-resume"):
            click(160, 93)
            reach(11500)
            save("create")
        if stage in ("popup", "mouse-callback", "left-popup", "pointer-selection", "button-release", "party"):
            if stage == "mouse-callback":
                s.client.set_breakpoint(f"{0x44D0 + delta:04X}:0053")
            if stage == "pointer-selection":
                s.client.set_breakpoint(f"{0x3D72 + delta:04X}:01FC")
            if stage == "button-release":
                s.client.set_breakpoint(f"{0x3D72 + delta:04X}:0404")
            click(70, 107, "left" if stage == "left-popup" else "right")
            # Discovery only: retain actual fields, never act on an unknown window.
            for _ in range(100):
                advance()
                current = observe()
                if current["handler"] == "566A:0052":
                    break
            else:
                (run / "failed-menu.png").write_bytes(s.client.screenshot())
                raise RuntimeError("character menu handler readiness budget exhausted")
            save("popup-observation")
        if stage == "party":
            click(115, 129)
            reach(18501)
            click(252, 47)
            current = reach(11500)
            if current["slot_kinds"] != [0, 0, 2, 0]:
                raise RuntimeError("added character metadata differs")
            save("added")
            click(70, 107, "right")
            reach(14000)
            click(115, 129)
            reach(14000)
            save("dropped")
            click(115, 117)
            current = reach(11500, empty_box=2)
            if current["box_presence"][2] != 0:
                raise RuntimeError("dropped character box is still occupied")
            save("afterdrop")
        if stage in ("play-entry", "play"):
            for index, (x, y, mx, my) in enumerate(((121, 47, 158, 69), (121, 107, 158, 129))):
                click(x, y, "right")
                reach(14000)
                click(mx, my)
                reach(18501)
                if index == 1:
                    click(125, 56)
                click(252, 47)
                reach(11500)
            keypress("esc")
            reach(19500)
        if stage in ("talk-entry", "play-entry", "talk", "play"):
            click(157, 76)
            previous = None
            skipped_intro = False
            for _ in range(1000):
                advance()
                current = observe()
                if stage in ("play-entry", "play") and current["mode"] == 1:
                    advance(until=rng.address(0x39D1, 0x097D))
                    current = observe()
                    if (ready(current, 19501) and current["mode"] == 1
                            and current["slot_kinds"] == [0, 2, 0, 2]):
                        save("play")
                        break
                if (stage in ("talk", "play") and not skipped_intro
                        and ready(current, 19501) and current["mode"] == 0
                        and current == previous):
                    keypress("esc")
                    skipped_intro = True
                if current == previous and any(ready(current, number) for number in (12500, 12501, 12503)):
                    save("conversation-entry")
                    break
                previous = current
            else:
                raise RuntimeError("conversation window readiness budget exhausted")
        if stage == "play-resume":
            current = observe()
            if not ready(current, 19501) or current["mode"] != 1 or current["slot_kinds"] != [0, 2, 0, 2]:
                raise RuntimeError("source is not the declared two-character map state")
            keypress("2")
            for _ in range(64):
                advance(until=rng.address(0x39D1, 0x097D))
                current = observe()
                if ready(current, 19501) and current["mode"] == 1 and current["leader"] == 1:
                    save("play-ready")
                    break
            else:
                raise RuntimeError("two-character leader selection gate exhausted")
        if resuming and stage != "play-resume":
            dispatcher = rng.address(0x39D1, 0x097D)
            def conversation_window(number):
                for _ in range(64):
                    advance(until=dispatcher)
                    current = observe()
                    numbers = number if isinstance(number, tuple) else (number,)
                    if any(ready(current, n) for n in numbers) and current["mode"] == 2:
                        return current
                raise RuntimeError("conversation input loop/window gate exhausted")
            current = conversation_window((12500, 12501))
            save("conversation-ready")
            if stage == "talk-resume":
                if current["window"]["number"] != 12501:
                    keypress("esc")
                    conversation_window(12501)
                save("responses")
                click(100, 175)
                conversation_window(12500)
                save("talk")


if __name__ == "__main__":
    main()
