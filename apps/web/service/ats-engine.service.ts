import { apiUrl } from "@/lib/api";
import {
  AtsKeywordAnalysisResponse,
  AtsEnginePipelineResult,
  AtsEngineStage,
  AtsFinalAssessmentResponse,
  AtsHeaderValidationResponse,
  AtsKeywordExtractionResponse,
  AtsKeywordScoringResponse,
} from "@/types/AtsEngine/ats-engine.type";

export type RunAtsEngineInput = {
  aiModel: string;
  resumeFile: File;
  jobDescription: string;
  onStageChange?: (stage: AtsEngineStage) => void;
};

async function readErrorMessage(response: Response, fallback: string) {
  try {
    const errorData = await response.json();
    return errorData?.message || fallback;
  } catch {
    return fallback;
  }
}

async function postJson<TResponse>(
  path: string,
  body: unknown,
  fallbackError: string,
): Promise<TResponse> {
  const response = await fetch(apiUrl(path), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(await readErrorMessage(response, fallbackError));
  }

  return response.json();
}

async function postForm<TResponse>(
  path: string,
  formData: FormData,
  fallbackError: string,
): Promise<TResponse> {
  const response = await fetch(apiUrl(path), {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    throw new Error(await readErrorMessage(response, fallbackError));
  }

  return response.json();
}

export async function runAtsEngine({
  aiModel,
  resumeFile,
  jobDescription,
  onStageChange,
}: RunAtsEngineInput): Promise<AtsEnginePipelineResult> {
  onStageChange?.("header-validation");
  const headerFormData = new FormData();
  headerFormData.append("resume", resumeFile);

  const headerValidationPromise = postForm<AtsHeaderValidationResponse>(
    "/api/ats-engine/header-validation",
    headerFormData,
    "Header validation failed.",
  );

  if (!jobDescription.trim()) {
    const headerValidation = await headerValidationPromise;
    return {
      headerValidation,
    };
  }

  onStageChange?.("keyword-extraction");
  const keywordExtractionPromise = postJson<AtsKeywordExtractionResponse>(
    "/api/ats-engine/keyword-extraction",
    {
      job_description: jobDescription.trim(),
      ai_model: aiModel,
    },
    "Keyword extraction failed.",
  );

  const [headerValidation, keywordExtraction] = await Promise.all([
    headerValidationPromise,
    keywordExtractionPromise,
  ]);

  onStageChange?.("keyword-analysis");
  const contextualFormData = new FormData();
  contextualFormData.append("resume", resumeFile);
  contextualFormData.append("keywords_json", JSON.stringify(keywordExtraction));
  contextualFormData.append("ai_model", aiModel);

  const contextualScoring = await postForm<AtsKeywordAnalysisResponse>(
    "/api/ats-engine/keyword-analysis",
    contextualFormData,
    "Keyword analysis failed.",
  );

  onStageChange?.("keyword-scoring");
  const keywordScoring = await postJson<AtsKeywordScoringResponse>(
    "/api/ats-engine/keyword-scoring",
    {
      keyword_scores: contextualScoring.keyword_scores,
    },
    "Keyword scoring failed.",
  );

  onStageChange?.("final-assessment");
  const finalAssessment = await postJson<AtsFinalAssessmentResponse>(
    "/api/ats-engine/final-assessment",
    {
      job_title: keywordExtraction.job_title,
      header_quality_score: headerValidation.header_quality_score,
      keyword_scores: keywordScoring.keyword_scores,
    },
    "Final assessment failed.",
  );

  return {
    headerValidation,
    keywordExtraction,
    contextualScoring,
    keywordScoring,
    finalAssessment,
  };
}
