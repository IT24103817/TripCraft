import type { ReactNode } from 'react';

export type ToastKind = 'success' | 'error';

export interface ToastItem {
  id: number;
  kind: ToastKind;
  message: string;
}

export interface ToastApi {
  success: (message: string) => void;
  error: (message: string) => void;
}

export interface ToastProviderProps {
  children: ReactNode;
}
