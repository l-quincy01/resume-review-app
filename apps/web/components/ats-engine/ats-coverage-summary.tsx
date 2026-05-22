"use client";

import {
  AtsCoverage,
  AtsScoredKeyword,
  AtsTierBreakdown,
} from "@/types/AtsEngine/ats-engine.type";
import MetricRow from "./metric-row";

interface AtsCoverageSummaryProps {
  coverage: AtsCoverage;
  tierBreakdown?: AtsTierBreakdown;
  keywordScores?: AtsScoredKeyword[];
}

export default function AtsCoverageSummary({
  coverage,
  tierBreakdown,
  keywordScores,
}: AtsCoverageSummaryProps) {
  return (
    <div className="grid gap-2 md:grid-cols-3">
      <div className="flex flex-col gap-1 rounded-md border p-3">
        <div className="text-xs text-muted-foreground">
          Overall Keywords found
        </div>
        <div className="text-2xl font-bold">
          {(
            (coverage.total_keywords_found / coverage.total_keywords_expected) *
            100
          ).toFixed(2)}
          %
        </div>
        <div className="text-xs text-muted-foreground">
          {coverage.total_keywords_found} / {coverage.total_keywords_expected}{" "}
          present
        </div>
      </div>

      <div className="flex flex-col gap-1 rounded-md border p-3">
        <div className="text-xs text-muted-foreground">
          Very important Keywords
        </div>
        <div className="text-2xl font-bold">{coverage.tier_0_coverage}%</div>
        <div className="text-xs text-muted-foreground">
          {tierBreakdown && tierBreakdown["tier_0"].present}/
          {tierBreakdown && tierBreakdown["tier_0"].total} present
        </div>
      </div>

      <div className="flex flex-col gap-1 rounded-md border p-3">
        <div className="text-xs text-muted-foreground">Important Keywords</div>
        <div className="text-2xl font-bold">{coverage.tier_1_coverage}%</div>
        <div className="text-xs text-muted-foreground">
          {tierBreakdown && tierBreakdown["tier_1"].present}/
          {tierBreakdown && tierBreakdown["tier_1"].total} present
        </div>
      </div>
    </div>
  );
}

// <MetricRow
//   label=""
//   value={`${coverage.total_keywords_found}/${coverage.total_keywords_expected}`}
// />
// <MetricRow
//   label="Very important"
//   value={`${coverage.tier_0_coverage}%`}
// />
// <MetricRow label="Important" value={`${coverage.tier_1_coverage}%`} />
// <MetricRow
//   label="Must-have coverage"
//   value={`${coverage.must_have_coverage}%`}
// />
