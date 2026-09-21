import { useEffect, useState } from 'react';
import {
  Box, Typography, Button, Grid, Card, CardContent, CardMedia, Divider, Table, TableBody,
  TableCell, TableHead, TableRow, Paper, TableContainer, LinearProgress, Alert,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { useSnackbar } from 'notistack';
import { api } from '../api/client';
import { createCatch } from '../api/catchSave';
import type { BigThree, Catch, CompetitionStats, Fish, FisherMan, User } from '../api/types';
import { CatchStatus } from '../api/types';
import { EditCatchDialog, type CatchSaveResult } from '../components/EditCatchDialog';
import { useAuth } from '../auth/AuthContext';
import { useCompetition, splitDuration } from '../context/CompetitionContext';

const EMPTY_ID = '00000000-0000-0000-0000-000000000000';

function newCatch(competitionId: string): Catch {
  return {
    id: crypto.randomUUID(), competitionId, catchNumber: 0, userEmail: null, userName: null,
    registerUserEmail: null, registerUserName: null, fish: 'Pike', length: 0, teamName: null,
    catchDate: new Date().toISOString(), logDate: new Date().toISOString(), editDate: new Date().toISOString(),
    globalCatchNumber: 0, measurePhotoUrl: null, catchPhotoUrl: null, measureThumbnailUrl: null,
    catchThumbnailUrl: null, status: CatchStatus.Pending,
  };
}

function FishCard({ label, c }: { label: string; c: Catch | null }) {
  if (!c || !c.fish) return null;
  return (
    <Grid item xs={12} sm={4}>
      <Card>
        <CardContent sx={{ pb: 1 }}>
          <Typography variant="subtitle2" color="text.secondary">{label}</Typography>
          <Typography variant="body1">{c.fish} ({c.length} cm)</Typography>
        </CardContent>
        {c.catchPhotoUrl && <CardMedia component="img" height="160" image={c.catchPhotoUrl} alt={c.fish} />}
      </Card>
    </Grid>
  );
}

export default function Home() {
  const competition = useCompetition();
  const { isUser } = useAuth();
  const { enqueueSnackbar } = useSnackbar();

  const [bigThree, setBigThree] = useState<BigThree[] | null>(null);
  const [fishermen, setFishermen] = useState<FisherMan[] | null>(null);
  const [stats, setStats] = useState<CompetitionStats | null>(null);
  const [fishOptions, setFishOptions] = useState<Fish[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState<string | null>(null);

  const [addOpen, setAddOpen] = useState(false);
  const [current, setCurrent] = useState<Catch | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (competition.loading) return;
    (async () => {
      try {
        const fish = await api.get<Fish[]>('/fish');
        setFishOptions(fish.data);
        if (!competition.active) {
          const [b3, fm] = await Promise.all([
            api.get<BigThree[]>('/leaderboard/bigthree'),
            api.get<FisherMan[]>('/leaderboard/fishermen'),
          ]);
          setBigThree(b3.data);
          setFishermen(fm.data);
        } else if (competition.competitionEnded) {
          try {
            const s = await api.get<CompetitionStats>(`/leaderboard/stats/${competition.competitionId}`);
            setStats(s.data);
          } catch { /* no stats yet */ }
        }
      } catch (e: any) {
        setError(e?.message ?? 'Failed to load.');
      }
      if (isUser) {
        try {
          const u = await api.get<User[]>('/adminuser/users');
          setUsers(u.data);
        } catch { /* ignore */ }
      }
    })();
  }, [competition.loading, competition.active, competition.competitionEnded, competition.competitionId, isUser]);

  const canAdd = isUser && (!competition.active || (!competition.competitionEnded && !competition.competitionNotStarted));

  const openAdd = () => {
    setCurrent(newCatch(competition.active ? competition.competitionId : EMPTY_ID));
    setAddOpen(true);
  };

  const handleSave = async (result: CatchSaveResult) => {
    setSaving(true);
    try {
      await createCatch(result);
      enqueueSnackbar('Catch saved', { variant: 'success' });
      setAddOpen(false);
    } catch (e: any) {
      enqueueSnackbar(e?.response?.data?.message ?? 'Saving catch failed', { variant: 'error' });
    } finally {
      setSaving(false);
    }
  };

  const end = splitDuration(competition.timeTillEnd);
  const start = splitDuration(competition.timeTillStart);

  return (
    <Box>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {canAdd && (
        <Button variant="contained" size="large" startIcon={<AddIcon />} onClick={openAdd} sx={{ mb: 3 }}>
          Add new catch
        </Button>
      )}

      {competition.active ? (
        <Box>
          <Typography variant="h4" gutterBottom>Fokkers Competition: {competition.competitionName}</Typography>
          {competition.competitionEnded ? (
            <>
              <Typography variant="h6" color="secondary">Competition ended.</Typography>
              {stats && (
                <Typography>Catches made: {stats.fishCaught}. Total length caught: {stats.totalLength} cm</Typography>
              )}
            </>
          ) : competition.competitionNotStarted ? (
            <>
              <Typography variant="h6" color="secondary">Competition has not started</Typography>
              <Typography color="text.secondary">
                Starts in {start.days} days, {start.hours} hours, {start.minutes} minutes
              </Typography>
            </>
          ) : (
            <>
              <Typography variant="h6" color="secondary">
                Competition active. Top-catches overview is disabled during the competition.
              </Typography>
              <Typography color="text.secondary">
                Ends in {end.days} days, {end.hours} hours, {end.minutes} minutes
              </Typography>
            </>
          )}
        </Box>
      ) : (
        <Box>
          <Typography variant="h4" gutterBottom>Big Three</Typography>
          {bigThree === null ? (
            <LinearProgress />
          ) : (
            bigThree
              .slice()
              .sort((a, b) => b.totalLength - a.totalLength)
              .map((big, i) => (
                <Box key={`${big.name}-${i}`} sx={{ mb: 3 }}>
                  <Typography variant="h6">#{i + 1} — {big.name}</Typography>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Total length: {big.totalLength} cm
                  </Typography>
                  <Grid container spacing={2}>
                    <FishCard label="Pike" c={big.pike} />
                    <FishCard label="Bass" c={big.bass} />
                    <FishCard label="Zander" c={big.zander} />
                  </Grid>
                  <Divider sx={{ mt: 2 }} />
                </Box>
              ))
          )}

          <Typography variant="h5" sx={{ mt: 4 }} gutterBottom>Top Fishermen</Typography>
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
      )}

      {current && addOpen && (
        <EditCatchDialog
          open={addOpen}
          currentCatch={current}
          fishOptions={fishOptions}
          users={users}
          editState="UserFull"
          saving={saving}
          onCancel={() => setAddOpen(false)}
          onSave={handleSave}
        />
      )}
    </Box>
  );
}
