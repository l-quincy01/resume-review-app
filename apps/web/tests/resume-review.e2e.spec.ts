import { expect, test } from "@playwright/test";
import { fulfillResumeReview, pdfBuffer } from "./fixtures/resumeReview";

test.describe("resume review flow", () => {
  test("happy path submits a multipart resume and renders the report", async ({ page }) => {
    let sawMultipartRequest = false;

    await page.route("**/api/resume-review", async (route) => {
      const request = route.request();
      sawMultipartRequest =
        request.method() === "POST" &&
        (request.headers()["content-type"] ?? "").includes("multipart/form-data");

      await fulfillResumeReview(route);
    });

    await page.goto("/resume");
    await page.getByLabel("Paste A Job Description For Your Desired Job").fill("Build reliable APIs.");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByLabel("Consent to AI resume processing").click();
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByText("Test Candidate Resume Review Report")).toBeVisible();
    await expect(page.getByText("Jobs To Look Out For")).toBeVisible();
    expect(sawMultipartRequest).toBe(true);
  });

  test("failure path shows the API error message and stays on the form", async ({ page }) => {
    await page.addInitScript(() => {
      window.console.error = () => {};
    });

    page.on("dialog", async (dialog) => {
      expect(dialog.message()).toContain("Resume PDF must be 5MB or smaller.");
      await dialog.accept();
    });

    await page.route("**/api/resume-review", async (route) => {
      await route.fulfill({
        status: 400,
        contentType: "application/json",
        body: JSON.stringify({ message: "Resume PDF must be 5MB or smaller." }),
      });
    });

    await page.goto("/resume");
    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });
    await page.getByLabel("Consent to AI resume processing").click();
    await page.getByRole("button", { name: "Submit" }).click();

    await expect(page.getByRole("button", { name: "Submit" })).toBeVisible();
  });
});
