"""Synthetic controls for private probe drive preparation (no licensed inputs)."""
import json
from pathlib import Path
import sys
import tempfile
import unittest

import xxhash

sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_drive import prepare_drive


class DriveChecks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.source, self.destination = self.root / "source", self.root / "drive"
        self.source.mkdir()
        self.destination.mkdir()
        self.manifest = self.root / "manifest.json"
        self.body = b"synthetic resource"
        (self.source / "RESOURCE.GFF").write_bytes(self.body)
        self.rows = [{"path": "RESOURCE.GFF", "size": len(self.body),
                      "xxh3": xxhash.xxh3_128_hexdigest(self.body)}]

    def run_copy(self):
        self.manifest.write_text(json.dumps({"files": self.rows}))
        return prepare_drive(self.source, self.destination, self.manifest)

    def test_only_manifest_files(self):
        (self.source / "captures").mkdir()
        (self.source / "captures" / "ignored.bin").write_bytes(b"not selected")
        self.rows.append({"path": "game.ins"})
        self.assertEqual(self.run_copy(), 1)
        self.assertEqual(list(self.destination.iterdir()), [self.destination / "RESOURCE.GFF"])
        self.assertEqual((self.destination / "RESOURCE.GFF").read_bytes(), self.body)

    def test_bad_source_publishes_nothing(self):
        for missing in (False, True):
            with self.subTest(missing=missing):
                path = self.source / "RESOURCE.GFF"
                if missing:
                    path.unlink()
                else:
                    path.write_bytes(b"x" * len(self.body))
                with self.assertRaises(ValueError):
                    self.run_copy()
                self.assertEqual(list(self.destination.iterdir()), [])

    def test_reject_paths_and_duplicates(self):
        for name in ("../outside", "/absolute", "C:/outside", "a\\b", "a//b", "a/./b"):
            with self.subTest(name=name):
                self.rows[0]["path"] = name
                with self.assertRaises(ValueError):
                    self.run_copy()
        self.rows[0]["path"] = "RESOURCE.GFF"
        self.rows.append(dict(self.rows[0], path="resource.gff"))
        with self.assertRaises(ValueError):
            self.run_copy()
        self.assertEqual(list(self.destination.iterdir()), [])

    def test_nonempty_destination(self):
        (self.destination / "keep").write_bytes(b"keep")
        with self.assertRaises(ValueError):
            self.run_copy()
        self.assertEqual((self.destination / "keep").read_bytes(), b"keep")

    def test_linked_source(self):
        target = self.source / "RESOURCE.GFF"
        target.unlink()
        outside = self.root / "outside"
        outside.write_bytes(self.body)
        try:
            target.symlink_to(outside)
        except OSError as error:
            self.skipTest(f"host cannot create synthetic symlink: {error}")
        with self.assertRaises(ValueError):
            self.run_copy()


if __name__ == "__main__":
    unittest.main()
