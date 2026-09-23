import { useEffect, useState } from 'react';
import {
  Box, Typography, LinearProgress, Alert, Paper, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow,
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { api } from '../api/client';
import type { Catch, Fish, FisherMan } from '../api/types';
import { CatchTable } from '../components/CatchTable';
import { useCompetition } from '../context/CompetitionContext';

export default function LeaderboardCompetition() {
  const { t } = useTranslation();
  const competition = useCompetition();
  const [catches, setCatches] = useState<Catch[] | null>(null);
  const [fishermen, setFishermen] = useState<FisherMan[]>([]);
  const [fishOptions, setFishOptions] = useState<Fish[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (competition.loading || !competition.active) return;
    (async () => {
      try {
        const [c, fm, fish] = await Promise.all([
          api.get<Catch[]>(`/leaderboard/${competition.competitionId}`),
          api.get<FisherMan[]>(`/leaderboard/fishermen/${competition.competitionId}`),
          api.get<Fish[]>('/fish'),
        ]);
        setCatches(c.data);
        setFishermen(fm.data);
        setFishOptions(fish.data);
      } catch (e: any) {
        setError(e?.message ?? t('leaderboardComp.loadFailed'));
      }
    })();
  }, [competition.loading, competition.active, competition.competitionId, t]);

  if (!competition.active) {
    return <Alert severity="info">{t('leaderboardComp.noActive')}</Alert>;
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>{t('leaderboardComp.title')}</Typography>
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}
      {catches === null ? (
        <LinearProgress />
      ) : (
        <CatchTable catches={catches} fishOptions={fishOptions} viewState="CompetitionLeaderboard" />
      )}

      <Typography variant="h5" sx={{ mt: 4 }} gutterBottom>{t('leaderboardComp.topFishermen')}</Typography>
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>{t('home.fisherman')}</TableCell>
              <TableCell align="right">{t('home.totalFishLength')}</TableCell>
              <TableCell align="right">{t('home.totalFishCaught')}</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {fishermen.map((f) => (
              <TableRow key={f.userEmail} hover>
                <TableCell>{f.userName}</TableCell>
                <TableCell align="right">{f.totalLength}</TableCell>
                <TableCell align="right">{f.fishCount}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}
