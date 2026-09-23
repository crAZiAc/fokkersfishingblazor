import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, CircularProgress, Alert, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../auth/AuthContext';

/**
 * Landing route for the external-login round trip. The API redirects here with
 * `#token=...&returnUrl=...` (or `#error=...`). We stash the token and go home.
 */
export default function AuthCallback() {
  const { t } = useTranslation();
  const { applyToken } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const hash = window.location.hash.startsWith('#') ? window.location.hash.slice(1) : window.location.hash;
    const params = new URLSearchParams(hash);
    const token = params.get('token');
    const err = params.get('error');
    const returnUrl = params.get('returnUrl') || '/';

    if (err) {
      setError(t('auth.externalFailed', { error: err }));
      return;
    }
    if (token) {
      applyToken(token).then(() => navigate(returnUrl, { replace: true }));
    } else {
      setError(t('auth.noToken'));
    }
  }, [applyToken, navigate, t]);

  if (error) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8, px: 2 }}>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mt: 8, gap: 2 }}>
      <CircularProgress />
      <Typography>{t('auth.signingIn')}</Typography>
    </Box>
  );
}
