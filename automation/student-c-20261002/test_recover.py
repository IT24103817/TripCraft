from datetime import timedelta
import unittest

from publish import START, END
from recover import wait_seconds


class RecoveryTimingTests(unittest.TestCase):
    def test_missed_items_are_due_immediately(self):
        for completed in range(5):
            self.assertEqual(wait_seconds(completed, START + timedelta(hours=4)), 0)

    def test_next_item_keeps_its_original_hour(self):
        self.assertEqual(wait_seconds(5, START + timedelta(hours=4, minutes=10)), 50 * 60)

    def test_tenth_item_cannot_be_published_early(self):
        self.assertEqual(wait_seconds(9, START + timedelta(hours=8, minutes=59)), 60)

    def test_completion_and_end_of_day_stop_recovery(self):
        self.assertIsNone(wait_seconds(10, START + timedelta(hours=9)))
        self.assertIsNone(wait_seconds(0, END))


if __name__ == '__main__':
    unittest.main()
