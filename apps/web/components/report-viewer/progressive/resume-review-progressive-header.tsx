"use client";

import { X } from "lucide-react";
import { Progress } from "@/components/ui/progress";
import ShimmerText from "@/components/ui/shimmer-text";
import { Typewriter } from "@/components/ui/typewriter";
import { loadingWords } from "@/constants/constants";
import { atsContent } from "@/types/atsReport.type";
import { ResumeReviewStreamSection } from "@/types/payload/response-payload";

interface ResumeReviewProgressiveHeaderProps {
  atsContent?: atsContent;
  aiModel: string;
  isComplete: boolean;
  progress: number;
  completedSections: ResumeReviewStreamSection[];
  warnings?: string[];
}

const pendingCards = [
  "ATS Compatibility",
  "Format & Presentation",
  "Contact Information",
  "Work Experience",
  "Skills",
  "Projects",
  "Education",
];

export default function ResumeReviewProgressiveHeader({
  atsContent,
  aiModel,
  isComplete,
  progress,
  completedSections,
  warnings = [],
}: ResumeReviewProgressiveHeaderProps) {
  const scores = atsContent?.content
    .map((item) => item.score)
    .filter((score) => Number.isFinite(score)) ?? [];
  const totalScore = scores.reduce((sum, score) => sum + score, 0);
  const avgScore = scores.length === 0 ? 0 : Math.round(totalScore / scores.length);
  const title = atsContent?.resumeName
    ? `${atsContent.resumeName} Resume Review Report¹`
    : "Resume Review Report¹";

  return (
    <div className="flex flex-col gap-4 p-2">
      {isComplete ? (
        <StatusBanner
          title="Resume reviewed successfully"
          message="The report is ready. Export or review each section before applying changes to your resume."
          tone="success"
        />
      ) : (
        <div className="flex flex-col gap-3 rounded-md border p-4">
          <div className="flex items-center justify-between gap-3">
            <ShimmerText className="text-sm text-muted-foreground">
              <Typewriter
                words={loadingWords}
                speed={40}
                delayBetweenWords={2000}
                cursor={false}
              />
            </ShimmerText>
            <span className="text-sm font-medium text-muted-foreground">
              {Math.round(progress)}%
            </span>
          </div>
          <Progress value={progress} className="w-full" />
          <div className="text-sm text-muted-foreground">
            AI is reviewing your resume and may take a few minutes.
          </div>
        </div>
      )}

      {warnings.length > 0 && (
        <StatusBanner
          title="Some sections could not be generated"
          message={warnings.join(" ")}
          tone="warning"
        />
      )}

      <div className="flex flex-col gap-1 md:flex-row md:items-start md:justify-between">
        <div className="flex flex-col">
          <div className="text-lg font-semibold">{title}</div>
          {isComplete && (
            <div className="text-sm text-muted-foreground">
              Generated with {aiModel}
            </div>
          )}
          {!isComplete && completedSections.length > 0 && (
            <div className="text-sm text-muted-foreground">
              {completedSections.length} of 4 sections ready
            </div>
          )}
        </div>

        <div className="text-xl text-muted-foreground font-bold">
          <span className="font-extrabold text-4xl text-card-foreground">
            {atsContent ? formatScore(avgScore) : "—"}
          </span>
          /10
        </div>
      </div>

      <div className="grid grid-cols-1 border md:grid-cols-2">
        {atsContent
          ? atsContent.content.map((item, index) => (
              <ScoreCard
                key={`${item.section}-${index}`}
                title={item.section}
                score={item.score}
              />
            ))
          : pendingCards.map((title) => (
              <ScoreCard key={title} title={title} score={null} />
            ))}
      </div>
    </div>
  );
}

function StatusBanner({
  title,
  message,
  tone,
}: {
  title: string;
  message: string;
  tone: "success" | "warning";
}) {
  const toneClass =
    tone === "success"
      ? "border-green-500/20 bg-green-500/10 text-green-800 dark:text-green-200"
      : "border-destructive/30 bg-destructive/10 text-destructive";

  return (
    <div className={`flex items-start justify-between gap-4 rounded-md border p-4 ${toneClass}`}>
      <div className="flex flex-col gap-1">
        <div className="font-semibold">{title}</div>
        <div className="text-sm">{message}</div>
      </div>
      <X className="h-4 w-4 shrink-0" />
    </div>
  );
}

function ScoreCard({
  title,
  score,
}: {
  title: string;
  score: number | null;
}) {
  const safeScore = typeof score === "number" && Number.isFinite(score) ? score : null;

  return (
    <div className="flex min-h-28 flex-col justify-between gap-4 border-b border-r p-4">
      <div className="text-base font-semibold">{title}</div>
      <div className="flex items-center justify-between gap-4">
        <Progress
          value={safeScore ?? 0}
          className={`h-2 ${safeScore === null ? "opacity-50" : ""}`}
        />
        <div className="min-w-16 text-right text-xl font-bold text-muted-foreground">
          <span className="text-2xl text-card-foreground">
            {safeScore === null ? "—" : formatScore(safeScore)}
          </span>
          {safeScore !== null && "/10"}
        </div>
      </div>
    </div>
  );
}

function formatScore(score: number) {
  return Number.isFinite(score) ? score / 10 : 0;
}
