"""Keep an already-running cloud job alive when cron triggers have not arrived."""
import argparse
from datetime import datetime, timedelta, timezone
from pathlib import Path
import sys
import time

from publish import END, START, completed, report, run


def wait_seconds(count, now):
    if count >= 10 or now >= END:
        return None
    return max(0, (START + timedelta(hours=count) - now).total_seconds())


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--student', type=Path, required=True)
    parser.add_argument('--integration', type=Path, required=True)
    args = parser.parse_args()
    student = args.student.resolve()
    integration = args.integration.resolve()
    began = time.monotonic()
    while True:
        count = completed(student)
        now = datetime.now(timezone.utc)
        delay = wait_seconds(count, now)
        if delay is None:
            if count < 10:
                raise RuntimeError('Date window closed before all changes were published')
            report('Recovery finished: all ten changes are on the remote branch.')
            return
        if time.monotonic() - began > 5.75 * 3600:
            raise RuntimeError('Recovery runner time budget reached; restart to continue safely')
        if delay:
            due = START + timedelta(hours=count)
            report(f'Cloud runner waiting for change {count + 1}/10 at {due.isoformat()}.')
            while datetime.now(timezone.utc) < due:
                if time.monotonic() - began > 5.75 * 3600:
                    raise RuntimeError('Recovery runner time budget reached')
                time.sleep(min(30, max(0, (due - datetime.now(timezone.utc)).total_seconds())))
        run(sys.executable, str(Path(__file__).with_name('publish.py')),
            '--student', str(student), '--integration', str(integration), '--mode', 'publish')
        if completed(student) != count + 1:
            raise RuntimeError('Publisher made no progress; stopping instead of repeatedly running')


if __name__ == '__main__':
    main()
