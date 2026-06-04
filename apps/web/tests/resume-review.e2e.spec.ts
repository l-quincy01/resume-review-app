import { expect, test } from "@playwright/test";
import type { Route } from "@playwright/test";
import {
  buildResumeReviewStreamBodyWithCompletedPayloadMissingProfile,
  buildResumeReviewStreamBodyWithSectionFailure,
  fulfillResumeReviewStream,
  pdfBuffer,
} from "./fixtures/resumeReview";

test.describe("resume review flow", () => {
  test("happy path submits a multipart resume and renders the report", async ({ page }) => {
    let sawMultipartRequest = false;
    let sawApiKeyHeader = false;
    let sawApiKeyInBody = false;

    await page.route("**/api/resume-review/stream", async (route) => {
      const request = route.request();
      const body = request.postData() ?? "";
      sawMultipartRequest =
        request.method() === "POST" &&
        (request.headers()["content-type"] ?? "").includes("multipart/form-data");
      sawApiKeyHeader = request.headers()["x-openai-api-key"] === "sk-test-key";
      sawApiKeyInBody = body.includes("sk-test-key");

      await fulfillResumeReviewStream(route);
    });

    await page.goto("/resume");
    await page.getByLabel("OpenAI API Key").fill("sk-test-key");
    await page.getByLabel("Paste A Job Description For Your Desired Job").fill("Build reliable APIs.");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Test Candidate Resume Review Report")).toBeVisible();
    await expect(page.getByText("Jobs To Look Out For")).toBeVisible();
    await expect(page.locator('iframe[title="PDF.js"]')).toHaveAttribute(
      "src",
      /\/pdfjs\/web\/viewer\.html\?file=/,
    );
    await expect(page.locator('iframe[title="PDF.js"]')).not.toHaveAttribute(
      "src",
      /blob:/,
    );
    expect(sawMultipartRequest).toBe(true);
    expect(sawApiKeyHeader).toBe(true);
    expect(sawApiKeyInBody).toBe(false);
    const storageSnapshot = await page.evaluate(() =>
      JSON.stringify({
        localStorage: { ...window.localStorage },
        sessionStorage: { ...window.sessionStorage },
      }),
    );
    expect(storageSnapshot).not.toContain("sk-test-key");
  });

  test("submits streamed job search profile and renders returned job listings", async ({ page }) => {
    let jobListingsRequestCount = 0;
    let sawApiKeyHeader = false;
    let sawApiKeyInBody = false;
    let requestedProfile: unknown = null;

    await page.route("**/api/resume-review/stream", fulfillResumeReviewStream);
    await page.route("**/api/job-listings", async (route) => {
      if (await fulfillJobListingsPreflight(route)) return;

      const request = route.request();
      const body = request.postData() ?? "";
      jobListingsRequestCount += 1;
      sawApiKeyHeader = request.headers()["x-openai-api-key"] === "sk-test-key";
      sawApiKeyInBody = body.includes("sk-test-key");
      requestedProfile = JSON.parse(body);

      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          jobListings: [
            {
              listingTitle: "Backend Developer",
              companyName: "Acme",
              locationName: "Remote",
              whyItsGreat: "Matches backend API experience.",
              listingLink: "https://example.com/job",
              searchQuery: "Backend Developer Remote",
            },
          ],
        }),
      });
    });

    await page.goto("/resume");
    await page.getByLabel("OpenAI API Key").fill("sk-test-key");
    await page.getByLabel("Paste A Job Description For Your Desired Job").fill("Build reliable APIs.");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("checkbox").click();
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Suggested Listings⁴")).toBeVisible();
    await expect(page.getByText("Comapny: Acme")).toBeVisible();
    expect(jobListingsRequestCount).toBe(1);
    expect(sawApiKeyHeader).toBe(true);
    expect(sawApiKeyInBody).toBe(false);
    expect(requestedProfile).toMatchObject({
      titles: ["Backend Developer"],
      keywords: ["C#", "API"],
    });
  });

  test("does not rerun job listings when the completed stream payload repeats the profile", async ({ page }) => {
    let jobListingsRequestCount = 0;

    await page.route("**/api/resume-review/stream", fulfillResumeReviewStream);
    await page.route("**/api/job-listings", async (route) => {
      if (await fulfillJobListingsPreflight(route)) return;
      jobListingsRequestCount += 1;

      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          jobListings:
            jobListingsRequestCount === 1
              ? [
                  {
                    listingTitle: "Backend Developer",
                    companyName: "Acme",
                    locationName: "Remote",
                    whyItsGreat: "Matches backend API experience.",
                    listingLink: "https://example.com/job",
                    searchQuery: "Backend Developer Remote",
                  },
                ]
              : [],
        }),
      });
    });

    await page.goto("/resume");
    await page.getByLabel("OpenAI API Key").fill("sk-test-key");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("checkbox").click();
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Comapny: Acme")).toBeVisible();
    await expect.poll(() => jobListingsRequestCount).toBe(1);
  });

  test("keeps streamed job listings when the completed stream payload has no profile", async ({ page }) => {
    let jobListingsRequestCount = 0;

    await page.route("**/api/resume-review/stream", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "text/event-stream",
        body: buildResumeReviewStreamBodyWithCompletedPayloadMissingProfile(),
      });
    });
    await page.route("**/api/job-listings", async (route) => {
      if (await fulfillJobListingsPreflight(route)) return;
      jobListingsRequestCount += 1;

      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          jobListings: [
            {
              listingTitle: "Backend Developer",
              companyName: "Acme",
              locationName: "Remote",
              whyItsGreat: "Matches backend API experience.",
              listingLink: "https://example.com/job",
              searchQuery: "Backend Developer Remote",
            },
          ],
        }),
      });
    });

    await page.goto("/resume");
    await page.getByLabel("OpenAI API Key").fill("sk-test-key");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("checkbox").click();
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Comapny: Acme")).toBeVisible();
    await expect.poll(() => jobListingsRequestCount).toBe(1);
  });

  test("shows an empty state when job listings search succeeds with no listings", async ({ page }) => {
    let jobListingsRequestCount = 0;

    await page.route("**/api/resume-review/stream", fulfillResumeReviewStream);
    await page.route("**/api/job-listings", async (route) => {
      if (await fulfillJobListingsPreflight(route)) return;
      jobListingsRequestCount += 1;

      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({ jobListings: [] }),
      });
    });

    await page.goto("/resume");
    await page.getByLabel("OpenAI API Key").fill("sk-test-key");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("checkbox").click();
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(
      page.getByText("No matching job listings found right now."),
    ).toBeVisible();
    await expect.poll(() => jobListingsRequestCount).toBe(1);
  });

  test("shows job listings unavailable when job listings API fails", async ({ page }) => {
    await page.route("**/api/resume-review/stream", fulfillResumeReviewStream);
    await page.route("**/api/job-listings", async (route) => {
      if (await fulfillJobListingsPreflight(route)) return;

      await route.fulfill({
        status: 502,
        contentType: "application/json",
        body: JSON.stringify({ message: "Job listings search failed." }),
      });
    });

    await page.goto("/resume");
    await page.getByLabel("OpenAI API Key").fill("sk-test-key");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("checkbox").click();
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Job listings search failed.")).toBeVisible();
  });

  test("failure path shows the API error message and stays on the form", async ({ page }) => {
    await page.addInitScript(() => {
      window.console.error = () => {};
    });

    page.on("dialog", async (dialog) => {
      expect(dialog.message()).toContain("Resume PDF must be 5MB or smaller.");
      await dialog.accept();
    });

    await page.route("**/api/resume-review/stream", async (route) => {
      await route.fulfill({
        status: 400,
        contentType: "application/json",
        body: JSON.stringify({ message: "Resume PDF must be 5MB or smaller." }),
      });
    });

    await page.goto("/resume");
    await page.getByLabel("OpenAI API Key").fill("sk-test-key");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByRole("button", { name: "Submit" })).toBeVisible();
  });

  test("streamed section failure keeps completed sections visible", async ({ page }) => {
    await page.route("**/api/resume-review/stream", async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "text/event-stream",
        body: buildResumeReviewStreamBodyWithSectionFailure(),
      });
    });

    await page.goto("/resume");
    await page.getByLabel("OpenAI API Key").fill("sk-test-key");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Test Candidate Resume Review Report")).toBeVisible();
    await expect(
      page.getByText(
        "spelling and grammar could not be generated. Other report sections may still be usable.",
      ),
    ).toBeVisible();
    await expect(page.getByText("Jobs To Look Out For")).toBeVisible();
  });
});

async function fulfillJobListingsPreflight(route: Route) {
  if (route.request().method() !== "OPTIONS") {
    return false;
  }

  await route.fulfill({
    status: 204,
    headers: {
      "Access-Control-Allow-Origin": "http://localhost:3000",
      "Access-Control-Allow-Methods": "POST, OPTIONS",
      "Access-Control-Allow-Headers": "Content-Type, X-OpenAI-Api-Key",
    },
  });

  return true;
}
