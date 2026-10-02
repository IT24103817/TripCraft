import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createRef } from 'react';
import { describe, expect, it, vi } from 'vitest';
import { Button } from '@/shared/components';

describe('Button', () => {
  it('is a primary, non-submitting button by default and calls onClick', async () => {
    const onClick = vi.fn();
    render(<Button onClick={onClick}>Save</Button>);

    const button = screen.getByRole('button', { name: 'Save' });
    expect(button).toHaveAttribute('type', 'button');
    expect(button).toHaveClass('btn-primary');

    await userEvent.click(button);
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it('uses the Hallmark class for each variant and keeps extra classes', () => {
    render(
      <>
        <Button variant="secondary">Cancel</Button>
        <Button variant="danger" className="w-full">
          Delete
        </Button>
      </>,
    );

    expect(screen.getByRole('button', { name: 'Cancel' })).toHaveClass('btn-secondary');
    expect(screen.getByRole('button', { name: 'Delete' })).toHaveClass('btn-danger', 'w-full');
  });

  it('while loading shows the loading text, is disabled, busy and ignores clicks', async () => {
    const onClick = vi.fn();
    render(
      <Button isLoading loadingText="Saving…" onClick={onClick}>
        Save
      </Button>,
    );

    const button = screen.getByRole('button', { name: 'Saving…' });
    expect(button).toBeDisabled();
    expect(button).toHaveAttribute('aria-busy', 'true');

    await userEvent.click(button);
    expect(onClick).not.toHaveBeenCalled();
  });

  it('forwards the ref to the native button (e.g. to focus it)', () => {
    const ref = createRef<HTMLButtonElement>();
    render(<Button ref={ref}>Focus me</Button>);

    ref.current?.focus();
    expect(screen.getByRole('button', { name: 'Focus me' })).toHaveFocus();
  });
});
