import {
  AtsContextualKeywordScore,
  AtsCriticalGap,
  AtsTierBreakdown,
} from "@/types/AtsEngine/ats-engine.type";
import type React from "react";

interface AtsKeywordUsageProps {
  atsContextType?: AtsContextualKeywordScore[];
  atsCriticalGap?: AtsCriticalGap[];
}

export default function AtsKeywordUsage({
  atsContextType,
  atsCriticalGap,
}: AtsKeywordUsageProps) {
  if (!atsContextType) {
    return (
      <KeywordUsageShell>
        <div className="text-sm text-muted-foreground">
          No keyword usage data available.
        </div>
      </KeywordUsageShell>
    );
  }

  const presentKeywords = atsContextType.filter((keyword) => keyword.present);

  if (presentKeywords.length === 0) {
    return (
      <KeywordUsageShell>
        <div className="text-sm text-muted-foreground">
          No present keywords to analyse for usage.
        </div>
      </KeywordUsageShell>
    );
  }

  const stuffedKeywords = presentKeywords.filter(isStuffedKeyword);
  const criticalGaps = atsCriticalGap ?? [];

  return (
    <KeywordUsageShell>
      <div className="flex flex-col gap-2">
        {presentKeywords.map((keyword) => (
          <KeywordUsageCard key={keyword.keyword} keyword={keyword} />
        ))}
      </div>
      <WeakUsageSection keywords={stuffedKeywords} />
      <StuffedKeywordsBlock keywords={stuffedKeywords} />
      <CriticalGapsSection gaps={criticalGaps} />
    </KeywordUsageShell>
  );
}

function KeywordUsageShell({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-2 rounded-md ">
      <div className="text-lg font-semibold">Keyword Usage</div>
      {children}
    </div>
  );
}

function KeywordUsageCard({ keyword }: { keyword: AtsContextualKeywordScore }) {
  const qualitySignals = getActiveQualitySignals(keyword);
  const sectionSignals = getActiveSectionSignals(keyword);
  const hasStrongQuality = qualitySignals.length >= 2;
  const hasSectionPlacement = sectionSignals.length > 0;

  return (
    <div className="flex flex-col gap-3 rounded-md border p-3 text-sm">
      <div className="flex items-start justify-between gap-3">
        <div className="flex flex-col gap-1">
          <div className="font-medium">{keyword.keyword}</div>
          <div className="text-muted-foreground">{keyword.context}</div>
        </div>
        <div className="text-xs text-muted-foreground">
          {keyword.keyword_type}
        </div>
      </div>

      <SignalPills signals={[...qualitySignals, ...sectionSignals]} />

      <FeedbackLine
        isPositive={hasStrongQuality}
        text={
          hasStrongQuality
            ? "Strong contextual use: this keyword is supported by multiple quality signals."
            : getQualitySuggestion(keyword)
        }
      />
      <FeedbackLine
        isPositive={hasSectionPlacement}
        text={
          hasSectionPlacement
            ? "Placed in a relevant resume section."
            : "Use this keyword in Work Experience, Projects, or Summary where it genuinely fits."
        }
      />

      <ResumeContextUsage keyword={keyword} />
    </div>
  );
}

function ResumeContextUsage({
  keyword,
}: {
  keyword: AtsContextualKeywordScore;
}) {
  return (
    <div className="flex flex-col gap-2 rounded-md border bg-muted/20 p-3">
      <div className="text-xs font-medium text-muted-foreground">
        Resume Context
      </div>
      {keyword.evidence.length > 0 ? (
        <div className="flex flex-col gap-2">
          {keyword.evidence.map((evidence) => (
            <div
              key={`${keyword.keyword}-${evidence.section}-${evidence.matched_term}-${evidence.text}`}
              className="flex flex-col gap-1"
            >
              <div className="flex flex-row flex-wrap gap-2 text-xs text-muted-foreground">
                <span>{evidence.section}</span>
                <span>Matched: {evidence.matched_term}</span>
              </div>
              <div className="text-sm">{evidence.text}</div>
            </div>
          ))}
        </div>
      ) : (
        <div className="text-sm text-muted-foreground">
          No resume snippet returned for this keyword.
        </div>
      )}
    </div>
  );
}

function StuffedKeywordsBlock({
  keywords,
}: {
  keywords: AtsContextualKeywordScore[];
}) {
  return (
    <div className="flex flex-col gap-2 rounded-md border border-destructive/30 bg-destructive/10 p-3">
      <div className="font-semibold text-destructive">Stuffed Keywords</div>
      <div className="text-sm text-muted-foreground">
        Keywords found in the resume but not supported by achievement, metric,
        action, or section context. ATS systems penalise keyword stuffing.
      </div>
      {keywords.length > 0 ? (
        <div className="flex flex-row flex-wrap gap-2">
          {keywords.map((keyword) => (
            <span
              key={`stuffed-${keyword.keyword}`}
              className="rounded-md border border-destructive/30 bg-background px-2.5 py-1 text-xs font-medium text-destructive"
            >
              {keyword.keyword}
            </span>
          ))}
        </div>
      ) : (
        <div className="text-sm text-muted-foreground">
          No stuffed keywords detected.
        </div>
      )}
    </div>
  );
}

function CriticalGapsSection({ gaps }: { gaps: AtsCriticalGap[] }) {
  if (gaps.length === 0) {
    return null;
  }

  return (
    <UsageProblemSection title="Critical Gaps">
      {gaps.map((gap) => (
        <UsageProblemCard
          key={`usage-gap-${gap.keyword}`}
          keyword={gap.keyword}
          meta={gap.tier}
          context={gap.context}
          body={gap.reason}
        />
      ))}
    </UsageProblemSection>
  );
}

function WeakUsageSection({
  keywords,
}: {
  keywords: AtsContextualKeywordScore[];
}) {
  if (keywords.length === 0) {
    return null;
  }

  return (
    <UsageProblemSection title="Weak Keyword Usage">
      {keywords.map((keyword) => (
        <UsageProblemCardWeakKeyword
          key={`usage-weak-${keyword.keyword}`}
          keyword={keyword.keyword}
          tier={keyword.tier}
          context={keyword.context}
          body="Keyword appears in the resume, but it is not supported by achievement, metric, action, or section context."
        />
      ))}
    </UsageProblemSection>
  );
}

function UsageProblemSection({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="flex flex-col gap-2">
      <div className="text-sm font-semibold">{title}</div>
      <div className="flex flex-col gap-2">{children}</div>
    </div>
  );
}

function UsageProblemCardWeakKeyword({
  keyword,
  tier,
  context,
  body,
}: {
  keyword: string;
  tier: number;
  context: string;
  body: string;
}) {
  const tiers = [
    "Very important Keyword",
    "Important Keyword",
    "Less Important Keyword",
    "Least important Keyword",
  ];
  const keywordImportanceColors = [
    "rounded-md border border-red-500/30 bg-red-500/15 px-2.5 py-1 text-xs font-medium text-red-700 dark:text-red-300",

    "rounded-md border border-orange-500/30 bg-orange-500/15 px-2.5 py-1 text-xs font-medium text-orange-700 dark:text-orange-300",

    "rounded-md border border-yellow-500/30 bg-yellow-500/15 px-2.5 py-1 text-xs font-medium text-yellow-700 dark:text-yellow-300",

    "rounded-md border border-blue-500/30 bg-blue-500/15 px-2.5 py-1 text-xs font-medium text-blue-700 dark:text-blue-300",
  ];

  return (
    <div className="flex flex-col gap-2 rounded-md border p-3 text-sm bg-accent/30 ">
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-center">
          <span className="font-light text-muted-foreground"> Keyword |</span>
          <div className="font-semibold px-2.5 py-1  border bg-white dark:bg-black   text-accent-foreground rounded-md">
            {keyword}
          </div>
        </div>
        <span className={keywordImportanceColors[tier]}>{tiers[tier]}</span>
      </div>
      <div className="text-muted-foreground">{context}</div>
      <div className="">{body}</div>
    </div>
  );
}
function UsageProblemCard({
  keyword,
  meta,
  context,
  body,
}: {
  keyword: string;
  meta: number;
  context: string;
  body: string;
}) {
  const tiers = [
    "Very important Keyword",
    "Important Keyword",
    "Less Important Keyword",
    "Least important Keyword",
  ];
  const keywordImportanceColors = [
    "rounded-md border border-red-500/30 bg-red-500/15 px-2.5 py-1 text-xs font-medium text-red-700 dark:text-red-300",

    "rounded-md border border-orange-500/30 bg-orange-500/15 px-2.5 py-1 text-xs font-medium text-orange-700 dark:text-orange-300",

    "rounded-md border border-yellow-500/30 bg-yellow-500/15 px-2.5 py-1 text-xs font-medium text-yellow-700 dark:text-yellow-300",

    "rounded-md border border-blue-500/30 bg-blue-500/15 px-2.5 py-1 text-xs font-medium text-blue-700 dark:text-blue-300",
  ];

  return (
    <div className="flex flex-col gap-2 rounded-md border p-3 text-sm bg-accent/30 ">
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-center">
          <span className="font-light text-muted-foreground"> Keyword |</span>
          <div className="font-semibold px-2.5 py-1  border bg-white dark:bg-black   text-accent-foreground rounded-md">
            {keyword}
          </div>
        </div>
        <span className={keywordImportanceColors[meta]}>{tiers[meta]}</span>
      </div>
      <div className="">
        <span className=" font-semibold">Context in Job Description: </span>
        <div className="">
          <span className="text-muted-foreground  ">
            {" "}
            {context}{" "}
            {/* <span className="text-[0.6rem] p-1  border rounded-md">
              {" "}
              Job Title{" "}
            </span> */}
          </span>
        </div>
      </div>
      <div className="text-red-500">{body}</div>
    </div>
  );
}

function SignalPills({ signals }: { signals: string[] }) {
  if (signals.length === 0) {
    return null;
  }

  return (
    <div className="flex flex-row flex-wrap gap-2">
      {signals.map((signal) => (
        <span
          key={signal}
          className="rounded-md border border-primary/30 bg-primary/10 px-2.5 py-1 text-xs font-medium text-primary"
        >
          {signal}
        </span>
      ))}
    </div>
  );
}

function FeedbackLine({
  isPositive,
  text,
}: {
  isPositive: boolean;
  text: string;
}) {
  const className = isPositive
    ? "border-primary/30 bg-primary/10 text-primary"
    : "border-destructive/30 bg-destructive/10 text-destructive";

  return (
    <div className={`rounded-md border px-3 py-2 text-xs ${className}`}>
      {text}
    </div>
  );
}

function getActiveQualitySignals(keyword: AtsContextualKeywordScore) {
  const signals: string[] = [];

  if (keyword.context_type.has_achievement) {
    signals.push("Achievement");
  }
  if (keyword.context_type.has_metric) {
    signals.push("Metric");
  }
  if (keyword.context_type.has_action_verb) {
    signals.push("Action Verb");
  }

  return signals;
}

function getActiveSectionSignals(keyword: AtsContextualKeywordScore) {
  const signals: string[] = [];

  if (keyword.context_type.in_experience_section) {
    signals.push("Experience");
  }
  if (keyword.context_type.in_project_section) {
    signals.push("Projects");
  }
  if (keyword.context_type.in_summary_section) {
    signals.push("Summary");
  }

  return signals;
}

function getQualitySuggestion(keyword: AtsContextualKeywordScore) {
  if (!keyword.context_type.has_action_verb) {
    return "Add an action verb around this keyword.";
  }

  if (!keyword.context_type.has_metric) {
    return "Add a measurable result or metric.";
  }

  return "Use an X-Y-Z bullet formula: achieved X, measured by Y, by doing Z.";
}

function isStuffedKeyword(keyword: AtsContextualKeywordScore) {
  return (
    !keyword.context_type.has_achievement &&
    !keyword.context_type.has_metric &&
    !keyword.context_type.has_action_verb &&
    !keyword.context_type.in_experience_section &&
    !keyword.context_type.in_project_section &&
    !keyword.context_type.in_summary_section
  );
}
