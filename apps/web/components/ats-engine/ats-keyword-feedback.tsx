import {
  AtsContextualKeywordScore,
  AtsRecommendation,
  AtsStrength,
} from "@/types/AtsEngine/ats-engine.type";
import type React from "react";

interface AtsKeywordFeedbackProps {
  atsStrength?: AtsStrength[];
  recommendations?: AtsRecommendation[];
  atsContextType?: AtsContextualKeywordScore[];
}

export default function AtsKeywordFeedback({
  atsStrength,
  recommendations,
  atsContextType,
}: AtsKeywordFeedbackProps) {
  const strengths = atsStrength ?? [];
  const keywordRecommendations = recommendations ?? [];
  const contextualKeywords = atsContextType ?? [];
  const stuffedKeywords = contextualKeywords.filter(isStuffedKeyword);
  const weakKeywords = contextualKeywords.filter(isWeakKeywordUsage);
  const hasFeedback =
    strengths.length > 0 ||
    keywordRecommendations.length > 0 ||
    weakKeywords.length > 0 ||
    stuffedKeywords.length > 0;

  return (
    <div className="flex flex-col gap-3">
      <div className="text-lg font-semibold">Keyword Feedback</div>

      {hasFeedback ? (
        <>
          <StrengthsSection strengths={strengths} />
          <div className="text-lg underline underline-offset-2 font-semibold">
            Recommendations
          </div>
          <RecommendationsSection recommendations={keywordRecommendations} />
          <StuffedKeywordRecommendationSection keywords={stuffedKeywords} />
          <WeakKeywordRecommendationSection keywords={weakKeywords} />
        </>
      ) : (
        <div className="rounded-md border p-3 text-sm text-muted-foreground">
          No keyword feedback available.
        </div>
      )}
    </div>
  );
}

function WeakKeywordRecommendationSection({
  keywords,
}: {
  keywords: AtsContextualKeywordScore[];
}) {
  if (keywords.length === 0) {
    return null;
  }

  return (
    <FeedbackSection title="Weak Keyword Usage">
      {keywords.map((keyword) => (
        <WeakKeywordCard
          key={`weak-recommendation-${keyword.keyword}`}
          keyword={keyword.keyword}
          meta={keyword.keyword_type}
          context={keyword.context}
          body={getWeakKeywordRecommendation(keyword)}
          variant="neutral"
        />
      ))}
    </FeedbackSection>
  );
}

function StuffedKeywordRecommendationSection({
  keywords,
}: {
  keywords: AtsContextualKeywordScore[];
}) {
  if (keywords.length === 0) {
    return null;
  }

  return (
    <FeedbackSection title="Stuffed Keywords">
      <div>
        Use this keyword in a truthful Work Experience or Projects bullet with
        an action verb and measurable result, or remove it if it is only listed
        without context.
      </div>
      {keywords.map((keyword, index) => (
        <div
          key={index}
          className="rounded-md border px-2.5 py-1 text-xs font-medium border-muted-foreground/30 bg-muted text-muted-foreground"
        >
          {keyword.keyword}
        </div>
      ))}
    </FeedbackSection>
  );
}

function StrengthsSection({ strengths }: { strengths: AtsStrength[] }) {
  if (strengths.length === 0) {
    return null;
  }

  return (
    <FeedbackSection title="Strengths">
      {strengths.map((strength) => (
        <FeedbackCard
          key={`strength-${strength.keyword}`}
          keyword={strength.keyword}
          meta={`${strength.keyword_type} · ${strength.score}/100`}
          context={strength.context}
          body={strength.reason}
          variant="positive"
        />
      ))}
    </FeedbackSection>
  );
}

function RecommendationsSection({
  recommendations,
}: {
  recommendations: AtsRecommendation[];
}) {
  if (recommendations.length === 0) {
    return null;
  }

  return (
    <div className="flex flex-col gap-2">
      <FeedbackSection title="Missing Keywords">
        <div>
          Add these keywords naturally into a relevant section in your resume.
          Include them with an action verb, metric or achivement. Also use the
          X-Y-Z formula.
        </div>
        {recommendations.map((recommendation, index) => (
          <div
            key={index}
            className="rounded-md border px-2.5 py-1 text-xs font-medium border-muted-foreground/30 bg-muted text-muted-foreground"
          >
            {recommendation.keyword}
          </div>
        ))}
      </FeedbackSection>
    </div>
  );
}

function FeedbackSection({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="flex flex-col gap-4 mt-2 w-full">
      <div className="flex flex-row w-full justify-between">
        <div className="text-md font-semibold">{title}</div>
        {title === "Missing Keywords" && (
          <span
            className={`rounded-md border px-2.5 py-1 text-xs font-medium    border-destructive/30 bg-destructive/10 text-destructive"`}
          >
            High Priority
          </span>
        )}
        {title === "Stuffed Keywords" && (
          <span
            className={`rounded-md border px-2.5 py-1 text-xs font-medium    border-destructive/30 bg-destructive/10 text-destructive"`}
          >
            High Priority
          </span>
        )}
        {title === "Weak Keyword Usage" && (
          <span
            className={`rounded-md border px-2.5 py-1 text-xs font-medium   border-yellow-500/30 bg-yellow-500/15 text-yellow-700 dark:text-yellow-300"`}
          >
            Moderate Priority
          </span>
        )}
      </div>
      <div className="flex flex-row flex-wrap gap-2">{children}</div>
    </div>
  );
}

function FeedbackCard({
  keyword,
  meta,
  context,
  body,
  variant,
}: {
  keyword: string;
  meta: string;
  context: string;
  body: string;
  variant: "positive" | "negative" | "neutral";
}) {
  const pillClassName = getVariantClassName(variant);

  return (
    <div className="flex flex-col gap-2 rounded-md border p-3 text-sm w-full">
      <div className="flex items-start justify-between gap-3">
        <div className="font-medium">{keyword}</div>
        <span
          className={`rounded-md border px-2.5 py-1 text-xs font-medium ${pillClassName}`}
        >
          {meta}
        </span>
      </div>
      <div className="text-muted-foreground">{context}</div>
      <div>{body}</div>
    </div>
  );
}
function WeakKeywordCard({
  keyword,
  meta,
  context,
  body,
  variant,
}: {
  keyword: string;
  meta: string;
  context: string;
  body: string;
  variant: "positive" | "negative" | "neutral";
}) {
  const pillClassName = getVariantClassName(variant);

  return (
    <div className="flex flex-col gap-2 rounded-md border p-3 text-sm w-full">
      <div className="flex flex-row items-center  ">
        <div className="font-medium rounded-md border px-2.5 py-1 text-xs border-muted-foreground/30 bg-muted text-muted-foreground">
          {keyword}
        </div>{" "}
        <span className=" px-2.5 py-1">{body}</span>
      </div>

      <div>
        Context:
        <span className="text-muted-foreground"> {context}</span>
      </div>
    </div>
  );
}

/*

        <div>
          Add these keywords naturally into a relevant section in your resume.
          Include them with an action verb, metric or achivement. Also use the
          X-Y-Z formula.
        </div>
        {recommendations.map((recommendation, index) => (
          <div
            key={index}
            className="rounded-md border px-2.5 py-1 text-xs font-medium border-muted-foreground/30 bg-muted text-muted-foreground"
          >
            {recommendation.keyword}
          </div>
        ))}
*/

function StuffedKeyWordCard({
  keyword,
  meta,
  context,
  body,
  variant,
}: {
  keyword: string;
  meta: string;
  context: string;
  body: string;
  variant: "positive" | "negative" | "neutral";
}) {
  const pillClassName = getVariantClassName(variant);

  return (
    <div className="flex flex-col gap-2 rounded-md border p-3 text-sm w-full">
      <div className="flex items-start justify-between gap-3">
        <div className="font-medium">{keyword}</div>
        <span
          className={`rounded-md border px-2.5 py-1 text-xs font-medium ${pillClassName}`}
        >
          {meta}
        </span>
      </div>
      <div className="text-muted-foreground">{context}</div>
      <div>{body}</div>
    </div>
  );
}

function getVariantClassName(variant: "positive" | "negative" | "neutral") {
  if (variant === "positive") {
    return "border-primary/30 bg-primary/10 text-primary";
  }

  if (variant === "negative") {
    return "border-destructive/30 bg-destructive/10 text-destructive";
  }

  return "border-muted-foreground/30 bg-muted text-muted-foreground";
}

function isWeakKeywordUsage(keyword: AtsContextualKeywordScore) {
  if (!keyword.present || isStuffedKeyword(keyword)) {
    return false;
  }

  return getQualitySignalCount(keyword) < 2 || !hasSectionPlacement(keyword);
}

function isStuffedKeyword(keyword: AtsContextualKeywordScore) {
  return (
    keyword.present &&
    !keyword.context_type.has_achievement &&
    !keyword.context_type.has_metric &&
    !keyword.context_type.has_action_verb &&
    !keyword.context_type.in_experience_section &&
    !keyword.context_type.in_project_section &&
    !keyword.context_type.in_summary_section
  );
}

function getWeakKeywordRecommendation(keyword: AtsContextualKeywordScore) {
  if (!keyword.context_type.has_action_verb) {
    return "Add an action verb around this keyword.";
  }

  if (!keyword.context_type.has_metric) {
    return "Add a measurable result or metric.";
  }

  if (!keyword.context_type.has_achievement) {
    return "Connect this keyword to a concrete achievement.";
  }

  if (!hasSectionPlacement(keyword)) {
    return "Use this keyword in Work Experience, Projects, or Summary where it genuinely fits.";
  }

  return "Strengthen this keyword with a clearer result-driven resume bullet.";
}

function getQualitySignalCount(keyword: AtsContextualKeywordScore) {
  return [
    keyword.context_type.has_achievement,
    keyword.context_type.has_metric,
    keyword.context_type.has_action_verb,
  ].filter(Boolean).length;
}

function hasSectionPlacement(keyword: AtsContextualKeywordScore) {
  return (
    keyword.context_type.in_experience_section ||
    keyword.context_type.in_project_section ||
    keyword.context_type.in_summary_section
  );
}
