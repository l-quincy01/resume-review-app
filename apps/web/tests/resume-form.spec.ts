import { expect, test } from "@playwright/test";
import { pdfBuffer } from "./fixtures/resumeReview";

test.describe("resume review form", () => {
  test("renders model choices and upload controls", async ({ page }) => {
    await page.goto("/resume");

    await expect(page.getByText("GPT-4.1 Mini")).toBeVisible();
    await expect(page.getByText("GPT-5 Mini")).toBeVisible();
    await expect(page.getByText("GPT-5.4")).toBeVisible();
    await expect(page.getByLabel("Paste A Job Description For Your Desired Job")).toBeVisible();
    await expect(page.getByLabel("Upload CV")).toHaveAttribute("accept", "application/pdf,.pdf");
    await expect(page.getByLabel("Consent to AI resume processing")).toBeVisible();
    await expect(page.getByRole("link", { name: "Privacy Policy" })).toHaveAttribute("href", "/privacy");
    await expect(page.getByRole("link", { name: "Terms" })).toHaveAttribute("href", "/terms");
  });

  test("requires consent before enabling submit", async ({ page }) => {
    await page.goto("/resume");

    const submit = page.getByRole("button", { name: "Submit" });
    await expect(submit).toBeDisabled();

    await page.getByLabel("Consent to AI resume processing").click();

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
