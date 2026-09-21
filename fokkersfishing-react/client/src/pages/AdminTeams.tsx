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
import { api } from '../api/client';
import type { Team, User } from '../api/types';
import { ConfirmDialog } from '../components/ConfirmDialog';

export default function AdminTeams() {
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
      setError(e?.message ?? 'Failed to load teams.');
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
      enqueueSnackbar('Team saved', { variant: 'success' });
      setEdit(null);
      void load();
    } catch {
      enqueueSnackbar('Save failed', { variant: 'error' });
    }
  };

  const addMember = async (team: Team, user: User) => {
    try {
      const updated: Team = { ...team, users: [...team.users, user] };
      await api.put(`/team/${team.id}`, updated);
      enqueueSnackbar('Member added', { variant: 'success' });
      setAddMemberTeam(null);
      void load();
    } catch {
      enqueueSnackbar('Add member failed', { variant: 'error' });
    }
  };

  const removeMember = async (team: Team, email: string) => {
    try {
      const updated: Team = { ...team, users: team.users.filter((u) => u.email !== email) };
      await api.put(`/team/${team.id}`, updated);
      enqueueSnackbar('Member removed', { variant: 'info' });
      void load();
    } catch {
      enqueueSnackbar('Remove member failed', { variant: 'error' });
    }
  };

  const confirmDelete = async () => {
    if (!deleteId) return;
    try {
      await api.delete(`/team/${deleteId}`);
      setTeams((prev) => (prev ?? []).filter((t) => t.id !== deleteId));
      enqueueSnackbar('Team deleted', { variant: 'info' });
    } catch {
      enqueueSnackbar('Delete failed', { variant: 'error' });
    } finally {
      setDeleteId(null);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Teams</Typography>
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}
      <Button
        variant="contained"
        startIcon={<AddIcon />}
        sx={{ mb: 2 }}
        onClick={() => setEdit({ isNew: true, team: { id: crypto.randomUUID(), name: '', description: '', users: [] } })}
      >
        Add new team
      </Button>

      {teams === null ? (
        <LinearProgress />
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Team Name</TableCell>
                <TableCell>Description</TableCell>
                <TableCell>Members</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {teams.map((t) => (
                <TableRow key={t.id} hover>
                  <TableCell>{t.name}</TableCell>
                  <TableCell>{t.description}</TableCell>
                  <TableCell>
                    <List dense disablePadding>
                      {t.users.map((u) => (
                        <ListItem
                          key={u.email}
                          disableGutters
                          secondaryAction={
                            <IconButton size="small" color="error" onClick={() => removeMember(t, u.email)}>
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          }
                        >
                          <ListItemText primary={u.userName} secondary={u.email} />
                        </ListItem>
                      ))}
                      {t.users.length === 0 && <Typography variant="body2" color="text.secondary">No members</Typography>}
                    </List>
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="Add member">
                      <IconButton size="small" onClick={() => setAddMemberTeam(t)}>
                        <PersonAddIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Edit">
                      <IconButton size="small" onClick={() => setEdit({ team: t, isNew: false })}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Delete">
                      <IconButton size="small" color="error" onClick={() => setDeleteId(t.id)}>
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
        title="Delete team"
        message="Do you want to delete this team?"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteId(null)}
      />
    </Box>
  );
}

function EditTeamDialog({
  team, isNew, onCancel, onSave,
}: { team: Team; isNew: boolean; onCancel: () => void; onSave: (t: Team) => void }) {
  const [name, setName] = useState(team.name);
  const [description, setDescription] = useState(team.description);
  return (
    <Dialog open onClose={onCancel} fullWidth maxWidth="xs">
      <DialogTitle>{isNew ? 'New team' : 'Edit team'}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField label="Team name" value={name} onChange={(e) => setName(e.target.value)} fullWidth />
          <TextField label="Description" value={description} onChange={(e) => setDescription(e.target.value)} fullWidth multiline />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>Cancel</Button>
        <Button variant="contained" onClick={() => onSave({ ...team, name, description })}>Save</Button>
      </DialogActions>
    </Dialog>
  );
}

function AddMemberDialog({
  team, users, onCancel, onAdd,
}: { team: Team; users: User[]; onCancel: () => void; onAdd: (u: User) => void }) {
  const available = users.filter((u) => !team.users.some((tu) => tu.email === u.email));
  const [email, setEmail] = useState('');
  return (
    <Dialog open onClose={onCancel} fullWidth maxWidth="xs">
      <DialogTitle>Add member to {team.name}</DialogTitle>
      <DialogContent>
        <TextField select label="User" value={email} onChange={(e) => setEmail(e.target.value)} fullWidth sx={{ mt: 1 }}>
          {available.map((u) => (
            <MenuItem key={u.email} value={u.email}>{u.userName} ({u.email})</MenuItem>
          ))}
        </TextField>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>Cancel</Button>
        <Button
          variant="contained"
          disabled={!email}
          onClick={() => {
            const u = available.find((x) => x.email === email);
            if (u) onAdd(u);
          }}
        >
          Add
        </Button>
      </DialogActions>
    </Dialog>
  );
}
