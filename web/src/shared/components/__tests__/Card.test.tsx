import { render, screen } from '@testing-library/react';
import { useForm } from 'react-hook-form';
import { describe, expect, it } from 'vitest';
import { Card, FormField, StatusBadge } from '@/shared/components';

describe('Card', () => {
  it('shows the title as a heading with the actions beside it', () => {
    render(
      <Card title="Itinerary" actions={<StatusBadge status="PendingApproval" />}>
        <p>Day 1 — Kandy</p>
      </Card>,
    );

    expect(screen.getByRole('heading', { level: 2, name: 'Itinerary' })).toBeInTheDocument();
    expect(screen.getByText('Pending approval')).toBeInTheDocument();
    expect(screen.getByText('Day 1 — Kandy')).toBeInTheDocument();
  });

  it('renders only its children, with the card class and ARIA props, when there is no title', () => {
    render(
      <Card role="alert" className="bg-red-50">
        Could not load
      </Card>,
    );

    const card = screen.getByRole('alert');
    expect(card).toHaveClass('card', 'bg-red-50');
    expect(screen.queryByRole('heading')).not.toBeInTheDocument();
  });
});

const FieldWithError = ({ error }: { error?: string }) => {
  const { register } = useForm<{ email: string }>();
  return <FormField label="Email" registration={register('email')} error={error} />;
};

describe('FormField', () => {
  it('marks the input invalid and links the error message when there is an error', () => {
    render(<FieldWithError error="Enter a valid email" />);

    const input = screen.getByLabelText('Email');
    expect(input).toHaveAttribute('aria-invalid', 'true');
    expect(input).toHaveAccessibleDescription('Enter a valid email');
  });

  it('is not marked invalid without an error', () => {
    render(<FieldWithError />);

    expect(screen.getByLabelText('Email')).not.toHaveAttribute('aria-invalid');
  });
});
