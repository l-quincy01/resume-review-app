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


export interface JobListingsResponse {

  jobListings: jobListing[];
}
