/** Row-level delete link that does not also open the row. */
export function DeleteButton({ label, onClick }: { label: string; onClick: () => void }) {
  return (
    <button
      type="button"
      className="text-red-700 hover:underline"
      aria-label={`Delete ${label}`}
      onClick={(event) => {
        event.stopPropagation();
        onClick();
      }}
    >
      Delete
    </button>
  );
}
