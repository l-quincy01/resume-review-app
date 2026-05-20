import React from "react";

export default function KeywordPillSection({
  title,
  keywords,
  variant,
}: {
  title: string;
  keywords: string[];
  variant: "missing" | "present";
}) {
  return (
    <div className="flex flex-col gap-2">
      <div className="text-xs font-medium text-muted-foreground">{title}</div>
      {keywords.length > 0 ? (
        <div className="flex flex-row flex-wrap gap-2">
          {keywords.map((keyword) => (
            <KeywordPill key={`${variant}-${keyword}`} variant={variant}>
              {keyword}
            </KeywordPill>
          ))}
        </div>
      ) : (
        <div className="text-sm text-muted-foreground">None</div>
      )}
    </div>
  );
}

function KeywordPill({
  children,
  variant,
}: {
  children: string;
  variant: "missing" | "present";
}) {
  const className =
    variant === "present"
      ? "border-primary/30 bg-primary/10 text-primary"
      : "border-destructive/30 bg-destructive/10 text-destructive";

  return (
    <span
      className={`rounded-md border px-2.5 py-1 text-xs font-medium ${className}`}
    >
      {children}
    </span>
  );
}
