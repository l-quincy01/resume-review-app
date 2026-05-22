import React from "react";

interface KeywordTypeMetrics {
  present: number;
  total: number;
  presenceRate: number;
}

export default function KeywordTypePresenceTile({
  label,
  metrics,
}: {
  label: string;
  metrics: KeywordTypeMetrics;
}) {
  return (
    <div className="flex flex-col gap-1 rounded-md border p-3">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className="text-2xl font-bold">{metrics.presenceRate}%</div>
      <div className="text-xs text-muted-foreground">
        {metrics.present}/{metrics.total} present
      </div>
    </div>
  );
}
