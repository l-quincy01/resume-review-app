"use client";

import {
  AtsEnginePipelineResult,
  AtsEngineStage,
} from "@/types/AtsEngine/ats-engine.type";
import AtsEngineProgressiveHeader from "./ats-engine-progressive-header";
import AtsFinalAssessmentSummary from "./ats-final-assessment-summary";
import AtsHeaderValidationSummary from "./ats-header-validation-summary";
import {
  AtsFinalAssessmentSummarySkeleton,
  AtsHeaderValidationSummarySkeleton,
} from "./ats-engine-results-skeleton";

interface AtsEngineResultsProps {
  aiModel: string;
  completedStages: AtsEngineStage[];
  currentStage?: AtsEngineStage | null;
  error?: string | null;
  hasJobDescription: boolean;
  isComplete: boolean;
  isLoading: boolean;
  result?: Partial<AtsEnginePipelineResult> | null;
}

export default function AtsEngineResults({
  aiModel,
  completedStages,
  currentStage,
  error,
  hasJobDescription,
  isComplete,
  isLoading,
  result,
}: AtsEngineResultsProps) {
  const headerValidation = result?.headerValidation;
  const finalAssessment = result?.finalAssessment;
  const shouldShowFinalSkeleton =
    hasJobDescription && isLoading && !finalAssessment && !error;

  return (
    <div className="flex flex-col gap-4 border-t pt-4">
      <AtsEngineProgressiveHeader
        aiModel={aiModel}
        completedStages={completedStages}
        currentStage={currentStage}
        error={error}
        hasJobDescription={hasJobDescription}
        isComplete={isComplete}
        isLoading={isLoading}
        result={result}
      />

      {headerValidation && (
        <AtsHeaderValidationSummary headerValidation={headerValidation} />
      )}

      {!headerValidation && !error && <AtsHeaderValidationSummarySkeleton />}

      {finalAssessment && (
        <AtsFinalAssessmentSummary
          atsEnginePipelineResult={result as AtsEnginePipelineResult}
          assessment={finalAssessment}
          keywordScores={result?.keywordScoring?.keyword_scores ?? []}
        />
      )}

      {shouldShowFinalSkeleton && <AtsFinalAssessmentSummarySkeleton />}
    </div>
  );
}
