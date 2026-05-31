# 💍 Wedding RSVP Site — Bespoke Guest Links

**Stack:** Angular 17 + .NET 8 Web API + PostgreSQL  
**Hosting:** Netlify (frontend) + Railway (API + DB)

---

## How it works

1. You add guests in the Admin dashboard (one by one or bulk CSV import)
2. Each guest gets a unique token → generates a personal link like `yoursite.com/rsvp/abc123def456`
3. You copy & send each guest their link (WhatsApp, email, etc.)
4. Guest opens link → sees their name, fills out RSVP (with or without +1 based on their allowance)
5. After submitting, the link shows a read-only summary of their response forever
6. You track everything in the Admin dashboard and export to CSV anytime

---

## API Endpoints

### Public (no auth)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/rsvp/{token}` | Load guest details by token |
| POST | `/api/rsvp/{token}` | Submit RSVP |

### Admin (requires `X-Admin-Key` header)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/guests` | List all guests + RSVP status |
| GET | `/api/guests/stats` | Summary counts |
| POST | `/api/guests` | Add single guest |
| POST | `/api/guests/import` | Bulk import via CSV upload |
| DELETE | `/api/guests/{id}` | Remove guest |
| GET | `/api/guests/export` | Download all as CSV |
| GET | `/api/guests/template` | Download blank CSV template |

---

## CSV Import Format

Download the template from Admin → "CSV template". Columns:

```
FullName, Email, AllowedPlusOne, GroupName
Maria Santos, maria@email.com, true, Family
Jose Reyes, jose@email.com, false, Work
```

- `AllowedPlusOne`: `true` or `false`
- `Email` and `GroupName` are optional

---

## Local Development

### API
```bash
cd WeddingApi
# Edit appsettings.json with your local Postgres connection string
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

### Frontend
```bash
cd wedding-ui
npm install
ng serve
```

---

## Deployment

### Railway (API)
1. Push `WeddingApi/` to GitHub
2. railway.app → New Project → Deploy from GitHub
3. Add a PostgreSQL database (New → Database → PostgreSQL)
4. Set environment variables:
   - `AdminKey` → long random string
   - `AllowedOrigins__0` → your Netlify URL

### Netlify (Angular)
1. Update `environment.prod.ts` with your Railway API URL and Netlify URL
2. netlify.com → New site → Import from GitHub
3. Build: `ng build --configuration production`
4. Publish dir: `dist/wedding-ui/browser`
5. Add `public/_redirects` file:
   ```
   /* /index.html 200
   ```

---

## Customization Checklist

- [ ] Update couple names, date, venue in `home.component.html` and `details.component.html`
- [ ] Update meal options in `rsvp.component.html`
- [ ] Set a strong `AdminKey` in Railway env vars
- [ ] Update `siteUrl` in `environment.prod.ts` (used to generate guest links)
- [ ] Update `AllowedOrigins` in Railway env vars
