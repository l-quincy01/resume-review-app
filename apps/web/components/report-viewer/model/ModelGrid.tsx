"use client";
import React from "react";

import clsx from "clsx";
import { useAIModelStore } from "@/stores/store";

const models = [
  {
    name: "GPT-4.1 Mini",
    company: "OpenAI",
    value: "gpt-4.1-mini",
  },
  {
    name: "GPT-5 Mini",
    company: "OpenAI",
    value: "gpt-5-mini",
  },
  {
    name: "GPT-5 Nano",
    company: "OpenAI",
    value: "gpt-5-nano",
  },
  {
    name: "GPT-5.4",
    company: "OpenAI",
    value: "gpt-5.4",
  },
];

export default function ModelGrid() {
  const aiModel = useAIModelStore((state) => state.aiModel);
  const setAIModel = useAIModelStore((state) => state.setAIModel);

  return (
    <div className="p-2">
      <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
        {models.map((model) => {
          const isSelected = aiModel === model.value;

          return (
            <div
              key={model.value}
              onClick={() => setAIModel(model.value)}
              className={clsx(
                "p-3 flex flex-col gap-1 border rounded-md cursor-pointer transition",
                "hover:bg-accent",
                isSelected && "border-primary bg-accent",
              )}
            >
              <div className="font-semibold text-sm">{model.name}</div>
              <div className="text-muted-foreground text-xs">
                {model.company}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
