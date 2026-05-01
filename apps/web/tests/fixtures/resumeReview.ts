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
  jobMatch: {
    name: "Backend Developer",
    targetJob: [
      {
        score: 80,
        type: "Skills Match",
        content: "Strong API and backend alignment.",
      },
    ],
    overallScore: 80,
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
