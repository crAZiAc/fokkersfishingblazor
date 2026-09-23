import { useEffect, useState } from 'react';
import { useNavigate, useLocation, Link as RouterLink } from 'react-router-dom';
import {
  Box, Button, Card, CardContent, Stack, TextField, Typography, Divider, Alert, CircularProgress,
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { api } from '../api/client';
import { useAuth } from '../auth/AuthContext';
import type { ExternalProviderInfo } from '../api/types';

export default function Login() {
  const { t } = useTranslation();
  const { login, loginExternal, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [providers, setProviders] = useState<ExternalProviderInfo[]>([]);

  useEffect(() => {
    api.get<ExternalProviderInfo[]>('/auth/providers').then((r) => setProviders(r.data)).catch(() => setProviders([]));
  }, []);

  useEffect(() => {
    if (isAuthenticated) {
      const from = (location.state as { from?: string } | null)?.from ?? '/';
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, navigate, location.state]);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await login(email, password);
    } catch (err: any) {
      setError(err?.response?.data?.message ?? t('auth.loginFailed'));
    } finally {
      setBusy(false);
    }
  };

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8, px: 2 }}>
      <Card sx={{ width: 400, maxWidth: '100%' }}>
        <CardContent>
          <Typography variant="h5" gutterBottom>
            {t('auth.loginTitle')}
          </Typography>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          <form onSubmit={submit}>
            <Stack spacing={2}>
              <TextField label={t('auth.email')} type="email" value={email} onChange={(e) => setEmail(e.target.value)} required fullWidth autoFocus />
              <TextField label={t('auth.password')} type="password" value={password} onChange={(e) => setPassword(e.target.value)} required fullWidth />
              <Button type="submit" variant="contained" disabled={busy} startIcon={busy ? <CircularProgress size={18} /> : undefined}>
                {t('common.logIn')}
              </Button>
            </Stack>
          </form>

          {providers.length > 0 && (
            <>
              <Divider sx={{ my: 2 }}>{t('auth.or')}</Divider>
              <Stack spacing={1}>
                {providers.map((p) => (
                  <Button key={p.name} variant="outlined" onClick={() => loginExternal(p.name)}>
                    {t('auth.continueWith', { provider: p.displayName })}
                  </Button>
                ))}
              </Stack>
            </>
          )}

          <Typography variant="body2" sx={{ mt: 2 }}>
            {t('auth.noAccount')} <RouterLink to="/register">{t('common.register')}</RouterLink>
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
}
