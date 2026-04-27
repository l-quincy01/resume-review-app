import React from "react";
import { Skeleton } from "@/components/ui/skeleton";

export default function AtsHeaderSkeleton() {
  return (
    <div className="flex flex-col gap-2 p-2">
      <div className="flex flex-row justify-between">
        <div className="flex flex-col gap-2">
          <Skeleton className="h-6 w-64" />
          <Skeleton className="h-4 w-36" />
        </div>

        <div className="flex items-end gap-2">
          <Skeleton className="h-10 w-16" />
          <Skeleton className="h-6 w-8" />
        </div>
      </div>

      <div className="grid grid-cols-3 gap-2">
        {Array.from({ length: 6 }).map((_, index) => (
          <div
            key={index}
            className="flex flex-col gap-3 p-2 py-4 items-start border rounded-lg"
          >
            <div className="flex flex-row w-full justify-between items-start">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-4 w-10" />
            </div>
            <Skeleton className="h-2.5 w-full rounded-full" />
          </div>
        ))}
      </div>
    </div>
  );
}
