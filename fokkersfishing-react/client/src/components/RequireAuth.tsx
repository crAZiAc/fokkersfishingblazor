import { Navigate, useLocation } from 'react-router-dom';
import { CircularProgress, Box } from '@mui/material';
import type { ReactNode } from 'react';
import { useAuth } from '../auth/AuthContext';

export function RequireAuth({ roles, children }: { roles?: string[]; children: ReactNode }) {
  const { isAuthenticated, loading, hasRole } = useAuth();
  const location = useLocation();

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />;
  }

  if (roles && roles.length > 0 && !hasRole(...roles)) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}
