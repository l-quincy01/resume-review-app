"use client";

import PdfViewer from "@/components/pdf-viewer/PdfViewer";
import Recommendations from "@/components/report-viewer/recommendations/Recommendations";
import AtsContent from "@/components/report-viewer/ats/AtsContent";
import ModelGrid from "@/components/report-viewer/model/ModelGrid";
import Disclaimer from "@/components/resume/disclaimer";
import React, { useCallback, useEffect, useState } from "react";
import RecommendedListings from "@/components/report-viewer/recommendations/RecommendedListings";
import {
  useAIModelStore,
  useCheckJobListings,
  useJobDescriptionStore,
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
import { apiUrl } from "@/lib/api";
import { clientLogger } from "@/lib/client-logger";
import { runAtsEngine } from "@/service/ats-engine.service";
import { AtsEnginePipelineResult } from "@/types/AtsEngine/ats-engine.type";
import AtsEngineResults from "@/components/ats-engine/ats-engine-results";
import AtsEngineResultsSkeleton from "@/components/ats-engine/ats-engine-results-skeleton";
import ResumeReviewProgressiveHeader from "@/components/report-viewer/progressive/resume-review-progressive-header";

export default function Page() {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoadingJobListings, setIsLoadingJobListings] = useState(false);
  const [fileUrl, setFileUrl] = useState<string | null>(null);

  const [reportData, setReportData] =
    useState<Partial<ResumeAnalysisResponse> | null>(null);
  const [jobListings, setJobListings] = useState<JobListingsResponse | null>(
    null,
  );
  const [atsEngineResult, setAtsEngineResult] =
    useState<AtsEnginePipelineResult | null>(null);
  const [atsEngineError, setAtsEngineError] = useState<string | null>(null);
  const [isAtsEngineLoading, setIsAtsEngineLoading] = useState(false);
  const [isResumeReviewComplete, setIsResumeReviewComplete] = useState(false);
  const [resumeReviewCompletedSections, setResumeReviewCompletedSections] =
    useState<ResumeReviewStreamSection[]>([]);

  const aiModel = useAIModelStore((state) => state.aiModel);

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
    if (!resumeFile) {
      alert("Please upload your resume PDF.");
      return;
    }

    const url = URL.createObjectURL(resumeFile);
    setFileUrl(url);

    try {
      setIsSubmitting(true);

      setReportData(null);
      setJobListings(null);
      setAtsEngineResult(null);
      setAtsEngineError(null);
      setIsAtsEngineLoading(true);
      setIsResumeReviewComplete(false);
      setResumeReviewCompletedSections([]);
      setProgress(0);
      setIsLoadingJobListings(false);

      const atsEnginePromise = runAtsEngine({
        aiModel,
        resumeFile,
        jobDescription,
        onStageChange: (stage) => {
          clientLogger.info("ats_engine_stage_started", {
            stage,
            aiModel,
            hasJobDescription: Boolean(jobDescription.trim()),
          });
        },
      })
        .then((result) => {
          setAtsEngineResult(result);
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

  const submitJobListingsSearch = useCallback(
    async (profile: ResumeAnalysisResponse["jobSearchProfile"]) => {
      if (!profile) return;

      try {
        setIsLoadingJobListings(true);

        const response = await fetch(apiUrl("/api/job-listings"), {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(profile),
        });

        if (!response.ok) {
          throw new Error("Failed to fetch job listings");
        }

        const data: JobListingsResponse = await response.json();
        setJobListings(data);
        clientLogger.info("job_listings_search_completed", {
          listingCount: data.jobListings.length,
        });
      } catch (error) {
        clientLogger.error("job_listings_search_failed", error, {
          titles: profile.titles,
          seniority: profile.seniority,
        });
        setJobListings(null);
      } finally {
        setIsLoadingJobListings(false);
      }
    },
    [],
  );

  useEffect(() => {
    if (!reportData?.jobSearchProfile) {
      setJobListings(null);
      return;
    }

    if (checkJobListings === true) {
      submitJobListingsSearch(reportData.jobSearchProfile);
    }
  }, [checkJobListings, reportData?.jobSearchProfile, submitJobListingsSearch]);

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
                {isAtsEngineLoading && !atsEngineResult && !atsEngineError && (
                  <AtsEngineResultsSkeleton />
                )}
                {atsEngineResult && (
                  <AtsEngineResults result={atsEngineResult} />
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

                {isLoadingJobListings && (
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
                {jobListings?.jobListings &&
                  jobListings.jobListings.length > 0 && (
                    <RecommendedListings
                      jobListings={jobListings.jobListings}
                    />
                  )}
                {atsEngineError && (
                  <div className="w-full p-2">
                    <div className="rounded-md border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
                      {atsEngineError}
                    </div>
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
