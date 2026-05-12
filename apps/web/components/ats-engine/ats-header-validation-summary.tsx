"use client";

import { AtsHeaderValidationResponse } from "@/types/ats-engine.type";

interface AtsHeaderValidationSummaryProps {
  headerValidation: AtsHeaderValidationResponse;
}

export default function AtsHeaderValidationSummary({
  headerValidation,
}: AtsHeaderValidationSummaryProps) {
  return (
    <div className="flex flex-col gap-3">
      <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
        <div className="flex flex-col">
          <div className="text-sm text-muted-foreground">
            Structure quality: {headerValidation.structure_quality}
          </div>
          <div className="text-lg font-semibold">ATS Header Validation</div>
        </div>

        <div className="text-xl text-muted-foreground font-bold">
          <span className="font-extrabold text-4xl text-card-foreground">
            {headerValidation.header_quality_score}
          </span>
          /100
        </div>
      </div>

      <div className="grid gap-2 md:grid-cols-2">
        <HeaderList title="Headers Found" items={headerValidation.headers_found} />
        <HeaderList
          title="Headers Missing"
          items={headerValidation.headers_missing}
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
    </div>
  );
}

function HeaderList({ title, items }: { title: string; items: string[] }) {
  return (
    <div className="flex flex-col gap-2 rounded-md border p-3">
      <div className="font-semibold">{title}</div>
      {items.length > 0 ? (
        <div className="text-sm text-muted-foreground">{items.join(", ")}</div>
      ) : (
        <div className="text-sm text-muted-foreground">None</div>
      )}
    </div>
  );
}
