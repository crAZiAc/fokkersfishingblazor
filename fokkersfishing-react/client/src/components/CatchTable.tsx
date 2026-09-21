import { useMemo, useState } from 'react';
import {
  Box, Chip, IconButton, MenuItem, Paper, Stack, Table, TableBody, TableCell, TableContainer,
  TableHead, TablePagination, TableRow, TextField, Tooltip, Avatar,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import PictureInPictureIcon from '@mui/icons-material/PictureInPicture';
import FitnessCenterIcon from '@mui/icons-material/FitnessCenter';
import ImageIcon from '@mui/icons-material/Image';
import type { Catch, Fish } from '../api/types';
import { CatchStatus } from '../api/types';
import { formatDateTime, statusMeta } from '../utils/format';
import { PhotoViewer } from './PhotoViewer';

export type ViewState = 'Personal' | 'Team' | 'Admin' | 'Pending' | 'CompetitionLeaderboard';

interface Props {
  catches: Catch[];
  fishOptions: Fish[];
  viewState: ViewState;
  onShowEdit?: (id: string) => void;
  onShowEditFull?: (id: string) => void;
  onShowDelete?: (id: string) => void;
}

export function CatchTable({ catches, fishOptions, viewState, onShowEdit, onShowEditFull, onShowDelete }: Props) {
  const [fishFilter, setFishFilter] = useState('');
  const [fishermanFilter, setFishermanFilter] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(25);
  const [viewer, setViewer] = useState<{ url: string | null; item: Catch } | null>(null);

  const showTeam = viewState === 'CompetitionLeaderboard';
  const showCommands = viewState !== 'CompetitionLeaderboard';

  const fishermen = useMemo(
    () => Array.from(new Set(catches.map((c) => c.userName).filter(Boolean) as string[])).sort(),
    [catches],
  );
  const fishes = useMemo(
    () => Array.from(new Set(catches.map((c) => c.fish).filter(Boolean))).sort(),
    [catches],
  );

  const filtered = useMemo(() => {
    return catches.filter((c) => {
      if (fishFilter && c.fish !== fishFilter) return false;
      if (fishermanFilter && c.userName !== fishermanFilter) return false;
      return true;
    });
  }, [catches, fishFilter, fishermanFilter]);

  const paged = filtered.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage);

  const renderActions = (c: Catch) => {
    const canFullEdit = viewState === 'Admin';
    const buttons: JSX.Element[] = [];
    const fullEdit = (
      <Tooltip title="Full edit" key="full">
        <IconButton size="small" onClick={() => onShowEditFull?.(c.id)}>
          <PictureInPictureIcon fontSize="small" />
        </IconButton>
      </Tooltip>
    );
    const edit = (
      <Tooltip title="Edit" key="edit">
        <IconButton size="small" onClick={() => onShowEdit?.(c.id)}>
          <EditIcon fontSize="small" />
        </IconButton>
      </Tooltip>
    );
    const del = (
      <Tooltip title="Delete" key="del">
        <IconButton size="small" color="error" onClick={() => onShowDelete?.(c.id)}>
          <DeleteIcon fontSize="small" />
        </IconButton>
      </Tooltip>
    );

    if (c.status === CatchStatus.Approved) {
      if (canFullEdit) { buttons.push(fullEdit); buttons.push(edit); }
      buttons.push(del);
    } else if (c.status === CatchStatus.Pending) {
      if (viewState !== 'Pending') buttons.push(fullEdit);
      buttons.push(edit);
      buttons.push(del);
    } else {
      // Rejected
      if (canFullEdit) { buttons.push(fullEdit); buttons.push(edit); }
      buttons.push(del);
    }
    return buttons;
  };

  return (
    <Paper sx={{ mt: 2 }}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ p: 2 }}>
        <TextField select label="Fish" size="small" value={fishFilter} onChange={(e) => setFishFilter(e.target.value)} sx={{ minWidth: 180 }}>
          <MenuItem value="">All</MenuItem>
          {fishes.map((f) => (
            <MenuItem key={f} value={f}>{f}</MenuItem>
          ))}
        </TextField>
        <TextField select label="Fisherman" size="small" value={fishermanFilter} onChange={(e) => setFishermanFilter(e.target.value)} sx={{ minWidth: 180 }}>
          <MenuItem value="">All</MenuItem>
          {fishermen.map((f) => (
            <MenuItem key={f} value={f}>{f}</MenuItem>
          ))}
        </TextField>
      </Stack>

      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Catch #</TableCell>
              <TableCell>Catch Date</TableCell>
              <TableCell>Fish</TableCell>
              <TableCell align="right">Length (cm)</TableCell>
              <TableCell>Fisherman</TableCell>
              {showTeam && <TableCell>Team</TableCell>}
              <TableCell>Catch</TableCell>
              <TableCell>Measure</TableCell>
              {showCommands && <TableCell align="right">Status / Actions</TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {paged.map((c) => {
              const meta = statusMeta(c.status);
              return (
                <TableRow key={c.id} hover>
                  <TableCell>{c.globalCatchNumber}</TableCell>
                  <TableCell>{formatDateTime(c.catchDate)}</TableCell>
                  <TableCell>{c.fish}</TableCell>
                  <TableCell align="right">{c.length}</TableCell>
                  <TableCell>{c.userName}</TableCell>
                  {showTeam && <TableCell>{c.teamName}</TableCell>}
                  <TableCell>
                    {c.catchThumbnailUrl ? (
                      <Avatar
                        variant="rounded"
                        src={c.catchThumbnailUrl}
                        sx={{ width: 40, height: 40, cursor: 'pointer' }}
                        onClick={() => setViewer({ url: c.catchPhotoUrl, item: c })}
                      />
                    ) : (
                      <ImageIcon color="disabled" />
                    )}
                  </TableCell>
                  <TableCell>
                    {c.measureThumbnailUrl ? (
                      <Avatar
                        variant="rounded"
                        src={c.measureThumbnailUrl}
                        sx={{ width: 40, height: 40, cursor: 'pointer' }}
                        onClick={() => setViewer({ url: c.measurePhotoUrl, item: c })}
                      />
                    ) : (
                      <ImageIcon color="disabled" />
                    )}
                  </TableCell>
                  {showCommands && (
                    <TableCell align="right">
                      <Stack direction="row" spacing={0.5} justifyContent="flex-end" alignItems="center">
                        {c.caughtInCompetition && (
                          <Tooltip title="Caught in competition">
                            <FitnessCenterIcon fontSize="small" color="action" />
                          </Tooltip>
                        )}
                        <Chip size="small" label={meta.label} color={meta.color} />
                        {renderActions(c)}
                      </Stack>
                    </TableCell>
                  )}
                </TableRow>
              );
            })}
            {paged.length === 0 && (
              <TableRow>
                <TableCell colSpan={9}>
                  <Box sx={{ py: 3, textAlign: 'center', color: 'text.secondary' }}>No catches.</Box>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        component="div"
        count={filtered.length}
        page={page}
        onPageChange={(_, p) => setPage(p)}
        rowsPerPage={rowsPerPage}
        onRowsPerPageChange={(e) => {
          setRowsPerPage(parseInt(e.target.value, 10));
          setPage(0);
        }}
        rowsPerPageOptions={[10, 25, 50, 100]}
      />

      <PhotoViewer
        open={!!viewer}
        url={viewer?.url}
        catchMade={viewer?.item ?? null}
        onClose={() => setViewer(null)}
      />
    </Paper>
  );
}
