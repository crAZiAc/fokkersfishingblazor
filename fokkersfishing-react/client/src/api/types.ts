// TypeScript mirrors of the API DTOs. Property names are camelCase to match the
// backend's System.Text.Json (web defaults) serialization.

export enum CatchStatus {
  Approved = 1,
  Pending = 2,
  Rejected = 3,
}

export enum PhotoType {
  Catch = 0,
  Measure = 1,
}

export interface Catch {
  id: string;
  competitionId: string;
  catchNumber: number;
  userEmail: string | null;
  userName: string | null;
  registerUserEmail: string | null;
  registerUserName: string | null;
  fish: string;
  length: number;
  teamName: string | null;
  catchDate: string;
  logDate: string;
  editDate: string;
  globalCatchNumber: number;
  measurePhotoUrl: string | null;
  catchPhotoUrl: string | null;
  measureThumbnailUrl: string | null;
  catchThumbnailUrl: string | null;
  status: CatchStatus;
  caughtInCompetition?: boolean;
}

export interface Competition {
  id: string;
  competitionName: string;
  active: boolean;
  showLeaderboardAfterCompetitionEnds: boolean;
  startDate: string;
  endDate: string;
}

export interface Fish {
  id: string;
  name: string;
  genericName: string;
  kind: string;
  predator: boolean;
  includeInCompetition: boolean;
}

export interface Role {
  name: string;
  isInRole: boolean;
  id: string;
}

export interface User {
  email: string;
  userName: string;
  roles: Role[];
  roleArray: Role[];
  loginProvider: string | null;
  roleList: string;
}

export interface Team {
  id: string;
  name: string;
  description: string;
  users: User[];
}

export interface FisherMan {
  id: string;
  userName: string;
  totalLength: number;
  fishCount: number;
  userEmail: string;
}

export interface BigThree {
  name: string;
  pike: Catch | null;
  bass: Catch | null;
  zander: Catch | null;
  totalLength: number;
}

export interface BigThreeWinner {
  name: string;
  fish: string;
  totalLength: number;
}

export interface Ranking {
  teamName: string;
  rank: number;
  score: number;
  big3: boolean;
}

export interface TeamScore {
  fish: string;
  totalLength: number;
  fishCount: number;
  teamName: string;
  ranking: number;
}

export interface CompetitionStats {
  fishCaught: number;
  totalLength: number;
}

export interface UploadPhotoResponse {
  photoUrl: string;
  thumbnailUrl: string;
}

export interface UserInfo {
  isAuthenticated: boolean;
  name: string | null;
  id: string | null;
  identityProvider: string | null;
  email: string | null;
  roles: string[];
}

export interface AuthResponse {
  token: string;
  user: UserInfo;
}

export interface ExternalProviderInfo {
  name: string;
  displayName: string;
}
