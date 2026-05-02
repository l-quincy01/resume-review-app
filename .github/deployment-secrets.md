# Deployment Secret Handling

Store deployment values in GitHub environment secrets and variables under the `production` environment. Do not commit real secrets to `.env`, `appsettings.*.json`, workflow files, or source code.

Required environment variables:

- `NEXT_PUBLIC_API_BASE`: public API base URL used by the web build.
- `LOG_LEVEL`: frontend server log level for Winston. Use `info` in production unless debugging an incident.
- `AZURE_API_WEBAPP_NAME`: Azure Web App name for the API deployment.

Required environment secrets:

- `VERCEL_TOKEN`: Vercel deployment token.
- `VERCEL_ORG_ID`: Vercel organization ID.
- `VERCEL_PROJECT_ID`: Vercel project ID for `apps/web`.
- `AZURE_API_PUBLISH_PROFILE`: Azure publish profile XML for the API Web App.
- `OpenAI__ApiKey`: API runtime setting for the OpenAI key. Configure this in the hosting provider, not in the repository.

Production runtime settings:

- Configure `OpenAI__ApiKey` in the API host.
- Configure `Cors__AllowedOrigins__0` and additional indexed origins in the API host.
- Configure `OpenAI__AllowedModels__0`, `OpenAI__RequestTimeoutSeconds`, and other OpenAI options in the API host when production values differ from `appsettings.json`.
- Configure `Serilog__MinimumLevel__Default` in the API host when production log verbosity needs to change.

Secret rotation:

- Rotate publish profiles and API keys after accidental local exposure.
- Prefer short-lived deployment tokens where the provider supports them.
- Use protected GitHub environments with required reviewers before production deployments.
