FROM node:22-alpine AS base

ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
ENV NEXT_TELEMETRY_DISABLED=1

RUN corepack enable

WORKDIR /app

FROM base AS deps

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml turbo.json ./
COPY packages ./packages
COPY apps/web/package.json ./apps/web/package.json
COPY apps/docs/package.json ./apps/docs/package.json
COPY apps/api/package.json ./apps/api/package.json

RUN pnpm install --frozen-lockfile

FROM deps AS builder

ARG NEXT_PUBLIC_API_BASE=http://localhost:5053
ARG LOG_LEVEL=info

ENV NEXT_PUBLIC_API_BASE=$NEXT_PUBLIC_API_BASE
ENV LOG_LEVEL=$LOG_LEVEL

COPY . .

RUN pnpm --filter web build

FROM base AS runner

ENV NODE_ENV=production
ENV PORT=3000
ENV HOSTNAME=0.0.0.0
ENV NEXT_PUBLIC_API_BASE=http://localhost:5053
ENV LOG_LEVEL=info

COPY --from=builder /app /app

EXPOSE 3000

CMD ["pnpm", "--filter", "web", "exec", "next", "start", "-H", "0.0.0.0", "-p", "3000"]
