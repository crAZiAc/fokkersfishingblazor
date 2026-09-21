import { useState } from 'react';
import { Outlet, useNavigate, useLocation, Link as RouterLink } from 'react-router-dom';
import {
  AppBar, Box, Drawer, IconButton, List, ListItemButton, ListItemIcon, ListItemText,
  Toolbar, Typography, Divider, Avatar, Button, Container, useMediaQuery,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import HomeIcon from '@mui/icons-material/Home';
import LeaderboardIcon from '@mui/icons-material/Leaderboard';
import PhishingIcon from '@mui/icons-material/Phishing';
import GroupsIcon from '@mui/icons-material/Groups';
import PeopleIcon from '@mui/icons-material/People';
import ListAltIcon from '@mui/icons-material/ListAlt';
import PendingActionsIcon from '@mui/icons-material/PendingActions';
import EventIcon from '@mui/icons-material/Event';
import EmojiEventsIcon from '@mui/icons-material/EmojiEvents';
import { useTheme } from '@mui/material/styles';
import { useAuth } from '../auth/AuthContext';
import { useCompetition } from '../context/CompetitionContext';

const drawerWidth = 240;

interface NavItem {
  label: string;
  to: string;
  icon: JSX.Element;
  roles?: string[];
  requiresCompetition?: boolean;
}

export function Layout() {
  const theme = useTheme();
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'));
  const [mobileOpen, setMobileOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { user, isAuthenticated, isAdmin, isUser, logout } = useAuth();
  const competition = useCompetition();

  const items: NavItem[] = [
    { label: 'Home', to: '/', icon: <HomeIcon /> },
    { label: 'Leaderboard', to: '/leaders', icon: <LeaderboardIcon /> },
    { label: 'My Catches', to: '/catches', icon: <PhishingIcon />, roles: ['Administrator', 'User'] },
    { label: 'Team Leaderboard', to: '/leaders/team', icon: <EmojiEventsIcon />, roles: ['Administrator', 'User'], requiresCompetition: true },
    { label: 'Competition Leads', to: '/leaders/competition', icon: <LeaderboardIcon />, roles: ['Administrator'], requiresCompetition: true },
    { label: 'Users', to: '/adminusers', icon: <PeopleIcon />, roles: ['Administrator'] },
    { label: 'Teams', to: '/adminteams', icon: <GroupsIcon />, roles: ['Administrator'] },
    { label: 'All Catches', to: '/admincatches', icon: <ListAltIcon />, roles: ['Administrator'] },
    { label: 'Pending Catches', to: '/pendingcatches', icon: <PendingActionsIcon />, roles: ['Administrator'] },
    { label: 'Competitions', to: '/admincompetitions', icon: <EventIcon />, roles: ['Administrator'] },
  ];

  const visible = items.filter((item) => {
    if (item.requiresCompetition && !competition.active) return false;
    if (!item.roles) return true;
    if (item.roles.includes('Administrator') && isAdmin) return true;
    if (item.roles.includes('User') && isUser) return true;
    return false;
  });

  const drawer = (
    <Box>
      <Toolbar>
        <PhishingIcon sx={{ mr: 1 }} />
        <Typography variant="h6" noWrap>
          FVD 2026
        </Typography>
      </Toolbar>
      <Divider />
      <List>
        {visible.map((item) => {
          const selected = location.pathname === item.to;
          return (
            <ListItemButton
              key={item.to}
              component={RouterLink}
              to={item.to}
              selected={selected}
              onClick={() => setMobileOpen(false)}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItemButton>
          );
        })}
      </List>
    </Box>
  );

  return (
    <Box sx={{ display: 'flex' }}>
      <AppBar position="fixed" sx={{ zIndex: (t) => t.zIndex.drawer + 1 }}>
        <Toolbar>
          {!isDesktop && (
            <IconButton color="inherit" edge="start" onClick={() => setMobileOpen(!mobileOpen)} sx={{ mr: 2 }}>
              <MenuIcon />
            </IconButton>
          )}
          <Typography variant="h6" sx={{ flexGrow: 1 }} noWrap>
            FVD 2026
          </Typography>
          {isAuthenticated ? (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Avatar sx={{ width: 30, height: 30 }}>{user?.name?.[0]?.toUpperCase() ?? '?'}</Avatar>
              <Typography variant="body2" sx={{ display: { xs: 'none', sm: 'block' } }}>
                {user?.name}
              </Typography>
              <Button color="inherit" onClick={logout}>
                Sign out
              </Button>
            </Box>
          ) : (
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Button color="inherit" onClick={() => navigate('/login')}>
                Log in
              </Button>
              <Button color="inherit" onClick={() => navigate('/register')}>
                Register
              </Button>
            </Box>
          )}
        </Toolbar>
      </AppBar>

      <Box component="nav" sx={{ width: { md: drawerWidth }, flexShrink: { md: 0 } }}>
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={() => setMobileOpen(false)}
          ModalProps={{ keepMounted: true }}
          sx={{
            display: { xs: 'block', md: 'none' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
          }}
        >
          {drawer}
        </Drawer>
        <Drawer
          variant="permanent"
          open
          sx={{
            display: { xs: 'none', md: 'block' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
          }}
        >
          {drawer}
        </Drawer>
      </Box>

      <Box component="main" sx={{ flexGrow: 1, width: { md: `calc(100% - ${drawerWidth}px)` } }}>
        <Toolbar />
        <Container maxWidth="lg" sx={{ py: 3 }}>
          <Outlet />
        </Container>
      </Box>
    </Box>
  );
}
