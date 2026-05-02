type LogLevel = "debug" | "info" | "warn" | "error";

type LogMetadata = Record<string, unknown>;

type ClientLogEvent = {
  level: LogLevel;
  message: string;
  metadata?: LogMetadata;
};

function normalizeError(error: unknown) {
  if (error instanceof Error) {
    return {
      name: error.name,
      message: error.message,
      stack: error.stack,
    };
  }

  return { message: String(error) };
}

async function sendLog(event: ClientLogEvent) {
  if (typeof window === "undefined") return;

  const payload = {
    ...event,
    metadata: {
      ...event.metadata,
      path: window.location.pathname,
      userAgent: window.navigator.userAgent,
    },
  };

  try {
    await fetch("/api/log", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
      keepalive: true,
    });
  } catch {
    // Logging must never break the user flow.
  }
}

export const clientLogger = {
  debug(message: string, metadata?: LogMetadata) {
    void sendLog({ level: "debug", message, metadata });
  },
  info(message: string, metadata?: LogMetadata) {
    void sendLog({ level: "info", message, metadata });
  },
  warn(message: string, metadata?: LogMetadata) {
    void sendLog({ level: "warn", message, metadata });
  },
  error(message: string, error?: unknown, metadata?: LogMetadata) {
    void sendLog({
      level: "error",
      message,
      metadata: {
        ...metadata,
        error: error === undefined ? undefined : normalizeError(error),
      },
    });
  },
};

