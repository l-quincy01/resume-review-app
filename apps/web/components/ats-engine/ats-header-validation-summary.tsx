"use client";

import { AtsHeaderValidationResponse } from "@/types/AtsEngine/ats-engine.type";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";

interface AtsHeaderValidationSummaryProps {
  headerValidation: AtsHeaderValidationResponse;
}

export default function AtsHeaderValidationSummary({
  headerValidation,
}: AtsHeaderValidationSummaryProps) {
  const criticalHeaders = [
    "summary",
    "work experience",
    "experience",
    "education",
    "skills",
  ];

  const missingCriticalHeaders = headerValidation.headers_missing.filter(
    (header) => criticalHeaders.includes(header.toLowerCase()),
  );
  const missingOptionallHeaders = headerValidation.headers_missing.filter(
    (header) => !criticalHeaders.includes(header.toLowerCase()),
  );

  return (
    <div className="flex flex-col gap-3">
      <Accordion
        type="multiple"
        defaultValue={["default"]}
        className="rounded-md  border-0 border-none"
      >
        <AccordionItem value={"default"}>
          <AccordionTrigger className="w-full">
            <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between w-full">
              <div className="flex flex-col">
                <div className="text-sm text-muted-foreground">
                  Structure quality: {headerValidation.structure_quality}
                </div>
                <div className="text-lg font-semibold">
                  ATS Header Validation
                </div>
              </div>

              <div className="text-xl text-muted-foreground font-bold">
                <span className="font-extrabold text-4xl text-card-foreground">
                  {headerValidation.header_quality_score}
                </span>
                /100
              </div>
            </div>
          </AccordionTrigger>
          <AccordionContent className="flex flex-col gap-4 h-fit">
            <div className="flex flex-col gap-2 justify-center">
              <HeaderList
                title="Critical Headers Found"
                items={headerValidation.headers_found}
              />
              <HeaderList
                title="Critical Headers Not Found"
                items={missingCriticalHeaders}
              />
              <HeaderList
                title="Optional Headers Missing"
                items={missingOptionallHeaders}
              />
            </div>

            {headerValidation.non_standard_headers &&
              headerValidation.non_standard_headers.length > 0 && (
                <div className="flex flex-col gap-2 rounded-md border p-3 text-sm">
                  <div className="font-semibold">Non-standard Headers</div>
                  {headerValidation.non_standard_headers.map((header) => (
                    <div
                      key={`${header.header_found}-${header.mapped_to}`}
                      className="text-muted-foreground"
                    >
                      {header.header_found} → use {header.recommended_header}
                    </div>
                  ))}
                </div>
              )}

            <div className="flex flex-col gap-4 rounded-md border p-3 text-sm">
              <div className="font-semibold">High Priority Fix</div>

              <div className="text-muted-foreground">
                Include:
                <span
                  className={`rounded-2xl  w-fit  px-2.5 py-1 text-sm font-semibold  bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200 mx-2 `}
                >
                  {missingCriticalHeaders.join(", ")}
                </span>
                as {missingCriticalHeaders.length > 1 ? "headers" : "a header"}{" "}
                in your resume.
              </div>
            </div>
          </AccordionContent>
        </AccordionItem>
      </Accordion>
    </div>
  );
}

function HeaderList({ title, items }: { title: string; items: string[] }) {
  return (
    <div className="flex flex-col gap-2 rounded-md  ">
      <div className="font-semibold">{title}</div>

      {items.length > 0 ? (
        <div className="flex flex-row flex-wrap gap-2 text-sm text-muted-foreground">
          {items.map((header, index) => (
            <span
              key={index}
              // className={`rounded-md border px-2.5 py-1 text-xs font-medium ${title === "Headers Found" ? "border-primary/30 bg-primary/10 text-primary" : "border-destructive/30 bg-destructive/10 text-destructive"}`}
              className={`rounded-md border px-2.5 py-1 text-sm font-semibold `}
            >
              {header}
            </span>
          ))}
        </div>
      ) : (
        <div className="text-sm text-muted-foreground">None</div>
      )}
    </div>
  );
}
