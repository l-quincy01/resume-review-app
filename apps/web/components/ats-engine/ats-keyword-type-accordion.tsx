"use client";

import { Accordion } from "@/components/ui/accordion";

import { AtsScoredKeyword } from "@/types/AtsEngine/ats-engine.type";
import KeywordGroupAccordionItem from "./accordion-group/keywordGroupAccordionItem";

interface AtsKeywordTypeAccordionProps {
  keywordScores: AtsScoredKeyword[];
}

export default function AtsKeywordTypeAccordion({
  keywordScores,
}: AtsKeywordTypeAccordionProps) {
  const atomicKeywords = getKeywordGroups(keywordScores, "single_word");
  const nonAtomicKeywords = getKeywordGroups(keywordScores, "multi_word");

  return (
    <Accordion
      type="multiple"
      defaultValue={["atomic-keywords", "non-atomic-keywords"]}
      className="rounded-md  border-0 border-none"
    >
      <KeywordGroupAccordionItem
        value="atomic-keywords"
        title="Atomic Keywords"
        groups={atomicKeywords}
      />
      <KeywordGroupAccordionItem
        value="non-atomic-keywords"
        title="Non Atomic Keywords"
        groups={nonAtomicKeywords}
      />
    </Accordion>
  );
}

interface KeywordGroups {
  missing: string[];
  present: string[];
}
function getKeywordGroups(
  keywordScores: AtsScoredKeyword[],
  keywordType: "single_word" | "multi_word",
): KeywordGroups {
  const matchingKeywords = keywordScores.filter(
    (keyword) => keyword.keyword_type === keywordType,
  );

  return {
    missing: matchingKeywords
      .filter((keyword) => !keyword.present)
      .map((keyword) => keyword.keyword),
    present: matchingKeywords
      .filter((keyword) => keyword.present)
      .map((keyword) => keyword.keyword),
  };
}
