import type { Route } from "@playwright/test";

export const pdfBuffer = Buffer.from(
  "%PDF-1.4\n1 0 obj\n<< /Type /Catalog >>\nendobj\ntrailer\n<< /Root 1 0 R >>\n%%EOF",
);

export const resumeReviewResponse = {
  jobRecommendation: {
    yearsExperience: "3 years",
    jobTitles: ["Backend Developer"],
    roles: ["API development"],
    responsibilities: ["Build reliable APIs"],
    seniority: ["Mid-level"],
    industry: ["SaaS"],
    hardSkills: ["C#", "TypeScript"],
    softSkills: ["Communication"],
    workAndTeamEnvironment: ["Product engineering"],
    companySizeFit: ["Startup"],
    careerTrack: ["Backend engineering"],
  },
  spellingAndGrammar: {
    score: 90,
    grammarSuggestions: [],
    spellingSuggestions: [],
    personalPronounCheck: [],
    passiveVoiceCheck: [],
  },
  atsContent: {
    heading: {
      section: "ATS",
      score: 90,
    },
    resumeName: "Test Candidate",
    content: [
      {
        section: "Experience",
        summary: "Relevant backend experience.",
        strengths: ["Clear impact"],
        weaknesses: ["Needs more metrics"],
        suggestions: [
          {
            type: "Medium",
            content: "Add measurable outcomes.",
          },
        ],
        suggestedRewrites: [],
        score: 90,
      },
    ],
  },
  jobSearchProfile: {
    titles: ["Backend Developer"],
    keywords: ["C#", "API"],
    seniority: "Mid-level",
    locations: ["Remote"],
    exclude: [],
  },
  warnings: [],
};

export async function fulfillResumeReview(route: Route) {
  await route.fulfill({
    status: 200,
    contentType: "application/json",
    body: JSON.stringify(resumeReviewResponse),
  });
}

export async function fulfillResumeReviewStream(route: Route) {
  await route.fulfill({
    status: 200,
    contentType: "text/event-stream",
    body: buildResumeReviewStreamBody(),
  });
}

export function buildResumeReviewStreamBody() {
  return [
    sse("review_started", {
      completed_sections: [],
      timestamp: new Date().toISOString(),
    }),
    sse("section_completed", {
      section: "ats_content",
      payload: resumeReviewResponse.atsContent,
      completed_sections: ["ats_content"],
      timestamp: new Date().toISOString(),
    }),
    sse("section_completed", {
      section: "spelling_and_grammar",
      payload: resumeReviewResponse.spellingAndGrammar,
      completed_sections: ["ats_content", "spelling_and_grammar"],
      timestamp: new Date().toISOString(),
    }),
    sse("section_completed", {
      section: "job_recommendation",
      payload: resumeReviewResponse.jobRecommendation,
      completed_sections: [
        "ats_content",
        "spelling_and_grammar",
        "job_recommendation",
      ],
      timestamp: new Date().toISOString(),
    }),
    sse("section_completed", {
      section: "job_search_profile",
      payload: resumeReviewResponse.jobSearchProfile,
      completed_sections: [
        "ats_content",
        "spelling_and_grammar",
        "job_recommendation",
        "job_search_profile",
      ],
      timestamp: new Date().toISOString(),
    }),
    sse("review_completed", {
      payload: resumeReviewResponse,
      completed_sections: [
        "ats_content",
        "spelling_and_grammar",
        "job_recommendation",
        "job_search_profile",
      ],
      timestamp: new Date().toISOString(),
    }),
  ].join("");
}

export function buildResumeReviewStreamBodyWithSectionFailure() {
  return [
    sse("review_started", {
      completed_sections: [],
      timestamp: new Date().toISOString(),
    }),
    sse("section_completed", {
      section: "ats_content",
      payload: resumeReviewResponse.atsContent,
      completed_sections: ["ats_content"],
      timestamp: new Date().toISOString(),
    }),
    sse("section_failed", {
      section: "spelling_and_grammar",
      warning:
        "spelling and grammar could not be generated. Other report sections may still be usable.",
      completed_sections: ["ats_content"],
      timestamp: new Date().toISOString(),
    }),
    sse("section_completed", {
      section: "job_recommendation",
      payload: resumeReviewResponse.jobRecommendation,
      completed_sections: ["ats_content", "job_recommendation"],
      timestamp: new Date().toISOString(),
    }),
    sse("section_completed", {
      section: "job_search_profile",
      payload: resumeReviewResponse.jobSearchProfile,
      completed_sections: [
        "ats_content",
        "job_recommendation",
        "job_search_profile",
      ],
      timestamp: new Date().toISOString(),
    }),
    sse("review_completed", {
      payload: {
        ...resumeReviewResponse,
        spellingAndGrammar: undefined,
        warnings: [
          "spelling and grammar could not be generated. Other report sections may still be usable.",
        ],
      },
      completed_sections: [
        "ats_content",
        "job_recommendation",
        "job_search_profile",
      ],
      timestamp: new Date().toISOString(),
    }),
  ].join("");
}

function sse(event: string, data: unknown) {
  return `event: ${event}\ndata: ${JSON.stringify(data)}\n\n`;
}
