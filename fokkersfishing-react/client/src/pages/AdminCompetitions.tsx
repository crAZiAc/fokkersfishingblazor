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
import { api } from '../api/client';
import type { Competition } from '../api/types';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { formatDateTime } from '../utils/format';

export default function AdminCompetitions() {
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
      setError(e?.message ?? 'Failed to load competitions.');
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
      enqueueSnackbar('Competition saved', { variant: 'success' });
      setEdit(null);
      void load();
    } catch {
      enqueueSnackbar('Save failed', { variant: 'error' });
    }
  };

  const confirmDelete = async () => {
    if (!deleteId) return;
    try {
      await api.delete(`/competition/${deleteId}`);
      setItems((prev) => (prev ?? []).filter((c) => c.id !== deleteId));
      enqueueSnackbar('Competition deleted', { variant: 'info' });
    } catch {
      enqueueSnackbar('Delete failed', { variant: 'error' });
    } finally {
      setDeleteId(null);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Competitions</Typography>
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}
      <Button variant="contained" startIcon={<AddIcon />} onClick={openAdd} sx={{ mb: 2 }}>
        Add new competition
      </Button>

      {items === null ? (
        <LinearProgress />
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Start</TableCell>
                <TableCell>End</TableCell>
                <TableCell align="center">Active</TableCell>
                <TableCell align="center">Show leaderboard after end</TableCell>
                <TableCell align="right">Actions</TableCell>
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
                    <Tooltip title="Edit">
                      <IconButton size="small" onClick={() => setEdit({ competition: c, isNew: false })}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Delete">
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
        title="Delete competition"
        message="Do you want to delete this competition?"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteId(null)}
      />
    </Box>
  );
}

function EditCompetitionDialog({
  competition, isNew, onCancel, onSave,
}: { competition: Competition; isNew: boolean; onCancel: () => void; onSave: (c: Competition) => void }) {
  const [name, setName] = useState(competition.competitionName);
  const [active, setActive] = useState(competition.active);
  const [showLb, setShowLb] = useState(competition.showLeaderboardAfterCompetitionEnds);
  const [start, setStart] = useState<Dayjs | null>(dayjs(competition.startDate));
  const [end, setEnd] = useState<Dayjs | null>(dayjs(competition.endDate));

  return (
    <Dialog open onClose={onCancel} fullWidth maxWidth="sm">
      <DialogTitle>{isNew ? 'New competition' : 'Edit competition'}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField label="Competition name" value={name} onChange={(e) => setName(e.target.value)} fullWidth />
          <DateTimePicker label="Start date" value={start} onChange={setStart} ampm={false} format="DD-MM-YYYY HH:mm" />
          <DateTimePicker label="End date" value={end} onChange={setEnd} ampm={false} format="DD-MM-YYYY HH:mm" />
          <FormControlLabel control={<Checkbox checked={active} onChange={(e) => setActive(e.target.checked)} />} label="Active" />
          <FormControlLabel
            control={<Checkbox checked={showLb} onChange={(e) => setShowLb(e.target.checked)} />}
            label="Show leaderboard after competition has ended"
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>Cancel</Button>
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
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}
