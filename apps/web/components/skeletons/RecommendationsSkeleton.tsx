import React from "react";
import { Skeleton } from "@/components/ui/skeleton";

function TagSectionSkeleton() {
  return (
    <div className="flex flex-col gap-2">
      <Skeleton className="h-4 w-28" />
      <div className="flex flex-row flex-wrap gap-2">
        {Array.from({ length: 6 }).map((_, index) => (
          <Skeleton key={index} className="h-7 w-24 rounded-2xl" />
        ))}
      </div>
    </div>
  );
}

export default function RecommendationsSkeleton() {
  return (
    <div className="flex flex-col gap-4 p-4">
      <div className="flex items-center justify-between">
        <Skeleton className="h-6 w-40" />
      </div>

      {Array.from({ length: 7 }).map((_, index) => (
        <TagSectionSkeleton key={index} />
      ))}
    </div>
  );
}
