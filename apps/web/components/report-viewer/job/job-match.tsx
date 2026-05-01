"use client";
import React from "react";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";
import { jobMatch } from "@/types/jobMatch.type";

interface props {
  jobMatch: jobMatch;
}

export default function JobMatch({ jobMatch }: props) {
  return (
    <div>
      <div className="flex flex-col gap-2 p-2">
        <div className="flex flex-row justify-between">
          <div className="flex flex-col">
            <div className=" text-lg font-semibold">
              {/* {jobMatch.name} Overall Job Match² */}
              How You Fit To This Role
            </div>
            {/* <div className=" text-sm text-muted-foreground">
              Generated with {"Claude Opus 4.6"}
            </div> */}
          </div>

          <div className="text-xl text-muted-foreground font-bold ">
            <span className="font-extrabold text-4xl text-card-foreground">
              {jobMatch.overallScore / 10}
            </span>
            {"/"}
            10
          </div>
        </div>

        <div className="flex flex-col gap-2 text-sm">
          <Accordion
            type="multiple"
            className="w-full border-none bg-transparent  "
          >
            <AccordionItem
              className="border-none bg-transparent "
              value={"Job"}
            >
              <AccordionTrigger className="font-bold text-md  ">
                <span className="border-b w-full pb-4"> How You Match</span>
              </AccordionTrigger>
              <AccordionContent className=" flex flex-col gap-2 items- text-sm bg-transparent ">
                <div className="grid grid-cols-2 border  divide-x">
                  {jobMatch.targetJob.map((job, index) => (
                    <div key={index} className="flex flex-col gap-2 p-2">
                      <div className="flex flex-row justify-between gap-2">
                        <span className="  text-orange-400">{job.type}</span>
                        <span>{job.score / 10}/10</span>
                      </div>
                      {job.content}
                    </div>
                  ))}
                </div>
              </AccordionContent>
            </AccordionItem>
          </Accordion>
        </div>
      </div>
    </div>
  );
}
