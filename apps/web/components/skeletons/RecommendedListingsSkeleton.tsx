import React from "react";
import { Skeleton } from "@/components/ui/skeleton";

export default function RecommendedListingsSkeleton() {
  return (
    <div className="flex flex-col gap-4 p-4">
      <div className="flex items-center justify-between">
        <Skeleton className="h-6 w-36" />
      </div>

      {Array.from({ length: 3 }).map((_, index) => (
        <div
          key={index}
          className="flex flex-col gap-3 items-start bg-accent/60 rounded-xl p-3"
        >
          <Skeleton className="h-5 w-56" />

          <div className="flex flex-col gap-2 w-full">
            <Skeleton className="h-4 w-36" />
            <Skeleton className="h-4 w-28" />
          </div>

          <div className="w-full flex flex-col gap-2">
            <Skeleton className="h-4 w-20" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-11/12" />
          </div>

          <div className="flex flex-row items-center gap-3 w-full">
            <Skeleton className="h-4 w-28" />
            <Skeleton className="h-4 w-2" />
            <Skeleton className="h-4 w-40" />
          </div>
        </div>
      ))}
    </div>
  );
}
