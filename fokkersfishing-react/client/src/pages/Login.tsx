import { useEffect, useState } from 'react';
import { useNavigate, useLocation, Link as RouterLink } from 'react-router-dom';
import {
  Box, Button, Card, CardContent, Stack, TextField, Typography, Divider, Alert, CircularProgress,
} from '@mui/material';
import { api } from '../api/client';
import { useAuth } from '../auth/AuthContext';
import type { ExternalProviderInfo } from '../api/types';

export default function Login() {
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
      setError(err?.response?.data?.message ?? 'Login failed. Check your credentials.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8, px: 2 }}>
      <Card sx={{ width: 400, maxWidth: '100%' }}>
        <CardContent>
          <Typography variant="h5" gutterBottom>
            FVD 2026 — Log in
          </Typography>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          <form onSubmit={submit}>
            <Stack spacing={2}>
              <TextField label="Email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required fullWidth autoFocus />
              <TextField label="Password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required fullWidth />
              <Button type="submit" variant="contained" disabled={busy} startIcon={busy ? <CircularProgress size={18} /> : undefined}>
                Log in
              </Button>
            </Stack>
          </form>

          {providers.length > 0 && (
            <>
              <Divider sx={{ my: 2 }}>or</Divider>
              <Stack spacing={1}>
                {providers.map((p) => (
                  <Button key={p.name} variant="outlined" onClick={() => loginExternal(p.name)}>
                    Continue with {p.displayName}
                  </Button>
                ))}
              </Stack>
            </>
          )}

          <Typography variant="body2" sx={{ mt: 2 }}>
            No account? <RouterLink to="/register">Register</RouterLink>
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
}
