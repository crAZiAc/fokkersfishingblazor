import { useEffect, useState } from 'react';
import {
  Box, Typography, Alert, LinearProgress, Paper, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, IconButton, Tooltip, Button, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Stack, MenuItem, List, ListItem, ListItemText,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';
import PersonAddIcon from '@mui/icons-material/PersonAdd';
import { useSnackbar } from 'notistack';
import { useTranslation } from 'react-i18next';
import { api } from '../api/client';
import type { Team, User } from '../api/types';
import { ConfirmDialog } from '../components/ConfirmDialog';

export default function AdminTeams() {
  const { t } = useTranslation();
  const { enqueueSnackbar } = useSnackbar();
  const [teams, setTeams] = useState<Team[] | null>(null);
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [edit, setEdit] = useState<{ team: Team; isNew: boolean } | null>(null);
  const [addMemberTeam, setAddMemberTeam] = useState<Team | null>(null);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const load = async () => {
    try {
      const [teamsRes, usersRes] = await Promise.all([
        api.get<Team[]>('/team'),
        api.get<User[]>('/adminuser/users'),
      ]);
      setTeams(teamsRes.data);
      setUsers(usersRes.data);
    } catch (e: any) {
      setError(e?.message ?? t('adminTeams.loadFailed'));
    }
  };

  useEffect(() => {
    void load();
  }, []);

  const persistTeam = async (team: Team, isNew: boolean) => {
    if (isNew) {
      await api.post('/team', team);
    } else {
      await api.put(`/team/${team.id}`, team);
    }
  };

  const saveTeam = async (team: Team, isNew: boolean) => {
    try {
      await persistTeam(team, isNew);
      enqueueSnackbar(t('adminTeams.saved'), { variant: 'success' });
      setEdit(null);
      void load();
    } catch {
      enqueueSnackbar(t('adminTeams.saveFailed'), { variant: 'error' });
    }
  };

  const addMember = async (team: Team, user: User) => {
    try {
      const updated: Team = { ...team, users: [...team.users, user] };
      await api.put(`/team/${team.id}`, updated);
      enqueueSnackbar(t('adminTeams.memberAdded'), { variant: 'success' });
      setAddMemberTeam(null);
      void load();
    } catch {
      enqueueSnackbar(t('adminTeams.addMemberFailed'), { variant: 'error' });
    }
  };

  const removeMember = async (team: Team, email: string) => {
    try {
      const updated: Team = { ...team, users: team.users.filter((u) => u.email !== email) };
      await api.put(`/team/${team.id}`, updated);
      enqueueSnackbar(t('adminTeams.memberRemoved'), { variant: 'info' });
      void load();
    } catch {
      enqueueSnackbar(t('adminTeams.removeMemberFailed'), { variant: 'error' });
    }
  };

  const confirmDelete = async () => {
    if (!deleteId) return;
    try {
      await api.delete(`/team/${deleteId}`);
      setTeams((prev) => (prev ?? []).filter((tm) => tm.id !== deleteId));
      enqueueSnackbar(t('adminTeams.deleted'), { variant: 'info' });
    } catch {
      enqueueSnackbar(t('adminTeams.deleteFailed'), { variant: 'error' });
    } finally {
      setDeleteId(null);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>{t('adminTeams.title')}</Typography>
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}
      <Button
        variant="contained"
        startIcon={<AddIcon />}
        sx={{ mb: 2 }}
        onClick={() => setEdit({ isNew: true, team: { id: crypto.randomUUID(), name: '', description: '', users: [] } })}
      >
        {t('adminTeams.addNew')}
      </Button>

      {teams === null ? (
        <LinearProgress />
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>{t('adminTeams.teamName')}</TableCell>
                <TableCell>{t('adminTeams.description')}</TableCell>
                <TableCell>{t('adminTeams.members')}</TableCell>
                <TableCell align="right">{t('common.actions')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {teams.map((tm) => (
                <TableRow key={tm.id} hover>
                  <TableCell>{tm.name}</TableCell>
                  <TableCell>{tm.description}</TableCell>
                  <TableCell>
                    <List dense disablePadding>
                      {tm.users.map((u) => (
                        <ListItem
                          key={u.email}
                          disableGutters
                          secondaryAction={
                            <IconButton size="small" color="error" onClick={() => removeMember(tm, u.email)}>
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          }
                        >
                          <ListItemText primary={u.userName} secondary={u.email} />
                        </ListItem>
                      ))}
                      {tm.users.length === 0 && <Typography variant="body2" color="text.secondary">{t('adminTeams.noMembers')}</Typography>}
                    </List>
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title={t('adminTeams.addMember')}>
                      <IconButton size="small" onClick={() => setAddMemberTeam(tm)}>
                        <PersonAddIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title={t('common.edit')}>
                      <IconButton size="small" onClick={() => setEdit({ team: tm, isNew: false })}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title={t('common.delete')}>
                      <IconButton size="small" color="error" onClick={() => setDeleteId(tm.id)}>
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
        <EditTeamDialog
          team={edit.team}
          isNew={edit.isNew}
          onCancel={() => setEdit(null)}
          onSave={(t) => saveTeam(t, edit.isNew)}
        />
      )}

      {addMemberTeam && (
        <AddMemberDialog
          team={addMemberTeam}
          users={users}
          onCancel={() => setAddMemberTeam(null)}
          onAdd={(u) => addMember(addMemberTeam, u)}
        />
      )}

      <ConfirmDialog
        open={!!deleteId}
        title={t('adminTeams.deleteTitle')}
        message={t('adminTeams.deleteMsg')}
        onConfirm={confirmDelete}
        onCancel={() => setDeleteId(null)}
      />
    </Box>
  );
}

function EditTeamDialog({
  team, isNew, onCancel, onSave,
}: { team: Team; isNew: boolean; onCancel: () => void; onSave: (team: Team) => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState(team.name);
  const [description, setDescription] = useState(team.description);
  return (
    <Dialog open onClose={onCancel} fullWidth maxWidth="xs">
      <DialogTitle>{isNew ? t('adminTeams.newTeam') : t('adminTeams.editTeam')}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField label={t('adminTeams.teamName')} value={name} onChange={(e) => setName(e.target.value)} fullWidth />
          <TextField label={t('adminTeams.description')} value={description} onChange={(e) => setDescription(e.target.value)} fullWidth multiline />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>{t('common.cancel')}</Button>
        <Button variant="contained" onClick={() => onSave({ ...team, name, description })}>{t('common.save')}</Button>
      </DialogActions>
    </Dialog>
  );
}

function AddMemberDialog({
  team, users, onCancel, onAdd,
}: { team: Team; users: User[]; onCancel: () => void; onAdd: (u: User) => void }) {
  const { t } = useTranslation();
  const available = users.filter((u) => !team.users.some((tu) => tu.email === u.email));
  const [email, setEmail] = useState('');
  return (
    <Dialog open onClose={onCancel} fullWidth maxWidth="xs">
      <DialogTitle>{t('adminTeams.addMemberTitle', { team: team.name })}</DialogTitle>
      <DialogContent>
        <TextField select label={t('adminTeams.user')} value={email} onChange={(e) => setEmail(e.target.value)} fullWidth sx={{ mt: 1 }}>
          {available.map((u) => (
            <MenuItem key={u.email} value={u.email}>{u.userName} ({u.email})</MenuItem>
          ))}
        </TextField>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>{t('common.cancel')}</Button>
        <Button
          variant="contained"
          disabled={!email}
          onClick={() => {
            const u = available.find((x) => x.email === email);
            if (u) onAdd(u);
          }}
        >
          {t('common.add')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
