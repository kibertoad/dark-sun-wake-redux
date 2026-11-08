"""Fixed Q-CONFIG-014 native seed/opener continuation, with existing watches.

FND-UI-039 and startup_window.seed_caller supply the only admitted route.
No inputs, writes, restored states, arbitrary stops or seed substitutions.
"""
import struct

from regenerate_checkpoints import checked_read
from startup_window import seed_caller, wait_for_stop


def observe_opener(session, delta, report, save, history):
    def address(segment, offset):
        segment += delta
        if not 0 <= segment <= 0xFFFF:
            raise ValueError("opener relocation outside real mode")
        return f"{segment:04X}:{offset:04X}"

    data = 0x57E0 + delta
    setter, generator = address(0x1000, 0x0811), address(0x1000, 0x0822)
    opener = address(0x277B, 0x02E4)
    expected, phase = setter, "seed-entry"
    seed_expected = int.from_bytes(bytes.fromhex(report["initial_rng_state"]), "little")
    events = report["opener_events"] = []

    def seed():
        return int.from_bytes(checked_read(session.client, data, 0x384A, 4), "little")

    if seed() != seed_expected:
        raise ValueError("seed changed before the admitted native seed call")
    # The disc observer retains its setter/generator guards across this handoff.
    session.client.set_breakpoint(opener)
    for _ in range(200):
        operation = session.continue_()
        observed, exhausted = wait_for_stop(session, operation, attempts=2)
        stop = report["last_stop"] = observed.stop
        save()
        if exhausted:
            raise RuntimeError("opener observation budget exhausted")
        site = stop.get("cs_ip", "").upper()
        # A watch stop at a guarded instruction is ambiguous, never permission
        # to resume through the generator or a second/native seed call.
        if site == generator or site == setter and (phase != "seed-entry" or stop.get("reason") != "breakpoint"):
            raise ValueError("opener reached an unadmitted RNG/seed guard")
        if stop.get("reason") == "watchpoint":
            if site in (expected, opener):
                raise ValueError("watch collides with a required opener boundary")
            history.changed(session.client, stop)
            if phase != "seed-return" and seed() != seed_expected:
                raise ValueError("seed changed outside the admitted native call")
            save()
            continue
        history.check(session.client)
        history._stop(stop)
        registers = stop.get("registers", {})
        if stop.get("reason") != "breakpoint" or site != expected:
            raise ValueError("opener reached an undeclared boundary")
        if any(int(registers[key], 16) != data for key in ("ds", "ss")):
            raise ValueError("opener data/stack binding differs")
        sp = int(registers["sp"], 16)
        event = {"phase": phase, "stop": stop, "values": dict(history.previous), "seed": seed()}
        events.append(event)
        history.sequence, history.time = stop["seq"], stop["emu_ns"]
        if phase == "seed-entry":
            if event["seed"] != seed_expected:
                raise ValueError("unexpected seed at native setter entry")
            frame = checked_read(session.client, data, sp, 6)
            off, seg = struct.unpack("<HH", frame[:4])
            if off < 5:
                raise ValueError("native seed return offset underflows")
            trampoline = checked_read(session.client, 0x56B2 + delta, 0x0020, 5)
            call = checked_read(session.client, seg, off - 5, 5)
            expected, seed_expected = seed_caller(frame, trampoline, call, 0x1000 + delta)
            event["caller"] = {"overlay": 180, "return": expected, "argument": seed_expected,
                               "frame": frame.hex(), "validated_trampoline_and_call": True}
            session.client.set_breakpoint(expected)
            phase = "seed-return"
        elif phase == "seed-return":
            if event["seed"] != seed_expected:
                raise ValueError("native seed result differs from its argument")
            session.client.remove_breakpoints(expected)
            expected, phase = opener, "opener-call"
        else:
            if event["seed"] != seed_expected:
                raise ValueError("seed changed after the admitted native call")
            argument = int.from_bytes(checked_read(session.client, data, sp, 2), "little")
            event["opener_argument_word"] = argument
            if argument != 1:
                raise ValueError("opener argument differs from the fixed startup route")
            save()
            return
        save()
    raise RuntimeError("opener exceeds200 named stops")
