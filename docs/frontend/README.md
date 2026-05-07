# Frontend Documentation

Angular SPA client for Lanka.

---

## Current State

The frontend is functional but still evolving. Core collaboration workflows now work end-to-end, but test coverage is minimal and some areas still need polish.

| Area | Status | Notes |
|------|--------|-------|
| Authentication | Working | Login, register, logout via Keycloak |
| Instagram Linking | Working | OAuth flow, callback handling, status display |
| User Profile | Working | View and edit profile, avatar upload |
| Analytics Dashboard | Basic | Charts display data, but limited interactivity |
| Blogger Discovery | Working | Advanced search with filters, similar bloggers, sorting |
| Campaign Management | Working | Calendar/list views, role filters, reports, hosted payment initiation |
| Chats | Working | Global inbox plus offer/campaign-scoped conversations |
| Notifications | Working | Notification bell with read state and campaign navigation |
| Blogger Features | Working | Pacts, offers, reviews, public profile collaboration actions |
| Payout Settings | Working | Creator IBAN/currency capture with validation |
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
│   │   │   └── services/      # Cross-cutting services (incl. SearchService)
│   │   ├── features/          # Feature modules
│   │   │   ├── auth/          # Login, register, logout
│   │   │   ├── analytics/     # Instagram analytics dashboards
│   │   │   ├── bloggers/      # Blogger-specific features
│   │   │   ├── brands/        # Brand/campaign creation
│   │   │   ├── chats/         # Chat inbox and message thread
│   │   │   ├── link-instagram/# OAuth callback handling
│   │   │   ├── search/        # Blogger discovery & search
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
- `chat-agent.ts` — Chat threads and messages
- `notifications-agent.ts` — Notification inbox/read state
- `search-agent.ts` — Search, suggestions, similar bloggers
- `bloggers-agent.ts` — Profiles and payout account settings

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

### Blogger Discovery (Search)

The search page (`features/search/`) provides a full-featured blogger discovery experience backed by Elasticsearch:

**Search Bar** — PrimeNG `AutoComplete` with server-side suggestions (debounced, min 2 chars). Submit via button or Enter key.

**Filter Bar** — Six dropdown filters, all optional:
- Category (38 categories)
- Follower range (1K-10K, 10K-50K, 50K-100K, 100K-500K, 500K+)
- Engagement rate range (<1%, 1-3%, 3-5%, 5-8%, 8%+)
- Audience country (US, GB, UA, DE, FR, BR, IN, CA, AU, JP)
- Audience gender (Male, Female)
- Audience age group (13-17, 18-24, 25-34, 35-44, 45-54, 55-64, 65+)

**Results Table** — PrimeNG `p-table` with:
- Client-side column sorting (Followers, ER%, Posts, Category)
- Column tooltips explaining each metric
- Self-exclusion (current user never appears in results)
- Click-to-navigate to blogger profile

**Pagination** — Custom page navigation with ellipsis gaps for large result sets.

**Similar Bloggers** — After search, Elasticsearch More Like This (MLT) finds bloggers similar to the top result, displayed as cards below the table.

**State Management** — `SearchService` (singleton) manages search state via `BehaviorSubject`. State includes query, filters, page, results, loading, and error. Each search cancels any in-flight request.

### Campaigns

The campaigns page supports two views:

**Calendar view** — Three-month rolling calendar, upcoming campaign detail panel, role filtering, status filtering, and status-specific actions.

**List view** — Dense table view with search/status filters, role filters, action-required filtering, pagination, and inline campaign actions.

Campaign details include participants, status timeline, key dates, reference IDs, work report, payment state, and contextual actions. A client sees `Pay Now` for a campaign in `Done` state; payment opens a hosted WayForPay checkout by posting the signed fields returned by the API.

### Chats

The chat experience is available from the main nav and from public blogger offer rows. A chat thread can be:
- offer-scoped before a campaign exists;
- campaign-scoped after collaboration starts;
- populated with system messages from campaign lifecycle events.

The thread UI supports paged message history, sending, editing, deleting, read state, profile navigation, and SignalR-driven live updates.

### Notifications

The notification bell loads the current user's notification inbox, displays unread count, and links notifications back to the related campaign. Users can mark one notification or all notifications as read. Campaign lifecycle events create notification records on the backend.

### Public Blogger Profile

Public profiles expose:
- analytics overview and recent Instagram posts;
- average offer prices;
- pact content and offers;
- offer chat action for pre-campaign negotiation;
- campaign proposal action for starting a formal campaign;
- public calendar that shows the blogger's occupied campaign dates without exposing private campaign details.

### Settings

Profile settings now include creator payout account fields. The payout account stores IBAN and payout currency and is used by the campaign/payment workflow as creator payout metadata. Currency changes are blocked while active creator campaigns exist.

### Real-time Updates

SignalR connection established on app init. Used for:
- Instagram linking status
- Campaign notifications
- Chat messages, edits, deletes, and read state

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
5. **Payment provider scope** — WayForPay checkout is sufficient for local demo/development, but marketplace-style creator payouts are not automated by the current implementation

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
