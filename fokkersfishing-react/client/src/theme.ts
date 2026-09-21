import { createTheme } from '@mui/material/styles';

// A calm blue/teal palette befitting a fishing app.
export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#0b6e99' },
    secondary: { main: '#2e7d32' },
    background: { default: '#f4f6f8' },
  },
  shape: { borderRadius: 8 },
  components: {
    MuiButton: { defaultProps: { disableElevation: true } },
  },
});
