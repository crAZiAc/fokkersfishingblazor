import { CatchStatus } from '../api/types';
import { currentLocale } from '../i18n';

export function formatDateTime(value: string | Date | null | undefined): string {
  if (!value) return '';
  const d = typeof value === 'string' ? new Date(value) : value;
  if (Number.isNaN(d.getTime())) return '';
  return d.toLocaleString(currentLocale());
}

export function formatDate(value: string | Date | null | undefined): string {
  if (!value) return '';
  const d = typeof value === 'string' ? new Date(value) : value;
  if (Number.isNaN(d.getTime())) return '';
  return d.toLocaleDateString(currentLocale());
}

export interface StatusMeta {
  /** i18n key, e.g. "catches.status.approved" — pass through t(). */
  labelKey: string;
  color: 'success' | 'warning' | 'error' | 'default';
}

export function statusMeta(status: CatchStatus): StatusMeta {
  switch (status) {
    case CatchStatus.Approved:
      return { labelKey: 'catches.status.approved', color: 'success' };
    case CatchStatus.Pending:
      return { labelKey: 'catches.status.pending', color: 'warning' };
    case CatchStatus.Rejected:
      return { labelKey: 'catches.status.rejected', color: 'error' };
    default:
      return { labelKey: 'catches.status.unknown', color: 'default' };
  }
}
