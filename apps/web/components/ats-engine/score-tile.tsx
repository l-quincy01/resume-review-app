"use client";

interface ScoreTileProps {
  label: string;
  value: number;
}

export default function ScoreTile({ label, value }: ScoreTileProps) {
  return (
    <div className="flex flex-col gap-2 rounded-md border p-3">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className="text-4xl font-bold">{value}%</div>
    </div>
  );
}
