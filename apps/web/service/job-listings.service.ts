import { apiUrl } from "@/lib/api";
import {
  JobListingsResponse,
  ResumeAnalysisResponse,
} from "@/types/payload/response-payload";

export type SubmitJobListingsSearchInput = {
  openAiApiKey: string;
  profile: ResumeAnalysisResponse["jobSearchProfile"];
  signal?: AbortSignal;
};

export function hasUsableJobSearchProfile(
  profile: ResumeAnalysisResponse["jobSearchProfile"] | undefined,
) {
  return Boolean(
    profile &&
      ((profile.titles?.length ?? 0) > 0 || (profile.keywords?.length ?? 0) > 0),
  );
}

export async function submitJobListingsSearch({
  openAiApiKey,
  profile,
  signal,
}: SubmitJobListingsSearchInput): Promise<JobListingsResponse> {
  const response = await fetch(apiUrl("/api/job-listings"), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-OpenAI-Api-Key": openAiApiKey,
    },
    body: JSON.stringify(profile),
    signal,
  });

  if (!response.ok) {
    throw new Error(await readErrorMessage(response));
  }

  return response.json();
}

async function readErrorMessage(response: Response) {
  try {
    const errorData = await response.json();
    return errorData?.message || "Failed to fetch job listings.";
  } catch {
    return "Failed to fetch job listings.";
  }
}
