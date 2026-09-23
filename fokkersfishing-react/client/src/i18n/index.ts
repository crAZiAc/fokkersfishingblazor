import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import dayjs from 'dayjs';
import localizedFormat from 'dayjs/plugin/localizedFormat';
import 'dayjs/locale/nl';
import 'dayjs/locale/en-gb';

dayjs.extend(localizedFormat);

const STORAGE_KEY = 'fokkers.lang';

// Keep dayjs (used by the date/time pickers) in sync with the UI language.
function syncDayjs(lang: string) {
  dayjs.locale(lang === 'nl' ? 'nl' : 'en-gb');
}

/** BCP-47 locale for Intl/toLocale* formatting, derived from the UI language. */
export function currentLocale(): string {
  const lang = i18n.resolvedLanguage ?? i18n.language ?? 'nl';
  return lang === 'nl' ? 'nl-NL' : 'en-GB';
}

export const SUPPORTED_LANGS = ['nl', 'en'] as const;
export type Lang = (typeof SUPPORTED_LANGS)[number];

function initialLang(): Lang {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === 'nl' || stored === 'en') return stored;
  } catch {
    /* ignore */
  }
  return 'nl'; // default to Dutch
}

const en = {
  common: {
    save: 'Save', cancel: 'Cancel', delete: 'Delete', edit: 'Edit', add: 'Add',
    close: 'Close', download: 'Download', all: 'All', actions: 'Actions',
    logIn: 'Log in', register: 'Register', signOut: 'Sign out', resetPassword: 'Reset password',
    noData: 'No data.',
  },
  lang: { label: 'Language', nl: 'Nederlands', en: 'English' },
  nav: {
    home: 'Home', leaderboard: 'Leaderboard', myCatches: 'My Catches',
    teamLeaderboard: 'Team Leaderboard', competitionLeads: 'Competition Leads',
    users: 'Users', teams: 'Teams', allCatches: 'All Catches',
    pendingCatches: 'Pending Catches', competitions: 'Competitions',
  },
  auth: {
    loginTitle: 'FVD 2026 — Log in', registerTitle: 'Create an account',
    email: 'Email', password: 'Password', displayName: 'Display name',
    passwordHelper: 'At least 6 chars, with upper, lower and a digit.',
    or: 'or', continueWith: 'Continue with {{provider}}',
    noAccount: 'No account?', haveAccount: 'Already have an account?',
    loginFailed: 'Login failed. Check your credentials.',
    registrationFailed: 'Registration failed.',
    signingIn: 'Signing you in…', externalFailed: 'External sign-in failed ({{error}}).',
    noToken: 'No token received.',
  },
  home: {
    addNewCatch: 'Add new catch',
    competitionPrefix: 'Fokkers Competition: {{name}}',
    ended: 'Competition ended.',
    stats: 'Catches made: {{count}}. Total length caught: {{length}} cm',
    notStarted: 'Competition has not started',
    startsIn: 'Starts in {{days}} days, {{hours}} hours, {{minutes}} minutes',
    activeNote: 'Competition active. Top-catches overview is disabled during the competition.',
    endsIn: 'Ends in {{days}} days, {{hours}} hours, {{minutes}} minutes',
    bigThree: 'Big Three', place: '#{{n}} — {{name}}', totalLength: 'Total length: {{length}} cm',
    topFishermen: 'Top Fishermen', fisherman: 'Fisherman',
    totalFishLength: 'Total Fish Length', totalFishCaught: 'Total Fish Caught',
    pike: 'Pike', bass: 'Bass', zander: 'Zander',
  },
  catches: {
    fish: 'Fish', fisherman: 'Fisherman', catchNo: 'Catch #', catchDate: 'Catch Date',
    lengthCm: 'Length (cm)', team: 'Team', catchCol: 'Catch', measureCol: 'Measure',
    statusActions: 'Status / Actions', fullEdit: 'Full edit', caughtInCompetition: 'Caught in competition',
    noCatches: 'No catches.',
    status: { approved: 'Approved', pending: 'Pending', rejected: 'Rejected', unknown: 'Unknown' },
  },
  pagination: {
    rowsPerPage: 'Rows per page:',
    displayedRows: '{{from}}–{{to}} of {{count}}',
    displayedRowsMany: '{{from}}–{{to}} of more than {{to}}',
  },
  editCatch: {
    titleNew: '(new)', catchNo: 'Catch #', logDate: 'Log date', catchDateTime: 'Catch date and time',
    fishLength: 'Fish length (cm)', fishType: 'Fish type', fisherman: 'Fisherman', registerUser: '(register user)',
    photos: 'Photos', measurePhoto: 'Measure photo', catchPhoto: 'Catch photo',
    clearPhotos: 'Clear existing photos', status: 'Status', title: 'Catch #{{n}}',
  },
  photo: { none: 'No photo available.', title: 'Photo' },
  myCatches: {
    title: 'My Catches', onlyCompetition: 'Only showing catches in this competition.',
    deleteTitle: 'Delete catch', deleteMsg: 'Do you want to delete this catch?',
    saved: 'Catch saved', saveFailed: 'Saving catch failed', deleted: 'Catch deleted',
    deleteFailed: 'Delete failed', loadFailed: 'Failed to load catches.',
  },
  pending: { title: 'Pending Catches', updated: 'Catch updated', updateFailed: 'Update failed', loadFailed: 'Failed to load pending catches.' },
  adminCatches: { title: 'All Catches' },
  leaderboard: {
    title: 'Leaderboard', topCatches: 'Top Catches', mustLogin: 'Log in to view the competition leaderboard.',
    teamTitle: '{{name}} — Team Leaderboard', teamRanking: 'Team Ranking', rank: 'Rank', team: 'Team',
    score: 'Score', big3: 'Big 3', teamBigThree: 'Team Big Three', position: 'Position',
    totalCm: 'Total (cm)', noTeamData: 'No team data yet.', status: 'Status', loadFailed: 'Failed to load leaderboard.',
  },
  leaderboardComp: { title: 'Fokkers Competition Leaderboard', topFishermen: 'Top Fishermen', noActive: 'No competition is currently active.', loadFailed: 'Failed to load.' },
  leaderboardTeam: { title: 'Team Leaderboard', fishScores: 'Fish scores', total: 'Total', noActive: 'No competition is currently active.', loadFailed: 'Failed to load team leaderboard.' },
  adminUsers: {
    title: 'Users', userName: 'User Name', email: 'E-mail', loginProvider: 'Login Provider', roles: 'Roles',
    editTitle: 'Edit user', userSaved: 'User saved', saveFailed: 'Save failed',
    deleteTitle: 'Delete user', deleteMsg: 'Do you want to delete {{email}}?', deleted: 'User deleted', deleteFailed: 'Delete failed',
    resetTitle: 'Reset password — {{name}}', newPassword: 'New password', confirmPassword: 'Confirm password',
    passwordsNoMatch: 'Passwords do not match.', reset: 'Reset', passwordReset: 'Password reset', passwordResetFailed: 'Password reset failed',
    loadFailed: 'Failed to load users.',
  },
  adminCompetitions: {
    title: 'Competitions', addNew: 'Add new competition', name: 'Name', start: 'Start', end: 'End',
    active: 'Active', showLbAfterEnd: 'Show leaderboard after end', competitionName: 'Competition name',
    startDate: 'Start date', endDate: 'End date', showLbAfterEndFull: 'Show leaderboard after competition has ended',
    newComp: 'New competition', editComp: 'Edit competition', saved: 'Competition saved', saveFailed: 'Save failed',
    deleteTitle: 'Delete competition', deleteMsg: 'Do you want to delete this competition?', deleted: 'Competition deleted', deleteFailed: 'Delete failed',
    loadFailed: 'Failed to load competitions.',
  },
  adminTeams: {
    title: 'Teams', addNew: 'Add new team', teamName: 'Team Name', description: 'Description', members: 'Members',
    noMembers: 'No members', addMember: 'Add member', addMemberTitle: 'Add member to {{team}}', user: 'User',
    newTeam: 'New team', editTeam: 'Edit team', saved: 'Team saved', saveFailed: 'Save failed',
    memberAdded: 'Member added', addMemberFailed: 'Add member failed', memberRemoved: 'Member removed', removeMemberFailed: 'Remove member failed',
    deleteTitle: 'Delete team', deleteMsg: 'Do you want to delete this team?', deleted: 'Team deleted', deleteFailed: 'Delete failed',
    loadFailed: 'Failed to load teams.',
  },
};

const nl: typeof en = {
  common: {
    save: 'Opslaan', cancel: 'Annuleren', delete: 'Verwijderen', edit: 'Bewerken', add: 'Toevoegen',
    close: 'Sluiten', download: 'Downloaden', all: 'Alle', actions: 'Acties',
    logIn: 'Inloggen', register: 'Registreren', signOut: 'Uitloggen', resetPassword: 'Wachtwoord opnieuw instellen',
    noData: 'Geen gegevens.',
  },
  lang: { label: 'Taal', nl: 'Nederlands', en: 'English' },
  nav: {
    home: 'Home', leaderboard: 'Klassement', myCatches: 'Mijn vangsten',
    teamLeaderboard: 'Teamklassement', competitionLeads: 'Wedstrijdklassement',
    users: 'Gebruikers', teams: 'Teams', allCatches: 'Alle vangsten',
    pendingCatches: 'Vangsten in behandeling', competitions: 'Wedstrijden',
  },
  auth: {
    loginTitle: 'FVD 2026 — Inloggen', registerTitle: 'Account aanmaken',
    email: 'E-mail', password: 'Wachtwoord', displayName: 'Weergavenaam',
    passwordHelper: 'Minimaal 6 tekens, met hoofdletter, kleine letter en een cijfer.',
    or: 'of', continueWith: 'Doorgaan met {{provider}}',
    noAccount: 'Nog geen account?', haveAccount: 'Heb je al een account?',
    loginFailed: 'Inloggen mislukt. Controleer je gegevens.',
    registrationFailed: 'Registreren mislukt.',
    signingIn: 'Je wordt ingelogd…', externalFailed: 'Externe aanmelding mislukt ({{error}}).',
    noToken: 'Geen token ontvangen.',
  },
  home: {
    addNewCatch: 'Nieuwe vangst toevoegen',
    competitionPrefix: 'Fokkers Competition: {{name}}',
    ended: 'Wedstrijd beëindigd.',
    stats: 'Vangsten: {{count}}. Totale lengte gevangen: {{length}} cm',
    notStarted: 'Wedstrijd is nog niet begonnen',
    startsIn: 'Begint over {{days}} dagen, {{hours}} uur, {{minutes}} minuten',
    activeNote: 'Wedstrijd actief. Het overzicht met topvangsten is uitgeschakeld tijdens de wedstrijd.',
    endsIn: 'Eindigt over {{days}} dagen, {{hours}} uur, {{minutes}} minuten',
    bigThree: 'Big Three', place: '#{{n}} — {{name}}', totalLength: 'Totale lengte: {{length}} cm',
    topFishermen: 'Topvissers', fisherman: 'Visser',
    totalFishLength: 'Totale vislengte', totalFishCaught: 'Totaal aantal vissen',
    pike: 'Snoek', bass: 'Baars', zander: 'Snoekbaars',
  },
  catches: {
    fish: 'Vis', fisherman: 'Visser', catchNo: 'Vangst #', catchDate: 'Vangstdatum',
    lengthCm: 'Lengte (cm)', team: 'Team', catchCol: 'Vangst', measureCol: 'Meting',
    statusActions: 'Status / Acties', fullEdit: 'Volledig bewerken', caughtInCompetition: 'Gevangen in wedstrijd',
    noCatches: 'Geen vangsten.',
    status: { approved: 'Goedgekeurd', pending: 'In behandeling', rejected: 'Afgekeurd', unknown: 'Onbekend' },
  },
  pagination: {
    rowsPerPage: 'Rijen per pagina:',
    displayedRows: '{{from}}–{{to}} van {{count}}',
    displayedRowsMany: '{{from}}–{{to}} van meer dan {{to}}',
  },
  editCatch: {
    titleNew: '(nieuw)', catchNo: 'Vangst #', logDate: 'Registratiedatum', catchDateTime: 'Vangstdatum en -tijd',
    fishLength: 'Vislengte (cm)', fishType: 'Vissoort', fisherman: 'Visser', registerUser: '(registrerende gebruiker)',
    photos: 'Foto’s', measurePhoto: 'Metingsfoto', catchPhoto: 'Vangstfoto',
    clearPhotos: 'Bestaande foto’s wissen', status: 'Status', title: 'Vangst #{{n}}',
  },
  photo: { none: 'Geen foto beschikbaar.', title: 'Foto' },
  myCatches: {
    title: 'Mijn vangsten', onlyCompetition: 'Alleen vangsten in deze wedstrijd worden getoond.',
    deleteTitle: 'Vangst verwijderen', deleteMsg: 'Wil je deze vangst verwijderen?',
    saved: 'Vangst opgeslagen', saveFailed: 'Opslaan van vangst mislukt', deleted: 'Vangst verwijderd',
    deleteFailed: 'Verwijderen mislukt', loadFailed: 'Vangsten laden mislukt.',
  },
  pending: { title: 'Vangsten in behandeling', updated: 'Vangst bijgewerkt', updateFailed: 'Bijwerken mislukt', loadFailed: 'Laden van vangsten in behandeling mislukt.' },
  adminCatches: { title: 'Alle vangsten' },
  leaderboard: {
    title: 'Klassement', topCatches: 'Topvangsten', mustLogin: 'Log in om het wedstrijdklassement te bekijken.',
    teamTitle: '{{name}} — Teamklassement', teamRanking: 'Teamklassement', rank: 'Plaats', team: 'Team',
    score: 'Score', big3: 'Big 3', teamBigThree: 'Team Big Three', position: 'Positie',
    totalCm: 'Totaal (cm)', noTeamData: 'Nog geen teamgegevens.', status: 'Status', loadFailed: 'Klassement laden mislukt.',
  },
  leaderboardComp: { title: 'Fokkers wedstrijdklassement', topFishermen: 'Topvissers', noActive: 'Er is momenteel geen actieve wedstrijd.', loadFailed: 'Laden mislukt.' },
  leaderboardTeam: { title: 'Teamklassement', fishScores: 'Visscores', total: 'Totaal', noActive: 'Er is momenteel geen actieve wedstrijd.', loadFailed: 'Teamklassement laden mislukt.' },
  adminUsers: {
    title: 'Gebruikers', userName: 'Gebruikersnaam', email: 'E-mail', loginProvider: 'Aanmeldprovider', roles: 'Rollen',
    editTitle: 'Gebruiker bewerken', userSaved: 'Gebruiker opgeslagen', saveFailed: 'Opslaan mislukt',
    deleteTitle: 'Gebruiker verwijderen', deleteMsg: 'Wil je {{email}} verwijderen?', deleted: 'Gebruiker verwijderd', deleteFailed: 'Verwijderen mislukt',
    resetTitle: 'Wachtwoord opnieuw instellen — {{name}}', newPassword: 'Nieuw wachtwoord', confirmPassword: 'Bevestig wachtwoord',
    passwordsNoMatch: 'Wachtwoorden komen niet overeen.', reset: 'Opnieuw instellen', passwordReset: 'Wachtwoord opnieuw ingesteld', passwordResetFailed: 'Wachtwoord opnieuw instellen mislukt',
    loadFailed: 'Gebruikers laden mislukt.',
  },
  adminCompetitions: {
    title: 'Wedstrijden', addNew: 'Nieuwe wedstrijd toevoegen', name: 'Naam', start: 'Start', end: 'Einde',
    active: 'Actief', showLbAfterEnd: 'Klassement tonen na einde', competitionName: 'Wedstrijdnaam',
    startDate: 'Startdatum', endDate: 'Einddatum', showLbAfterEndFull: 'Klassement tonen nadat de wedstrijd is beëindigd',
    newComp: 'Nieuwe wedstrijd', editComp: 'Wedstrijd bewerken', saved: 'Wedstrijd opgeslagen', saveFailed: 'Opslaan mislukt',
    deleteTitle: 'Wedstrijd verwijderen', deleteMsg: 'Wil je deze wedstrijd verwijderen?', deleted: 'Wedstrijd verwijderd', deleteFailed: 'Verwijderen mislukt',
    loadFailed: 'Wedstrijden laden mislukt.',
  },
  adminTeams: {
    title: 'Teams', addNew: 'Nieuw team toevoegen', teamName: 'Teamnaam', description: 'Omschrijving', members: 'Leden',
    noMembers: 'Geen leden', addMember: 'Lid toevoegen', addMemberTitle: 'Lid toevoegen aan {{team}}', user: 'Gebruiker',
    newTeam: 'Nieuw team', editTeam: 'Team bewerken', saved: 'Team opgeslagen', saveFailed: 'Opslaan mislukt',
    memberAdded: 'Lid toegevoegd', addMemberFailed: 'Lid toevoegen mislukt', memberRemoved: 'Lid verwijderd', removeMemberFailed: 'Lid verwijderen mislukt',
    deleteTitle: 'Team verwijderen', deleteMsg: 'Wil je dit team verwijderen?', deleted: 'Team verwijderd', deleteFailed: 'Verwijderen mislukt',
    loadFailed: 'Teams laden mislukt.',
  },
};

void i18n
  .use(initReactI18next)
  .init({
    resources: { en: { translation: en }, nl: { translation: nl } },
    lng: initialLang(),
    fallbackLng: 'en',
    interpolation: { escapeValue: false },
  });

syncDayjs(i18n.language);

export function setLanguage(lang: Lang) {
  void i18n.changeLanguage(lang);
  syncDayjs(lang);
  try {
    localStorage.setItem(STORAGE_KEY, lang);
  } catch {
    /* ignore */
  }
}

export default i18n;
