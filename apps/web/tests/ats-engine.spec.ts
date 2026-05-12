import { expect, test } from "@playwright/test";
import { mockAtsEnginePipeline } from "./fixtures/atsEngine";
import { fulfillResumeReview, pdfBuffer } from "./fixtures/resumeReview";

test.describe("ATS Engine section", () => {
  test("runs the full ATS Engine sequence from the existing submit button", async ({
    page,
  }) => {
    const calls = await mockAtsEnginePipeline(page);

    await page.route("**/api/resume-review", fulfillResumeReview);

    await page.goto("/resume");
    await page.getByLabel("Paste A Job Description For Your Desired Job").fill("Build React applications with TypeScript.");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Test Candidate Resume Review Report")).toBeVisible();
    await expect(page.getByText("ATS Header Validation")).toBeVisible();
    await expect(page.getByText("ATS Readiness")).toBeVisible();
    await expect(page.getByText("82/100")).toBeVisible();
    await expect(page.getByText("Critical Gaps")).toBeVisible();
    await expect(page.getByText("Missing: TypeScript")).toBeVisible();
    await expect(page.getByText("Strengths")).toBeVisible();
    await expect(page.getByText("Well contextualised with an action verb")).toBeVisible();
    await expect(page.getByText("Add TypeScript naturally")).toBeVisible();

    expect(calls).toEqual([
      "header-validation",
      "keyword-extraction",
      "keyword-analysis",
      "keyword-scoring",
      "final-assessment",
    ]);
  });

  test("runs only header validation when no job description is provided", async ({
    page,
  }) => {
    const calls: string[] = [];

    await page.route("**/api/resume-review", fulfillResumeReview);
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

    await expect(page.getByText("ATS Header Validation")).toBeVisible();
    await expect(page.getByText("ATS Readiness")).not.toBeVisible();
    expect(calls).toEqual(["header-validation"]);
  });

  test("shows an ATS Engine error below the existing report when a stage fails", async ({ page }) => {
    await page.route("**/api/resume-review", fulfillResumeReview);
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
