"use client";

import { AtsFinalAssessmentResponse } from "@/types/ats-engine.type";
import AtsCoverageSummary from "./ats-coverage-summary";
import AtsKeywordResultList from "./ats-keyword-result-list";
import AtsTierBreakdownSummary from "./ats-tier-breakdown-summary";
import ScoreTile from "./score-tile";

interface AtsFinalAssessmentSummaryProps {
  assessment: AtsFinalAssessmentResponse;
}

export default function AtsFinalAssessmentSummary({
  assessment,
}: AtsFinalAssessmentSummaryProps) {
  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
        <div className="flex flex-col">
          <div className="text-sm text-muted-foreground">
            {assessment.job_title || "ATS assessment"}
          </div>
          <div className="text-lg font-semibold">ATS Readiness</div>
        </div>

        <div className="text-xl text-muted-foreground font-bold">
          <span className="font-extrabold text-4xl text-card-foreground">
            {assessment.ats_readiness_score}
          </span>
          /100
        </div>
      </div>

      <div className="grid gap-2 md:grid-cols-3">
        <ScoreTile
          label="Keyword score"
          value={assessment.overall_keyword_score}
        />
        <ScoreTile
          label="Header score"
          value={assessment.header_quality_score}
        />
        <ScoreTile
          label="Presence rate"
          value={assessment.coverage.overall_presence_rate}
        />
      </div>

      <div className="grid gap-2 md:grid-cols-2">
        <AtsCoverageSummary coverage={assessment.coverage} />
        <AtsTierBreakdownSummary tierBreakdown={assessment.tier_breakdown} />
      </div>

      <AtsKeywordResultList
        title="Critical Gaps"
        items={assessment.critical_gaps}
        variant="critical-gap"
      />
      <AtsKeywordResultList
        title="Strengths"
        items={assessment.strengths}
        variant="strength"
      />
      <AtsKeywordResultList
        title="Recommendations"
        items={assessment.recommendations}
        variant="recommendation"
      />
    </div>
  );
}
