"""One nonblocking private DARKRUN lock for the fixed failed-Save probe."""
import os
from pathlib import Path

LIMIT = 128 * 1024 * 1024


class PrivatePayloadLock:
    def __init__(self, run):
        self.run = Path(run)
        self.file = None
        self.locked = False

    def acquire(self):
        if os.name != "nt":
            raise ValueError("private payload lock requires Windows")
        if self.file is not None:
            raise ValueError("private payload lock already owns a handle")
        drive, path = self.run / "drive-c", self.run / "drive-c/DARKRUN.GFF"
        if self.run.parent.name != "rust-dos-save-failure":
            raise ValueError("lock requires the fixed failed-Save run root")
        if any(p.is_symlink() or p.is_junction() for p in (self.run.parent, self.run, drive, path)):
            raise ValueError("private payload path contains a link")
        if not path.is_file() or not 0 < path.stat().st_size <= LIMIT:
            raise ValueError("unsupported private payload file")
        import msvcrt
        self.file = path.open("r+b", buffering=0)
        try:
            self.file.seek(0)
            msvcrt.locking(self.file.fileno(), msvcrt.LK_NBLCK, LIMIT)
            self.locked = True
        except BaseException:
            self.file.close()
            self.file = None
            raise

    def release(self):
        if self.file is None:
            return
        import msvcrt
        try:
            if self.locked:
                self.file.seek(0)
                msvcrt.locking(self.file.fileno(), msvcrt.LK_UNLCK, LIMIT)
        finally:
            self.file.close()
            self.file, self.locked = None, False

    def __enter__(self):
        self.acquire()
        return self

    def __exit__(self, *args):
        self.release()
