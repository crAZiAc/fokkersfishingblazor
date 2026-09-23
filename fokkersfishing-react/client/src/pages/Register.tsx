import { useEffect, useState } from 'react';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { Box, Button, Card, CardContent, Stack, TextField, Typography, Alert, CircularProgress } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../auth/AuthContext';

export default function Register() {
  const { t } = useTranslation();
  const { register, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (isAuthenticated) navigate('/', { replace: true });
  }, [isAuthenticated, navigate]);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await register(email, userName, password);
    } catch (err: any) {
      setError(err?.response?.data?.message ?? t('auth.registrationFailed'));
    } finally {
      setBusy(false);
    }
  };

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8, px: 2 }}>
      <Card sx={{ width: 400, maxWidth: '100%' }}>
        <CardContent>
          <Typography variant="h5" gutterBottom>
            {t('auth.registerTitle')}
          </Typography>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          <form onSubmit={submit}>
            <Stack spacing={2}>
              <TextField label={t('auth.email')} type="email" value={email} onChange={(e) => setEmail(e.target.value)} required fullWidth autoFocus />
              <TextField label={t('auth.displayName')} value={userName} onChange={(e) => setUserName(e.target.value)} required fullWidth />
              <TextField
                label={t('auth.password')}
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                fullWidth
                helperText={t('auth.passwordHelper')}
              />
              <Button type="submit" variant="contained" disabled={busy} startIcon={busy ? <CircularProgress size={18} /> : undefined}>
                {t('common.register')}
              </Button>
            </Stack>
          </form>
          <Typography variant="body2" sx={{ mt: 2 }}>
            {t('auth.haveAccount')} <RouterLink to="/login">{t('common.logIn')}</RouterLink>
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
}
