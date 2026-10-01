import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '../api/http';
import { queryRoots } from '../api/queryKeys';

/** One in-app notification (NotificationDto in C#). Type is e.g. ReviewNeeded, ClientAccepted, ClientDeclined. */
export interface NotificationDto {
  id: string;
  type: string;
  title: string;
  body: string;
  tripRequestId: string | null;
  isRead: boolean;
  createdAt: string;
}

/** GET /api/notifications/mine: the newest 50 and the unread count. */
export interface NotificationListDto {
  unreadCount: number;
  items: NotificationDto[];
}

/** How often the bell asks for new notifications. */
export const NOTIFICATIONS_POLL_MS = 30_000;

const mineKey = [queryRoots.notifications, 'mine'] as const;

/** The signed-in user's notifications, refreshed every 30 seconds while the app is open. */
export function useMyNotifications() {
  return useQuery({
    queryKey: mineKey,
    queryFn: async () => (await http.get<NotificationListDto>('/api/notifications/mine')).data,
    refetchInterval: NOTIFICATIONS_POLL_MS,
  });
}

export function useMarkNotificationRead() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await http.post(`/api/notifications/${id}/read`);
    },
    onSuccess: () => client.invalidateQueries({ queryKey: mineKey }),
  });
}

export function useMarkAllNotificationsRead() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async () => {
      await http.post('/api/notifications/read-all');
    },
    onSuccess: () => client.invalidateQueries({ queryKey: mineKey }),
  });
}
