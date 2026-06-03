"use client";
import React from "react";

import clsx from "clsx";
import { useAIModelStore, useOpenAiApiKeyStore } from "@/stores/store";
import { Input } from "@/components/ui/input";

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
  // {
  //   name: "GPT-5 Nano",
  //   company: "OpenAI",
  //   value: "gpt-5-nano",
  // },
  {
    name: "GPT-5.4",
    company: "OpenAI",
    value: "gpt-5.4",
  },
];

export default function ModelGrid() {
  const aiModel = useAIModelStore((state) => state.aiModel);
  const setAIModel = useAIModelStore((state) => state.setAIModel);
  const openAiApiKey = useOpenAiApiKeyStore((state) => state.openAiApiKey);
  const setOpenAiApiKey = useOpenAiApiKeyStore(
    (state) => state.setOpenAiApiKey,
  );
  const isApiKeyMissing = openAiApiKey.trim().length === 0;

  return (
    <div className="p-2 flex flex-col gap-4 mb-2">
      <div className="grid grid-cols-2 gap-4 md:grid-cols-3">
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

      <div className="flex flex-col items-start gap-2">
        <label htmlFor="openai-api-key" className="font-semibold text-sm">
          OpenAI API Key
        </label>
        <Input
          id="openai-api-key"
          aria-describedby="openai-api-key-error"
          aria-invalid={isApiKeyMissing}
          autoComplete="off"
          className="rounded-sm"
          onChange={(event) => setOpenAiApiKey(event.target.value)}
          placeholder="Enter your OpenAI API key"
          spellCheck={false}
          type="password"
          value={openAiApiKey}
        />
        {isApiKeyMissing && (
          <p id="openai-api-key-error" className="text-destructive text-xs">
            OpenAI API key is required before submitting.
          </p>
        )}
      </div>
    </div>
  );
}
