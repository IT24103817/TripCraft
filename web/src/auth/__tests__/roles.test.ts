import { describe, expect, it } from 'vitest';
import { homeFor, isStaff } from '@/auth/roles';

describe('homeFor', () => {
  it.each([
    ['OperationsManager', '/dashboard'],
    ['Admin', '/dashboard'],
    ['Tourist', '/mobile-app'],
    ['Guide', '/mobile-app'],
  ] as const)('sends a %s to %s', (role, home) => {
    expect(homeFor(role)).toBe(home);
  });

  it('treats only Operations Managers and Admins as staff', () => {
    expect(isStaff('OperationsManager')).toBe(true);
    expect(isStaff('Admin')).toBe(true);
    expect(isStaff('Tourist')).toBe(false);
    expect(isStaff('Guide')).toBe(false);
    expect(isStaff(undefined)).toBe(false);
  });
});
