import { useEffect, useState } from 'react';
import {
  Box, Typography, LinearProgress, Alert, Paper, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, Chip,
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import { api } from '../api/client';
import type { BigThree, Catch, FisherMan, Ranking } from '../api/types';
import { formatDateTime, statusMeta } from '../utils/format';
import { useAuth } from '../auth/AuthContext';
import { useCompetition } from '../context/CompetitionContext';

export default function Leaderboard() {
  const competition = useCompetition();
  const { isAuthenticated, isUser } = useAuth();

  const [catches, setCatches] = useState<Catch[] | null>(null);
  const [fishermen, setFishermen] = useState<FisherMan[] | null>(null);
  const [teamBig3, setTeamBig3] = useState<BigThree[]>([]);
  const [rankings, setRankings] = useState<Ranking[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (competition.loading) return;
    (async () => {
      try {
        if (!competition.active) {
          const [c, fm] = await Promise.all([
            api.get<Catch[]>('/leaderboard'),
            api.get<FisherMan[]>('/leaderboard/fishermen'),
          ]);
          setCatches(c.data);
          setFishermen(fm.data);
        } else if (isUser) {
          try {
            const b3 = await api.get<BigThree[]>(`/leaderboard/team/bigthree/${competition.competitionId}`);
            setTeamBig3(Array.isArray(b3.data) ? b3.data : []);
          } catch { /* not in a team / no data */ }
          if (competition.competitionEnded) {
            try {
              const r = await api.get<Ranking[]>(`/leaderboard/team/bigthree/all/${competition.competitionId}`);
              setRankings(Array.isArray(r.data) ? r.data : []);
            } catch { /* ignore */ }
          }
        }
      } catch (e: any) {
        setError(e?.message ?? 'Failed to load leaderboard.');
      }
    })();
  }, [competition.loading, competition.active, competition.competitionEnded, competition.competitionId, isUser]);

  if (competition.active) {
    if (!isAuthenticated) {
      return <Alert severity="info">Log in to view the competition leaderboard.</Alert>;
    }
    return (
      <Box>
        <Typography variant="h4" gutterBottom>{competition.competitionName} — Team Leaderboard</Typography>
        {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}

        {competition.competitionEnded && rankings.length > 0 && (
          <>
            <Typography variant="h5" sx={{ mt: 2 }} gutterBottom>Team Ranking</Typography>
            <TableContainer component={Paper} sx={{ mb: 4 }}>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Rank</TableCell>
                    <TableCell>Team</TableCell>
                    <TableCell align="right">Score</TableCell>
                    <TableCell align="center">Big 3</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {rankings.map((r) => (
                    <TableRow key={r.teamName} hover>
                      <TableCell>{r.rank}</TableCell>
                      <TableCell>{r.teamName}</TableCell>
                      <TableCell align="right">{r.score}</TableCell>
                      <TableCell align="center">{r.big3 ? <CheckCircleIcon color="success" fontSize="small" /> : ''}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </>
        )}

        <Typography variant="h5" gutterBottom>Team Big Three</Typography>
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Position</TableCell>
                <TableCell align="right">Pike (cm)</TableCell>
                <TableCell align="right">Bass (cm)</TableCell>
                <TableCell align="right">Zander (cm)</TableCell>
                <TableCell align="right">Total (cm)</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {teamBig3.map((b) => (
                <TableRow key={b.name} hover>
                  <TableCell>{b.name}</TableCell>
                  <TableCell align="right">{b.pike?.length ?? 0}</TableCell>
                  <TableCell align="right">{b.bass?.length ?? 0}</TableCell>
                  <TableCell align="right">{b.zander?.length ?? 0}</TableCell>
                  <TableCell align="right">{b.totalLength}</TableCell>
                </TableRow>
              ))}
              {teamBig3.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5}>
                    <Box sx={{ py: 2, textAlign: 'center', color: 'text.secondary' }}>No team data yet.</Box>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Leaderboard</Typography>
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}

      <Typography variant="h5" gutterBottom>Top Catches</Typography>
      {catches === null ? (
        <LinearProgress />
      ) : (
        <TableContainer component={Paper} sx={{ mb: 4 }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Catch #</TableCell>
                <TableCell>Catch Date</TableCell>
                <TableCell>Fish</TableCell>
                <TableCell align="right">Length (cm)</TableCell>
                <TableCell>Fisherman</TableCell>
                <TableCell align="right">Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {catches.map((c) => {
                const meta = statusMeta(c.status);
                return (
                  <TableRow key={c.id} hover>
                    <TableCell>{c.globalCatchNumber}</TableCell>
                    <TableCell>{formatDateTime(c.catchDate)}</TableCell>
                    <TableCell>{c.fish}</TableCell>
                    <TableCell align="right">{c.length}</TableCell>
                    <TableCell>{c.userName}</TableCell>
                    <TableCell align="right"><Chip size="small" label={meta.label} color={meta.color} /></TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Typography variant="h5" gutterBottom>Top Fishermen</Typography>
      {fishermen === null ? (
        <LinearProgress />
      ) : (
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
      )}
    </Box>
  );
}
