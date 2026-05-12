"use client";

interface ScoreTileProps {
  label: string;
  value: number;
}

export default function ScoreTile({ label, value }: ScoreTileProps) {
  return (
    <div className="flex flex-col gap-1 rounded-md border p-3">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className="text-2xl font-bold">{value}%</div>
    </div>
  );
}
