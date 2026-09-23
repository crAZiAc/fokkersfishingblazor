import { Dialog, DialogTitle, DialogContent, DialogActions, Button, Box, Link } from '@mui/material';
import { useTranslation } from 'react-i18next';
import type { Catch } from '../api/types';
import { formatDateTime } from '../utils/format';

interface Props {
  open: boolean;
  url: string | null | undefined;
  catchMade: Catch | null;
  onClose: () => void;
}

export function PhotoViewer({ open, url, catchMade, onClose }: Props) {
  const { t } = useTranslation();
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm">
      <DialogTitle>
        {catchMade
          ? `#${catchMade.globalCatchNumber} · ${catchMade.fish} (${catchMade.length} cm) · ${formatDateTime(catchMade.catchDate)}`
          : t('photo.title')}
      </DialogTitle>
      <DialogContent>
        {url ? (
          <Box sx={{ display: 'flex', justifyContent: 'center' }}>
            <img src={url} alt="catch" style={{ maxWidth: '100%', maxHeight: 500 }} />
          </Box>
        ) : (
          <Box sx={{ p: 4, textAlign: 'center' }}>{t('photo.none')}</Box>
        )}
      </DialogContent>
      <DialogActions>
        {url && (
          <Link href={url} target="_blank" rel="noopener" sx={{ mr: 'auto', ml: 1 }}>
            {t('common.download')}
          </Link>
        )}
        <Button onClick={onClose}>{t('common.close')}</Button>
      </DialogActions>
    </Dialog>
  );
}
