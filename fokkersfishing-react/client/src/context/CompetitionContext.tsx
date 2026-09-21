import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { api } from '../api/client';
import type { Competition } from '../api/types';

interface CompetitionState {
  loading: boolean;
  active: boolean;
  competitionId: string;
  competitionName: string;
  showLeaderboardAfterCompetitionEnds: boolean;
  startDate: Date | null;
  endDate: Date | null;
  /** True once the end date has passed. */
  competitionEnded: boolean;
  /** True while the competition has NOT started yet (matches the original "CompetitionStarted" flag semantics). */
  competitionNotStarted: boolean;
  timeTillEnd: number; // ms
  timeTillStart: number; // ms
}

const EMPTY_ID = '00000000-0000-0000-0000-000000000000';

const CompetitionContext = createContext<CompetitionState | undefined>(undefined);

export function CompetitionProvider({ children }: { children: ReactNode }) {
  const [competition, setCompetition] = useState<Competition | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const { data } = await api.get<Competition[]>('/competition');
        const active = data.find((c) => c.active) ?? null;
        if (!cancelled) setCompetition(active);
      } catch {
        if (!cancelled) setCompetition(null);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const value = useMemo<CompetitionState>(() => {
    const now = Date.now();
    const start = competition ? new Date(competition.startDate) : null;
    const end = competition ? new Date(competition.endDate) : null;
    return {
      loading,
      active: !!competition?.active,
      competitionId: competition?.id ?? EMPTY_ID,
      competitionName: competition?.competitionName ?? '',
      showLeaderboardAfterCompetitionEnds: !!competition?.showLeaderboardAfterCompetitionEnds,
      startDate: start,
      endDate: end,
      competitionEnded: end ? now > end.getTime() : false,
      competitionNotStarted: start ? now < start.getTime() : false,
      timeTillEnd: end ? end.getTime() - now : 0,
      timeTillStart: start ? start.getTime() - now : 0,
    };
  }, [competition, loading]);

  return <CompetitionContext.Provider value={value}>{children}</CompetitionContext.Provider>;
}

export function useCompetition() {
  const ctx = useContext(CompetitionContext);
  if (!ctx) throw new Error('useCompetition must be used within CompetitionProvider');
  return ctx;
}

export function splitDuration(ms: number) {
  const total = Math.max(0, ms);
  const days = Math.floor(total / 86400000);
  const hours = Math.floor((total % 86400000) / 3600000);
  const minutes = Math.floor((total % 3600000) / 60000);
  return { days, hours, minutes };
}
