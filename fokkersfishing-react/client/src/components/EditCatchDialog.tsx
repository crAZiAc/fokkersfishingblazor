import { useEffect, useState } from 'react';
import {
  Dialog, DialogTitle, DialogContent, DialogActions, Button, Stack, TextField, MenuItem,
  FormControlLabel, Checkbox, Chip, Box, Typography, Divider, CircularProgress,
} from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import dayjs, { Dayjs } from 'dayjs';
import { useTranslation } from 'react-i18next';
import type { Catch, Fish, User } from '../api/types';
import { CatchStatus } from '../api/types';

export type EditState = 'User' | 'UserFull' | 'Admin' | 'AdminAdd' | 'AdminEditFull';

export interface CatchSaveResult {
  updated: Catch;
  measureFile: File | null;
  catchFile: File | null;
  clearPhotos: boolean;
}

interface Props {
  open: boolean;
  currentCatch: Catch;
  fishOptions: Fish[];
  users: User[];
  editState: EditState;
  saving?: boolean;
  onCancel: () => void;
  onSave: (result: CatchSaveResult) => void;
}

const STATUSES: { value: CatchStatus; labelKey: string; color: 'success' | 'warning' | 'error' }[] = [
  { value: CatchStatus.Approved, labelKey: 'catches.status.approved', color: 'success' },
  { value: CatchStatus.Pending, labelKey: 'catches.status.pending', color: 'warning' },
  { value: CatchStatus.Rejected, labelKey: 'catches.status.rejected', color: 'error' },
];

export function EditCatchDialog({ open, currentCatch, fishOptions, users, editState, saving, onCancel, onSave }: Props) {
  const { t } = useTranslation();
  const [fish, setFish] = useState(currentCatch.fish);
  const [length, setLength] = useState<number>(currentCatch.length);
  const [userEmail, setUserEmail] = useState<string>(currentCatch.userEmail ?? '');
  const [catchDate, setCatchDate] = useState<Dayjs | null>(dayjs(currentCatch.catchDate));
  const [status, setStatus] = useState<CatchStatus>(currentCatch.status);
  const [measureFile, setMeasureFile] = useState<File | null>(null);
  const [catchFile, setCatchFile] = useState<File | null>(null);
  const [clearPhotos, setClearPhotos] = useState(false);

  useEffect(() => {
    setFish(currentCatch.fish);
    setLength(currentCatch.length);
    setUserEmail(currentCatch.userEmail ?? '');
    setCatchDate(dayjs(currentCatch.catchDate));
    setStatus(currentCatch.status);
    setMeasureFile(null);
    setCatchFile(null);
    setClearPhotos(false);
  }, [currentCatch]);

  const showPhotos = editState === 'UserFull' || editState === 'AdminAdd' || editState === 'AdminEditFull';
  const showStatus = editState === 'Admin' || editState === 'AdminEditFull';

  const handleSave = () => {
    const updated: Catch = {
      ...currentCatch,
      fish,
      length: Number(length) || 0,
      userEmail: userEmail || currentCatch.userEmail,
      catchDate: (catchDate ?? dayjs()).toISOString(),
      status,
    };
    onSave({ updated, measureFile, catchFile, clearPhotos });
  };

  return (
    <Dialog open={open} onClose={onCancel} fullWidth maxWidth="sm">
      <DialogTitle>{t('editCatch.title', { n: currentCatch.globalCatchNumber || t('editCatch.titleNew') })}</DialogTitle>
      <DialogContent dividers>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <Stack direction="row" spacing={2}>
            <TextField label={t('editCatch.catchNo')} value={currentCatch.catchNumber} disabled fullWidth />
            <TextField
              label={t('editCatch.logDate')}
              value={currentCatch.logDate ? new Date(currentCatch.logDate).toLocaleString() : ''}
              disabled
              fullWidth
            />
          </Stack>

          <DateTimePicker
            label={t('editCatch.catchDateTime')}
            value={catchDate}
            onChange={(v) => setCatchDate(v)}
            ampm={false}
            format="L HH:mm"
          />

          <TextField
            label={t('editCatch.fishLength')}
            type="number"
            value={length}
            onChange={(e) => setLength(parseFloat(e.target.value))}
            fullWidth
          />

          <TextField select label={t('editCatch.fishType')} value={fish} onChange={(e) => setFish(e.target.value)} fullWidth>
            {fishOptions.map((f) => (
              <MenuItem key={f.id} value={f.name}>{f.name}</MenuItem>
            ))}
          </TextField>

          <TextField select label={t('editCatch.fisherman')} value={userEmail} onChange={(e) => setUserEmail(e.target.value)} fullWidth>
            <MenuItem value="">{t('editCatch.registerUser')}</MenuItem>
            {users.map((u) => (
              <MenuItem key={u.email} value={u.email}>{u.userName}</MenuItem>
            ))}
          </TextField>

          {showPhotos && (
            <>
              <Divider />
              <Typography variant="subtitle2">{t('editCatch.photos')}</Typography>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <Button component="label" variant="outlined" startIcon={measureFile ? <CheckCircleIcon color="success" /> : <UploadFileIcon />}>
                  {measureFile ? measureFile.name : t('editCatch.measurePhoto')}
                  <input hidden type="file" accept="image/*" onChange={(e) => setMeasureFile(e.target.files?.[0] ?? null)} />
                </Button>
                <Button component="label" variant="outlined" startIcon={catchFile ? <CheckCircleIcon color="success" /> : <UploadFileIcon />}>
                  {catchFile ? catchFile.name : t('editCatch.catchPhoto')}
                  <input hidden type="file" accept="image/*" onChange={(e) => setCatchFile(e.target.files?.[0] ?? null)} />
                </Button>
              </Stack>
              <FormControlLabel
                control={<Checkbox checked={clearPhotos} onChange={(e) => setClearPhotos(e.target.checked)} />}
                label={t('editCatch.clearPhotos')}
              />
            </>
          )}

          {showStatus && (
            <>
              <Divider />
              <Typography variant="subtitle2">{t('editCatch.status')}</Typography>
              <Box sx={{ display: 'flex', gap: 1 }}>
                {STATUSES.map((s) => (
                  <Chip
                    key={s.value}
                    label={t(s.labelKey)}
                    color={status === s.value ? s.color : 'default'}
                    variant={status === s.value ? 'filled' : 'outlined'}
                    onClick={() => setStatus(s.value)}
                  />
                ))}
              </Box>
            </>
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>{t('common.cancel')}</Button>
        <Button variant="contained" onClick={handleSave} disabled={saving} startIcon={saving ? <CircularProgress size={18} /> : undefined}>
          {t('common.save')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
