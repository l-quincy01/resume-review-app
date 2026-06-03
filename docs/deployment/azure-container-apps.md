# Azure Container Apps Deployment

This project deploys to Azure as two public Container Apps:

- `resume-review-web`: Next.js app on port `3000`
- `resume-review-api`: ASP.NET Core API on port `5053`

The API uses user-supplied OpenAI keys from `X-OpenAI-Api-Key`, so no production OpenAI key is stored in Azure.

## 1. Choose Azure Names

Use production-safe names like:

```txt
AZURE_RESOURCE_GROUP=resume-review-prod-rg
AZURE_LOCATION=westeurope
AZURE_CONTAINER_REGISTRY=resumereviewprodacr
AZURE_CONTAINER_APP_ENV=resume-review-prod-env
AZURE_WEB_APP_NAME=resume-review-web
AZURE_API_APP_NAME=resume-review-api
```

The Azure Container Registry name must be globally unique and contain only lowercase letters and numbers.

## 2. Create GitHub Deployment Credentials

Create a service principal scoped to your Azure subscription:

```bash
az ad sp create-for-rbac \
  --name resume-review-github-deploy \
  --role contributor \
  --scopes /subscriptions/<AZURE_SUBSCRIPTION_ID> \
  --sdk-auth
```

Save the full JSON output as this GitHub Actions secret:

```txt
AZURE_CREDENTIALS
```

## 3. Configure GitHub Actions Variables

In GitHub, go to `Settings -> Secrets and variables -> Actions -> Variables` and add:

```txt
AZURE_RESOURCE_GROUP=resume-review-prod-rg
AZURE_LOCATION=westeurope
AZURE_CONTAINER_REGISTRY=resumereviewprodacr
AZURE_CONTAINER_APP_ENV=resume-review-prod-env
AZURE_WEB_APP_NAME=resume-review-web
AZURE_API_APP_NAME=resume-review-api
WEB_BASE_URL=https://app.your-domain.com
API_BASE_URL=https://api.your-domain.com
API_ALLOWED_HOSTS=api.your-domain.com
LOG_LEVEL=info
```

`WEB_BASE_URL` and `API_BASE_URL` must be HTTPS origins. `NEXT_PUBLIC_API_BASE` is baked into the Next.js image at build time, so rerun the deploy workflow any time the public API URL changes.

## 4. Run The Workflow

Run `Deploy Azure Container Apps` from GitHub Actions, or push to `main` after the workflow is committed.

The workflow will:

- Create or update the resource group.
- Deploy Azure Container Registry, Log Analytics, Container Apps environment, and a managed pull identity.
- Build and push `resume-review-api:<commit-sha>` and `resume-review-web:<commit-sha>`.
- Deploy or update both Container Apps.
- Smoke test API health endpoints and the web app.

## 5. Configure Domains

In Azure Portal, add custom domains to the two Container Apps:

```txt
app.your-domain.com -> resume-review-web
api.your-domain.com -> resume-review-api
```

Follow Azure's requested DNS records and enable managed certificates for both domains.

After custom domains are active, confirm the GitHub variables match the final domains and rerun the workflow so the web image is rebuilt with the final `API_BASE_URL`.

## 6. Verify Production Security

The API Container App runs with:

```txt
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:5053
Cors__AllowedOrigins__0=https://app.your-domain.com
AllowedHosts=api.your-domain.com
```

Confirm:

- Azure ingress has `allowInsecure: false`.
- CORS only allows the web origin.
- Browser requests send the user key only as `X-OpenAI-Api-Key`.
- Azure diagnostics, edge logs, and APM logs do not store `X-OpenAI-Api-Key`.

## 7. Smoke Test

After deployment:

```bash
curl --fail https://api.your-domain.com/health/live
curl --fail https://api.your-domain.com/health/ready
curl --fail --head https://app.your-domain.com
```

Then test the browser flow:

- Open `https://app.your-domain.com`.
- Enter a user OpenAI API key.
- Upload a resume.
- Confirm API traffic goes to `https://api.your-domain.com`.
- Confirm job listings use `/api/job-listings`.

## 8. Optional Redaction Check

When you have a real Azure Log Analytics query command, add this GitHub Actions variable:

```txt
EDGE_LOG_SEARCH_COMMAND=<az monitor logs query command>
```

The deploy workflow will run `scripts/security/check-edge-key-redaction.sh`, which sends a fake key and fails if the fake key or header name appears in searched logs.
