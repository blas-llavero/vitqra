"""Run the independent synthetic reference checks and compare their JSON results."""

import json
from pathlib import Path
import subprocess
import sys


def check(directory):
    baseline = directory / "results.json"
    scripts = sorted(directory.glob("check_*.js")) + sorted(directory.glob("check_*.py"))
    if len(scripts) != 1 or not baseline.is_file():
        raise ValueError("Expected one check_*.js or check_*.py script and results.json")

    script = scripts[0]
    interpreter = ["node"] if script.suffix == ".js" else [sys.executable, "-E"]
    try:
        result = subprocess.run(
            [*interpreter, str(script)], capture_output=True, encoding="utf-8", timeout=60
        )
    except FileNotFoundError as error:
        raise ValueError(f"Interpreter unavailable ({interpreter[0]})") from error
    except subprocess.TimeoutExpired as error:
        raise ValueError("Check timed out after 60 seconds") from error
    if result.returncode != 0:
        details = result.stderr.strip() or result.stdout.strip()
        raise ValueError(f"Script exited with status {result.returncode}\n{details}".rstrip())

    expected = json.loads(baseline.read_text(encoding="utf-8"))
    actual = json.loads(result.stdout)
    if json.dumps(actual, sort_keys=True) != json.dumps(expected, sort_keys=True):
        raise ValueError("Script output differs from results.json")


def main():
    root = Path(__file__).resolve().parent
    directories = sorted({
        path.parent
        for pattern in ("*/results.json", "*/check_*.js", "*/check_*.py")
        for path in root.glob(pattern)
    })
    if not directories:
        print("No offline reference checks found", file=sys.stderr)
        return 1

    failures = 0
    for directory in directories:
        try:
            check(directory)
        except (OSError, ValueError) as error:
            print(f"FAIL {directory.name} ({error})", flush=True)
            failures += 1
        else:
            print(f"PASS {directory.name}", flush=True)

    print(f"Offline reference checks: {len(directories) - failures} passed, {failures} failed")
    return int(failures != 0)


if __name__ == "__main__":
    sys.exit(main())
