import { atsContent, spellingAndGrammar } from "../atsReport.type";
import { jobListing, jobListings, jobSearchProfile } from "../jobListing.type";
import { jobMatch } from "../jobMatch.type";
import { JobRecommendation } from "../jobRecommendations.type";

export interface ResumeAnalysisResponse {
  jobRecommendation: JobRecommendation;
  spellingAndGrammar: spellingAndGrammar;
  jobMatch: jobMatch;
  atsContent: atsContent;
  jobSearchProfile: jobSearchProfile;



}


export interface JobListingsResponse {

  jobListings: jobListing[];
}
