"use client";

import { AtsEnginePipelineResult } from "@/types/AtsEngine/ats-engine.type";
import AtsFinalAssessmentSummary from "./ats-final-assessment-summary";
import AtsHeaderValidationSummary from "./ats-header-validation-summary";

interface AtsEngineResultsProps {
  result: AtsEnginePipelineResult;
}

export default function AtsEngineResults({ result }: AtsEngineResultsProps) {
  const { headerValidation, finalAssessment } = result;

  return (
    <div className="flex flex-col gap-4 border-t pt-4">
      {/* <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
        <div className="flex flex-col">
          <div className="text-sm text-muted-foreground">
            {finalAssessment?.job_title || "ATS assessment"}
          </div>
          <div className="text-lg font-semibold">ATS Readiness</div>
        </div>

        <div className="text-xl text-muted-foreground font-bold">
          <span className="font-extrabold text-4xl text-card-foreground">
            {headerValidation.header_quality_score}
          </span>
          /100
        </div>
      </div> */}

      <AtsHeaderValidationSummary headerValidation={headerValidation} />

      {finalAssessment && (
        <AtsFinalAssessmentSummary
          atsEnginePipelineResult={result}
          assessment={finalAssessment}
          keywordScores={result.keywordScoring?.keyword_scores ?? []}
        />
      )}
    </div>
  );
}
