import { create } from "zustand";

type AIModelState = {
  aiModel: string;
  setAIModel: (model: string) => void;
};

type JobDescriptionState = {
  jobDescription: string;
  setJobDescription: (value: string) => void;
};

type ResumeFileState = {
  resumeFile: File | null;
  setResumeFile: (file: File | null) => void;
};

type CheckJobListings = {
  checkJobListings: boolean;
  setCheckJobListings: (value: boolean) => void;
};

type OpenAiApiKeyState = {
  openAiApiKey: string;
  setOpenAiApiKey: (value: string) => void;
  clearOpenAiApiKey: () => void;
};

export const useAIModelStore = create<AIModelState>((set) => ({
  aiModel: "gpt-4.1-mini",
  setAIModel: (model) => set({ aiModel: model }),
}));

export const useJobDescriptionStore = create<JobDescriptionState>((set) => ({
  jobDescription: "",
  setJobDescription: (value) => set({ jobDescription: value }),
}));

export const useResumeFileStore = create<ResumeFileState>((set) => ({
  resumeFile: null,
  setResumeFile: (file) => set({ resumeFile: file }),
}));

export const useCheckJobListings = create<CheckJobListings>((set) => ({
  checkJobListings: false,
  setCheckJobListings: (check) => set({ checkJobListings: check }),
}));

export const useOpenAiApiKeyStore = create<OpenAiApiKeyState>((set) => ({
  openAiApiKey: "",
  setOpenAiApiKey: (value) => set({ openAiApiKey: value }),
  clearOpenAiApiKey: () => set({ openAiApiKey: "" }),
}));
