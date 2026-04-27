import React from "react";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";
import { atsContent, spellingAndGrammar } from "@/types/atsReport.type";
import AtsSuggestedCorrection from "./ats-grammar-suggestions";

export interface props {
  atsContent: atsContent;
  spellingAndGrammar: spellingAndGrammar;
}

export default function AtsContent({ atsContent, spellingAndGrammar }: props) {
  return (
    <div className="flex flex-col ">
      <Accordion
        type="multiple"
        collapsible={true}
        className="w-full border-none bg-transparent  "
      >
        {atsContent.content.map((item, index) => (
          <AccordionItem
            className="border-none bg-transparent "
            key={index}
            value={item.section}
          >
            <AccordionTrigger className="font-bold text-md  ">
              <div className="border-b w-full pb-4">
                <div className="flex flex-row justify-between">
                  {item.section}
                  <div className="font-bold text-muted-foreground">
                    <span className="text-xl text-card-foreground ">
                      {" "}
                      {item.score / 10}{" "}
                    </span>
                    {"/"}10
                  </div>
                </div>
              </div>
            </AccordionTrigger>
            <AccordionContent className=" flex flex-col gap-2 items- text-sm bg-transparent ">
              <span>{item.summary}</span>

              <div className="flex flex-col gap-2 ">
                <span className="font-semibold text-sm"> Strengths</span>
                <ul className="pl-4 ">
                  {item.strengths.map((strength, index) => (
                    <li key={index} className="list-disc my-2 ">
                      {strength}
                    </li>
                  ))}
                </ul>
              </div>
              <div className="flex flex-col gap-2 ">
                <span className="font-semibold text-sm"> Weakness</span>
                <ul className="pl-4 ">
                  {item.weaknesses.map((weakness, index) => (
                    <li key={index} className="list-disc my-2 ">
                      {weakness}
                    </li>
                  ))}
                </ul>
              </div>

              <div className="flex flex-col gap-2 ">
                <span className="font-semibold text-sm"> Suggestions</span>
                <ul className="pl-4 ">
                  {item.suggestions.map((suggestion, index) => (
                    <li key={index} className="list-disc my-2 ">
                      <div className="flex flex-row items-center gap-2">
                        <span
                          className={`${suggestion.type === "High" ? "text-red-500" : suggestion.type === "Medium" ? "text-orange-400" : "text-blue-500"}`}
                        >
                          {suggestion.type}
                        </span>{" "}
                        {"—"} {suggestion.content}
                      </div>
                    </li>
                  ))}
                </ul>
              </div>

              <div className="flex flex-col gap-2 ">
                {item.suggestedRewrites && (
                  <span className="font-semibold text-sm">
                    {" "}
                    Suggested Rewrites
                  </span>
                )}

                {item.suggestedRewrites?.map((suggested, index) => (
                  <div key={index} className="flex flex-col ">
                    <div className="grid grid-cols-2 border  divide-x">
                      <div className="flex flex-col gap-2 p-2">
                        <span className=" uppercase text-orange-400">
                          Current
                        </span>
                        {suggested.current}
                      </div>
                      <div className="flex flex-col gap-2 p-2">
                        <span className="uppercase text-indigo-400">
                          Improved example
                        </span>
                        {suggested.suggestion}
                      </div>
                    </div>

                    <div className="border border-t-0 flex flex-col gap-2 p-2">
                      <span className="uppercase"> Explanation</span>
                      <div>{suggested.expplanation}</div>
                    </div>
                  </div>
                ))}
              </div>
            </AccordionContent>
          </AccordionItem>
        ))}
      </Accordion>

      <Accordion
        type="multiple"
        collapsible={true}
        className="w-full border-none bg-transparent  "
      >
        <AccordionItem
          className="border-none bg-transparent "
          key={"spellingAndGrammer"}
          value={"spellingAndGrammer"}
        >
          <AccordionTrigger className="font-bold text-md  ">
            <div className="border-b w-full pb-4">
              <div className="flex flex-row justify-between">
                Spelling & Grammar
                <div className="font-bold text-muted-foreground">
                  <span className="text-xl text-card-foreground ">
                    {" "}
                    {spellingAndGrammar.score / 10}{" "}
                  </span>
                  {"/"}10
                </div>
              </div>
            </div>
          </AccordionTrigger>

          <AccordionContent className=" flex flex-col gap-2 items- text-sm bg-transparent ">
            <AtsSuggestedCorrection
              contentType="Grammar"
              spellingAndGrammarContent={spellingAndGrammar.grammarSuggestions}
            />
            <AtsSuggestedCorrection
              contentType="Spelling"
              spellingAndGrammarContent={spellingAndGrammar.spellingSuggestions}
            />
            <AtsSuggestedCorrection
              contentType="PersonalPronouns"
              spellingAndGrammarContent={
                spellingAndGrammar.personalPronounCheck
              }
            />
            <AtsSuggestedCorrection
              contentType="PassiveVoiceCheck"
              spellingAndGrammarContent={spellingAndGrammar.passiveVoiceCheck}
            />
          </AccordionContent>
        </AccordionItem>
      </Accordion>
    </div>
  );
}
