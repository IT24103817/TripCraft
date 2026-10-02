import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { StatusBadge } from '@/shared/components';

describe('StatusBadge', () => {
  it('shows the readable status with a decorative dot', () => {
    const { container } = render(<StatusBadge status="NeedsOperator" />);

    expect(screen.getByText('Needs operator')).toBeInTheDocument();
    expect(container.querySelector('[aria-hidden="true"]')).toHaveClass('bg-current');
  });

  it('uses the label override when a page gives one', () => {
    render(<StatusBadge status="Approved" label="Sent to client" />);

    expect(screen.getByText('Sent to client')).toBeInTheDocument();
    expect(screen.queryByText('Approved')).not.toBeInTheDocument();
  });
});
