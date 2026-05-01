export interface targetJobMatch {
  score: number;
  type:
    | "Skills Match"
    | "Keywords & ATS Optimization"
    | "Education & Qualifications"
    | "Industry/Domain Relevance"
    | "Job Title Alignment"
    | "Seniority/Experience Level"
    | "Accomplishments & Metrics"
    | "Cultural & Values Fit Signals";
  content: string;
}

export interface jobMatch {
  name?: string;
  targetJob: targetJobMatch[];
  jobDescription?: string;
  overallScore: number;
}
