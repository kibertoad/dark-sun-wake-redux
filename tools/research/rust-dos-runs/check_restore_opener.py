"""Synthetic checkpoint entry controls; no original or emulator is started."""
from pathlib import Path
import struct
import sys
import tempfile
from types import SimpleNamespace
import unittest
import xxhash

sys.path.insert(0, str(Path(__file__).resolve().parent))
from restore_opener import opener_mapping, verify_checkpoint_executable


class EntryChecks(unittest.TestCase):
    def test_copied_executable_must_match_approved_fingerprint(self):
        with tempfile.TemporaryDirectory() as folder:
            state = SimpleNamespace(directory=Path(folder))
            source = state.directory / "drive-c/DSUN.EXE"
            source.parent.mkdir()
            body = b"synthetic program identity"
            expected = {"byteLength": len(body), "xxh3_128": xxhash.xxh3_128_hexdigest(body)}
            with self.assertRaises(ValueError):
                verify_checkpoint_executable(state, expected)
            source.write_bytes(body)
            verify_checkpoint_executable(state, expected)
            source.write_bytes(b"x" * len(body))
            with self.assertRaises(ValueError):
                verify_checkpoint_executable(state, expected)
            source.write_bytes(body + b"changed")
            with self.assertRaises(ValueError):
                verify_checkpoint_executable(state, expected)

    def test_relocates_known_opener(self):
        self.assertEqual(opener_mapping("1D35:02E4", struct.pack("<BHH", 0x9A, 0x20, 0x4D05)), 0x05BA)

    def test_rejects_changed_stop_and_call(self):
        for stop, call in (
            ("1D35:02E9", struct.pack("<BHH", 0x9A, 0x20, 0x4D05)),
            ("1000:02E4", struct.pack("<BHH", 0x9A, 0x20, 0x4D05)),
            ("1D35:02E4", struct.pack("<BHH", 0x9A, 0x25, 0x4D05)),
            ("1D35:02E4", struct.pack("<BHH", 0xEA, 0x20, 0x4D05)),
            ("1D35:02E4", b"short"[:4]),
        ):
            with self.subTest(stop=stop, call=call):
                with self.assertRaises(ValueError):
                    opener_mapping(stop, call)


if __name__ == "__main__":
    unittest.main()
