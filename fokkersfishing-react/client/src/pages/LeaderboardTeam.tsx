import { useCallback, useEffect, useState } from 'react';
import {
  Box, Typography, LinearProgress, Alert, Paper, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow,
} from '@mui/material';
import { useSnackbar } from 'notistack';
import { useTranslation } from 'react-i18next';
import { api } from '../api/client';
import { updateUserCatch, deleteTeamCatch } from '../api/catchSave';
import type { Catch, Fish, TeamScore, User } from '../api/types';
import { CatchTable } from '../components/CatchTable';
import { EditCatchDialog, type CatchSaveResult, type EditState } from '../components/EditCatchDialog';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { useCompetition } from '../context/CompetitionContext';

export default function LeaderboardTeam() {
  const { t } = useTranslation();
  const competition = useCompetition();
  const { enqueueSnackbar } = useSnackbar();
  const [catches, setCatches] = useState<Catch[] | null>(null);
  const [scores, setScores] = useState<TeamScore[]>([]);
  const [fishOptions, setFishOptions] = useState<Fish[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState<string | null>(null);

  const [current, setCurrent] = useState<Catch | null>(null);
  const [editState, setEditState] = useState<EditState>('User');
  const [editOpen, setEditOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      const [c, s, fish, u] = await Promise.all([
        api.get<Catch[]>(`/leaderboard/team/${competition.competitionId}`),
        api.get<TeamScore[]>(`/leaderboard/team/scores/${competition.competitionId}`),
        api.get<Fish[]>('/fish'),
        api.get<User[]>('/adminuser/users'),
      ]);
      setCatches(c.data);
      setScores(s.data ?? []);
      setFishOptions(fish.data);
      setUsers(u.data);
    } catch (e: any) {
      setError(e?.message ?? t('leaderboardTeam.loadFailed'));
    }
  }, [competition.competitionId, t]);

  useEffect(() => {
    if (!competition.loading && competition.active) void load();
  }, [competition.loading, competition.active, load]);

  const handleSave = async (result: CatchSaveResult) => {
    setSaving(true);
    try {
      await updateUserCatch(result);
      enqueueSnackbar(t('myCatches.saved'), { variant: 'success' });
      setEditOpen(false);
      void load();
    } catch (e: any) {
      enqueueSnackbar(e?.response?.data?.message ?? t('myCatches.saveFailed'), { variant: 'error' });
    } finally {
      setSaving(false);
    }
  };

  const confirmDelete = async () => {
    if (!deleteId) return;
    try {
      await deleteTeamCatch(deleteId);
      setCatches((prev) => (prev ?? []).filter((c) => c.id !== deleteId));
      enqueueSnackbar(t('myCatches.deleted'), { variant: 'info' });
    } catch {
      enqueueSnackbar(t('myCatches.deleteFailed'), { variant: 'error' });
    } finally {
      setDeleteId(null);
    }
  };

  if (!competition.active) {
    return <Alert severity="info">{t('leaderboardTeam.noActive')}</Alert>;
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>{t('leaderboardTeam.title')}</Typography>
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}

      {catches === null ? (
        <LinearProgress />
      ) : (
        <CatchTable
          catches={catches}
          fishOptions={fishOptions}
          viewState="Team"
          onShowEdit={(id) => {
            setCurrent(catches.find((c) => c.id === id) ?? null);
            setEditState('User');
            setEditOpen(true);
          }}
          onShowEditFull={(id) => {
            setCurrent(catches.find((c) => c.id === id) ?? null);
            setEditState('UserFull');
            setEditOpen(true);
          }}
          onShowDelete={(id) => setDeleteId(id)}
        />
      )}

      <Typography variant="h5" sx={{ mt: 4 }} gutterBottom>{t('leaderboardTeam.fishScores')}</Typography>
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>{t('catches.fish')}</TableCell>
              <TableCell align="right">{t('leaderboard.totalCm')}</TableCell>
              <TableCell align="right">{t('home.totalFishCaught')}</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {scores.map((s) => (
              <TableRow key={s.fish} hover>
                <TableCell>{s.fish === '_Total' ? t('leaderboardTeam.total') : s.fish}</TableCell>
                <TableCell align="right">{s.totalLength}</TableCell>
                <TableCell align="right">{s.fishCount}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {current && editOpen && (
        <EditCatchDialog
          open={editOpen}
          currentCatch={current}
          fishOptions={fishOptions}
          users={users}
          editState={editState}
          saving={saving}
          onCancel={() => setEditOpen(false)}
          onSave={handleSave}
        />
      )}

      <ConfirmDialog
        open={!!deleteId}
        title={t('myCatches.deleteTitle')}
        message={t('myCatches.deleteMsg')}
        onConfirm={confirmDelete}
        onCancel={() => setDeleteId(null)}
      />
    </Box>
  );
}
