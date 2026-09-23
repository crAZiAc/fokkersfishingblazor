import { useCallback, useEffect, useState } from 'react';
import { Box, Button, Typography, Alert, LinearProgress } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { useSnackbar } from 'notistack';
import { useTranslation } from 'react-i18next';
import { api } from '../api/client';
import { createCatch, updateUserCatch, deleteUserCatch } from '../api/catchSave';
import type { Catch, Fish, User } from '../api/types';
import { CatchStatus } from '../api/types';
import { CatchTable } from '../components/CatchTable';
import { EditCatchDialog, type CatchSaveResult, type EditState } from '../components/EditCatchDialog';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { useCompetition } from '../context/CompetitionContext';

function newCatch(competitionId: string): Catch {
  return {
    id: crypto.randomUUID(),
    competitionId,
    catchNumber: 0,
    userEmail: null,
    userName: null,
    registerUserEmail: null,
    registerUserName: null,
    fish: 'Pike',
    length: 0,
    teamName: null,
    catchDate: new Date().toISOString(),
    logDate: new Date().toISOString(),
    editDate: new Date().toISOString(),
    globalCatchNumber: 0,
    measurePhotoUrl: null,
    catchPhotoUrl: null,
    measureThumbnailUrl: null,
    catchThumbnailUrl: null,
    status: CatchStatus.Pending,
  };
}

const EMPTY_ID = '00000000-0000-0000-0000-000000000000';

export default function MyCatches() {
  const { t } = useTranslation();
  const competition = useCompetition();
  const { enqueueSnackbar } = useSnackbar();
  const [catches, setCatches] = useState<Catch[] | null>(null);
  const [fishOptions, setFishOptions] = useState<Fish[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState<string | null>(null);

  const [editOpen, setEditOpen] = useState(false);
  const [editState, setEditState] = useState<EditState>('User');
  const [isNew, setIsNew] = useState(false);
  const [current, setCurrent] = useState<Catch | null>(null);
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
      const url = competition.active ? `/catch/competition/${competition.competitionId}` : '/catch';
      const catchesRes = await api.get<Catch[]>(url);
      setCatches(catchesRes.data);
    } catch (e: any) {
      setError(e?.message ?? t('myCatches.loadFailed'));
    }
  }, [competition.active, competition.competitionId, t]);

  useEffect(() => {
    if (!competition.loading) void load();
  }, [competition.loading, load]);

  const openAdd = () => {
    setCurrent(newCatch(competition.active ? competition.competitionId : EMPTY_ID));
    setIsNew(true);
    setEditState('UserFull');
    setEditOpen(true);
  };

  const openEdit = (id: string, full: boolean) => {
    const item = catches?.find((c) => c.id === id) ?? null;
    setCurrent(item);
    setIsNew(false);
    setEditState(full ? 'UserFull' : 'User');
    setEditOpen(true);
  };

  const handleSave = async (result: CatchSaveResult) => {
    setSaving(true);
    try {
      if (isNew) {
        const created = await createCatch(result);
        if (created.userEmail === created.registerUserEmail) {
          setCatches((prev) => [created, ...(prev ?? [])]);
        }
      } else {
        const updated = await updateUserCatch(result);
        setCatches((prev) => (prev ?? []).map((c) => (c.id === updated.id ? updated : c)));
      }
      enqueueSnackbar(t('myCatches.saved'), { variant: 'success' });
      setEditOpen(false);
    } catch (e: any) {
      enqueueSnackbar(e?.response?.data?.message ?? t('myCatches.saveFailed'), { variant: 'error' });
    } finally {
      setSaving(false);
    }
  };

  const confirmDelete = async () => {
    if (!deleteId) return;
    try {
      await deleteUserCatch(deleteId);
      setCatches((prev) => (prev ?? []).filter((c) => c.id !== deleteId));
      enqueueSnackbar(t('myCatches.deleted'), { variant: 'info' });
    } catch {
      enqueueSnackbar(t('myCatches.deleteFailed'), { variant: 'error' });
    } finally {
      setDeleteId(null);
    }
  };

  const canAdd = !competition.active || (!competition.competitionEnded && !competition.competitionNotStarted);

  return (
    <Box>
      <Typography variant="h4" gutterBottom>{t('myCatches.title')}</Typography>
      {competition.active && <Typography color="text.secondary">{t('myCatches.onlyCompetition')}</Typography>}
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}

      {canAdd && (
        <Button variant="contained" startIcon={<AddIcon />} onClick={openAdd} sx={{ mt: 2 }}>
          {t('home.addNewCatch')}
        </Button>
      )}

      {catches === null ? (
        <LinearProgress sx={{ mt: 3 }} />
      ) : (
        <CatchTable
          catches={catches}
          fishOptions={fishOptions}
          viewState="Personal"
          onShowEdit={(id) => openEdit(id, false)}
          onShowEditFull={(id) => openEdit(id, true)}
          onShowDelete={(id) => setDeleteId(id)}
        />
      )}

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
