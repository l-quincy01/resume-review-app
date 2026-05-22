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
