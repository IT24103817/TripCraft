import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { cn } from '../utils/cn';
import { formatDateTime } from '../utils/format';
import {
  useMarkAllNotificationsRead,
  useMarkNotificationRead,
  useMyNotifications,
  type NotificationDto,
} from './notificationsApi';

/**
 * The bell in the top bar: the unread count (polled every 30 s) and a drop-down list. Clicking a notification
 * marks it read and opens its trip, when it has one.
 */
export function NotificationBell() {
  const navigate = useNavigate();
  const notifications = useMyNotifications();
  const markRead = useMarkNotificationRead();
  const markAllRead = useMarkAllNotificationsRead();
  const [open, setOpen] = useState(false);

  // Escape closes the list, like the dialogs.
  useEffect(() => {
    if (!open) return;
    const onKey = (event: KeyboardEvent) => event.key === 'Escape' && setOpen(false);
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [open]);

  const unread = notifications.data?.unreadCount ?? 0;
  const items = notifications.data?.items ?? [];

  const openNotification = (item: NotificationDto) => {
    if (!item.isRead) markRead.mutate(item.id);
    setOpen(false);
    if (item.tripRequestId) navigate(`/trips/${item.tripRequestId}`);
  };

  return (
    <div className="relative">
      <button
        type="button"
        className="btn-secondary relative"
        aria-expanded={open}
        aria-controls="notification-list"
        aria-label={`Notifications, ${unread} unread`}
        onClick={() => setOpen((value) => !value)}
      >
        <svg aria-hidden="true" viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor">
          <path
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M15 17h5l-1.4-1.4A2 2 0 0 1 18 14.2V11a6 6 0 0 0-5-5.9V4a1 1 0 1 0-2 0v1.1A6 6 0 0 0 6 11v3.2a2 2 0 0 1-.6 1.4L4 17h5m6 0a3 3 0 1 1-6 0"
          />
        </svg>
        {unread > 0 && (
          <span
            aria-hidden="true"
            className="absolute -right-1 -top-1 min-w-5 rounded-full bg-red-600 px-1 text-center text-xs font-semibold text-white"
          >
            {unread}
          </span>
        )}
      </button>
      {open && (
        <div
          id="notification-list"
          className="absolute right-0 z-40 mt-2 w-80 max-w-[calc(100vw-2rem)] rounded-lg border border-slate-200 bg-surface shadow-xl"
        >
          <div className="flex items-center justify-between border-b border-slate-200 px-3 py-2">
            <h2 className="text-sm font-semibold text-slate-900">Notifications</h2>
            <button
              type="button"
              className="text-xs text-brand-700 hover:underline disabled:text-slate-400"
              disabled={unread === 0 || markAllRead.isPending}
              onClick={() => markAllRead.mutate()}
            >
              Mark all read
            </button>
          </div>
          {items.length === 0 ? (
            <p className="px-3 py-6 text-center text-sm text-slate-500">No notifications yet.</p>
          ) : (
            <ul aria-label="Notifications" className="max-h-96 divide-y divide-slate-100 overflow-y-auto">
              {items.map((item) => (
                <li key={item.id}>
                  <button
                    type="button"
                    className={cn(
                      'block w-full px-3 py-2 text-left text-sm hover:bg-slate-50',
                      item.isRead ? 'text-slate-600' : 'bg-brand-50 text-slate-900',
                    )}
                    onClick={() => openNotification(item)}
                  >
                    <span className="block font-medium">
                      {item.title}
                      {!item.isRead && <span className="sr-only"> (unread)</span>}
                    </span>
                    <span className="block text-xs">{item.body}</span>
                    <span className="block text-xs text-slate-500">{formatDateTime(item.createdAt)}</span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
}
