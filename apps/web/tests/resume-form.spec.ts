import { expect, test } from "@playwright/test";
import { pdfBuffer } from "./fixtures/resumeReview";

test.describe("resume review form", () => {
  test("renders model choices and upload controls", async ({ page }) => {
    await page.goto("/resume");

    await expect(page.getByText("GPT-4.1 Mini")).toBeVisible();
    await expect(page.getByText("GPT-5 Mini")).toBeVisible();
    await expect(page.getByText("GPT-5.4")).toBeVisible();
    await expect(page.getByLabel("OpenAI API Key")).toBeVisible();
    await expect(page.getByLabel("Paste A Job Description For Your Desired Job")).toBeVisible();
    await expect(page.getByLabel("Upload CV")).toHaveAttribute("accept", "application/pdf,.pdf");
  });

  test("requires an OpenAI API key before enabling submit", async ({ page }) => {
    await page.goto("/resume");

    const submit = page.getByRole("button", { name: "Submit" });
    await expect(submit).toBeDisabled();
    await expect(page.getByText("OpenAI API key is required before submitting.")).toBeVisible();

    await page.getByLabel("OpenAI API Key").fill("sk-test-key");
    await expect(submit).toBeEnabled();
  });

  test("shows the selected PDF file name", async ({ page }) => {
    await page.goto("/resume");

    await page.getByLabel("Upload CV").setInputFiles({
      name: "resume.pdf",
      mimeType: "application/pdf",
      buffer: pdfBuffer,
    });

    await expect(page.getByText("Selected file: resume.pdf")).toBeVisible();
  });
});
