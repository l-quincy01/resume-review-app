"use client";

import React from "react";
import { Skeleton } from "@/components/ui/skeleton";

export default function JobMatchSkeleton() {
  return (
    <div>
      <div className="flex flex-col gap-2 p-2">
        <div className="flex flex-row justify-between">
          <div className="flex flex-col gap-2">
            <Skeleton className="h-6 w-44" />
            <Skeleton className="h-4 w-32" />
          </div>

          <div className="flex items-end gap-2">
            <Skeleton className="h-10 w-16" />
            <Skeleton className="h-6 w-8" />
          </div>
        </div>

        <div className="flex flex-col gap-2 text-sm">
          <div className="border-b w-full pb-4">
            <Skeleton className="h-5 w-28" />
          </div>

          <div className="grid grid-cols-2 border divide-x">
            {Array.from({ length: 6 }).map((_, index) => (
              <div key={index} className="flex flex-col gap-3 p-2">
                <div className="flex flex-row justify-between gap-2">
                  <Skeleton className="h-4 w-36" />
                  <Skeleton className="h-4 w-10" />
                </div>
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-11/12" />
                <Skeleton className="h-4 w-9/12" />
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
