import React from "react";

export default function HomeHeader() {
  return (
    <div className="text-sm flex flex-col gap-2">
      <div className="flex flex-row items-center gap-2">
        <span className="text-xl font-extrabold">AI Resume Review</span>
        <span className="px-2 py-1 rounded-2xl bg-primary/40 font-bold text-[0.6rem] w-fit">
          Beta Preview
        </span>
      </div>

      <div className="text-muted-foreground">
        Upload your resume PDF and get expert AI-powered feedback trained on
        FAANG standards. Receive section-by-section feedback, specific bullet
        point rewrites, and ATS compatibility analysis.
      </div>
    </div>
  );
}
