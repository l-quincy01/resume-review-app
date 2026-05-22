import { expect, test } from "@playwright/test";
import {
  atsContextualScoringResponse,
  atsFinalAssessmentResponse,
  atsHeaderValidationResponse,
  atsKeywordExtractionResponse,
  atsKeywordScoringResponse,
  fulfillJson,
  mockAtsEnginePipeline,
} from "./fixtures/atsEngine";
import { fulfillResumeReviewStream, pdfBuffer } from "./fixtures/resumeReview";

test.describe("ATS Engine section", () => {
  test("runs the full ATS Engine sequence from the existing submit button", async ({
    page,
  }) => {
    const calls = await mockAtsEnginePipeline(page);

    await page.route("**/api/resume-review/stream", fulfillResumeReviewStream);

    await page.goto("/resume");
    await page.getByText("GPT-5 Mini").click();
    await page.getByLabel("Paste A Job Description For Your Desired Job").fill("Build React applications with TypeScript.");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Test Candidate Resume Review Report")).toBeVisible();
    await expect(page.getByText("Quantitative ATS Analysis")).toBeVisible();
    await expect(page.getByTestId("ats-header-validation-summary")).toBeVisible();
    await expect(page.getByText("Weighted Keyword score")).toBeVisible();
    await expect(page.getByText("Present:").first()).toBeVisible();
    await expect(page.getByText("React").first()).toBeVisible();
    await expect(page.getByText("Missing:").first()).toBeVisible();
    await expect(page.getByText("TypeScript").first()).toBeVisible();
    await expect(page.getByText("Keyword Usage", { exact: true })).toBeVisible();
    await expect(
      page.getByText(
        "Strong contextual use: this keyword is supported by multiple quality signals.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText("Add a measurable result or metric.").first(),
    ).toBeVisible();
    await expect(
      page.getByText("Placed in a relevant resume section.").first(),
    ).toBeVisible();
    await expect(page.getByText("Resume Context").first()).toBeVisible();
    await expect(page.getByText("Matched: React")).toBeVisible();
    await expect(
      page.getByText("Built React dashboards that improved reporting speed by 35%."),
    ).toBeVisible();
    await expect(
      page.getByText("No resume snippet returned for this keyword."),
    ).toBeVisible();
    await expect(
      page.getByText("Stuffed Keywords", { exact: true }).first(),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Keywords found in the resume but not supported by achievement, metric, action, or section context.",
      ),
    ).toBeVisible();
    await expect(page.getByText("Docker").first()).toBeVisible();
    await expect(page.getByText("Keyword Feedback")).toBeVisible();
    await expect(page.getByText("Critical Gaps")).toBeVisible();
    await expect(page.getByText("Strengths")).toBeVisible();
    await expect(
      page.getByText("Weak Keyword Usage", { exact: true }).first(),
    ).toBeVisible();
    await expect(
      page.getByText("Recommendations", { exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText("Well contextualised with an action verb"),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Keyword appears in the resume, but it is not supported by achievement, metric, action, or section context.",
      ),
    ).toBeVisible();
    await expect(
      page.getByText(
        "Add these keywords naturally into a relevant section in your resume.",
      ),
    ).toBeVisible();
    await expect(page.getByText("Improve Weak Keyword Usage")).toBeVisible();
    await expect(
      page.getByText("Add a measurable result or metric.").last(),
    ).toBeVisible();
    await expect(page.getByText("Fix Stuffed Keywords")).toBeVisible();
    await expect(
      page.getByText(
        "Use this keyword in a truthful Work Experience or Projects bullet with an action verb and measurable result, or remove it if it is only listed without context.",
      ),
    ).toBeVisible();

    expect(calls).toContain("header-validation");
    expect(calls).toContain("keyword-extraction");
    expect(calls.indexOf("keyword-analysis")).toBeGreaterThan(
      calls.indexOf("keyword-extraction"),
    );
    expect(calls.slice(-3)).toEqual([
      "keyword-analysis",
      "keyword-scoring",
      "final-assessment",
    ]);
  });

  test("hydrates ATS results stage by stage", async ({ page }) => {
    let releaseKeywordExtraction: () => void = () => {};
    const keywordExtractionGate = new Promise<void>((resolve) => {
      releaseKeywordExtraction = resolve;
    });

    await page.route("**/api/resume-review/stream", fulfillResumeReviewStream);
    await page.route("**/api/ats-engine/header-validation", async (route) => {
      await fulfillJson(route, atsHeaderValidationResponse);
    });
    await page.route("**/api/ats-engine/keyword-extraction", async (route) => {
      await keywordExtractionGate;
      await fulfillJson(route, atsKeywordExtractionResponse);
    });
    await page.route("**/api/ats-engine/keyword-analysis", async (route) => {
      await fulfillJson(route, atsContextualScoringResponse);
    });
    await page.route("**/api/ats-engine/keyword-scoring", async (route) => {
      await fulfillJson(route, atsKeywordScoringResponse);
    });
    await page.route("**/api/ats-engine/final-assessment", async (route) => {
      await fulfillJson(route, atsFinalAssessmentResponse);
    });

    await page.goto("/resume");
    await page.getByLabel("Paste A Job Description For Your Desired Job").fill("Build React applications with TypeScript.");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Quantitative ATS Analysis")).toBeVisible();
    await expect(page.getByTestId("ats-header-validation-summary")).toBeVisible();
    await expect(page.getByText("Weighted Keyword score")).not.toBeVisible();

    releaseKeywordExtraction();

    await expect(page.getByTestId("ats-final-assessment-summary")).toBeVisible();
    await expect(page.getByText("Weighted Keyword score")).toBeVisible();
    await expect(page.getByText("Keyword Usage", { exact: true })).toBeVisible();
  });

  test("runs only header validation when no job description is provided", async ({
    page,
  }) => {
    const calls: string[] = [];

    await page.route("**/api/resume-review/stream", fulfillResumeReviewStream);
    await page.route("**/api/ats-engine/header-validation", async (route) => {
      calls.push("header-validation");
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          header_quality_score: 90,
          headers_found: ["Skills", "Work Experience", "Education"],
          headers_missing: ["Summary"],
          unclear_headers: [],
          non_standard_headers: [],
          structure_quality: "strong",
        }),
      });
    });
    await page.route("**/api/ats-engine/keyword-extraction", async (route) => {
      calls.push("keyword-extraction");
      await route.abort();
    });

    await page.goto("/resume");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Quantitative ATS Analysis")).toBeVisible();
    await expect(page.getByTestId("ats-header-validation-summary")).toBeVisible();
    await expect(page.getByText("Weighted Keyword score")).not.toBeVisible();
    await expect(page.getByText("Keyword Usage")).not.toBeVisible();
    expect(calls).toEqual(["header-validation"]);
  });

  test("shows an ATS Engine error below the existing report when a stage fails", async ({ page }) => {
    await page.route("**/api/resume-review/stream", fulfillResumeReviewStream);
    await page.route("**/api/ats-engine/header-validation", async (route) => {
      await route.fulfill({
        status: 502,
        contentType: "application/json",
        body: JSON.stringify({ message: "Header validation failed." }),
      });
    });

    await page.goto("/resume");
    await page.getByLabel("Paste A Job Description For Your Desired Job").fill("Build React applications.");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Test Candidate Resume Review Report")).toBeVisible();
    await expect(page.getByText("Header validation failed.")).toBeVisible();
  });
});
