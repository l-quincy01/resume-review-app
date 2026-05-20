"use client";

import {
  AtsScoredKeyword,
  AtsTierBreakdown,
  AtsTierBreakdownItem,
} from "@/types/AtsEngine/ats-engine.type";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";

interface AtsTierBreakdownSummaryProps {
  tierBreakdown: AtsTierBreakdown;
  keywordScores: AtsScoredKeyword[];
}

const tiers: Array<{
  key: keyof AtsTierBreakdown;
  label: string;
}> = [
  { key: "tier_0", label: "Very important Keywords" },
  { key: "tier_1", label: "Important Keywords" },
  { key: "tier_2", label: "Less Important Keywords" },
  { key: "tier_3", label: "Least important Keywords" },
];

export default function AtsTierBreakdownSummary({
  tierBreakdown,
  keywordScores,
}: AtsTierBreakdownSummaryProps) {
  return (
    <div className="flex flex-col gap-2 rounded-md  border-0 border-none p-3 h-fit">
      <div className="text-lg font-semibold">Keyword Ranking Breakdown</div>
      <div className="flex flex-col gap-2 divide-accent divide-y ">
        {tiers.map((tier) => (
          <TierRow
            key={tier.key}
            label={tier.label}
            item={tierBreakdown[tier.key]}
            presentKeywords={getPresentKeywordsForTier(keywordScores, tier.key)}
          />
        ))}
      </div>
    </div>
  );
}

function TierRow({
  label,
  item,
  presentKeywords,
}: {
  label: string;
  item: AtsTierBreakdownItem;
  presentKeywords: string[];
}) {
  return (
    <div className="flex flex-col gap-2 text-sm my-4 pb-4">
      <Accordion
        type="multiple"
        defaultValue={["default"]}
        className="rounded-md border-none border-0 "
      >
        <AccordionItem value={"default"}>
          <AccordionTrigger className="w-full">
            <div className="flex items-center justify-between gap-3 w-full">
              <span className="text-muted-foreground">{label}</span>
              <span>
                {item.average_score}% · {item.present}/{item.total}
              </span>
            </div>
          </AccordionTrigger>

          <AccordionContent className="flex flex-col gap-4 h-fit">
            {presentKeywords.length > 0 && (
              <div className="text-sm text-muted-foreground flex flex-col gap-2">
                <div className=" flex flex-row flex-wrap gap-2">
                  Present:
                  {presentKeywords.map((presentKeyword, index) => (
                    <span
                      key={index}
                      className="rounded-md border px-2.5 py-1 text-xs font-light border-primary/30 bg-primary/10 text-primary"
                    >
                      {presentKeyword}
                    </span>
                  ))}
                </div>
              </div>
            )}
            {item.missing.length > 0 && (
              <div className="text-sm text-muted-foreground flex flex-col gap-2">
                <div className=" flex flex-row flex-wrap gap-2">
                  Missing:
                  {item.missing.map((missingKeyword, index) => (
                    <span
                      key={index}
                      className="rounded-md border px-2.5 py-1 text-xs font-light  border-destructive/30 bg-destructive/10 text-destructive"
                    >
                      {missingKeyword}
                    </span>
                  ))}
                </div>
              </div>
            )}
          </AccordionContent>
        </AccordionItem>
      </Accordion>
    </div>
  );
}

function getPresentKeywordsForTier(
  keywordScores: AtsScoredKeyword[],
  tierKey: keyof AtsTierBreakdown,
) {
  const tier = Number(tierKey.replace("tier_", ""));

  return keywordScores
    .filter(
      (keywordScore) => keywordScore.tier === tier && keywordScore.present,
    )
    .map((keywordScore) => keywordScore.keyword);
}
