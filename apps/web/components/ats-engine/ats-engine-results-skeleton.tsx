"use client";

import { Skeleton } from "@/components/ui/skeleton";

export default function AtsEngineResultsSkeleton() {
  return (
    <div className="flex flex-col gap-4 border-t pt-4">
      <AtsHeaderValidationSummarySkeleton />
      <AtsFinalAssessmentSummarySkeleton />
    </div>
  );
}

function AtsHeaderValidationSummarySkeleton() {
  return (
    <div className="flex flex-col gap-3">
      <div className="rounded-md border-0">
        <div className="flex w-full flex-col gap-3 py-4 md:flex-row md:items-start md:justify-between">
          <div className="flex flex-col gap-2">
            <Skeleton className="h-4 w-40" />
            <Skeleton className="h-6 w-48" />
          </div>

          <div className="flex items-end gap-2">
            <Skeleton className="h-10 w-16" />
            <Skeleton className="h-6 w-10" />
          </div>
        </div>

        <div className="flex flex-col gap-4 pb-4">
          <HeaderPillGroupSkeleton titleWidth="w-44" pillWidths={["w-24", "w-36", "w-28"]} />
          <HeaderPillGroupSkeleton titleWidth="w-48" pillWidths={["w-32", "w-24"]} />
          <HeaderPillGroupSkeleton titleWidth="w-44" pillWidths={["w-28", "w-36", "w-24"]} />

          <div className="flex flex-col gap-3 rounded-md border p-3">
            <Skeleton className="h-5 w-36" />
            <div className="flex flex-wrap items-center gap-2">
              <Skeleton className="h-4 w-16" />
              <Skeleton className="h-8 w-28 rounded-2xl" />
              <Skeleton className="h-4 w-44" />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function AtsFinalAssessmentSummarySkeleton() {
  return (
    <div className="flex flex-col gap-4 border-t">
      <div className="flex w-full flex-col gap-3 py-4 md:flex-row md:items-start md:justify-between">
        <Skeleton className="h-6 w-56" />
        <div className="flex items-end gap-2">
          <Skeleton className="h-10 w-16" />
          <Skeleton className="h-6 w-10" />
        </div>
      </div>

      <div className="flex flex-col gap-4 pb-4">
        <Skeleton className="h-6 w-44" />
        <div className="grid gap-4 md:grid-cols-3">
          {Array.from({ length: 3 }).map((_, index) => (
            <div key={index} className="flex flex-col gap-3 rounded-md border p-6">
              <Skeleton className="h-5 w-36" />
              <Skeleton className="h-10 w-24" />
              <Skeleton className="h-5 w-28" />
            </div>
          ))}
        </div>

        <div className="flex flex-col gap-5 rounded-md border p-3">
          <Skeleton className="h-6 w-56" />
          {Array.from({ length: 3 }).map((_, index) => (
            <KeywordTierSkeleton key={index} />
          ))}
        </div>

        <div className="flex flex-col gap-5 rounded-md border p-3">
          <Skeleton className="h-6 w-36" />
          {Array.from({ length: 3 }).map((_, index) => (
            <KeywordUsageSkeleton key={index} />
          ))}
        </div>
      </div>
    </div>
  );
}

function HeaderPillGroupSkeleton({
  titleWidth,
  pillWidths,
}: {
  titleWidth: string;
  pillWidths: string[];
}) {
  return (
    <div className="flex flex-col gap-2">
      <Skeleton className={`h-5 ${titleWidth}`} />
      <div className="flex flex-row flex-wrap gap-2">
        {pillWidths.map((width, index) => (
          <Skeleton key={index} className={`h-8 ${width}`} />
        ))}
      </div>
    </div>
  );
}

function KeywordTierSkeleton() {
  return (
    <div className="flex flex-col gap-3 border-b pb-5 last:border-b-0 last:pb-0">
      <div className="flex items-center justify-between gap-4">
        <Skeleton className="h-5 w-44" />
        <Skeleton className="h-5 w-20" />
      </div>
      <div className="flex flex-wrap gap-2">
        {["w-28", "w-20", "w-24", "w-32", "w-24"].map((width, index) => (
          <Skeleton key={index} className={`h-8 ${width} rounded-2xl`} />
        ))}
      </div>
      <div className="flex flex-wrap gap-2">
        {["w-32", "w-28"].map((width, index) => (
          <Skeleton key={index} className={`h-8 ${width} rounded-2xl`} />
        ))}
      </div>
    </div>
  );
}

function KeywordUsageSkeleton() {
  return (
    <div className="flex flex-col gap-3 rounded-md border p-3">
      <div className="flex items-start justify-between gap-3">
        <Skeleton className="h-5 w-28" />
        <Skeleton className="h-7 w-24" />
      </div>
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-10/12" />
      <div className="flex flex-wrap gap-2">
        {["w-24", "w-20", "w-28"].map((width, index) => (
          <Skeleton key={index} className={`h-7 ${width}`} />
        ))}
      </div>
    </div>
  );
}
