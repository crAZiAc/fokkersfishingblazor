import { useEffect, useState } from 'react';
import {
  Box, Typography, Alert, LinearProgress, Paper, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, IconButton, Tooltip, Button, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, FormControlLabel, Checkbox, Stack,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';
import CheckIcon from '@mui/icons-material/Check';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import dayjs, { Dayjs } from 'dayjs';
import { useSnackbar } from 'notistack';
import { useTranslation } from 'react-i18next';
import { api } from '../api/client';
import type { Competition } from '../api/types';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { formatDateTime } from '../utils/format';

export default function AdminCompetitions() {
  const { t } = useTranslation();
  const { enqueueSnackbar } = useSnackbar();
  const [items, setItems] = useState<Competition[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [edit, setEdit] = useState<{ competition: Competition; isNew: boolean } | null>(null);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const load = async () => {
    try {
      const { data } = await api.get<Competition[]>('/competition');
      setItems(data);
    } catch (e: any) {
      setError(e?.message ?? t('adminCompetitions.loadFailed'));
    }
  };

  useEffect(() => {
    void load();
  }, []);

  const openAdd = () =>
    setEdit({
      isNew: true,
      competition: {
        id: crypto.randomUUID(),
        competitionName: '',
        active: false,
        showLeaderboardAfterCompetitionEnds: false,
        startDate: dayjs().toISOString(),
        endDate: dayjs().add(8, 'hour').toISOString(),
      },
    });

  const save = async (competition: Competition, isNew: boolean) => {
    try {
      if (isNew) await api.post('/competition', competition);
      else await api.put(`/competition/${competition.id}`, competition);
      enqueueSnackbar(t('adminCompetitions.saved'), { variant: 'success' });
      setEdit(null);
      void load();
    } catch {
      enqueueSnackbar(t('adminCompetitions.saveFailed'), { variant: 'error' });
    }
  };

  const confirmDelete = async () => {
    if (!deleteId) return;
    try {
      await api.delete(`/competition/${deleteId}`);
      setItems((prev) => (prev ?? []).filter((c) => c.id !== deleteId));
      enqueueSnackbar(t('adminCompetitions.deleted'), { variant: 'info' });
    } catch {
      enqueueSnackbar(t('adminCompetitions.deleteFailed'), { variant: 'error' });
    } finally {
      setDeleteId(null);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>{t('adminCompetitions.title')}</Typography>
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}
      <Button variant="contained" startIcon={<AddIcon />} onClick={openAdd} sx={{ mb: 2 }}>
        {t('adminCompetitions.addNew')}
      </Button>

      {items === null ? (
        <LinearProgress />
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>{t('adminCompetitions.name')}</TableCell>
                <TableCell>{t('adminCompetitions.start')}</TableCell>
                <TableCell>{t('adminCompetitions.end')}</TableCell>
                <TableCell align="center">{t('adminCompetitions.active')}</TableCell>
                <TableCell align="center">{t('adminCompetitions.showLbAfterEnd')}</TableCell>
                <TableCell align="right">{t('common.actions')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {items.map((c) => (
                <TableRow key={c.id} hover>
                  <TableCell>{c.competitionName}</TableCell>
                  <TableCell>{formatDateTime(c.startDate)}</TableCell>
                  <TableCell>{formatDateTime(c.endDate)}</TableCell>
                  <TableCell align="center">{c.active ? <CheckIcon color="success" fontSize="small" /> : ''}</TableCell>
                  <TableCell align="center">{c.showLeaderboardAfterCompetitionEnds ? <CheckIcon color="success" fontSize="small" /> : ''}</TableCell>
                  <TableCell align="right">
                    <Tooltip title={t('common.edit')}>
                      <IconButton size="small" onClick={() => setEdit({ competition: c, isNew: false })}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title={t('common.delete')}>
                      <IconButton size="small" color="error" onClick={() => setDeleteId(c.id)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {edit && (
        <EditCompetitionDialog
          competition={edit.competition}
          isNew={edit.isNew}
          onCancel={() => setEdit(null)}
          onSave={(c) => save(c, edit.isNew)}
        />
      )}

      <ConfirmDialog
        open={!!deleteId}
        title={t('adminCompetitions.deleteTitle')}
        message={t('adminCompetitions.deleteMsg')}
        onConfirm={confirmDelete}
        onCancel={() => setDeleteId(null)}
      />
    </Box>
  );
}

function EditCompetitionDialog({
  competition, isNew, onCancel, onSave,
}: { competition: Competition; isNew: boolean; onCancel: () => void; onSave: (c: Competition) => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState(competition.competitionName);
  const [active, setActive] = useState(competition.active);
  const [showLb, setShowLb] = useState(competition.showLeaderboardAfterCompetitionEnds);
  const [start, setStart] = useState<Dayjs | null>(dayjs(competition.startDate));
  const [end, setEnd] = useState<Dayjs | null>(dayjs(competition.endDate));

  return (
    <Dialog open onClose={onCancel} fullWidth maxWidth="sm">
      <DialogTitle>{isNew ? t('adminCompetitions.newComp') : t('adminCompetitions.editComp')}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField label={t('adminCompetitions.competitionName')} value={name} onChange={(e) => setName(e.target.value)} fullWidth />
          <DateTimePicker label={t('adminCompetitions.startDate')} value={start} onChange={setStart} ampm={false} format="L HH:mm" />
          <DateTimePicker label={t('adminCompetitions.endDate')} value={end} onChange={setEnd} ampm={false} format="L HH:mm" />
          <FormControlLabel control={<Checkbox checked={active} onChange={(e) => setActive(e.target.checked)} />} label={t('adminCompetitions.active')} />
          <FormControlLabel
            control={<Checkbox checked={showLb} onChange={(e) => setShowLb(e.target.checked)} />}
            label={t('adminCompetitions.showLbAfterEndFull')}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>{t('common.cancel')}</Button>
        <Button
          variant="contained"
          onClick={() =>
            onSave({
              ...competition,
              competitionName: name,
              active,
              showLeaderboardAfterCompetitionEnds: showLb,
              startDate: (start ?? dayjs()).toISOString(),
              endDate: (end ?? dayjs()).toISOString(),
            })
          }
        >
          {t('common.save')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
