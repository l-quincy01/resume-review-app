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

The deployment also creates an `AcrPull` role assignment for the Container Apps managed identity. `Contributor` cannot create role assignments, so the GitHub service principal also needs one RBAC administration role at the resource group or subscription scope.

After creating the service principal, grant it `User Access Administrator` at the resource group scope from an Azure account that has Owner/User Access Administrator permissions:

```bash
az role assignment create \
  --assignee <CLIENT_ID_FROM_AZURE_CREDENTIALS_JSON> \
  --role "User Access Administrator" \
  --scope /subscriptions/<AZURE_SUBSCRIPTION_ID>/resourceGroups/resume-review-prod-rg
```

If the resource group does not exist yet, create it first:

```bash
az group create \
  --name resume-review-prod-rg \
  --location westeurope
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
LOG_LEVEL=info
```

You do not need a custom domain for the first deployment. If `WEB_BASE_URL` and `API_BASE_URL` are omitted, the workflow uses Azure's generated `https://*.azurecontainerapps.io` URLs.

When you add custom domains later, add or update:

```txt
WEB_BASE_URL=https://app.your-domain.com
API_BASE_URL=https://api.your-domain.com
API_ALLOWED_HOSTS=api.your-domain.com
```

`WEB_BASE_URL` and `API_BASE_URL` must be HTTPS origins when provided. `NEXT_PUBLIC_API_BASE` is baked into the Next.js image at build time, so rerun the deploy workflow any time the public API URL changes.

## 4. Run The Workflow

Run `Deploy Azure Container Apps` from GitHub Actions, or push to `main` after the workflow is committed.

The workflow will:

- Create or update the resource group.
- Deploy Azure Container Registry, Log Analytics, Container Apps environment, and a managed pull identity.
- Build and push `resume-review-api:<commit-sha>`.
- Deploy the API once with a temporary HTTPS CORS origin.
- Read the generated API URL if `API_BASE_URL` is not configured.
- Build and push `resume-review-web:<commit-sha>` with the resolved API URL.
- Deploy the web app and read the generated web URL if `WEB_BASE_URL` is not configured.
- Redeploy the API with the final web CORS origin and API host setting.
- Smoke test API health endpoints and the web app.

## 5. Configure Domains

In Cloudflare or your DNS provider, create validation and routing records for the two Container Apps:

```txt
www.your-domain.com -> resume-review-web generated Azure hostname
api.your-domain.com -> resume-review-api generated Azure hostname
asuid.www -> web Container App domain verification ID
asuid.api -> API Container App domain verification ID
```

For subdomains such as `www` and `api`, use CNAME records and keep them DNS-only if you use Cloudflare. Azure managed certificates require the CNAME to map directly to the Container App generated hostname.

After DNS is in place, set the final GitHub variables and rerun the workflow:

```txt
WEB_BASE_URL=https://www.your-domain.com
API_BASE_URL=https://api.your-domain.com
API_ALLOWED_HOSTS=api.your-domain.com
```

The workflow binds the custom hostnames to the Container Apps and creates Azure managed certificates with CNAME validation. Keep custom domains in the workflow variables instead of only adding them manually in the Azure Portal; redeploying a Container App from Bicep can replace ingress settings and remove portal-only custom domain bindings.

## 6. Verify Production Security

The API Container App runs with:

```txt
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:5053
Cors__AllowedOrigins__0=https://app.your-domain.com
AllowedHosts=api.your-domain.com
```

If you have not configured custom domains, those values resolve to the generated Azure Container Apps HTTPS hostnames.

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

Without custom domains, use the generated API and web URLs printed by the deploy workflow.

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
