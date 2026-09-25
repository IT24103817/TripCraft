import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { CreateUserRequest, UserDto } from '../types';

/** GET /api/admin/users returns the full list (no paging on the API), so the page pages client-side. */
export function useUsers() {
  return useQuery({
    queryKey: [queryRoots.users],
    queryFn: async () => (await http.get<UserDto[]>('/api/admin/users')).data,
  });
}

export function useCreateUser() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (request: CreateUserRequest) =>
      (await http.post<UserDto>('/api/admin/users', request)).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.users] }),
  });
}

export function useDeactivateUser() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await http.post(`/api/admin/users/${id}/deactivate`);
    },
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.users] }),
  });
}
