"use client";

import PdfViewer from "@/components/pdf-viewer/PdfViewer";
import Recommendations from "@/components/report-viewer/recommendations/Recommendations";
import AtsContent from "@/components/report-viewer/ats/AtsContent";
import ModelGrid from "@/components/report-viewer/model/ModelGrid";
import Disclaimer from "@/components/resume/disclaimer";
import React, { useEffect, useRef, useState } from "react";
import RecommendedListings from "@/components/report-viewer/recommendations/RecommendedListings";
import {
  useAIModelStore,
  useCheckJobListings,
  useJobDescriptionStore,
  useOpenAiApiKeyStore,
  useResumeFileStore,
} from "@/stores/store";
import { submitResumeReviewStream } from "@/service/resume-review.service";
import HomeHeader from "@/components/home/home-header";
import ResumeReviewForm from "@/components/home/resume-review-form";
import {
  JobListingsResponse,
  ResumeAnalysisResponse,
  ResumeReviewStreamSection,
} from "@/types/payload/response-payload";
import AtsContentSkeleton from "@/components/skeletons/atsContentSkeleton";
import RecommendationsSkeleton from "@/components/skeletons/RecommendationsSkeleton";
import RecommendedListingsSkeleton from "@/components/skeletons/RecommendedListingsSkeleton";
import { Progress } from "@/components/ui/progress";
import ShimmerText from "@/components/ui/shimmer-text";
import { Typewriter } from "@/components/ui/typewriter";
import { jobListingWords } from "@/constants/constants";
import { clientLogger } from "@/lib/client-logger";
import { runAtsEngine } from "@/service/ats-engine.service";
import {
  hasUsableJobSearchProfile,
  submitJobListingsSearch,
} from "@/service/job-listings.service";
import {
  AtsEnginePipelineResult,
  AtsEngineStage,
} from "@/types/AtsEngine/ats-engine.type";
import AtsEngineResults from "@/components/ats-engine/ats-engine-results";
import ResumeReviewProgressiveHeader from "@/components/report-viewer/progressive/resume-review-progressive-header";

type JobListingsStatus =
  | "idle"
  | "waiting_for_profile"
  | "loading"
  | "success"
  | "empty"
  | "error";

export default function Page() {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [fileUrl, setFileUrl] = useState<string | null>(null);

  const [reportData, setReportData] =
    useState<Partial<ResumeAnalysisResponse> | null>(null);
  const [jobSearchProfile, setJobSearchProfile] = useState<
    ResumeAnalysisResponse["jobSearchProfile"] | null
  >(null);
  const [jobListingsStatus, setJobListingsStatus] =
    useState<JobListingsStatus>("idle");
  const [jobListingsResult, setJobListingsResult] =
    useState<JobListingsResponse | null>(null);
  const [jobListingsError, setJobListingsError] = useState<string | null>(null);
  const [atsEngineResult, setAtsEngineResult] =
    useState<Partial<AtsEnginePipelineResult> | null>(null);
  const [atsEngineError, setAtsEngineError] = useState<string | null>(null);
  const [isAtsEngineLoading, setIsAtsEngineLoading] = useState(false);
  const [isAtsEngineComplete, setIsAtsEngineComplete] = useState(false);
  const [atsEngineCurrentStage, setAtsEngineCurrentStage] =
    useState<AtsEngineStage | null>(null);
  const [atsEngineCompletedStages, setAtsEngineCompletedStages] = useState<
    AtsEngineStage[]
  >([]);
  const [isResumeReviewComplete, setIsResumeReviewComplete] = useState(false);
  const [resumeReviewCompletedSections, setResumeReviewCompletedSections] =
    useState<ResumeReviewStreamSection[]>([]);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [activeReviewRunId, setActiveReviewRunId] = useState(0);
  const currentReviewRunIdRef = useRef(0);
  const jobListingsRequestKeyRef = useRef<string | null>(null);

  const aiModel = useAIModelStore((state) => state.aiModel);
  const openAiApiKey = useOpenAiApiKeyStore((state) => state.openAiApiKey);

  const jobDescription = useJobDescriptionStore(
    (state) => state.jobDescription,
  );
  const setJobDescription = useJobDescriptionStore(
    (state) => state.setJobDescription,
  );

  const checkJobListings = useCheckJobListings(
    (state) => state.checkJobListings,
  );
  const setCheckJobListings = useCheckJobListings(
    (state) => state.setCheckJobListings,
  );

  const resumeFile = useResumeFileStore((state) => state.resumeFile);
  const setResumeFile = useResumeFileStore((state) => state.setResumeFile);

  const shouldWarnBeforeUnload = !!reportData && !isSubmitting;

  useEffect(() => {
    if (openAiApiKey.trim()) {
      setSubmitError(null);
    }
  }, [openAiApiKey]);

  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (!shouldWarnBeforeUnload) return;

      e.preventDefault();
      e.returnValue = "";
    };

    window.addEventListener("beforeunload", handleBeforeUnload);

    return () => {
      window.removeEventListener("beforeunload", handleBeforeUnload);
    };
  }, [shouldWarnBeforeUnload]);

  useEffect(() => {
    if (resumeFile) {
      const url = URL.createObjectURL(resumeFile);
      setFileUrl(url);
    }
  }, [resumeFile]);

  const handleSubmit = async () => {
    const trimmedOpenAiApiKey = openAiApiKey.trim();
    if (!trimmedOpenAiApiKey) {
      setSubmitError("OpenAI API key is required before submitting.");
      return;
    }

    if (!resumeFile) {
      alert("Please upload your resume PDF.");
      return;
    }

    const url = URL.createObjectURL(resumeFile);
    setFileUrl(url);
    const reviewRunId = currentReviewRunIdRef.current + 1;
    currentReviewRunIdRef.current = reviewRunId;
    jobListingsRequestKeyRef.current = null;
    setActiveReviewRunId(reviewRunId);

    try {
      setIsSubmitting(true);

      setReportData(null);
      setJobSearchProfile(null);
      setJobListingsResult(null);
      setJobListingsError(null);
      setJobListingsStatus(checkJobListings ? "waiting_for_profile" : "idle");
      setAtsEngineResult(null);
      setAtsEngineError(null);
      setIsAtsEngineLoading(true);
      setIsAtsEngineComplete(false);
      setAtsEngineCurrentStage(null);
      setAtsEngineCompletedStages([]);
      setIsResumeReviewComplete(false);
      setResumeReviewCompletedSections([]);
      setProgress(0);
      setSubmitError(null);

      const atsEnginePromise = runAtsEngine({
        aiModel,
        openAiApiKey: trimmedOpenAiApiKey,
        resumeFile,
        jobDescription,
        onStageChange: (stage) => {
          clientLogger.info("ats_engine_stage_started", {
            stage,
            aiModel,
            hasJobDescription: Boolean(jobDescription.trim()),
          });
          setAtsEngineCurrentStage(stage);
        },
        onStageCompleted: (stage, _payload, partialResult) => {
          setAtsEngineResult(partialResult);
          setAtsEngineCompletedStages((current) =>
            current.includes(stage) ? current : [...current, stage],
          );
          clientLogger.info("ats_engine_stage_completed", {
            stage,
            aiModel,
            hasJobDescription: Boolean(jobDescription.trim()),
            keywordCount: partialResult.keywordExtraction?.keywords.length ?? 0,
            scoredKeywordCount:
              partialResult.keywordScoring?.keyword_scores.length ?? 0,
          });
        },
        onCompleted: (result) => {
          setAtsEngineResult(result);
          setIsAtsEngineComplete(true);
          setAtsEngineCurrentStage(null);
        },
      })
        .then((result) => {
          setAtsEngineResult(result);
          setIsAtsEngineComplete(true);
          setAtsEngineCurrentStage(null);
          clientLogger.info("ats_engine_completed", {
            aiModel,
            hasJobDescription: Boolean(jobDescription.trim()),
            keywordCount: result.keywordExtraction?.keywords.length ?? 0,
            scoredKeywordCount:
              result.keywordScoring?.keyword_scores.length ?? 0,
          });
        })
        .catch((error) => {
          setAtsEngineError(
            error instanceof Error
              ? error.message
              : "ATS Engine assessment failed.",
          );
          clientLogger.error("ats_engine_failed", error, {
            aiModel,
            hasJobDescription: Boolean(jobDescription.trim()),
          });
        })
        .finally(() => {
          setIsAtsEngineLoading(false);
        });

      const resumeReviewPromise = submitResumeReviewStream({
        aiModel,
        openAiApiKey: trimmedOpenAiApiKey,
        resumeFile,
        jobDescription,
        onSectionStarted: (section) => {
          clientLogger.info("resume_review_section_started", {
            aiModel,
            section,
          });
        },
        onSectionCompleted: (section, payload) => {
          setReportData((current) =>
            applyResumeReviewSection(current, section, payload),
          );
          setResumeReviewCompletedSections((current) =>
            current.includes(section) ? current : [...current, section],
          );
          clientLogger.info("resume_review_section_completed", {
            aiModel,
            section,
          });

          if (section === "job_search_profile") {
            const profile =
              payload as ResumeAnalysisResponse["jobSearchProfile"];
            setJobSearchProfile(profile);
          }
        },
        onSectionFailed: (section, warning) => {
          setReportData((current) => ({
            ...(current ?? {}),
            warnings: [...(current?.warnings ?? []), warning],
          }));
          clientLogger.error(
            "resume_review_section_failed",
            new Error(warning),
            {
              aiModel,
              section,
            },
          );
        },
        onCompleted: (result) => {
          setReportData(result);
          setIsResumeReviewComplete(true);
        },
      }).then((result) => {
        setReportData(result);
        setIsResumeReviewComplete(true);
        clientLogger.info("resume_review_completed", {
          aiModel,
          hasJobDescription: Boolean(jobDescription.trim()),
          checkJobListings,
        });
      });

      void atsEnginePromise;
      await resumeReviewPromise;
    } catch (error) {
      clientLogger.error("resume_review_failed", error, {
        aiModel,
        hasJobDescription: Boolean(jobDescription.trim()),
      });
      alert(error instanceof Error ? error.message : "Something went wrong");
    } finally {
      setIsSubmitting(false);
    }
  };

  useEffect(() => {
    if (!checkJobListings || !activeReviewRunId) {
      return;
    }

    if (!jobSearchProfile) {
      setJobListingsStatus((current) =>
        current === "idle" ? "waiting_for_profile" : current,
      );
      return;
    }

    if (!hasUsableJobSearchProfile(jobSearchProfile)) {
      clientLogger.warn("job_listings_search_skipped_invalid_profile");
      setJobListingsResult(null);
      setJobListingsError("Job listings are unavailable for this profile.");
      setJobListingsStatus("error");
      return;
    }

    const trimmedOpenAiApiKey = openAiApiKey.trim();

    if (!trimmedOpenAiApiKey) {
      clientLogger.warn("job_listings_search_skipped_missing_api_key");
      setJobListingsResult(null);
      setJobListingsError("OpenAI API key is required to search job listings.");
      setJobListingsStatus("error");
      return;
    }

    const profileKey = buildJobSearchProfileKey(jobSearchProfile);
    const requestKey = `${activeReviewRunId}:${profileKey}`;

    if (jobListingsRequestKeyRef.current === requestKey) {
      return;
    }

    const controller = new AbortController();
    jobListingsRequestKeyRef.current = requestKey;
    setJobListingsStatus("loading");
    setJobListingsError(null);

    clientLogger.info("job_listings_search_started", {
      status: "loading",
      reviewRunId: activeReviewRunId,
      requestKeyHash: buildShortHash(requestKey),
      titleCount: jobSearchProfile.titles?.length ?? 0,
      keywordCount: jobSearchProfile.keywords?.length ?? 0,
      locationCount: jobSearchProfile.locations?.length ?? 0,
      excludeCount: jobSearchProfile.exclude?.length ?? 0,
      hasSeniority: Boolean(jobSearchProfile.seniority?.trim()),
      ...(process.env.NODE_ENV === "production"
        ? {}
        : {
            titles: jobSearchProfile.titles ?? [],
            keywords: jobSearchProfile.keywords ?? [],
            seniority: jobSearchProfile.seniority ?? "",
            locations: jobSearchProfile.locations ?? [],
            exclude: jobSearchProfile.exclude ?? [],
          }),
    });

    submitJobListingsSearch({
      openAiApiKey: trimmedOpenAiApiKey,
      profile: jobSearchProfile,
      signal: controller.signal,
    })
      .then((data) => {
        if (
          controller.signal.aborted ||
          currentReviewRunIdRef.current !== activeReviewRunId ||
          jobListingsRequestKeyRef.current !== requestKey
        ) {
          return;
        }

        const listingCount = data.jobListings?.length ?? 0;
        setJobListingsResult(data);
        setJobListingsStatus(listingCount > 0 ? "success" : "empty");
        clientLogger.info("job_listings_search_completed", {
          listingCount,
        });
      })
      .catch((error) => {
        if (
          controller.signal.aborted ||
          currentReviewRunIdRef.current !== activeReviewRunId ||
          jobListingsRequestKeyRef.current !== requestKey
        ) {
          return;
        }

        clientLogger.error("job_listings_search_failed", error, {
          titleCount: jobSearchProfile.titles?.length ?? 0,
          keywordCount: jobSearchProfile.keywords?.length ?? 0,
          locationCount: jobSearchProfile.locations?.length ?? 0,
          hasSeniority: Boolean(jobSearchProfile.seniority?.trim()),
        });
        setJobListingsResult(null);
        setJobListingsError(
          error instanceof Error
            ? error.message
            : "Job listings search failed.",
        );
        setJobListingsStatus("error");
      });

    return () => {
      controller.abort();
    };
  }, [activeReviewRunId, checkJobListings, jobSearchProfile, openAiApiKey]);

  const [progress, setProgress] = useState(0);

  useEffect(() => {
    if (!isSubmitting || isResumeReviewComplete) {
      return;
    }

    const duration = 60000;
    const intervalTime = 350;
    const steps = duration / intervalTime;
    const increment = 90 / steps;

    const interval = setInterval(() => {
      setProgress((prev) => {
        const next = prev + increment;
        if (next >= 90) {
          clearInterval(interval);
          return 90;
        }
        return next;
      });
    }, intervalTime);

    return () => clearInterval(interval);
  }, [isResumeReviewComplete, isSubmitting]);

  const resumeReviewProgress = isResumeReviewComplete
    ? 100
    : Math.max(
        progress,
        Math.min(95, resumeReviewCompletedSections.length * 22),
      );

  return (
    <div className="flex flex-col items-center justify-center container mx-auto ">
      <div className=" w-full  md:px-36 px-4 pt-4">
        {!reportData && !isSubmitting ? (
          <div className="flex flex-col gap-4 justify-center px-4  ">
            <HomeHeader />

            <ModelGrid />

            <div className="text-sm text-muted-foreground">
              Tip: For significantly better review quality, we recommend using
              GPT 5 models.
            </div>

            <ResumeReviewForm
              jobDescription={jobDescription}
              setJobDescription={setJobDescription}
              checkJobListings={checkJobListings}
              setCheckJobListings={setCheckJobListings}
              setResumeFile={setResumeFile}
              resumeFile={resumeFile}
              handleSubmit={handleSubmit}
              isSubmitting={isSubmitting}
              isSubmitDisabled={!openAiApiKey.trim()}
              submitError={submitError}
            />
          </div>
        ) : (
          <div className="flex flex-col gap-2">
            <div className="flex flex-col-reverse divide-y-accent md:gap-2 gap-8 md:flex-row   w-full md:divide-x items-start">
              <div className=" w-full md:w-1/2 md:p-2 md:sticky md:top-16">
                <div className="sticky top-16">
                  <PdfViewer pdfUrl={`${fileUrl}`} />
                </div>
              </div>

              {/* QUALITATIVE ANALYSIS */}
              <div className=" md:w-1/2 md:p-2 flex flex-col divide-y">
                {(isSubmitting || reportData) && (
                  <ResumeReviewProgressiveHeader
                    atsContent={reportData?.atsContent}
                    aiModel={aiModel}
                    isComplete={isResumeReviewComplete}
                    progress={resumeReviewProgress}
                    completedSections={resumeReviewCompletedSections}
                    warnings={reportData?.warnings}
                  />
                )}
                {reportData?.atsContent && reportData?.spellingAndGrammar && (
                  <AtsContent
                    atsContent={reportData.atsContent}
                    spellingAndGrammar={reportData.spellingAndGrammar}
                  />
                )}
                {isSubmitting &&
                  (!reportData?.atsContent ||
                    !reportData?.spellingAndGrammar) && <AtsContentSkeleton />}

                {/* QUANTITATIVE ANALYSIS */}
                {(isAtsEngineLoading || atsEngineResult || atsEngineError) && (
                  <AtsEngineResults
                    aiModel={aiModel}
                    completedStages={atsEngineCompletedStages}
                    currentStage={atsEngineCurrentStage}
                    error={atsEngineError}
                    hasJobDescription={Boolean(jobDescription.trim())}
                    isComplete={isAtsEngineComplete}
                    isLoading={isAtsEngineLoading}
                    result={atsEngineResult}
                  />
                )}

                {/* JOB RECOMMENDATION  */}
                {reportData?.jobRecommendation && (
                  <Recommendations
                    jobRecommendation={reportData.jobRecommendation}
                  />
                )}
                {isSubmitting && !reportData?.jobRecommendation && (
                  <RecommendationsSkeleton />
                )}

                {/* JOB SEARCH  */}
                {jobListingsStatus === "loading" && (
                  <div className="w-full flex flex-col gap-2 items-start justify-center px-12 py-2">
                    <ShimmerText className="text-muted-foreground text-sm">
                      <Typewriter
                        words={jobListingWords}
                        speed={40}
                        delayBetweenWords={2000}
                        cursor={false}
                      />
                    </ShimmerText>

                    <Progress value={progress} className="w-full" />
                    <RecommendedListingsSkeleton />
                  </div>
                )}

                {jobListingsResult?.jobListings &&
                  jobListingsResult.jobListings.length > 0 && (
                    <RecommendedListings
                      jobListings={jobListingsResult.jobListings}
                    />
                  )}

                {jobListingsStatus === "empty" && (
                  <div
                    className="rounded-md border border-muted bg-muted/30 p-3 text-sm text-muted-foreground"
                    role="status"
                  >
                    No matching job listings found right now.
                  </div>
                )}
                {jobListingsError && jobListingsStatus !== "loading" && (
                  <div
                    className="rounded-md border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive"
                    role="alert"
                  >
                    {jobListingsError}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
        <Disclaimer />
      </div>
    </div>
  );
}

function applyResumeReviewSection(
  current: Partial<ResumeAnalysisResponse> | null,
  section: ResumeReviewStreamSection,
  payload: unknown,
): Partial<ResumeAnalysisResponse> {
  switch (section) {
    case "ats_content":
      return {
        ...(current ?? {}),
        atsContent: payload as ResumeAnalysisResponse["atsContent"],
      };
    case "spelling_and_grammar":
      return {
        ...(current ?? {}),
        spellingAndGrammar:
          payload as ResumeAnalysisResponse["spellingAndGrammar"],
      };
    case "job_recommendation":
      return {
        ...(current ?? {}),
        jobRecommendation:
          payload as ResumeAnalysisResponse["jobRecommendation"],
      };
    case "job_search_profile":
      return {
        ...(current ?? {}),
        jobSearchProfile: payload as ResumeAnalysisResponse["jobSearchProfile"],
      };
  }
}

function buildJobSearchProfileKey(
  profile: ResumeAnalysisResponse["jobSearchProfile"],
) {
  return JSON.stringify({
    titles: profile.titles ?? [],
    keywords: profile.keywords ?? [],
    seniority: profile.seniority ?? "",
    locations: profile.locations ?? [],
    exclude: profile.exclude ?? [],
  });
}

function buildShortHash(value: string) {
  let hash = 5381;

  for (let index = 0; index < value.length; index += 1) {
    hash = (hash * 33) ^ value.charCodeAt(index);
  }

  return (hash >>> 0).toString(16);
}
