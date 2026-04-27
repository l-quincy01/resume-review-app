import React from "react";
import { Skeleton } from "@/components/ui/skeleton";

export default function AtsContentSkeleton() {
  return (
    <div className="flex flex-col gap-4">
      {Array.from({ length: 4 }).map((_, index) => (
        <div key={index} className="border-none bg-transparent">
          <div className="border-b w-full pb-4">
            <div className="flex flex-row justify-between items-center">
              <Skeleton className="h-5 w-40" />
              <Skeleton className="h-6 w-14" />
            </div>
          </div>

          <div className="flex flex-col gap-4 pt-4">
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-11/12" />
            <Skeleton className="h-4 w-10/12" />

            <div className="flex flex-col gap-2">
              <Skeleton className="h-4 w-20" />
              <div className="flex flex-col gap-2 pl-4">
                <Skeleton className="h-4 w-11/12" />
                <Skeleton className="h-4 w-10/12" />
                <Skeleton className="h-4 w-9/12" />
              </div>
            </div>

            <div className="flex flex-col gap-2">
              <Skeleton className="h-4 w-20" />
              <div className="flex flex-col gap-2 pl-4">
                <Skeleton className="h-4 w-10/12" />
                <Skeleton className="h-4 w-8/12" />
              </div>
            </div>

            <div className="flex flex-col gap-2">
              <Skeleton className="h-4 w-24" />
              <div className="flex flex-col gap-2 pl-4">
                <Skeleton className="h-4 w-full" />
                <Skeleton className="h-4 w-11/12" />
                <Skeleton className="h-4 w-9/12" />
              </div>
            </div>

            <div className="flex flex-col gap-2">
              <Skeleton className="h-4 w-32" />
              <div className="flex flex-col gap-3">
                <div className="grid grid-cols-2 border divide-x">
                  <div className="flex flex-col gap-2 p-2">
                    <Skeleton className="h-4 w-16" />
                    <Skeleton className="h-4 w-full" />
                    <Skeleton className="h-4 w-10/12" />
                    <Skeleton className="h-4 w-8/12" />
                  </div>

                  <div className="flex flex-col gap-2 p-2">
                    <Skeleton className="h-4 w-28" />
                    <Skeleton className="h-4 w-full" />
                    <Skeleton className="h-4 w-10/12" />
                    <Skeleton className="h-4 w-9/12" />
                  </div>
                </div>

                <div className="border border-t-0 flex flex-col gap-2 p-2">
                  <Skeleton className="h-4 w-20" />
                  <Skeleton className="h-4 w-full" />
                  <Skeleton className="h-4 w-11/12" />
                </div>
              </div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
