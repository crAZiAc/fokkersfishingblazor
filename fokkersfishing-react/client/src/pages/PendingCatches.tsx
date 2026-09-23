import { useCallback, useEffect, useState } from 'react';
import { Box, Typography, Alert, LinearProgress } from '@mui/material';
import { useSnackbar } from 'notistack';
import { useTranslation } from 'react-i18next';
import { api } from '../api/client';
import { updateAdminCatch, deleteAdminCatch } from '../api/catchSave';
import type { Catch, Fish, User } from '../api/types';
import { CatchStatus } from '../api/types';
import { CatchTable } from '../components/CatchTable';
import { EditCatchDialog, type CatchSaveResult } from '../components/EditCatchDialog';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { useCompetition } from '../context/CompetitionContext';

export default function PendingCatches() {
  const { t } = useTranslation();
  const competition = useCompetition();
  const { enqueueSnackbar } = useSnackbar();
  const [catches, setCatches] = useState<Catch[] | null>(null);
  const [fishOptions, setFishOptions] = useState<Fish[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState<string | null>(null);

  const [current, setCurrent] = useState<Catch | null>(null);
  const [editOpen, setEditOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      const [fishRes, usersRes] = await Promise.all([
        api.get<Fish[]>('/fish'),
        api.get<User[]>('/adminuser/users'),
      ]);
      setFishOptions(fishRes.data);
      setUsers(usersRes.data);
      const url = competition.active
        ? `/catch/admin/pending/competition/${competition.competitionId}`
        : '/catch/admin/pending';
      const res = await api.get<Catch[]>(url);
      setCatches(res.data);
    } catch (e: any) {
      setError(e?.message ?? t('pending.loadFailed'));
    }
  }, [competition.active, competition.competitionId, t]);

  useEffect(() => {
    if (!competition.loading) void load();
  }, [competition.loading, load]);

  const handleSave = async (result: CatchSaveResult) => {
    setSaving(true);
    try {
      const updated = await updateAdminCatch(result);
      setCatches((prev) =>
        (prev ?? [])
          .map((c) => (c.id === updated.id ? { ...c, ...updated } : c))
          .filter((c) => c.status === CatchStatus.Pending),
      );
      enqueueSnackbar(t('pending.updated'), { variant: 'success' });
      setEditOpen(false);
    } catch (e: any) {
      enqueueSnackbar(e?.response?.data?.message ?? t('pending.updateFailed'), { variant: 'error' });
    } finally {
      setSaving(false);
    }
  };

  const confirmDelete = async () => {
    if (!deleteId) return;
    try {
      await deleteAdminCatch(deleteId);
      setCatches((prev) => (prev ?? []).filter((c) => c.id !== deleteId));
      enqueueSnackbar(t('myCatches.deleted'), { variant: 'info' });
    } catch {
      enqueueSnackbar(t('myCatches.deleteFailed'), { variant: 'error' });
    } finally {
      setDeleteId(null);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>{t('pending.title')}</Typography>
      {competition.active && <Typography color="text.secondary">{t('myCatches.onlyCompetition')}</Typography>}
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}

      {catches === null ? (
        <LinearProgress sx={{ mt: 3 }} />
      ) : (
        <CatchTable
          catches={catches}
          fishOptions={fishOptions}
          viewState="Pending"
          onShowEdit={(id) => {
            setCurrent(catches.find((c) => c.id === id) ?? null);
            setEditOpen(true);
          }}
          onShowDelete={(id) => setDeleteId(id)}
        />
      )}

      {current && editOpen && (
        <EditCatchDialog
          open={editOpen}
          currentCatch={current}
          fishOptions={fishOptions}
          users={users}
          editState="Admin"
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
