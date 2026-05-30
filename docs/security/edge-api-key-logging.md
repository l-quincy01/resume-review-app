# Edge API Key Logging Controls

The `X-OpenAI-Api-Key` request header is a user secret. It may pass through the edge to the API, but it must not be stored by CDN, load balancer, WAF, proxy, app, APM, or diagnostic logs.

## Required Controls

- Serve browser-to-edge traffic over HTTPS only.
- Serve edge-to-origin traffic over HTTPS only.
- Redirect or reject HTTP at the edge.
- Keep API `Cors:AllowedOrigins` restricted to known frontend origins.
- Do not use wildcard CORS origins in production.
- Do not log request headers wholesale.
- Exclude or redact `X-OpenAI-Api-Key` in CDN, load balancer, WAF, reverse proxy, application, APM, and diagnostic logging.
- Run the staging smoke test before enabling production traffic.

## AWS

Recommended layouts:

- CloudFront to ALB/ECS/App Runner.
- ALB directly to ECS/App Runner when a CDN is not needed.

Required checks:

- Use HTTPS viewer protocol policy and HTTPS origin protocol policy.
- Verify CloudFront standard logs, real-time logs, WAF logs, Lambda@Edge, CloudFront Functions, ALB logs, and third-party log sinks do not contain `X-OpenAI-Api-Key` or the fake redaction key.
- If enabling richer edge logging, explicitly exclude request headers or configure redaction before logs leave the edge service.

## Azure

Recommended layouts:

- Azure Front Door to App Service or Container Apps.
- Application Gateway to App Service or Container Apps.

Required checks:

- Use HTTPS frontend listeners and HTTPS backend/origin protocol.
- Verify Front Door logs, Application Gateway logs, WAF logs, Diagnostic Settings, Log Analytics, Application Insights, and any custom telemetry do not contain `X-OpenAI-Api-Key` or the fake redaction key.
- Use a Log Analytics query after staging smoke tests to search for the fake key and header name.

Example KQL shape:

```kql
search "sk-test-redaction-check" or "X-OpenAI-Api-Key"
| take 50
```

## Staging Smoke Test

Run after deployment and after edge/proxy logging is enabled:

```bash
API_BASE_URL=https://api.example.com \
LOG_SEARCH_COMMAND='your cloud log search command here' \
bash scripts/security/check-edge-key-redaction.sh
```

The script:

- Sends a request containing `X-OpenAI-Api-Key: sk-test-redaction-check`.
- Confirms the API responds.
- Runs the supplied log search command.
- Fails if the fake key or header name appears in log search output.

The log search command must query every enabled edge, WAF, load balancer, app, APM, and diagnostic sink that could observe requests.
