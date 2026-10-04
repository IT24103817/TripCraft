"""Publish one pre-reviewed Student C change when its real scheduled time arrives."""
import argparse
from datetime import datetime, timedelta, timezone
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import shutil
import subprocess
import urllib.request

BASE = 'ac2f049eeed39b38a9cfa42ada6adbea1b046766'
BRANCH = 'IT24103079'
START = datetime(2026, 10, 2, 2, 30, tzinfo=timezone.utc)  # 08:00 Asia/Colombo
END = datetime(2026, 10, 2, 18, 30, tzinfo=timezone.utc)  # next local midnight
QUEUE = Path(__file__).resolve().parent
FILTER = 'FullyQualifiedName~TripCraft.Tests.Quotations|FullyQualifiedName~TripCraft.Tests.Workflows'


def run(*args, cwd=None, capture=False, env=None):
    result = subprocess.run(args, cwd=cwd, env=env, check=True,
                            text=True, encoding='utf-8', stdout=subprocess.PIPE if capture else None)
    return result.stdout.strip() if capture else ''


def sha(text):
    return hashlib.sha256(text.replace('\r\n', '\n').encode('utf-8')).hexdigest()


def due_count(now):
    if now < START or now >= END:
        return 0
    return min(10, int((now - START).total_seconds() // 3600) + 1)


def marker(number):
    return f'Student-C-Schedule: 2026-10-02/{number:02d}'


def completed(student):
    run('git', 'merge-base', '--is-ancestor', BASE, 'HEAD', cwd=student)
    commits = run('git', 'rev-list', '--reverse', f'{BASE}..HEAD', cwd=student, capture=True).splitlines()
    if len(commits) > 10:
        raise RuntimeError('Unexpected commits after the approved baseline; no publication performed.')
    for number, commit in enumerate(commits, 1):
        body = run('git', 'show', '-s', '--format=%B', commit, cwd=student, capture=True)
        parents = run('git', 'show', '-s', '--format=%P', commit, cwd=student, capture=True).split()
        if marker(number) not in body.splitlines() or len(parents) != 1:
            raise RuntimeError('Student C history changed outside this queue; refusing to overwrite it.')
    return len(commits)


def read_item(number):
    item = json.loads((QUEUE / f'{number:02d}.json').read_text(encoding='utf-8'))
    if item['id'] != number or not item['message'].startswith('test('):
        raise ValueError('Unexpected queue item')
    return item


def apply_item(student, item):
    paths = []
    for change in item['files']:
        relative = PurePosixPath(change['path'])
        if relative.is_absolute() or '..' in relative.parts or '\\' in str(relative):
            raise ValueError('Unsafe file path')
        if not (str(relative).startswith('backend/tests/TripCraft.Tests/') and relative.suffix == '.cs'
                or str(relative) == 'docs/student-c-files.txt'):
            raise ValueError('Queue may only update Student C tests and their manifest')
        path = student / relative
        if not path.resolve().is_relative_to(student.resolve()):
            raise ValueError('File escapes the checkout')
        expected = change['before_sha256']
        if expected is None:
            if path.exists():
                raise RuntimeError(f'New file already exists: {relative}')
        elif not path.exists() or sha(path.read_text(encoding='utf-8-sig')) != expected:
            raise RuntimeError(f'File changed since preparation: {relative}')
        paths.append((path, change['content']))
    # Validate every precondition before writing any file in this item.
    for path, content in paths:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(content, encoding='utf-8', newline='\n')


def verify(student, integration):
    tracked = run('git', 'ls-files', '--', 'backend', cwd=student, capture=True).splitlines()
    added = run('git', 'ls-files', '--others', '--exclude-standard', '--', 'backend',
                cwd=student, capture=True).splitlines()
    for relative in sorted(set(tracked + added)):
        destination = integration / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(student / relative, destination)
    project = integration / 'backend/tests/TripCraft.Tests/TripCraft.Tests.csproj'
    run('dotnet', 'test', str(project), '--filter', FILTER, '--verbosity', 'minimal',
        '--logger', 'trx;LogFileName=student-c.trx', '--results-directory', str(integration / 'test-results'))
    run('dotnet', 'build', str(integration / 'backend/TripCraft.sln'), '--no-restore', '--verbosity', 'minimal')


def report(message):
    print(message, flush=True)
    if os.environ.get('GITHUB_STEP_SUMMARY'):
        with open(os.environ['GITHUB_STEP_SUMMARY'], 'a', encoding='utf-8') as summary:
            summary.write(message + '\n')


def disable_schedule():
    token = os.environ.get('GH_TOKEN')
    repo = os.environ.get('GITHUB_REPOSITORY')
    if not token or repo != 'ilhamhilmy63/TripCraft':
        raise RuntimeError('Expected GitHub workflow credentials are unavailable')
    request = urllib.request.Request(
        f'https://api.github.com/repos/{repo}/actions/workflows/student-c-hourly.yml/disable',
        method='PUT', headers={'Authorization': f'Bearer {token}',
                               'Accept': 'application/vnd.github+json',
                               'X-GitHub-Api-Version': '2022-11-28'})
    with urllib.request.urlopen(request, timeout=30) as response:
        if response.status != 204:
            raise RuntimeError('Could not disable the completed schedule')
    report('All ten changes published. This workflow is now disabled.')


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--student', type=Path, required=True)
    parser.add_argument('--integration', type=Path, required=True)
    parser.add_argument('--mode', choices=['validate', 'publish'], default='validate')
    args = parser.parse_args()
    student = args.student.resolve()
    integration = args.integration.resolve()
    if run('git', 'status', '--porcelain', cwd=student, capture=True):
        raise RuntimeError('Student checkout must start clean')
    if run('git', 'branch', '--show-current', cwd=student, capture=True) != BRANCH:
        raise RuntimeError('Wrong target branch')
    count = completed(student)
    if args.mode == 'validate':
        for number in range(count + 1, 11):
            apply_item(student, read_item(number))
        verify(student, integration)
        report('Validation passed for the complete prepared queue. No commits or pushes were made.')
        return

    now = datetime.now(timezone.utc)
    if not START <= now < END:
        report('Outside the October 2, 2026 publishing window. No changes made.')
        return
    if count == 10:
        disable_schedule()
        return
    number = count + 1
    if number > due_count(now):
        report(f'Change {number:02d} is not due yet. Retry runs do not duplicate commits.')
        return
    item = read_item(number)
    original_head = run('git', 'rev-parse', 'HEAD', cwd=student, capture=True)
    apply_item(student, item)
    verify(student, integration)
    # Abort instead of merging, resetting, or force-pushing if somebody changed the target.
    run('git', 'fetch', 'origin', BRANCH, cwd=student)
    if run('git', 'rev-parse', f'origin/{BRANCH}', cwd=student, capture=True) != original_head:
        raise RuntimeError('Target changed while tests ran; no commit or push performed')
    if not START <= datetime.now(timezone.utc) < END:
        raise RuntimeError('Publishing window closed while tests ran')
    paths = [change['path'] for change in item['files']]
    run('git', 'add', '--', *paths, cwd=student)
    staged = run('git', 'diff', '--cached', '--name-only', cwd=student, capture=True).splitlines()
    if sorted(staged) != sorted(paths):
        raise RuntimeError('Unexpected staged files')
    run('git', 'diff', '--cached', '--check', cwd=student)
    env = os.environ.copy()
    env.update(GIT_AUTHOR_NAME='inzam2659-pixel', GIT_AUTHOR_EMAIL='inzam2659@gmail.com',
               GIT_COMMITTER_NAME='github-actions[bot]',
               GIT_COMMITTER_EMAIL='41898282+github-actions[bot]@users.noreply.github.com')
    # Actual current Git timestamps: no backdating or simulated work history.
    env.pop('GIT_AUTHOR_DATE', None)
    env.pop('GIT_COMMITTER_DATE', None)
    body = ('Prepared and tested before publication; published automatically after cloud verification.\n\n'
            + marker(number))
    run('git', 'commit', '-m', item['message'], '-m', body, cwd=student, env=env)
    run('git', 'push', 'origin', f'HEAD:refs/heads/{BRANCH}', cwd=student)
    tip = run('git', 'rev-parse', 'HEAD', cwd=student, capture=True)
    remote = run('git', 'ls-remote', '--heads', 'origin', f'refs/heads/{BRANCH}', cwd=student, capture=True)
    if not remote.startswith(tip + '\t'):
        raise RuntimeError('Remote verification failed')
    report(f'Published {number}/10: {item["message"]} (`{tip}`).')
    if number == 10:
        disable_schedule()


if __name__ == '__main__':
    main()
