"use client";

import type React from "react";
import {
  AtsCriticalGap,
  AtsRecommendation,
  AtsStrength,
} from "@/types/AtsEngine/ats-engine.type";

type AtsKeywordResultItem = AtsCriticalGap | AtsStrength | AtsRecommendation;

interface AtsKeywordResultListProps {
  title: string;
  items: AtsKeywordResultItem[];
  variant: "critical-gap" | "strength" | "recommendation";
}

export default function AtsKeywordResultList({
  title,
  items,
  variant,
}: AtsKeywordResultListProps) {
  if (items.length === 0) {
    return null;
  }

  return (
    <ResultList title={title}>
      {items.map((item) => (
        <li
          key={`${item.keyword}-${getItemMeta(item, variant)}`}
          className="rounded-md border p-3"
        >
          <div className="flex items-start justify-between gap-3">
            <div className="font-medium">{item.keyword}</div>
            <div className="text-muted-foreground">
              {item.keyword_type}
              {getItemMeta(item, variant) && ` · ${getItemMeta(item, variant)}`}
            </div>
          </div>
          <div className="text-muted-foreground">{item.context}</div>
          <div className="text-muted-foreground">
            {getItemBody(item, variant)}
          </div>
        </li>
      ))}
    </ResultList>
  );
}

function getItemMeta(
  item: AtsKeywordResultItem,
  variant: AtsKeywordResultListProps["variant"],
) {
  if (variant === "strength" && "score" in item) {
    return `${item.score}/100`;
  }

  if (variant === "recommendation" && "priority" in item) {
    return item.priority;
  }

  return "";
}

function getItemBody(
  item: AtsKeywordResultItem,
  variant: AtsKeywordResultListProps["variant"],
) {
  if (variant === "recommendation" && "suggestion" in item) {
    return item.suggestion;
  }

  return "reason" in item ? item.reason : "";
}

function ResultList({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="flex flex-col gap-2">
      <div className="font-semibold">{title}</div>
      <ul className="flex flex-col gap-2 text-sm">{children}</ul>
    </div>
  );
}
