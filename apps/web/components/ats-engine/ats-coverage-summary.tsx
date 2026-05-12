"use client";

import { AtsCoverage } from "@/types/ats-engine.type";
import MetricRow from "./metric-row";

interface AtsCoverageSummaryProps {
  coverage: AtsCoverage;
}

export default function AtsCoverageSummary({
  coverage,
}: AtsCoverageSummaryProps) {
  return (
    <div className="flex flex-col gap-2 rounded-md border p-3">
      <div className="font-semibold">Coverage</div>
      <MetricRow
        label="Keywords found"
        value={`${coverage.total_keywords_found}/${coverage.total_keywords_expected}`}
      />
      <MetricRow label="Tier 0 coverage" value={`${coverage.tier_0_coverage}%`} />
      <MetricRow label="Tier 1 coverage" value={`${coverage.tier_1_coverage}%`} />
      <MetricRow
        label="Must-have coverage"
        value={`${coverage.must_have_coverage}%`}
      />
    </div>
  );
}
