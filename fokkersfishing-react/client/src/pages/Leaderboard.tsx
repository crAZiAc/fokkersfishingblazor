import { useEffect, useState } from 'react';
import {
  Box, Typography, LinearProgress, Alert, Paper, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, Chip,
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import { useTranslation } from 'react-i18next';
import { api } from '../api/client';
import type { BigThree, Catch, FisherMan, Ranking } from '../api/types';
import { formatDateTime, statusMeta } from '../utils/format';
import { useAuth } from '../auth/AuthContext';
import { useCompetition } from '../context/CompetitionContext';

export default function Leaderboard() {
  const { t } = useTranslation();
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
        setError(e?.message ?? t('leaderboard.loadFailed'));
      }
    })();
  }, [competition.loading, competition.active, competition.competitionEnded, competition.competitionId, isUser, t]);

  if (competition.active) {
    if (!isAuthenticated) {
      return <Alert severity="info">{t('leaderboard.mustLogin')}</Alert>;
    }
    return (
      <Box>
        <Typography variant="h4" gutterBottom>{t('leaderboard.teamTitle', { name: competition.competitionName })}</Typography>
        {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}

        {competition.competitionEnded && rankings.length > 0 && (
          <>
            <Typography variant="h5" sx={{ mt: 2 }} gutterBottom>{t('leaderboard.teamRanking')}</Typography>
            <TableContainer component={Paper} sx={{ mb: 4 }}>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>{t('leaderboard.rank')}</TableCell>
                    <TableCell>{t('leaderboard.team')}</TableCell>
                    <TableCell align="right">{t('leaderboard.score')}</TableCell>
                    <TableCell align="center">{t('leaderboard.big3')}</TableCell>
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

        <Typography variant="h5" gutterBottom>{t('leaderboard.teamBigThree')}</Typography>
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>{t('leaderboard.position')}</TableCell>
                <TableCell align="right">{t('home.pike')} (cm)</TableCell>
                <TableCell align="right">{t('home.bass')} (cm)</TableCell>
                <TableCell align="right">{t('home.zander')} (cm)</TableCell>
                <TableCell align="right">{t('leaderboard.totalCm')}</TableCell>
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
                    <Box sx={{ py: 2, textAlign: 'center', color: 'text.secondary' }}>{t('leaderboard.noTeamData')}</Box>
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
      <Typography variant="h4" gutterBottom>{t('leaderboard.title')}</Typography>
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}

      <Typography variant="h5" gutterBottom>{t('leaderboard.topCatches')}</Typography>
      {catches === null ? (
        <LinearProgress />
      ) : (
        <TableContainer component={Paper} sx={{ mb: 4 }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>{t('catches.catchNo')}</TableCell>
                <TableCell>{t('catches.catchDate')}</TableCell>
                <TableCell>{t('catches.fish')}</TableCell>
                <TableCell align="right">{t('catches.lengthCm')}</TableCell>
                <TableCell>{t('catches.fisherman')}</TableCell>
                <TableCell align="right">{t('leaderboard.status')}</TableCell>
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
                    <TableCell align="right"><Chip size="small" label={t(meta.labelKey)} color={meta.color} /></TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Typography variant="h5" gutterBottom>{t('home.topFishermen')}</Typography>
      {fishermen === null ? (
        <LinearProgress />
      ) : (
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
      )}
    </Box>
  );
}
