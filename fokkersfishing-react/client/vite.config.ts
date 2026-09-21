import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// API route prefixes served by the .NET backend. In dev they are proxied to the
// API (default http://localhost:5080); in production the API serves the built SPA
// from wwwroot so no proxy is needed.
const apiPrefixes = [
  '/auth',
  '/catch',
  '/competition',
  '/team',
  '/leaderboard',
  '/fish',
  '/photo',
  '/adminuser',
  '/swagger',
];

const apiTarget = process.env.VITE_API_TARGET ?? 'http://localhost:5080';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: Object.fromEntries(
      apiPrefixes.map((p) => [p, { target: apiTarget, changeOrigin: true, secure: false }]),
    ),
  },
  build: {
    // Emit the production build straight into the API's wwwroot so a single
    // Azure Web App serves both the SPA and the API.
    outDir: '../server/wwwroot',
    emptyOutDir: true,
  },
});
