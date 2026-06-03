#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${API_BASE_URL:-}" ]]; then
  echo "API_BASE_URL is required, for example: https://api.example.com" >&2
  exit 2
fi

if [[ -z "${LOG_SEARCH_COMMAND:-}" ]]; then
  echo "LOG_SEARCH_COMMAND is required and must query edge/app/APM logs for the staging window." >&2
  exit 2
fi

test_key="${TEST_OPENAI_API_KEY:-sk-test-redaction-check}"
header_name="X-OpenAI-Api-Key"
settle_seconds="${LOG_SETTLE_SECONDS:-30}"
api_base="${API_BASE_URL%/}"

curl --fail --silent --show-error \
  --header "${header_name}: ${test_key}" \
  "${api_base}/health/live" >/dev/null

sleep "${settle_seconds}"

log_output="$(bash -c "${LOG_SEARCH_COMMAND}")"

if grep -Fq "${test_key}" <<<"${log_output}"; then
  echo "Redaction check failed: fake API key was found in searched logs." >&2
  exit 1
fi

if grep -Fiq "${header_name}" <<<"${log_output}"; then
  echo "Redaction check failed: API key header name was found in searched logs." >&2
  exit 1
fi

echo "Redaction check passed: fake API key and header name were not found in searched logs."
