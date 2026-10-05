import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';

export type LlmProvider = 'ollama' | 'gemini' | 'groq';

/** GET /api/admin/settings (Admin): the operator settings the API reads on every request. */
export interface SettingsDto {
  llmProvider: LlmProvider;
  cancellationCutoffDays: number;
  marginPct: number;
  depositPct: number;
  operatorContact: string;
  updatedAt: string;
}

/** Body of PUT /api/admin/settings: the same fields without updatedAt. */
export type SaveSettingsRequest = Omit<SettingsDto, 'updatedAt'>;

export function useSettings() {
  return useQuery({
    queryKey: [queryRoots.settings],
    queryFn: async () => (await http.get<SettingsDto>('/api/admin/settings')).data,
  });
}

/** PUT /api/admin/settings, then loads the stored values again (with the new updatedAt). */
export function useSaveSettings() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (body: SaveSettingsRequest) => {
      await http.put('/api/admin/settings', body);
    },
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.settings] }),
  });
}
