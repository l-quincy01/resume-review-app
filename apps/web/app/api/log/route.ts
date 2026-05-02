import { NextRequest, NextResponse } from "next/server";
import { serverLogger } from "@/lib/server-logger";

export const runtime = "nodejs";

const allowedLevels = new Set(["debug", "info", "warn", "error"]);

export async function POST(request: NextRequest) {
  let body: unknown;

  try {
    body = await request.json();
  } catch {
    return NextResponse.json({ message: "Invalid JSON payload" }, { status: 400 });
  }

  if (!isLogPayload(body)) {
    return NextResponse.json({ message: "Invalid log payload" }, { status: 400 });
  }

  const forwardedFor = request.headers.get("x-forwarded-for");
  const clientIp = forwardedFor?.split(",")[0]?.trim();

  serverLogger.log(body.level, body.message, {
    ...body.metadata,
    source: "browser",
    clientIp,
  });

  return new NextResponse(null, { status: 204 });
}

function isLogPayload(value: unknown): value is {
  level: "debug" | "info" | "warn" | "error";
  message: string;
  metadata?: Record<string, unknown>;
} {
  if (typeof value !== "object" || value === null) return false;

  const candidate = value as Record<string, unknown>;

  return (
    typeof candidate.level === "string" &&
    allowedLevels.has(candidate.level) &&
    typeof candidate.message === "string" &&
    candidate.message.length > 0 &&
    (candidate.metadata === undefined ||
      (typeof candidate.metadata === "object" && candidate.metadata !== null))
  );
}

