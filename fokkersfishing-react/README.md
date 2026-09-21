# Fokkers Fishing — Vite + React + MUI (with .NET 8 API)

A full re-implementation of the original Blazor WebAssembly app as a **Vite + React + MUI (TypeScript)** single-page app served by a **.NET 8 Web API**. It reuses:

- the **same Azure Table Storage** tables (`Catches`, `Fish`, `Competitions`, `Teams`, `TeamMembers`) and the `catches` blob container for photos;
- the **same ASP.NET Core Identity (SQLite) accounts** — existing users and password hashes keep working;
- **Google & Microsoft** external login (Facebook was intentionally left out).

IdentityServer/OIDC was replaced with a plain **JWT bearer** flow, and the Windows-only `System.Drawing` thumbnailing with cross-platform **ImageSharp**, so it runs on either Windows or Linux Azure Web Apps.

```
fokkersfishing-react/
  server/   .NET 8 Web API (Table Storage + Identity + JWT). Serves the built SPA from wwwroot.
  client/   Vite + React + MUI + TypeScript SPA.
```

## Prerequisites

- .NET 8 SDK (the project targets `net8.0`)
- Node.js 18+ / npm

## Configuration (server)

Set these via **user-secrets** (dev) or **App Service application settings** (prod). App Service settings use the double-underscore form shown in parentheses.

| Setting | Purpose |
| --- | --- |
| `Storage:ConnectionString` (`Storage__ConnectionString`) | Azure Storage connection string (tables + `catches` blob container). |
| `ConnectionStrings:DefaultConnection` (`ConnectionStrings__DefaultConnection`) | SQLite Identity DB, e.g. `DataSource=app.db`. Point at your existing `app.db` to reuse accounts. |
| `Jwt:Key` (`Jwt__Key`) | Signing key, **32+ chars**. Required in production. |
| `Jwt:Issuer`, `Jwt:Audience` | Optional issuer/audience validation. |
| `Authentication:Google:ClientId` / `ClientSecret` | Enables the "Continue with Google" button. Optional. |
| `Authentication:Microsoft:ClientId` / `ClientSecret` | Enables the "Continue with Microsoft" button. Optional. |
| `Api:User` / `Api:Key` | Credentials for the Basic-auth machine data-export endpoints (`/catch/admin/data`, `/team/data`). Optional. |
| `KeyVaultName` | If set (and in Production), secrets are also loaded from that Key Vault via managed identity. |

Example, from `server/`:

```bash
dotnet user-secrets init
dotnet user-secrets set "Storage:ConnectionString" "<your-azure-storage-connection-string>"
dotnet user-secrets set "Jwt:Key" "<a-long-random-32+char-secret>"
dotnet user-secrets set "Authentication:Google:ClientId" "<google-client-id>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<google-client-secret>"
```

For the external providers, register the redirect URI `https://<your-host>/signin-google` and `https://<your-host>/signin-microsoft`.

## Run in development

Two terminals:

```bash
# 1) API on http://localhost:5080
cd server
dotnet run
```

```bash
# 2) SPA on http://localhost:5173 (proxies API calls to :5080)
cd client
npm install
npm run dev
```

Open http://localhost:5173. The Vite dev server proxies `/auth`, `/catch`, `/competition`, `/team`, `/leaderboard`, `/fish`, `/photo`, `/adminuser` to the API. Override the API target with `VITE_API_TARGET` if needed.

The first admin: register an account, then grant it the `Administrator` role (either directly in the SQLite `AspNetUserRoles` table, or from the **Users** admin screen once one admin exists).

## Build for production (single Azure Web App)

`npm run build` emits the SPA straight into `server/wwwroot`, which the API serves alongside the API routes (one origin, no CORS):

```bash
cd client && npm run build       # -> ../server/wwwroot
cd ../server && dotnet publish -c Release -o ./publish
```

Deploy the contents of `server/publish` to an Azure Web App (Windows or Linux, .NET 8). `MapFallbackToFile("index.html")` handles client-side routing.

### One-command deploy

`deploy.ps1` builds the client, publishes the API (with the SPA baked into `wwwroot`), zips it, and pushes it to your Web App via the Azure CLI. Run `az login` first, then:

```powershell
./deploy.ps1 -ResourceGroup <resource-group> -AppName <web-app-name>
```

Options: `-Slot <name>` (deployment slot), `-Subscription <id>`, `-SkipClient` (reuse the existing `wwwroot` build), `-SkipServer` (build the package only). It does not create the Web App or set app settings — configure those (see the table above) once, beforehand.

### Visual Studio publish profile

`server/Properties/PublishProfiles/AzureWebApp.pubxml` is a Web Deploy profile for VS's **Publish** dialog. Replace `YOUR-APP-NAME` in it with your Web App name, then right-click the project → **Publish** → pick the profile (it prompts for the deployment password on first use, or download the `.PublishSettings` from the portal and use **Import Profile**). The profile sets `BuildSpaOnPublish=true`, so publishing from VS builds the React client into `wwwroot` and ships client + server together. (`dotnet publish` from the CLI leaves this off by default so it won't rebuild the SPA unless you pass `-p:BuildSpaOnPublish=true`.)

### Notes

- On startup the API ensures the `User` and `Administrator` roles exist and creates the SQLite schema if the DB is new.
- Catch photos are uploaded to the `catches` blob container (created automatically, public-blob access) and referenced by URL, exactly as before.
- The `/swagger` UI is available in Development.
