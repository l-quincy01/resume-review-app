import { ResumeAnalysisResponse } from "@/types/payload/response-payload";
import { apiUrl } from "@/lib/api";

export type SubmitResumeReviewInput = {
  aiModel: string;
  resumeFile: File;
  jobDescription?: string;
};

export async function submitResumeReview({
  aiModel,
  resumeFile,
  jobDescription,
}: SubmitResumeReviewInput): Promise<ResumeAnalysisResponse> {
  const formData = new FormData();

  formData.append("aiModel", aiModel);
  formData.append("resume", resumeFile);

  if (jobDescription?.trim()) {
    formData.append("jobDescription", jobDescription.trim());
  }

  const response = await fetch(apiUrl("/api/resume-review"), {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    let errorMessage = "Failed to submit resume review";

    try {
      const errorData = await response.json();
      errorMessage = errorData?.message || errorMessage;
    } catch {
      //
    }

    throw new Error(errorMessage);
  }

  return response.json();
}
