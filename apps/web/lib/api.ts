const LOCAL_API_BASE_URL = "http://localhost:5053";

function getApiBaseUrl() {
  const configuredBaseUrl = process.env.NEXT_PUBLIC_API_BASE?.trim();

  if (configuredBaseUrl) {
    return configuredBaseUrl.replace(/\/+$/, "");
  }

  if (process.env.NODE_ENV !== "production") {
    return LOCAL_API_BASE_URL;
  }

  throw new Error("NEXT_PUBLIC_API_BASE is required in production.");
}

export function apiUrl(path: string) {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;

  return `${getApiBaseUrl()}${normalizedPath}`;
}
