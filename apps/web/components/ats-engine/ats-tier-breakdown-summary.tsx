"use client";

import {
  AtsTierBreakdown,
  AtsTierBreakdownItem,
} from "@/types/ats-engine.type";

interface AtsTierBreakdownSummaryProps {
  tierBreakdown: AtsTierBreakdown;
}

const tiers: Array<{
  key: keyof AtsTierBreakdown;
  label: string;
}> = [
  { key: "tier_0", label: "Tier 0" },
  { key: "tier_1", label: "Tier 1" },
  { key: "tier_2", label: "Tier 2" },
  { key: "tier_3", label: "Tier 3" },
];

export default function AtsTierBreakdownSummary({
  tierBreakdown,
}: AtsTierBreakdownSummaryProps) {
  return (
    <div className="flex flex-col gap-2 rounded-md border p-3">
      <div className="font-semibold">Tier Breakdown</div>
      {tiers.map((tier) => (
        <TierRow
          key={tier.key}
          label={tier.label}
          item={tierBreakdown[tier.key]}
        />
      ))}
    </div>
  );
}

function TierRow({
  label,
  item,
}: {
  label: string;
  item: AtsTierBreakdownItem;
}) {
  return (
    <div className="flex flex-col gap-1 text-sm">
      <div className="flex items-center justify-between gap-3">
        <span className="text-muted-foreground">{label}</span>
        <span>
          {item.average_score}% · {item.present}/{item.total}
        </span>
      </div>
      {item.missing.length > 0 && (
        <div className="text-xs text-muted-foreground">
          Missing: {item.missing.join(", ")}
        </div>
      )}
    </div>
  );
}
