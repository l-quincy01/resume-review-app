export interface AtsHeaderValidationResponse {
  header_quality_score: number;
  headers_found: string[];
  headers_missing: string[];
  unclear_headers: string[];
  non_standard_headers?: AtsNonStandardHeader[];
  structure_quality: string;
}

export interface AtsNonStandardHeader {
  header_found: string;
  mapped_to: string;
  recommended_header: string;
}

export interface AtsKeywordExtractionResponse {
  job_title: string;
  keywords: AtsKeyword[];
}

export interface AtsKeyword {
  keyword: string;
  keyword_type: string;
  category: string;
  tier: number;
  requirement: string;
  context: string;
  variations: string[];
  frequency: number;
  boost_applied: boolean;
}

export interface AtsKeywordAnalysisResponse {
  keyword_scores: AtsContextualKeywordScore[];
}

export interface AtsContextualKeywordScore {
  keyword: string;
  present: boolean;
  tier: number;
  requirement: string;
  keyword_type: string;
  frequency: number;
  context: string;
  variations: string[];
  matched_terms: string[];
  context_type: AtsContextType;
  evidence: AtsEvidence[];
}

export interface AtsKeywordScoringResponse {
  keyword_scores: AtsScoredKeyword[];
}

export interface AtsScoredKeyword extends AtsContextualKeywordScore {
  requirement_multiplier: number;
  context_points: number;
  keyword_score: number;
}

export interface AtsContextType {
  has_achievement: boolean;
  has_metric: boolean;
  has_action_verb: boolean;
  in_experience_section: boolean;
  in_project_section: boolean;
  in_summary_section: boolean;
}

export interface AtsEvidence {
  section: string;
  text: string;
  matched_term: string;
}

export interface AtsFinalAssessmentResponse {
  job_title: string;
  ats_readiness_score: number;
  overall_keyword_score: number;
  header_quality_score: number;
  coverage: AtsCoverage;
  tier_breakdown: AtsTierBreakdown;
  critical_gaps: AtsCriticalGap[];
  strengths: AtsStrength[];
  recommendations: AtsRecommendation[];
}

export interface AtsCoverage {
  overall_presence_rate: number;
  tier_0_coverage: number;
  tier_1_coverage: number;
  must_have_coverage: number;
  total_keywords_found: number;
  total_keywords_expected: number;
}

export interface AtsTierBreakdown {
  tier_0: AtsTierBreakdownItem;
  tier_1: AtsTierBreakdownItem;
  tier_2: AtsTierBreakdownItem;
  tier_3: AtsTierBreakdownItem;
}

export interface AtsTierBreakdownItem {
  average_score: number;
  present: number;
  total: number;
  missing: string[];
}

export interface AtsCriticalGap {
  keyword: string;
  tier: number;
  requirement: string;
  reason: string;
}

export interface AtsStrength {
  keyword: string;
  score: number;
  reason: string;
  evidence: AtsEvidence[];
}

export interface AtsRecommendation {
  keyword: string;
  priority: string;
  issue: string;
  suggestion: string;
}

export interface AtsEnginePipelineResult {
  headerValidation: AtsHeaderValidationResponse;
  keywordExtraction?: AtsKeywordExtractionResponse;
  contextualScoring?: AtsKeywordAnalysisResponse;
  keywordScoring?: AtsKeywordScoringResponse;
  finalAssessment?: AtsFinalAssessmentResponse;
}

export type AtsEngineStage =
  | "header-validation"
  | "keyword-extraction"
  | "keyword-analysis"
  | "keyword-scoring"
  | "final-assessment";
