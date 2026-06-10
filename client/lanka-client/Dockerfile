# Build stage
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build -- --configuration=production

# Runtime stage
FROM nginx:alpine

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/lanka-client/browser /usr/share/nginx/html
COPY docker-entrypoint.sh /docker-entrypoint.d/40-lanka-runtime-config.sh
RUN chmod +x /docker-entrypoint.d/40-lanka-runtime-config.sh

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget --spider -q http://127.0.0.1/ || exit 1
