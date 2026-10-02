export interface TabItem {
  id: string;
  label: string;
}

export interface TabListProps {
  /** Accessible name of the tab row, e.g. "Trip sections". */
  label: string;
  tabs: TabItem[];
  selected: string;
  onSelect: (id: string) => void;
  className?: string;
}
