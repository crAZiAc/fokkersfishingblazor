import { Dialog, DialogTitle, DialogContent, DialogActions, Button, Box, Link } from '@mui/material';
import type { Catch } from '../api/types';
import { formatDateTime } from '../utils/format';

interface Props {
  open: boolean;
  url: string | null | undefined;
  catchMade: Catch | null;
  onClose: () => void;
}

export function PhotoViewer({ open, url, catchMade, onClose }: Props) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm">
      <DialogTitle>
        {catchMade
          ? `#${catchMade.globalCatchNumber} · ${catchMade.fish} (${catchMade.length} cm) · ${formatDateTime(catchMade.catchDate)}`
          : 'Photo'}
      </DialogTitle>
      <DialogContent>
        {url ? (
          <Box sx={{ display: 'flex', justifyContent: 'center' }}>
            <img src={url} alt="catch" style={{ maxWidth: '100%', maxHeight: 500 }} />
          </Box>
        ) : (
          <Box sx={{ p: 4, textAlign: 'center' }}>No photo available.</Box>
        )}
      </DialogContent>
      <DialogActions>
        {url && (
          <Link href={url} target="_blank" rel="noopener" sx={{ mr: 'auto', ml: 1 }}>
            Download
          </Link>
        )}
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}
