import { JobRecommendation } from "@/types/jobRecommendations.type";
import React from "react";

interface Props {
  jobRecommendation: JobRecommendation;
}

const TAG_COLORS: Record<
  keyof Omit<JobRecommendation, "yearsExperience">,
  string
> = {
  jobTitles: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200",
  roles:
    "bg-violet-100 text-violet-800 dark:bg-violet-900 dark:text-violet-200",

  responsibilities:
    "bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200",
  seniority:
    "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
  industry: "bg-teal-100 text-teal-800 dark:bg-teal-900 dark:text-teal-200",
  hardSkills: "bg-rose-100 text-rose-800 dark:bg-rose-900 dark:text-rose-200",
  softSkills: "bg-pink-100 text-pink-800 dark:bg-pink-900 dark:text-pink-200",
  workAndTeamEnvironment:
    "bg-sky-100 text-sky-800 dark:bg-sky-900 dark:text-sky-200",

  companySizeFit:
    "bg-cyan-100 text-cyan-800 dark:bg-cyan-900 dark:text-cyan-200",
  careerTrack:
    "bg-indigo-100 text-indigo-800 dark:bg-indigo-900 dark:text-indigo-200",
};

const SECTION_LABELS: Record<
  keyof Omit<JobRecommendation, "yearsExperience">,
  string
> = {
  jobTitles: "Job Titles",
  roles: "Roles",

  responsibilities: "Responsibilities",
  seniority: "Seniority",
  industry: "Industry",
  hardSkills: "Hard Skills",
  softSkills: "Soft Skills",
  workAndTeamEnvironment: "Work & Team Environment",

  companySizeFit: "Company Size Fit",
  careerTrack: "Career Track",
};

type TagSectionKey = keyof Omit<JobRecommendation, "yearsExperience">;
const TAG_SECTIONS = Object.keys(SECTION_LABELS) as TagSectionKey[];

function TagSection({
  label,
  items,
  colorClass,
}: {
  label: string;
  items: string[];
  colorClass: string;
}) {
  return (
    <div className="flex flex-col gap-2">
      <span className="font-semibold text-sm">{label}</span>
      <div className="flex flex-row flex-wrap gap-2">
        {items.map((item, i) => (
          <div
            key={i}
            className={`rounded-2xl px-3 py-1 text-xs font-medium w-fit ${colorClass}`}
          >
            {item}
          </div>
        ))}
      </div>
    </div>
  );
}

export default function Recommendations({ jobRecommendation }: Props) {
  return (
    <div className="flex flex-col gap-4 p-4">
      <div className="flex items-center justify-between">
        <span className="text-lg font-semibold">
          {/* Job Recommendations³ */}
          Jobs To Look Out For³
        </span>
      </div>

      {TAG_SECTIONS.map((key) => (
        <TagSection
          key={key}
          label={SECTION_LABELS[key]}
          items={jobRecommendation[key] ?? []}
          colorClass={TAG_COLORS[key]}
        />
      ))}
    </div>
  );
}
