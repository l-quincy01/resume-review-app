# Resume Reviewer

AI-powered resume review monorepo for uploading a PDF resume, comparing it against an optional job description, generating structured resume feedback, running ATS-style analysis, and optionally producing job listing recommendations.

## Repository Layout

This repository is a pnpm/Turborepo workspace with a .NET API solution.

- `apps/web`: Next.js frontend.
- `apps/api`: ASP.NET Core API that talks to OpenAI.
- `apps/api.Tests`: xUnit tests for API behavior, OpenAI parsing, privacy-sensitive logging, and ATS services.
- `apps/docs`: Next.js docs app.
- `packages/ui`: shared React UI package.
- `packages/eslint-config`: shared ESLint config.
- `packages/typescript-config`: shared TypeScript config.

## Prerequisites

- Node.js `>=18`
- pnpm `9.0.0`
- .NET SDK `9.x`
- An OpenAI API key for resume analysis and job listing features

## Local Setup

Install JavaScript dependencies from the repo root:

```sh
pnpm install
```

Create the web environment file:

```sh
cp apps/web/.env.example apps/web/.env.local
```

For local development, `apps/web/.env.local` should contain:

```sh
NEXT_PUBLIC_API_BASE=http://localhost:5053
LOG_LEVEL=info
```

Create API configuration from the example:

```sh
cp apps/api/appsettings.example.json apps/api/appsettings.Development.json
```

Set `OpenAI:ApiKey` in `apps/api/appsettings.Development.json`, or preferably use an environment variable:

```sh
OpenAI__ApiKey=your-api-key
```

Do not commit real secrets, `.env.local`, publish profiles, or local appsettings files containing keys.

## Running Locally

Run the API:

```sh
cd apps/api
dotnet watch run
```

The API listens on `http://localhost:5053` in the checked-in launch profile.

Run the web app in another terminal:

```sh
cd apps/web
pnpm dev
```

The web app listens on `http://localhost:3000`.

You can also run all workspace dev tasks from the root:

```sh
pnpm dev
```

For local resume review, make sure the API is running and `NEXT_PUBLIC_API_BASE` points to `http://localhost:5053`.

## Common Commands

Run these from the repo root unless noted otherwise.

```sh
pnpm lint
pnpm check-types
pnpm test
pnpm build
dotnet test resume-tracker.sln
dotnet build resume-tracker.sln
pnpm --filter web test
```

API-only commands from `apps/api`:

```sh
dotnet watch run
dotnet build
dotnet test ../../resume-tracker.sln
```

Web-only commands from `apps/web`:

```sh
pnpm dev
pnpm build
pnpm test
pnpm check-types
pnpm lint
```

## Configuration

### Web

The frontend calls the API through `NEXT_PUBLIC_API_BASE`. Do not hardcode production API URLs in components or services.

Required production web environment:

```sh
NEXT_PUBLIC_API_BASE=https://your-api-domain.example
LOG_LEVEL=info
```

### API

The API reads OpenAI settings from the `OpenAI` configuration section. In production, use environment variables or the cloud provider's secret/configuration system.

Required production API environment:

```sh
OpenAI__ApiKey=...
Cors__AllowedOrigins__0=https://your-web-domain.example
AllowedHosts=your-api-domain.example
```

Common optional overrides:

```sh
OpenAI__DefaultModel=gpt-4.1-mini
OpenAI__JobListingsModel=gpt-5-nano
OpenAI__RequestTimeoutSeconds=120
OpenAI__JobListingsTimeoutSeconds=180
OpenAI__MaxRetries=2
OpenAI__MaxResumeBytes=5242880
OpenAI__MaxJobDescriptionCharacters=12000
```

Production CORS origins must be configured. Outside development, the API fails startup if `Cors:AllowedOrigins` is empty.

### API Versioning And Health

New public API clients should use `/api/v1/...` routes.

Current unversioned `/api/...` routes remain available as backward-compatible aliases for v1 so existing frontend calls continue to work. Future breaking API changes should be introduced under `/api/v2/...` rather than changing v1 behavior.

Hosting platforms and load balancers can use:

```sh
GET /health/live
GET /health/ready
```

Both health endpoints are lightweight process probes. Readiness does not call OpenAI or any external dependency.

## Privacy And Logging

This app processes resume PDFs and job descriptions, so treat all uploaded data as sensitive.

- Do not log resume contents.
- Do not log uploaded file bytes.
- Do not log full OpenAI responses.
- Do not log raw job descriptions.
- Prefer metadata such as status code, model name, schema name, response body length, counts, and operation name.
- Uploaded provider files should be deleted after analysis. The API test suite includes coverage for cleanup success and cleanup failure behavior.

## Deployment Overview

Deploy the web and API as separate services.

- Web: Next.js Node server or container.
- API: ASP.NET Core service or container.
- Secrets: provider-managed secrets, not committed files.
- TLS: managed certificate on the hosting provider.
- Logs: provider-native log aggregation.
- CI/CD: run lint, typecheck, tests, and builds before release.

### AWS

Recommended AWS shape:

- `apps/web`: AWS App Runner from source or container image.
- `apps/api`: AWS App Runner or Elastic Beanstalk for ASP.NET Core.
- Secrets: AWS Secrets Manager or SSM Parameter Store.
- Logs: CloudWatch.
- Domain/TLS: App Runner custom domains, Elastic Beanstalk custom domain, CloudFront, and ACM as needed.

Minimum deployment steps:

1. Build and deploy the API with production environment variables:

   ```sh
   OpenAI__ApiKey=...
   Cors__AllowedOrigins__0=https://your-web-domain.example
   AllowedHosts=your-api-domain.example
   ASPNETCORE_ENVIRONMENT=Production
   ```

2. Build and deploy the web app with:

   ```sh
   NEXT_PUBLIC_API_BASE=https://your-api-domain.example
   LOG_LEVEL=info
   NODE_ENV=production
   ```

3. Configure the API service URL as the web `NEXT_PUBLIC_API_BASE`.
4. Configure the web domain as the API CORS allowed origin.
5. Verify resume upload, streamed review, ATS analysis, and job listing flows in production.

### Azure

Recommended Azure shape:

- `apps/web`: Azure App Service for Node.js or a containerized Next.js app.
- `apps/api`: Azure App Service for ASP.NET Core.
- Secrets: Azure Key Vault or App Service application settings.
- Logs/metrics: Application Insights and App Service logs.
- Domain/TLS: App Service managed certificates or Azure Front Door.

Minimum deployment steps:

1. Create an App Service for the API and configure:

   ```sh
   OpenAI__ApiKey=...
   Cors__AllowedOrigins__0=https://your-web-app.azurewebsites.net
   AllowedHosts=your-api-app.azurewebsites.net
   ASPNETCORE_ENVIRONMENT=Production
   ```

2. Create an App Service for the web app and configure:

   ```sh
   NEXT_PUBLIC_API_BASE=https://your-api-app.azurewebsites.net
   LOG_LEVEL=info
   NODE_ENV=production
   ```

3. Deploy `apps/api` to the API App Service.
4. Deploy `apps/web` to the web App Service.
5. Add custom domains and managed certificates.
6. Verify CORS, upload size limits, streaming responses, and logs.

## Production Readiness Checklist

Before a public launch:

- Fix any P1 privacy or data-flow issues.
- Require explicit consent before sending resume data to an AI provider.
- Confirm configured OpenAI models are valid for the production account.
- Add rate limiting and abuse protection around upload and AI endpoints.
- Add CI/CD for lint, typecheck, tests, and builds.
- Configure provider-managed secrets.
- Configure production CORS and `AllowedHosts`.
- Enable production log aggregation and alerting.
- Verify OpenAI file cleanup behavior in production logs.
- Document data retention and deletion behavior for users.

## Validation Before Handoff

Run the highest-signal checks before merging or deploying:

```sh
pnpm lint
pnpm check-types
pnpm test
pnpm build
dotnet test resume-tracker.sln
```

For API-specific changes, also run:

```sh
dotnet build resume-tracker.sln
```
