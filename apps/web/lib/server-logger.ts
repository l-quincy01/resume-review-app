import "server-only";
import winston from "winston";

const logLevel = process.env.LOG_LEVEL ?? "info";

export const serverLogger = winston.createLogger({
  level: logLevel,
  defaultMeta: {
    application: "resume-review-web",
    runtime: "nextjs",
    environment: process.env.NODE_ENV ?? "development",
  },
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.errors({ stack: true }),
    winston.format.json(),
  ),
  transports: [new winston.transports.Console()],
});

