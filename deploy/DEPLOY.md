# Lanka — Hostinger VPS Deploy Runbook

Pipeline overview:

```
git push main
  ↓
GitHub Actions  ─►  ghcr.io/iiia-ko/lanka-{api,gateway,client}:{latest,sha}
                       ↓ (curl)
                    Dokploy webhook on Hostinger VPS
                       ↓
                    docker compose pull + up -d
```

Single 4 GB / 1 vCPU host, no domain. Public access by VPS IP + port.

---

## 1. One-time VPS bring-up

SSH in (or use Hostinger's Browser Terminal):

```bash
# 8 GB swap — KVM-1 has 50 GB NVMe to spare
fallocate -l 8G /swapfile
chmod 600 /swapfile
mkswap /swapfile
swapon /swapfile
echo '/swapfile none swap sw 0 0' >> /etc/fstab
echo 'vm.swappiness=10' >> /etc/sysctl.conf
sysctl -p

# Dokploy installs Docker + Traefik on its own
curl -sSL https://dokploy.com/install.sh | sh
```

Hostinger hPanel → VPS → Firewall: allow inbound TCP **22, 80, 8080, 8081, 3000**.
- 80    — Angular client (`lanka-client`)
- 8080  — Gateway public (`lanka-gateway`)
- 8081  — Keycloak public (`lanka-identity`)
- 3000  — Dokploy admin UI

Browse `http://<vps-ip>:3000` and create the first admin user.

## 2. Dokploy Compose service

Dokploy UI → **Projects → Create Project → Compose**.

**Source tab:**
- Provider: `Raw`.
- Paste the contents of `deploy/docker-compose.prod.yml` from this repo.

**Environment tab — set the following:**

```env
GHCR_OWNER=iiia-ko
IMAGE_TAG=latest
POSTGRES_PASSWORD=<random-32-chars>
MONGO_PASSWORD=<random-32-chars>
REDIS_PASSWORD=<random-32-chars>
RABBITMQ_PASSWORD=<random-32-chars>
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=<random-32-chars>
PUBLIC_API_URL=http://<vps-ip>:8080
PUBLIC_AUTH_URL=http://<vps-ip>:8081
PUBLIC_CLIENT_ORIGIN=http://<vps-ip>
```

Generate randoms with `openssl rand -hex 24`. Replace `<vps-ip>` with the VPS public IP.

**Keycloak realm import — SSH bind mount:**

Dokploy 0.29 doesn't expose a File Mount in the compose UI. Drop the realm file
onto the host once via SSH; the compose already bind-mounts it into the
`lanka-identity` container:

```bash
ssh root@<vps-ip>
mkdir -p /etc/lanka
curl -L https://raw.githubusercontent.com/IIIA-KO/Lanka/main/.files/lanka-realm-export.json \
  -o /etc/lanka/lanka-realm-export.json
chmod 644 /etc/lanka/lanka-realm-export.json
```

If the repo is private, scp instead:
```bash
scp .files/lanka-realm-export.json root@<vps-ip>:/etc/lanka/
```

**Domains tab — host port mapping:**

| Service          | Container port | Published port |
|------------------|---------------:|---------------:|
| `lanka-client`   | 80             | 80             |
| `lanka-gateway`  | 8080           | 8080           |
| `lanka-identity` | 8080           | 8081           |

No domain configured → Dokploy / Traefik passes traffic through on the published port. No TLS yet (Instagram OAuth linking saga will fail until a domain + HTTPS are added; everything else works).

**Registry credentials:**

Two options:
1. Make the three `ghcr.io/iiia-ko/lanka-*` packages public after the first workflow run (GitHub → profile → Packages → settings → change visibility).
2. Otherwise, Dokploy → Settings → Registry → add `ghcr.io` with username `IIIA-KO` and a PAT carrying `read:packages`.

**Deploy webhook:**

Dokploy service page → **Webhooks → Create** → copy the URL.
GitHub repo → Settings → Secrets and variables → Actions → New repository secret:
- Name: `DOKPLOY_WEBHOOK`
- Value: the webhook URL.

The workflow's `notify-dokploy` job no-ops until this secret is set, so first manual deploy from Dokploy UI is needed; subsequent pushes auto-redeploy.

## 3. First deploy

1. `Deploy` button in Dokploy. Watch the event log — images pull, containers start in order.
2. Cold start takes ~3 minutes on KVM-1 (Elasticsearch + Keycloak warm slowly).
3. `docker stats` on the host — every container should sit below its `mem_limit`. If swap saturates, drop Elasticsearch heap further (`ES_JAVA_OPTS=-Xms192m -Xmx256m`) or remove the `lanka-search` service entirely (Matching module degrades gracefully).

## 4. Verification

```bash
# Health
curl -i http://<vps-ip>/                                         # 200, Angular index.html
curl -i http://<vps-ip>:8080/healthz                             # 200 from gateway
curl -i http://<vps-ip>:8081/realms/lanka/.well-known/openid-configuration | jq .issuer
# → "http://<vps-ip>:8081/realms/lanka"

# Migrations
docker exec lanka-postgres psql -U postgres -d lanka -c '\dn'
# → users, campaigns, analytics, matching schemas listed

# Wire-up
# Browser: http://<vps-ip>
# DevTools → Network → config.js carries apiUrl: 'http://<vps-ip>:8080'
# Login form → POST http://<vps-ip>:8080/users/login → 200 + JWT
```

## 5. Subsequent deploys

`git push origin main` → Actions runs three image jobs → webhook job hits Dokploy → Dokploy pulls fresh `:latest`, recreates containers. Named volumes persist all data (`lanka-postgres-data`, `lanka-mongo-data`, etc.).

## 6. Follow-ups (out of scope for this deploy)

- **Domain + TLS** — add A records → Dokploy auto-issues Let's Encrypt via Traefik. Unlocks Instagram OAuth.
- **Separate migration job** — run `dotnet ef database update` from CI before pulling new app images.
- **Aspire publish** — port the compose to Aspire's publisher once `PublishAsDockerFile()` is wired into AppHost.
- **Backups** — Dokploy supports scheduled `pg_dump` and `mongodump` snapshots; enable via the service UI after the first stable deploy.

## Quick recovery

- Container OOM-killed: `docker compose -f docker-compose.prod.yml restart <service>` in Dokploy.
- Bad image pushed: roll back `IMAGE_TAG` env var to a previous commit SHA, redeploy.
- Database corruption: `docker exec lanka-postgres pg_dump -U postgres lanka > backup.sql` (do this *before* destructive testing).
