"use client";

import type React from "react";
import {
  AtsEnginePipelineResult,
  AtsFinalAssessmentResponse,
  AtsHeaderValidationResponse,
  AtsTierBreakdown,
  AtsTierBreakdownItem,
} from "@/types/ats-engine.type";

interface AtsEngineResultsProps {
  result: AtsEnginePipelineResult;
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

export default function AtsEngineResults({
  result,
}: AtsEngineResultsProps) {
  const { headerValidation, finalAssessment } = result;

  return (
    <div className="flex flex-col gap-4 border-t pt-4">
      <HeaderValidationSummary headerValidation={headerValidation} />

      {finalAssessment && <FinalAssessmentSummary assessment={finalAssessment} />}
    </div>
  );
}

function HeaderValidationSummary({
  headerValidation,
}: {
  headerValidation: AtsHeaderValidationResponse;
}) {
  return (
    <div className="flex flex-col gap-3">
      <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
        <div className="flex flex-col">
          <div className="text-sm text-muted-foreground">
            Structure quality: {headerValidation.structure_quality}
          </div>
          <div className="text-lg font-semibold">ATS Header Validation</div>
        </div>

        <div className="text-xl text-muted-foreground font-bold">
          <span className="font-extrabold text-4xl text-card-foreground">
            {headerValidation.header_quality_score}
          </span>
          /100
        </div>
      </div>

      <div className="grid gap-2 md:grid-cols-2">
        <HeaderList title="Headers Found" items={headerValidation.headers_found} />
        <HeaderList
          title="Headers Missing"
          items={headerValidation.headers_missing}
        />
      </div>

      {headerValidation.non_standard_headers &&
        headerValidation.non_standard_headers.length > 0 && (
          <div className="flex flex-col gap-2 rounded-md border p-3 text-sm">
            <div className="font-semibold">Non-standard Headers</div>
            {headerValidation.non_standard_headers.map((header) => (
              <div
                key={`${header.header_found}-${header.mapped_to}`}
                className="text-muted-foreground"
              >
                {header.header_found} → use {header.recommended_header}
              </div>
            ))}
          </div>
        )}
    </div>
  );
}

function FinalAssessmentSummary({
  assessment,
}: {
  assessment: AtsFinalAssessmentResponse;
}) {
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
        <div className="flex flex-col gap-2 rounded-md border p-3">
          <div className="font-semibold">Coverage</div>
          <MetricRow
            label="Keywords found"
            value={`${assessment.coverage.total_keywords_found}/${assessment.coverage.total_keywords_expected}`}
          />
          <MetricRow
            label="Tier 0 coverage"
            value={`${assessment.coverage.tier_0_coverage}%`}
          />
          <MetricRow
            label="Tier 1 coverage"
            value={`${assessment.coverage.tier_1_coverage}%`}
          />
          <MetricRow
            label="Must-have coverage"
            value={`${assessment.coverage.must_have_coverage}%`}
          />
        </div>

        <div className="flex flex-col gap-2 rounded-md border p-3">
          <div className="font-semibold">Tier Breakdown</div>
          {tiers.map((tier) => (
            <TierRow
              key={tier.key}
              label={tier.label}
              item={assessment.tier_breakdown[tier.key]}
            />
          ))}
        </div>
      </div>

      {assessment.critical_gaps.length > 0 && (
        <ResultList title="Critical Gaps">
          {assessment.critical_gaps.map((gap) => (
            <li key={`${gap.keyword}-${gap.tier}`} className="rounded-md border p-3">
              <div className="font-medium">{gap.keyword}</div>
              <div className="text-muted-foreground">{gap.reason}</div>
            </li>
          ))}
        </ResultList>
      )}

      {assessment.strengths.length > 0 && (
        <ResultList title="Strengths">
          {assessment.strengths.map((strength) => (
            <li key={strength.keyword} className="rounded-md border p-3">
              <div className="flex items-start justify-between gap-3">
                <div className="font-medium">{strength.keyword}</div>
                <div className="text-muted-foreground">{strength.score}/100</div>
              </div>
              <div className="text-muted-foreground">{strength.reason}</div>
            </li>
          ))}
        </ResultList>
      )}

      {assessment.recommendations.length > 0 && (
        <ResultList title="Recommendations">
          {assessment.recommendations.map((recommendation) => (
            <li
              key={`${recommendation.keyword}-${recommendation.priority}`}
              className="rounded-md border p-3"
            >
              <div className="flex items-start justify-between gap-3">
                <div className="font-medium">{recommendation.keyword}</div>
                <div className="text-muted-foreground">
                  {recommendation.priority}
                </div>
              </div>
              <div className="text-muted-foreground">
                {recommendation.suggestion}
              </div>
            </li>
          ))}
        </ResultList>
      )}
    </div>
  );
}

function HeaderList({ title, items }: { title: string; items: string[] }) {
  return (
    <div className="flex flex-col gap-2 rounded-md border p-3">
      <div className="font-semibold">{title}</div>
      {items.length > 0 ? (
        <div className="text-sm text-muted-foreground">{items.join(", ")}</div>
      ) : (
        <div className="text-sm text-muted-foreground">None</div>
      )}
    </div>
  );
}

function ScoreTile({ label, value }: { label: string; value: number }) {
  return (
    <div className="flex flex-col gap-1 rounded-md border p-3">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className="text-2xl font-bold">{value}%</div>
    </div>
  );
}

function MetricRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between gap-3 text-sm">
      <span className="text-muted-foreground">{label}</span>
      <span>{value}</span>
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

function ResultList({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="flex flex-col gap-2">
      <div className="font-semibold">{title}</div>
      <ul className="flex flex-col gap-2 text-sm">{children}</ul>
    </div>
  );
}
