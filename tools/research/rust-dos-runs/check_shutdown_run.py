"""Synthetic fixed-Quit progression and private-file controls; no game launch."""
import copy
import hashlib
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from shutdown_run import QuitFlow, file_states, write_request


def window(number, flags=8, enabled=1):
    return {"window": {"number": number, "flags": flags}, "mouse_enabled": enabled}


class RunnerControls(unittest.TestCase):
    def test_verified_choice_sequence(self):
        for number in (14000, 14001):
            flow = QuitFlow()
            self.assertIsNone(flow.poll(window(19501), True))
            self.assertIsNone(flow.poll(window(number), False))
            self.assertIsNone(flow.poll(window(number, flags=0), True))
            self.assertIsNone(flow.poll(window(number, enabled=0), True))
            self.assertEqual(flow.poll(window(number), True), "q")
            self.assertIsNone(flow.poll(window(number), True))
            flow.exit_entry(True)
            self.assertEqual(flow.phase, "shutdown")

    def test_wrong_window_and_early_exit(self):
        with self.assertRaisesRegex(ValueError, "window"):
            QuitFlow().poll(window(18500), True)
        with self.assertRaisesRegex(ValueError, "before verified"):
            QuitFlow().exit_entry(True)
        flow = QuitFlow()
        flow.poll(window(14000), True)
        with self.assertRaisesRegex(ValueError, "before verified"):
            flow.exit_entry(False)
        flow.exit_entry(True)
        with self.assertRaisesRegex(ValueError, "before verified"):
            flow.exit_entry(True)

    def test_write_service_and_zero_count(self):
        registers = {"ax": "4000", "bx": "0009", "cx": "0000", "ds": "7000", "dx": "0004"}
        self.assertEqual(write_request(registers), {"handle": 9, "count": 0, "source": "7000:0004"})
        bad = copy.deepcopy(registers)
        bad["ax"] = "3F00"
        with self.assertRaisesRegex(ValueError, "AH40"):
            write_request(bad)
        bad = copy.deepcopy(registers)
        bad["ds"] = "10000"
        with self.assertRaisesRegex(ValueError, "word"):
            write_request(bad)

    def test_private_fixed_files(self):
        with tempfile.TemporaryDirectory() as directory:
            run = Path(directory)
            drive = run / "drive-c"
            drive.mkdir()
            (drive / "unrelated.txt").write_bytes(b"not included")
            (drive / "DARKRUN.GFF").write_bytes(b"synthetic")
            result = file_states(run)
            self.assertEqual(set(result), {"DARKRUN.GFF", "CHARSAVE.GFF", "SAVE02.SAV", "RGN032.GFF"})
            self.assertEqual(result["DARKRUN.GFF"], {"exists": True, "length": 9,
                            "sha256": hashlib.sha256(b"synthetic").hexdigest()})
            self.assertEqual(result["SAVE02.SAV"], {"exists": False})
            (drive / "CHARSAVE.GFF").mkdir()
            with self.assertRaisesRegex(ValueError, "unsupported"):
                file_states(run)


if __name__ == "__main__":
    unittest.main()
