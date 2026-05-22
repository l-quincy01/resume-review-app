"use client";

interface MetricRowProps {
  label: string;
  value: string;
}

export default function MetricRow({ label, value }: MetricRowProps) {
  return (
    <div className="flex items-center justify-between gap-3 text-sm">
      <span className="text-muted-foreground">{label}</span>
      <span>{value}</span>
    </div>
  );
}
