# Frontend Documentation

Angular SPA client for Lanka.

---

## Current State

The frontend is functional but still evolving. Core features work, but test coverage is minimal and some areas need polish.

| Area | Status | Notes |
|------|--------|-------|
| Authentication | Working | Login, register, logout via Keycloak |
| Instagram Linking | Working | OAuth flow, callback handling, status display |
| User Profile | Working | View and edit profile, avatar upload |
| Analytics Dashboard | Basic | Charts display data, but limited interactivity |
| Campaign Management | In Progress | Brand side partially implemented |
| Blogger Features | In Progress | Search, offers, reviews partially done |
| Test Coverage | Minimal | Infrastructure exists, few actual tests |
| i18n | Working | English and Ukrainian translations |

---

## Technology Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| Angular | 20.1 | Framework |
| Angular Material | 20.1 | UI components (forms, dialogs, navigation) |
| PrimeNG | 20.0 | Data-heavy widgets (tables, charts) |
| PrimeFlex | 4.0 | CSS utility classes |
| SignalR | 8.0 | Real-time updates from backend |
| ngx-translate | 16.0 | Runtime internationalization |
| chart.js / echarts | 4.5 / 5.6 | Analytics visualizations |
| TypeScript | 5.8 | Language |
| Node.js | 22.x | Build environment |

---

## Project Structure

```
client/lanka-client/
├── src/
│   ├── app/
│   │   ├── core/              # Singleton services, guards, interceptors
│   │   │   ├── api/           # API client services (*-agent.ts)
│   │   │   ├── guards/        # Route guards (auth, instagram-linked)
│   │   │   ├── interceptors/  # HTTP interceptors (error handling)
│   │   │   ├── layouts/       # Main layout, auth layout
│   │   │   └── services/      # Cross-cutting services
│   │   ├── features/          # Feature modules
│   │   │   ├── auth/          # Login, register, logout
│   │   │   ├── analytics/     # Instagram analytics dashboards
│   │   │   ├── bloggers/      # Blogger-specific features
│   │   │   ├── brands/        # Brand/campaign creation
│   │   │   ├── link-instagram/# OAuth callback handling
│   │   │   └── settings/      # Profile settings
│   │   ├── shared/            # Reusable components
│   │   └── app.routes.ts      # Route definitions
│   ├── assets/
│   │   └── i18n/              # Translation files (en.json, uk.json)
│   ├── environments/          # Environment configuration
│   └── styles.css             # Global styles
├── ssl/                       # Development SSL certificates
├── angular.json               # Angular CLI configuration
└── package.json
```

---

## Quick Start

```bash
cd client/lanka-client

# Install dependencies
npm install

# Start dev server (HTTPS on localhost:4200)
npm start

# Run linter
npm run lint

# Run tests
npm test

# Production build
npm run build -- --configuration production
```

**Prerequisites:** Node.js 22.x

---

## Development Notes

### API Communication

API clients are in `core/api/`. Each agent handles requests to a specific backend module:
- `users-agent.ts` — Authentication, profile
- `analytics-agent.ts` — Instagram statistics
- `campaigns-agent.ts` — Campaign operations

Base URL comes from environment files. Development points to `https://localhost:4308` (the gateway).

### Authentication Flow

1. User logs in via `auth/login`
2. Keycloak returns tokens
3. Tokens stored in service, attached to requests via interceptor
4. Guards protect routes requiring authentication

### Instagram Linking

1. User clicks "Connect Instagram"
2. Redirect to Instagram OAuth page
3. Instagram redirects back to `/link-instagram/callback` with code
4. Frontend sends code to backend
5. SignalR provides real-time status updates
6. UI updates when linking completes or fails

### Real-time Updates

SignalR connection established on app init. Used for:
- Instagram linking status
- (Future) Notifications, campaign updates

### Internationalization

Translations in `src/assets/i18n/`. Use the `translate` pipe in templates:

```html
<span>{{ 'common.save' | translate }}</span>
```

Add new keys to both `en.json` and `uk.json`.

---

## Architecture Decisions

### Standalone Components

All components are standalone (Angular 14+ pattern). No NgModules for features — imports declared directly in components.

### Two UI Libraries

We use both Angular Material and PrimeNG:
- **Material** for forms, dialogs, basic layout
- **PrimeNG** for data tables, advanced charts, complex widgets

This adds bundle size but provides good coverage of UI needs. In hindsight, picking one might have been simpler.

### Guards and Resolvers

Routes use guards for access control and resolvers for data preloading:
- `authGuard` — Requires authentication
- `unauthGuard` — Redirects authenticated users (login page)
- `instagramLinkedGuard` — Requires linked Instagram account
- `ProfileResolver` — Preloads user profile data

---

## Known Issues and Limitations

1. **Test coverage is low** — Infrastructure is set up, but most components lack tests
2. **Error handling varies** — Some areas show friendly errors, others don't
3. **Bundle size** — Using two UI libraries increases initial load
4. **No offline support** — Requires network connection
5. **Some features incomplete** — Campaign management, reviews need more work

---

## Configuration

### Environment Files

```typescript
// src/environments/environment.development.ts
export const environment = {
  apiUrl: 'https://localhost:4308',
  instagramClientId: 'your-client-id',
  // ...
};
```

Production environment should be created separately with real values.

### SSL for Development

Dev server uses HTTPS. Certificates in `ssl/` folder. If you get SSL errors:

```bash
# Generate new self-signed cert (macOS/Linux)
openssl req -x509 -newkey rsa:2048 -keyout ssl/ssl.key -out ssl/ssl.crt -days 365 -nodes
```

---

## Linting

ESLint with Angular-specific rules. Key rules enforced:
- `app-` prefix for component selectors
- No implicit `any`
- No unused variables
- Explicit member accessibility

```bash
# Check
npm run lint

# Auto-fix where possible
npm run lint -- --fix
```

---

## Building for Production

```bash
npm run build -- --configuration production
```

Output in `dist/lanka-client/`. Can be served by any static file server that supports SPA routing (serve `index.html` for unknown routes).

---

## What's Missing (Future Work)

- Comprehensive test coverage
- E2E tests
- PWA / offline support
- Performance optimization (lazy loading already in place, but more could be done)
- Accessibility audit and fixes
- Better error boundaries

---

## Related Documentation

- [Quick Start](../development/quick-start.md) — Full environment setup including frontend
- [Architecture Overview](../architecture/README.md) — How frontend fits with backend
