import type { Page, Route } from "@playwright/test";

export const atsHeaderValidationResponse = {
  header_quality_score: 90,
  headers_found: ["Summary", "Skills", "Work Experience", "Education"],
  headers_missing: ["Projects", "Certifications"],
  unclear_headers: [],
  non_standard_headers: [],
  structure_quality: "strong",
};

export const atsKeywordExtractionResponse = {
  job_title: "Frontend Developer",
  keywords: [
    {
      keyword: "React",
      keyword_type: "single_word",
      category: "framework",
      tier: 0,
      requirement: "must_have",
      context: "Used to build frontend interfaces.",
      variations: [],
      frequency: 3,
      boost_applied: true,
    },
    {
      keyword: "TypeScript",
      keyword_type: "single_word",
      category: "language",
      tier: 1,
      requirement: "must_have",
      context: "Used for typed frontend development.",
      variations: [],
      frequency: 1,
      boost_applied: false,
    },
  ],
};

export const atsContextualScoringResponse = {
  keyword_scores: [
    {
      keyword: "React",
      present: true,
      tier: 0,
      requirement: "must_have",
      keyword_type: "single_word",
      frequency: 3,
      context: "Used to build frontend interfaces.",
      variations: [],
      matched_terms: ["React"],
      context_type: {
        has_achievement: true,
        has_metric: true,
        has_action_verb: true,
        in_experience_section: true,
        in_project_section: false,
        in_summary_section: false,
      },
      evidence: [
        {
          section: "Work Experience",
          text: "Built React dashboards that improved reporting speed by 35%.",
          matched_term: "React",
        },
      ],
    },
    {
      keyword: "TypeScript",
      present: false,
      tier: 1,
      requirement: "must_have",
      keyword_type: "single_word",
      frequency: 1,
      context: "Used for typed frontend development.",
      variations: [],
      matched_terms: [],
      context_type: {
        has_achievement: false,
        has_metric: false,
        has_action_verb: false,
        in_experience_section: false,
        in_project_section: false,
        in_summary_section: false,
      },
      evidence: [],
    },
  ],
};

export const atsKeywordScoringResponse = {
  keyword_scores: [
    {
      ...atsContextualScoringResponse.keyword_scores[0],
      requirement_multiplier: 1.15,
      context_points: 72,
      keyword_score: 83,
    },
    {
      ...atsContextualScoringResponse.keyword_scores[1],
      requirement_multiplier: 1.15,
      context_points: 0,
      keyword_score: 0,
    },
  ],
};

export const atsFinalAssessmentResponse = {
  job_title: "Frontend Developer",
  ats_readiness_score: 82,
  overall_keyword_score: 78,
  header_quality_score: 90,
  coverage: {
    overall_presence_rate: 50,
    tier_0_coverage: 100,
    tier_1_coverage: 0,
    must_have_coverage: 50,
    total_keywords_found: 1,
    total_keywords_expected: 2,
  },
  tier_breakdown: {
    tier_0: {
      average_score: 83,
      present: 1,
      total: 1,
      missing: [],
    },
    tier_1: {
      average_score: 0,
      present: 0,
      total: 1,
      missing: ["TypeScript"],
    },
    tier_2: {
      average_score: 0,
      present: 0,
      total: 0,
      missing: [],
    },
    tier_3: {
      average_score: 0,
      present: 0,
      total: 0,
      missing: [],
    },
  },
  critical_gaps: [
    {
      keyword: "TypeScript",
      keyword_type: "single_word",
      context: "Used for typed frontend development.",
      tier: 1,
      requirement: "must_have",
      reason:
        "Missing entirely from the resume despite being a critical must-have requirement.",
    },
  ],
  strengths: [
    {
      keyword: "React",
      keyword_type: "single_word",
      context: "Used to build frontend interfaces.",
      score: 83,
      reason:
        "Well contextualised with an action verb, achievement, and measurable outcome.",
      evidence: [
        {
          section: "Work Experience",
          text: "Built React dashboards that improved reporting speed by 35%.",
          matched_term: "React",
        },
      ],
    },
  ],
  recommendations: [
    {
      keyword: "TypeScript",
      keyword_type: "single_word",
      context: "Used for typed frontend development.",
      priority: "High",
      issue: "Missing critical must-have keyword.",
      suggestion:
        "Add TypeScript naturally into a recent Work Experience or Projects bullet if it is genuinely part of your experience.",
    },
  ],
};

export async function fulfillJson(route: Route, body: unknown) {
  await route.fulfill({
    status: 200,
    contentType: "application/json",
    body: JSON.stringify(body),
  });
}

export async function mockAtsEnginePipeline(page: Page) {
  const calls: string[] = [];

  await page.route("**/api/ats-engine/header-validation", async (route) => {
    calls.push("header-validation");
    await fulfillJson(route, atsHeaderValidationResponse);
  });

  await page.route("**/api/ats-engine/keyword-extraction", async (route) => {
    calls.push("keyword-extraction");
    await fulfillJson(route, atsKeywordExtractionResponse);
  });

  await page.route(
    "**/api/ats-engine/keyword-analysis",
    async (route) => {
      calls.push("keyword-analysis");
      await fulfillJson(route, atsContextualScoringResponse);
    },
  );

  await page.route("**/api/ats-engine/keyword-scoring", async (route) => {
    calls.push("keyword-scoring");
    await fulfillJson(route, atsKeywordScoringResponse);
  });

  await page.route("**/api/ats-engine/final-assessment", async (route) => {
    calls.push("final-assessment");
    await fulfillJson(route, atsFinalAssessmentResponse);
  });

  return calls;
}
