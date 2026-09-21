import { Routes, Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { RequireAuth } from './components/RequireAuth';
import Home from './pages/Home';
import Leaderboard from './pages/Leaderboard';
import LeaderboardCompetition from './pages/LeaderboardCompetition';
import LeaderboardTeam from './pages/LeaderboardTeam';
import MyCatches from './pages/MyCatches';
import PendingCatches from './pages/PendingCatches';
import AdminCatches from './pages/AdminCatches';
import AdminCompetitions from './pages/AdminCompetitions';
import AdminTeams from './pages/AdminTeams';
import AdminUsers from './pages/AdminUsers';
import Login from './pages/Login';
import Register from './pages/Register';
import AuthCallback from './pages/AuthCallback';

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/auth-callback" element={<AuthCallback />} />

      <Route path="/" element={<Layout />}>
        <Route index element={<Home />} />
        <Route path="leaders" element={<Leaderboard />} />
        <Route
          path="leaders/competition"
          element={
            <RequireAuth roles={['Administrator']}>
              <LeaderboardCompetition />
            </RequireAuth>
          }
        />
        <Route
          path="leaders/team"
          element={
            <RequireAuth roles={['Administrator', 'User']}>
              <LeaderboardTeam />
            </RequireAuth>
          }
        />
        <Route
          path="catches"
          element={
            <RequireAuth roles={['Administrator', 'User']}>
              <MyCatches />
            </RequireAuth>
          }
        />
        <Route
          path="pendingcatches"
          element={
            <RequireAuth roles={['Administrator']}>
              <PendingCatches />
            </RequireAuth>
          }
        />
        <Route
          path="admincatches"
          element={
            <RequireAuth roles={['Administrator']}>
              <AdminCatches />
            </RequireAuth>
          }
        />
        <Route
          path="admincompetitions"
          element={
            <RequireAuth roles={['Administrator']}>
              <AdminCompetitions />
            </RequireAuth>
          }
        />
        <Route
          path="adminteams"
          element={
            <RequireAuth roles={['Administrator']}>
              <AdminTeams />
            </RequireAuth>
          }
        />
        <Route
          path="adminusers"
          element={
            <RequireAuth roles={['Administrator']}>
              <AdminUsers />
            </RequireAuth>
          }
        />
      </Route>
    </Routes>
  );
}
