import { atsContent, spellingAndGrammar } from "../atsReport.type";
import { jobListing, jobSearchProfile } from "../jobListing.type";
import { JobRecommendation } from "../jobRecommendations.type";

export interface ResumeAnalysisResponse {
  jobRecommendation: JobRecommendation;
  spellingAndGrammar: spellingAndGrammar;
  atsContent: atsContent;
  jobSearchProfile: jobSearchProfile;
  warnings?: string[];
}

export type ResumeReviewStreamSection =
  | "ats_content"
  | "spelling_and_grammar"
  | "job_recommendation"
  | "job_search_profile";

export type ResumeReviewStreamEventName =
  | "review_started"
  | "section_started"
  | "section_completed"
  | "section_failed"
  | "review_completed"
  | "review_failed";

export interface ResumeReviewStreamEvent {
  section?: ResumeReviewStreamSection;
  payload?: unknown;
  warning?: string;
  completed_sections: ResumeReviewStreamSection[];
  timestamp: string;
}


export interface JobListingsResponse {

  jobListings: jobListing[];
}
