from datetime import timedelta
from pathlib import Path
import tempfile
import unittest

from publish import START, END, apply_item, due_count, read_item, sha


class ScheduleTests(unittest.TestCase):
    def test_nothing_is_due_before_eight_am(self):
        self.assertEqual(due_count(START - timedelta(seconds=1)), 0)

    def test_each_hour_unlocks_exactly_one_more_change(self):
        for hour in range(10):
            self.assertEqual(due_count(START + timedelta(hours=hour)), hour + 1)
            self.assertEqual(due_count(START + timedelta(hours=hour, minutes=10)), hour + 1)

    def test_ten_is_the_maximum_and_later_dates_do_nothing(self):
        self.assertEqual(due_count(END - timedelta(seconds=1)), 10)
        self.assertEqual(due_count(END), 0)
        self.assertEqual(due_count(START.replace(year=2027)), 0)


class FileGuardTests(unittest.TestCase):
    def test_hashes_ignore_checkout_line_ending_conversion(self):
        self.assertEqual(sha('a\r\nb\r\n'), sha('a\nb\n'))

    def test_modified_manifest_prevents_all_writes(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            item = read_item(1)
            manifest = root / 'docs/student-c-files.txt'
            manifest.parent.mkdir()
            manifest.write_text('unexpected\n')
            with self.assertRaises(RuntimeError):
                apply_item(root, item)
            self.assertFalse((root / item['files'][0]['path']).exists())

    def test_existing_new_file_is_never_overwritten(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            item = read_item(1)
            target = root / item['files'][0]['path']
            target.parent.mkdir(parents=True)
            target.write_text('user work')
            with self.assertRaises(RuntimeError):
                apply_item(root, item)
            self.assertEqual(target.read_text(), 'user work')

    def test_path_traversal_and_unrelated_files_are_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            for path in ['../escape.cs', '/escape.cs', 'README.md', '.github/workflows/other.yml']:
                with self.assertRaises(ValueError):
                    apply_item(Path(directory), {'files': [
                        {'path': path, 'before_sha256': None, 'content': 'unwanted'}]})

    def test_queue_manifest_hashes_form_a_contiguous_chain(self):
        for number in range(2, 11):
            previous = read_item(number - 1)['files'][1]['content']
            self.assertEqual(sha(previous), read_item(number)['files'][1]['before_sha256'])


if __name__ == '__main__':
    unittest.main()
