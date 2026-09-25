import { screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { renderApp, signInAs } from '@/test/render';

describe('Resource screens', () => {
  it('say which API they are waiting for instead of showing invented data', async () => {
    signInAs('OperationsManager');
    renderApp('/resources/guides');

    expect(await screen.findByText('Available when Resource Management is merged')).toBeInTheDocument();
    expect(screen.getByText('GET/POST /api/guides')).toBeInTheDocument();
  });
});
