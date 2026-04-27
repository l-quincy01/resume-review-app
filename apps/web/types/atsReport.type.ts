export interface atsContent {
  heading: { section: string; score: number };
  resumeName: string;
  content: {
    section:
      | "ATS Compaitablity"
      | "Professional summary"
      | "Work experience"
      | "Education"
      | "Projects"
      | "Skills"
      | "Cerificates";
    summary: string;
    strengths: string[];
    weaknesses: string[];
    suggestions: { type: "Low" | "Medium" | "High"; content: string }[];
    suggestedRewrites?: {
      current: string;
      suggestion: string;
      expplanation: string;
    }[];
    score: number;
  }[];
}

export interface spellingAndGrammar {
  score: number;
  grammarSuggestions: {
    type: "Low" | "Medium" | "High";
    current: string;
    suggestedCorrection: string;
    explanation: string;
  }[];
  spellingSuggestions: {
    type: "Low" | "Medium" | "High";
    current: string;
    suggestedCorrection: string;
    explanation: string;
  }[];
  personalPronounCheck: {
    type: "Low" | "Medium" | "High";
    current: string;
    suggestedCorrection: string;
    explanation: string;
  }[];
  passiveVoiceCheck: {
    type: "Low" | "Medium" | "High";
    current: string;
    suggestedCorrection: string;
    explanation: string;
  }[];
}
