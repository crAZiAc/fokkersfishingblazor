import { useEffect, useState } from 'react';
import {
  Box, Typography, Alert, LinearProgress, Paper, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, IconButton, Tooltip, Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, FormControlLabel, Checkbox, Stack,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import LockResetIcon from '@mui/icons-material/LockReset';
import { useSnackbar } from 'notistack';
import { useTranslation } from 'react-i18next';
import { api } from '../api/client';
import type { Role, User } from '../api/types';
import { ConfirmDialog } from '../components/ConfirmDialog';

export default function AdminUsers() {
  const { t } = useTranslation();
  const { enqueueSnackbar } = useSnackbar();
  const [users, setUsers] = useState<User[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [editUser, setEditUser] = useState<User | null>(null);
  const [resetUser, setResetUser] = useState<User | null>(null);
  const [deleteEmail, setDeleteEmail] = useState<string | null>(null);

  const load = async () => {
    try {
      const { data } = await api.get<User[]>('/adminuser/users');
      setUsers(data);
    } catch (e: any) {
      setError(e?.message ?? t('adminUsers.loadFailed'));
    }
  };

  useEffect(() => {
    void load();
  }, []);

  const save = async (user: User) => {
    try {
      await api.put('/adminuser/users', user);
      enqueueSnackbar(t('adminUsers.userSaved'), { variant: 'success' });
      setEditUser(null);
      void load();
    } catch {
      enqueueSnackbar(t('adminUsers.saveFailed'), { variant: 'error' });
    }
  };

  const resetPassword = async (email: string, newPassword: string) => {
    try {
      await api.post(`/adminuser/users/${encodeURIComponent(email)}/password`, { newPassword });
      enqueueSnackbar(t('adminUsers.passwordReset'), { variant: 'success' });
      setResetUser(null);
    } catch (e: any) {
      enqueueSnackbar(e?.response?.data?.message ?? t('adminUsers.passwordResetFailed'), { variant: 'error' });
    }
  };

  const confirmDelete = async () => {
    if (!deleteEmail) return;
    try {
      await api.delete(`/adminuser/users/${encodeURIComponent(deleteEmail)}`);
      setUsers((prev) => (prev ?? []).filter((u) => u.email !== deleteEmail));
      enqueueSnackbar(t('adminUsers.deleted'), { variant: 'info' });
    } catch {
      enqueueSnackbar(t('adminUsers.deleteFailed'), { variant: 'error' });
    } finally {
      setDeleteEmail(null);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>{t('adminUsers.title')}</Typography>
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}
      {users === null ? (
        <LinearProgress />
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>{t('adminUsers.userName')}</TableCell>
                <TableCell>{t('adminUsers.email')}</TableCell>
                <TableCell>{t('adminUsers.loginProvider')}</TableCell>
                <TableCell>{t('adminUsers.roles')}</TableCell>
                <TableCell align="right">{t('common.actions')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {users.map((u) => (
                <TableRow key={u.email} hover>
                  <TableCell>{u.userName}</TableCell>
                  <TableCell>{u.email}</TableCell>
                  <TableCell>{u.loginProvider}</TableCell>
                  <TableCell>{u.roleList}</TableCell>
                  <TableCell align="right">
                    <Tooltip title={t('common.edit')}>
                      <IconButton size="small" onClick={() => setEditUser(u)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title={t('common.resetPassword')}>
                      <IconButton size="small" onClick={() => setResetUser(u)}>
                        <LockResetIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title={t('common.delete')}>
                      <IconButton size="small" color="error" onClick={() => setDeleteEmail(u.email)}>
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

      {editUser && <EditUserDialog user={editUser} onCancel={() => setEditUser(null)} onSave={save} />}

      {resetUser && (
        <ResetPasswordDialog
          user={resetUser}
          onCancel={() => setResetUser(null)}
          onReset={(pwd) => resetPassword(resetUser.email, pwd)}
        />
      )}

      <ConfirmDialog
        open={!!deleteEmail}
        title={t('adminUsers.deleteTitle')}
        message={t('adminUsers.deleteMsg', { email: deleteEmail })}
        onConfirm={confirmDelete}
        onCancel={() => setDeleteEmail(null)}
      />
    </Box>
  );
}

function ResetPasswordDialog({
  user, onCancel, onReset,
}: { user: User; onCancel: () => void; onReset: (newPassword: string) => void }) {
  const { t } = useTranslation();
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const mismatch = confirm.length > 0 && password !== confirm;
  return (
    <Dialog open onClose={onCancel} fullWidth maxWidth="xs">
      <DialogTitle>{t('adminUsers.resetTitle', { name: user.userName })}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField label={t('adminUsers.newPassword')} type="password" value={password} onChange={(e) => setPassword(e.target.value)} fullWidth autoFocus
            helperText={t('auth.passwordHelper')} />
          <TextField label={t('adminUsers.confirmPassword')} type="password" value={confirm} onChange={(e) => setConfirm(e.target.value)} fullWidth
            error={mismatch} helperText={mismatch ? t('adminUsers.passwordsNoMatch') : ' '} />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>{t('common.cancel')}</Button>
        <Button variant="contained" disabled={!password || mismatch} onClick={() => onReset(password)}>{t('adminUsers.reset')}</Button>
      </DialogActions>
    </Dialog>
  );
}

function EditUserDialog({ user, onCancel, onSave }: { user: User; onCancel: () => void; onSave: (u: User) => void }) {
  const { t } = useTranslation();
  const [userName, setUserName] = useState(user.userName);
  const [roles, setRoles] = useState<Role[]>(user.roleArray ?? user.roles ?? []);

  const toggleRole = (name: string, checked: boolean) =>
    setRoles((prev) => prev.map((r) => (r.name === name ? { ...r, isInRole: checked } : r)));

  return (
    <Dialog open onClose={onCancel} fullWidth maxWidth="xs">
      <DialogTitle>{t('adminUsers.editTitle')}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField label={t('adminUsers.email')} value={user.email} disabled fullWidth />
          <TextField label={t('adminUsers.userName')} value={userName} onChange={(e) => setUserName(e.target.value)} fullWidth />
          <Box>
            <Typography variant="subtitle2">{t('adminUsers.roles')}</Typography>
            {roles.map((r) => (
              <FormControlLabel
                key={r.name}
                control={<Checkbox checked={r.isInRole} onChange={(e) => toggleRole(r.name, e.target.checked)} />}
                label={r.name}
              />
            ))}
          </Box>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>{t('common.cancel')}</Button>
        <Button variant="contained" onClick={() => onSave({ ...user, userName, roles })}>
          {t('common.save')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
