"use client";

import {
  AtsEnginePipelineResult,
  AtsFinalAssessmentResponse,
  AtsScoredKeyword,
} from "@/types/AtsEngine/ats-engine.type";
import AtsCoverageSummary from "./ats-coverage-summary";
import AtsKeywordResultList from "./ats-keyword-result-list";
import AtsKeywordTypeAccordion from "./ats-keyword-type-accordion";
import AtsTierBreakdownSummary from "./ats-tier-breakdown-summary";
import ScoreTile from "./score-tile";
// import { Accordion } from "@/components/ui/accordion";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";
import KeywordTypePresenceTile from "./tiles/KeywordTypePresenceTile";
import AtsKeywordUsage from "./ats-keyword-usage";
import AtsKeywordFeedback from "./ats-keyword-feedback";

interface AtsFinalAssessmentSummaryProps {
  assessment: AtsFinalAssessmentResponse;
  keywordScores: AtsScoredKeyword[];
  atsEnginePipelineResult: AtsEnginePipelineResult;
}

export default function AtsFinalAssessmentSummary({
  assessment,
  keywordScores,
  atsEnginePipelineResult,
}: AtsFinalAssessmentSummaryProps) {
  const singleWordMetrics = getKeywordTypeMetrics(keywordScores, "single_word");
  const multiWordMetrics = getKeywordTypeMetrics(keywordScores, "multi_word");

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
    <div className="flex flex-col gap-4 border-t" data-testid="ats-final-assessment-summary">
      <Accordion
        type="multiple"
        defaultValue={["default"]}
        className="rounded-md border-0 border-none"
      >
        <AccordionItem value={"default"}>
          <AccordionTrigger className="w-full">
            <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between w-full">
              <div className="flex flex-col">
                <div className="text-lg font-semibold">
                  Weighted Keyword score
                </div>
              </div>

              <div className="text-xl text-muted-foreground font-bold">
                <span className="font-extrabold text-4xl text-card-foreground">
                  {scoreAggregate(assessment.overall_keyword_score)}
                </span>
                /100
              </div>
            </div>
          </AccordionTrigger>

          <AccordionContent className="flex flex-col gap-4 h-fit">
            <div className="flex flex-col gap-2 w-full"></div>

            <div className="text-lg font-semibold">Keyword Coverage</div>
            <AtsCoverageSummary
              tierBreakdown={assessment.tier_breakdown}
              keywordScores={keywordScores}
              coverage={assessment.coverage}
            />

            <div className="flex flex-col gap-2">
              <AtsTierBreakdownSummary
                tierBreakdown={assessment.tier_breakdown}
                keywordScores={keywordScores}
              />
            </div>

            {/* <AtsKeywordTypeAccordion keywordScores={keywordScores} /> */}

            <AtsKeywordUsage
              atsContextType={
                atsEnginePipelineResult.contextualScoring?.keyword_scores
              }
              atsCriticalGap={
                atsEnginePipelineResult.finalAssessment?.critical_gaps
              }
            />

            <AtsKeywordFeedback
              atsStrength={atsEnginePipelineResult.finalAssessment?.strengths}
              recommendations={
                atsEnginePipelineResult.finalAssessment?.recommendations
              }
              atsContextType={
                atsEnginePipelineResult.contextualScoring?.keyword_scores
              }
            />
          </AccordionContent>
        </AccordionItem>
      </Accordion>
    </div>
  );
}

interface KeywordTypeMetrics {
  present: number;
  total: number;
  presenceRate: number;
}

function getKeywordTypeMetrics(
  keywordScores: AtsScoredKeyword[],
  keywordType: "single_word" | "multi_word",
): KeywordTypeMetrics {
  const matchingKeywords = keywordScores.filter(
    (keyword) => keyword.keyword_type === keywordType,
  );
  const present = matchingKeywords.filter((keyword) => keyword.present).length;
  const total = matchingKeywords.length;

  return {
    present,
    total,
    presenceRate: total === 0 ? 0 : Math.round((present / total) * 100),
  };
}
