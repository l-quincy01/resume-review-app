import {
  ResumeAnalysisResponse,
  ResumeReviewStreamEvent,
  ResumeReviewStreamEventName,
  ResumeReviewStreamSection,
} from "@/types/payload/response-payload";
import { apiUrl } from "@/lib/api";

export type SubmitResumeReviewInput = {
  aiModel: string;
  resumeFile: File;
  jobDescription?: string;
};

export type SubmitResumeReviewStreamInput = SubmitResumeReviewInput & {
  onSectionStarted?: (section: ResumeReviewStreamSection) => void;
  onSectionCompleted?: (
    section: ResumeReviewStreamSection,
    payload: unknown,
  ) => void;
  onSectionFailed?: (
    section: ResumeReviewStreamSection | undefined,
    warning: string,
  ) => void;
  onCompleted?: (response: ResumeAnalysisResponse) => void;
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

export async function submitResumeReviewStream({
  aiModel,
  resumeFile,
  jobDescription,
  onSectionStarted,
  onSectionCompleted,
  onSectionFailed,
  onCompleted,
}: SubmitResumeReviewStreamInput): Promise<ResumeAnalysisResponse> {
  const formData = buildResumeReviewFormData({
    aiModel,
    resumeFile,
    jobDescription,
  });

  const response = await fetch(apiUrl("/api/resume-review/stream"), {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    throw new Error(
      await readErrorMessage(response, "Failed to submit resume review"),
    );
  }

  if (!response.body) {
    throw new Error("Resume review stream was empty.");
  }

  let completedResponse: ResumeAnalysisResponse | null = null;

  await readSseStream(response.body, (eventName, eventData) => {
    if (eventName === "section_started" && eventData.section) {
      onSectionStarted?.(eventData.section);
      return;
    }

    if (eventName === "section_completed" && eventData.section) {
      onSectionCompleted?.(eventData.section, eventData.payload);
      return;
    }

    if (eventName === "section_failed") {
      onSectionFailed?.(
        eventData.section,
        eventData.warning ?? "Resume review section failed.",
      );
      return;
    }

    if (eventName === "review_failed") {
      throw new Error(eventData.warning ?? "Resume review failed.");
    }

    if (eventName === "review_completed") {
      completedResponse = eventData.payload as ResumeAnalysisResponse;
      onCompleted?.(completedResponse);
    }
  });

  if (!completedResponse) {
    throw new Error("Resume review stream ended before completion.");
  }

  return completedResponse;
}

function buildResumeReviewFormData({
  aiModel,
  resumeFile,
  jobDescription,
}: SubmitResumeReviewInput) {
  const formData = new FormData();

  formData.append("aiModel", aiModel);
  formData.append("resume", resumeFile);

  if (jobDescription?.trim()) {
    formData.append("jobDescription", jobDescription.trim());
  }

  return formData;
}

async function readErrorMessage(response: Response, fallback: string) {
  let errorMessage = fallback;

  try {
    const errorData = await response.json();
    errorMessage = errorData?.message || errorMessage;
  } catch {
    //
  }

  return errorMessage;
}

async function readSseStream(
  stream: ReadableStream<Uint8Array>,
  onEvent: (
    eventName: ResumeReviewStreamEventName,
    eventData: ResumeReviewStreamEvent,
  ) => void,
) {
  const reader = stream.getReader();
  const decoder = new TextDecoder();
  let buffer = "";

  while (true) {
    const { value, done } = await reader.read();

    if (done) {
      break;
    }

    buffer += decoder.decode(value, { stream: true });
    const parts = buffer.split("\n\n");
    buffer = parts.pop() ?? "";

    for (const part of parts) {
      const parsedEvent = parseSseEvent(part);
      if (parsedEvent) {
        onEvent(parsedEvent.eventName, parsedEvent.eventData);
      }
    }
  }

  buffer += decoder.decode();
  const parsedEvent = parseSseEvent(buffer);
  if (parsedEvent) {
    onEvent(parsedEvent.eventName, parsedEvent.eventData);
  }
}

function parseSseEvent(rawEvent: string) {
  const lines = rawEvent.split("\n");
  const eventLine = lines.find((line) => line.startsWith("event:"));
  const dataLines = lines.filter((line) => line.startsWith("data:"));

  if (!eventLine || dataLines.length === 0) {
    return null;
  }

  const eventName = eventLine.replace("event:", "").trim();
  const data = dataLines
    .map((line) => line.replace("data:", "").trimStart())
    .join("\n");

  return {
    eventName: eventName as ResumeReviewStreamEventName,
    eventData: JSON.parse(data) as ResumeReviewStreamEvent,
  };
}
