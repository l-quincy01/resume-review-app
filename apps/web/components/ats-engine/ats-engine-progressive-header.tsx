"use client";

import { Check, X } from "lucide-react";
import { Progress } from "@/components/ui/progress";
import ShimmerText from "@/components/ui/shimmer-text";
import { Typewriter } from "@/components/ui/typewriter";
import {
  AtsEnginePipelineResult,
  AtsEngineStage,
} from "@/types/AtsEngine/ats-engine.type";

interface AtsEngineProgressiveHeaderProps {
  aiModel: string;
  currentStage?: AtsEngineStage | null;
  completedStages: AtsEngineStage[];
  error?: string | null;
  hasJobDescription: boolean;
  isComplete: boolean;
  isLoading: boolean;
  result?: Partial<AtsEnginePipelineResult> | null;
}

const stageLabels: Record<AtsEngineStage, string> = {
  "header-validation": "Header validation",
  "keyword-extraction": "Keyword extraction",
  "keyword-analysis": "Keyword analysis",
  "keyword-scoring": "Keyword scoring",
  "final-assessment": "Final assessment",
};

const atsLoadingWords = [
  "Checking out resume...",
  "Getting keywords out...",
  "Caving resume structure...",
  "Lining up contextual matches...",
  "Analysing keyword usage...",
  "Preparing ATS report...",
];

export default function AtsEngineProgressiveHeader({
  aiModel,
  currentStage,
  completedStages,
  error,
  hasJobDescription,
  isComplete,
  isLoading,
  result,
}: AtsEngineProgressiveHeaderProps) {
  const totalStages = hasJobDescription ? 5 : 1;
  const progress = isComplete
    ? 100
    : Math.min(95, Math.round((completedStages.length / totalStages) * 100));
  const score = result?.finalAssessment?.ats_readiness_score;

  function scoreAggregate(score: number) {
    let finalScore = 0;

    if (score > 35) {
      finalScore = score + 40;
    } else if (score > 30) {
      finalScore = score + 35;
    } else if (score > 25) {
      finalScore = score + 30;
    } else if (score > 15 && score < 25) {
      finalScore = score + 25;
    } else {
      finalScore = score;
    }

    finalScore = finalScore > 100 ? 100 : finalScore;

    return finalScore;
  }

  return (
    <div className="flex flex-col gap-4 p-2">
      {error ? (
        <StatusBanner
          title="ATS analysis failed"
          message={error}
          tone="error"
          testId="ats-engine-error"
        />
      ) : isComplete ? (
        <StatusBanner
          title="ATS analysis completed"
          message="The quantitative ATS report is ready."
          tone="success"
        />
      ) : isLoading ? (
        <div className="flex flex-col gap-3 rounded-md border p-4">
          <div className="flex items-center justify-between gap-3">
            <ShimmerText className="text-sm text-muted-foreground">
              <Typewriter
                words={atsLoadingWords}
                speed={40}
                delayBetweenWords={2000}
                cursor={false}
              />
            </ShimmerText>
            <span className="text-sm font-medium text-muted-foreground">
              {progress}%
            </span>
          </div>
          <Progress value={progress} className="w-full" />
          <div className="text-sm text-muted-foreground">
            {currentStage
              ? `${stageLabels[currentStage]} is running.`
              : "Preparing ATS analysis."}
          </div>
        </div>
      ) : null}

      <div className="flex flex-col gap-1 md:flex-row md:items-start md:justify-between">
        <div className="flex flex-col">
          <div className="text-lg font-semibold">Quantitative ATS Analysis</div>
          <div className="text-sm text-muted-foreground">
            {completedStages.length} of {totalStages} stages ready
            {aiModel ? ` · ${aiModel}` : ""}
          </div>
        </div>

        {score && (
          <div className="text-xl text-muted-foreground font-bold">
            <span className="font-extrabold text-4xl text-card-foreground">
              {score && scoreAggregate(score)}
            </span>
            /100
          </div>
        )}
      </div>

      <div className="flex flex-row flex-wrap gap-2">
        {getVisibleStages(hasJobDescription).map((stage) => {
          const isDone = completedStages.includes(stage);
          const isActive = currentStage === stage && isLoading;

          return (
            <span
              key={stage}
              className={`inline-flex items-center gap-2 rounded-md border px-2.5 py-1 text-xs font-medium ${
                isDone
                  ? "border-primary/30 bg-primary/10 text-primary"
                  : isActive
                    ? "border-muted-foreground/30 text-muted-foreground"
                    : "text-muted-foreground"
              }`}
            >
              {isDone && <Check className="h-3 w-3" />}
              {stageLabels[stage]}
            </span>
          );
        })}
      </div>
    </div>
  );
}

function StatusBanner({
  title,
  message,
  tone,
  testId,
}: {
  title: string;
  message: string;
  tone: "success" | "error";
  testId?: string;
}) {
  const toneClass =
    tone === "success"
      ? "border-green-500/20 bg-green-500/10 text-green-800 dark:text-green-200"
      : "border-destructive/30 bg-destructive/10 text-destructive";

  return (
    <div
      className={`flex items-start justify-between gap-4 rounded-md border p-4 ${toneClass}`}
      data-testid={testId}
    >
      <div className="flex flex-col gap-1">
        <div className="font-semibold">{title}</div>
        <div className="text-sm">{message}</div>
      </div>
      <X className="h-4 w-4 shrink-0" />
    </div>
  );
}

function getVisibleStages(hasJobDescription: boolean): AtsEngineStage[] {
  if (!hasJobDescription) {
    return ["header-validation"];
  }

  return [
    "header-validation",
    "keyword-extraction",
    "keyword-analysis",
    "keyword-scoring",
    "final-assessment",
  ];
}
