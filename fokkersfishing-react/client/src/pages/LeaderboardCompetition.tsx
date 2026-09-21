import { useEffect, useState } from 'react';
import {
  Box, Typography, LinearProgress, Alert, Paper, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow,
} from '@mui/material';
import { api } from '../api/client';
import type { Catch, Fish, FisherMan } from '../api/types';
import { CatchTable } from '../components/CatchTable';
import { useCompetition } from '../context/CompetitionContext';

export default function LeaderboardCompetition() {
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
        setError(e?.message ?? 'Failed to load.');
      }
    })();
  }, [competition.loading, competition.active, competition.competitionId]);

  if (!competition.active) {
    return <Alert severity="info">No competition is currently active.</Alert>;
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Fokkers Competition Leaderboard</Typography>
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}
      {catches === null ? (
        <LinearProgress />
      ) : (
        <CatchTable catches={catches} fishOptions={fishOptions} viewState="CompetitionLeaderboard" />
      )}

      <Typography variant="h5" sx={{ mt: 4 }} gutterBottom>Top Fishermen</Typography>
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Fisherman</TableCell>
              <TableCell align="right">Total Fish Length</TableCell>
              <TableCell align="right">Total Fish Caught</TableCell>
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
