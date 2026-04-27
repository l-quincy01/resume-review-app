export interface ResumeReviewRequest {
  aiModel: string;
  jobDescription?: string;
  resume: File;
}

export function buildResumeReviewPayload(params: {
  aiModel: string;
  resumeFile: File;
  jobDescription?: string;
}) {
  const formData = new FormData();

  formData.append("aiModel", params.aiModel);
  formData.append("resume", params.resumeFile);

  if (params.jobDescription?.trim()) {
    formData.append("jobDescription", params.jobDescription.trim());
  }

  return formData;
}
