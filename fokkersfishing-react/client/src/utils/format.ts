import { CatchStatus } from '../api/types';

export function formatDateTime(value: string | Date | null | undefined): string {
  if (!value) return '';
  const d = typeof value === 'string' ? new Date(value) : value;
  if (Number.isNaN(d.getTime())) return '';
  return d.toLocaleString();
}

export function formatDate(value: string | Date | null | undefined): string {
  if (!value) return '';
  const d = typeof value === 'string' ? new Date(value) : value;
  if (Number.isNaN(d.getTime())) return '';
  return d.toLocaleDateString();
}

export interface StatusMeta {
  label: string;
  color: 'success' | 'warning' | 'error' | 'default';
}

export function statusMeta(status: CatchStatus): StatusMeta {
  switch (status) {
    case CatchStatus.Approved:
      return { label: 'Approved', color: 'success' };
    case CatchStatus.Pending:
      return { label: 'Pending', color: 'warning' };
    case CatchStatus.Rejected:
      return { label: 'Rejected', color: 'error' };
    default:
      return { label: 'Unknown', color: 'default' };
  }
}
