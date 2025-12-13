# 🖥️ Lanka Frontend Guide

<div align="center">

*Angular SPA that powers the Lanka experience*

![Angular](https://img.shields.io/badge/Angular-20.1-ff1f2d?style=flat-square&logo=angular&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5.8-3178c6?style=flat-square&logo=typescript&logoColor=white)
![Node.js](https://img.shields.io/badge/Node.js-22.x-339933?style=flat-square&logo=node.js&logoColor=white)
![Lint](https://img.shields.io/badge/Lint-ESLint%209%20%2B%20angular--eslint%2020-4b32c3?style=flat-square&logo=eslint&logoColor=white)

</div>

---

## 📦 Stack at a Glance

| Area | Details |
|------|---------|
| Framework | Angular 20.1 (CLI @ 20.1.3) |
| UI Kits | Angular Material 20.1, PrimeNG 20.0 + PrimeFlex 4 |
| i18n | `@ngx-translate` 16 with HTTP loaders |
| Realtime | SignalR client 8.0 |
| Lint/Test | ESLint 9 + angular-eslint 20, Karma/Jasmine |
| Node | Targeted Node 22.x (used in CI) |

---

## 🗂️ Project Layout

- Location: `client/lanka-client`
- Source: `src/` (standalone components), styles in `src/styles.css`
- Environments: `src/environments/environment.development.ts` (gateway defaults to `https://localhost:4308`)
- Assets: `src/assets/i18n/*.json` for translations, `public/` for static files

---

## 🧱 Structure & Routing

- **App shell**: `core/layouts/main-layout` (authenticated shell) and `core/layouts/auth-layout` (login/register). Shared chrome lives under `core/components`.
- **Routing**: `app.routes.ts` uses guards (`authGuard`, `unauthGuard`, `instagramLinkedGuard`) and a `ProfileResolver`. Auth area under `/auth`, main area under `/` with lazy modules for `offers`, `reviews`, `analytics`, `settings/profile`, `calendar`, and `campaigns/*`.
- **Feature map** (see `src/app/features`):
  - Auth (`auth/`) — login/register/logout.
  - Bloggers (`bloggers/`) — search, campaigns, offers, pact, reviews, profile, public profile.
  - Brands (`brands/`) — campaign creation.
  - Analytics (`analytics/`) — charts/dashboards.
  - Link Instagram (`link-instagram/`) — connect/renew callbacks.
  - Settings, Calendar, FAQ, Privacy Policy.
- **Shared**: reusable UI in `shared/components` (loading, server-error, banners, language switcher, etc.).
- **Core services**: API agents in `core/api/*`, guards in `core/guards`, interceptors (e.g., `error.interceptor.ts`), and cross-cutting services under `core/services`.

---

## 🚀 Local Development

```bash
# From repo root
cd client/lanka-client

# Install dependencies (lockfile is gitignored, CI also uses npm install)
npm install

# Dev server with HTTPS (uses ssl/ssl.key & ssl/ssl.crt)
npm start

# Lint
npm run lint

# Unit tests
npm test

# Production build
npm run build -- --configuration production
```

---

## 🔧 Script Reference

| Command | What it does |
|---------|--------------|
| `npm start` | Runs `ng serve` with HTTPS using `ssl/ssl.key` and `ssl/ssl.crt`. |
| `npm run build -- --configuration production` | Production build to `dist/lanka-client` with hashing/budgets. |
| `npm run lint` | ESLint (typescript + angular-eslint) with template linting. |
| `npm run lint -- --fix` | Applies autofixes where possible. |
| `npm test` | Karma + Jasmine unit tests. |

---

## 🏗️ Build & CI Notes

- Build output: `client/lanka-client/dist/lanka-client`
- GitHub Actions frontend job (see `.github/workflows/build.yml`) uses Node 22, runs `npm install`, `npm run lint`, then `npm run build -- --configuration production`, and publishes the `frontend-dist` artifact.
- Keep SSL dev certs in `client/lanka-client/ssl` if you run `npm start` with HTTPS. Update the Angular `serve` options in `angular.json` if you change paths.

---

## 📚 Key Libraries

- Angular Material for form/layout primitives and PrimeNG for data-heavy widgets.
- `@ngx-translate` for runtime translations; update locale files in `src/assets/i18n/`.
- SignalR client for realtime updates from the backend gateway.
- Charting: `chart.js` and `echarts` for analytics visualizations.

---

## 🧹 Lint & Coding Standards

- ESLint with `angular-eslint` enforces `app-` selectors (`kebab-case` components, `camelCase` directives), lifecycle interfaces, and template accessibility checks.
- TypeScript rules: no implicit `any`, explicit member accessibility, member ordering, no unused vars, `no-console` except `warn`/`error`, single quotes, `prefer-const`.
- Templates linted via inline processor; fix most issues with `npm run lint -- --fix`, then adjust remaining type or selector errors manually.

---

## 🎨 Styling & UX

- Component-scoped CSS by default; global styles and variables live in `src/styles.css` alongside PrimeFlex utilities.
- Use Angular Material for form/layout primitives; PrimeNG for data-heavy widgets (tables, charts, menus) to stay consistent.
- Reuse shared components from `src/app/shared/components` before adding new chrome; keep translations in templates via the `translate` pipe.

---

## ✅ Quick Checklist

- [ ] Node 22.x installed
- [ ] Dependencies installed with `npm install`
- [ ] Lint clean: `npm run lint`
- [ ] Tests green: `npm test`
- [ ] Production build succeeds: `npm run build -- --configuration production`

---

## 🔧 Environments & Config

- Dev gateway is `https://localhost:4308` (set in `src/environments/environment.development.ts`).
- If you introduce additional environments, mirror the shape of the dev file and wire `fileReplacements` in `angular.json`.
- Dev HTTPS uses `ssl/ssl.key` and `ssl/ssl.crt`; update the `serve` options if you relocate or regenerate them.
- API base URL is read at build time—bump envs before building prod bundles.
- Instagram client/config IDs live in the dev env file for local flows; keep secrets out of version control for other environments.
- Angular compiler is in strict mode with strict templates and injectors; keep types tight to avoid runtime surprises.

---

## 🧭 Developer Workflow

- Package manager: `npm install` (no lockfile committed; CI mirrors this).
- Lint autofix: `npm run lint -- --fix` to resolve formatting/import issues quickly.
- Formatting: rely on Angular/TypeScript defaults via ESLint rules; avoid ad-hoc formatters that diverge from CI.
- i18n: add keys to `src/assets/i18n/en.json` and `uk.json`; keep keys consistent and prefer reusing existing namespaces.
- Testing: `npm test` runs Karma/Jasmine; keep new components covered with focused specs.

---

## 🔌 API & Data Flow

- API clients live under `core/api/*agent.ts` and use the gateway URL from environment files.
- `error.interceptor.ts` centralizes handling of HTTP errors; prefer propagating friendly messages via `FriendlyErrorService`.
- Guards: `authGuard`/`unauthGuard` protect auth flows; `instagramLinkedGuard` enforces connection before accessing certain routes.
- `ProfileResolver` preloads profile data for profile/settings routes to render faster and reduce duplicate calls.
- SignalR integration is wrapped in `core/services/signalr.service.ts` for realtime updates.

---

## 🎛️ UI Conventions

- Standalone components are the default; feature-specific components live under their feature folder, shared chrome under `shared/components`.
- Component selectors follow lint rules (`app-*`); keep inputs/outputs typed and prefer `readonly` where possible.
- Routes carry `data.titleKey` for i18n page titles—continue this pattern when adding routes.
- Favor resolver/guard + agent patterns for data preloading and access control instead of ad-hoc in-component calls.
- Avoid duplicating layout chrome; extend `main-layout`/`auth-layout` or shared primitives instead.

---

## 🛠️ Troubleshooting

- **Node version mismatch**: ensure Node 22.x; use `nvm use 22` or install from nodejs.org if CLI errors on startup.
- **SSL errors on `npm start`**: recreate cert/key in `ssl/` or point `angular.json > serve.options` to your files.
- **Lint fails on unused imports/any**: run `npm run lint -- --fix` and adjust types; CI treats lint warnings as failures.
- **Build timeouts**: clear Angular cache `npm run ng -- cache clean` and retry; ensure enough RAM when running alongside Docker infra.
- **Node-gyp / native deps**: if native builds fail, ensure build tools are installed (Xcode CLT on macOS, build-essentials on Linux, VS Build Tools on Windows).

---

## 📦 Release & Artifacts

- Production build emits to `dist/lanka-client`; CI uploads this as `frontend-dist`.
- Artifacts are ready for static hosting behind the gateway or any SPA-friendly web server (serves `index.html` for unknown routes).
- If you change the base href or deploy under a sub-path, update `angular.json > build.options.baseHref` accordingly before building.
