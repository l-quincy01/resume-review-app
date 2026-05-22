import React from "react";
import { Progress } from "@/components/ui/progress";
import { atsContent } from "@/types/atsReport.type";

export interface props {
  atsContent: atsContent;
}

export default function AtsHeader({ atsContent }: props) {
  const scores = atsContent.content
    .map((item) => item.score)
    .filter((score) => Number.isFinite(score));
  const totalScore = scores.reduce((sum, score) => sum + score, 0);
  const avgScore = scores.length === 0 ? 0 : Math.round(totalScore / scores.length);

  return (
    <div className="flex flex-col gap-2 p-2">
      <div className="flex flex-row justify-between">
        <div className="flex flex-col">
          <div className=" text-lg font-semibold">
            {atsContent.resumeName} Resume Review Report¹
          </div>
        </div>

        <div className="text-xl text-muted-foreground font-bold ">
          <span className="font-extrabold text-4xl text-card-foreground">
            {avgScore / 10}
          </span>
          {"/"}
          10
        </div>
      </div>

      <div className="grid grid-cols-3 gap-2  ">
        {atsContent.content.map((item, index) => (
          <div
            key={index}
            className="flex flex-col gap-2 p-2 py-4 items-start border rounded-lg"
          >
            <div className="flex flex-row w-full justify-between items-start text-xs">
              {" "}
              <span className="font-semibold "> {item.section}</span>
              <span>{formatScore(item.score)}/10</span>
            </div>
            <Progress value={toSafeScore(item.score)} />
          </div>
        ))}
      </div>
    </div>
  );
}

function formatScore(score: number) {
  return toSafeScore(score) / 10;
}

function toSafeScore(score: number) {
  return Number.isFinite(score) ? score : 0;
}
