import React from "react";
import {
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";
import KeywordPillSection from "./keyword-accordion/keywordPillSection";

interface KeywordGroups {
  missing: string[];
  present: string[];
}

export default function KeywordGroupAccordionItem({
  value,
  title,
  groups,
}: {
  value: string;
  title: string;
  groups: KeywordGroups;
}) {
  return (
    <AccordionItem value={value}>
      <AccordionTrigger className="font-semibold">{title}</AccordionTrigger>
      <AccordionContent className="flex flex-col gap-4 h-fit">
        <KeywordPillSection
          title="Missing Keywords"
          keywords={groups.missing}
          variant="missing"
        />
        <KeywordPillSection
          title="Present Keywords"
          keywords={groups.present}
          variant="present"
        />
      </AccordionContent>
    </AccordionItem>
  );
}
