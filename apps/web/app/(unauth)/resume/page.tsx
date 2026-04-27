"use client";

import PdfViewer from "@/components/pdf-viewer/PdfViewer";
import Recommendations from "@/components/report-viewer/recommendations/Recommendations";
import Ats from "@/components/report-viewer/ats/ats";
import ModelGrid from "@/components/report-viewer/model/ModelGrid";
import Disclaimer from "@/components/resume/disclaimer";
import ResumeHeader from "@/components/resume/resume-header";
import React, { useEffect, useState } from "react";
import RecommendedListings from "@/components/report-viewer/recommendations/RecommendedListings";
import JobMatch from "@/components/report-viewer/job/job-match";
import {
  useAIModelStore,
  useCheckJobListings,
  useJobDescriptionStore,
  useResumeFileStore,
} from "@/stores/store";
import { submitResumeReview } from "@/service/resume-review.service";
import HomeHeader from "@/components/home/home-header";
import ResumeReviewForm from "@/components/home/resume-review-form";
import {
  JobListingsResponse,
  ResumeAnalysisResponse,
} from "@/types/payload/response-payload";
import AtsHeaderSkeleton from "@/components/skeletons/AtsHeaderSkeleton";
import AtsContentSkeleton from "@/components/skeletons/atsContentSkeleton";
import JobMatchSkeleton from "@/components/skeletons/JobMatchSkeleton";
import RecommendationsSkeleton from "@/components/skeletons/RecommendationsSkeleton";
import RecommendedListingsSkeleton from "@/components/skeletons/RecommendedListingsSkeleton";
import { Progress } from "@/components/ui/progress";
import ShimmerText from "@/components/ui/shimmer-text";
import { Typewriter } from "@/components/ui/typewriter";
import { TextShimmer } from "@/components/ui/text-shimmer";
import { jobListingWords, loadingWords } from "@/constants/constants";

export default function Page() {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoadingJobListings, setIsLoadingJobListings] = useState(false);
  const [fileUrl, setFileUrl] = useState<string | null>(null);
  const [reportData, setReportData] = useState<ResumeAnalysisResponse | null>(
    null,
  );
  const [jobListings, setJobListings] = useState<JobListingsResponse | null>(
    null,
  );

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
      setIsLoadingJobListings(false);

      const result = await submitResumeReview({
        aiModel,
        resumeFile,
        jobDescription,
      });

      setReportData(result);
    } catch (error) {
      console.error(error);
      alert(error instanceof Error ? error.message : "Something went wrong");
    } finally {
      setIsSubmitting(false);
    }
  };

  const submitJobListingsSearch = async (
    profile: ResumeAnalysisResponse["jobSearchProfile"],
  ) => {
    if (!profile || isLoadingJobListings) return;

    try {
      setIsLoadingJobListings(true);

      const response = await fetch("http://localhost:5053/api/job-listings", {
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
    } catch (error) {
      console.error(error);
      setJobListings(null);
    } finally {
      setIsLoadingJobListings(false);
    }
  };

  useEffect(() => {
    if (!reportData?.jobSearchProfile) {
      setJobListings(null);
      return;
    }

    if (checkJobListings === true) {
      submitJobListingsSearch(reportData.jobSearchProfile);
    }
  }, [reportData?.jobSearchProfile]);

  const [progress, setProgress] = useState(0);

  useEffect(() => {
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
  }, []);

  return (
    <div>
      <div className="sticky top-0 z-50 bg-background">
        <ResumeHeader />
      </div>

      {!reportData && !isSubmitting ? (
        <div className="flex flex-col gap-4 justify-center py-12 px-32">
          <HomeHeader />

          <ModelGrid />

          <div className="text-sm text-muted-foreground">
            Tip: For significantly better review quality, we recommend using
            Claude or GPT reasoning models.
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
          <Disclaimer />
        </div>
      ) : isSubmitting ? (
        <div className="flex flex-col gap-2">
          <div className="flex w-full divide-x items-start">
            <div className="w-1/2 p-2 sticky top-16">
              <div className="sticky top-16">
                <PdfViewer pdfUrl={`${fileUrl}`} />
              </div>
            </div>

            <div className="w-1/2 p-2 flex flex-col divide-y">
              <div className="w-full flex flex-col items-start justify-center px-12 py-2">
                <ShimmerText className="text-muted-foreground text-sm">
                  <Typewriter
                    words={loadingWords}
                    speed={40}
                    delayBetweenWords={2000}
                    cursor={false}
                  />
                </ShimmerText>

                <Progress value={progress} className="w-full" />
              </div>
              <AtsHeaderSkeleton />
              <AtsContentSkeleton />
              <JobMatchSkeleton />
              <RecommendationsSkeleton />
            </div>
          </div>
        </div>
      ) : (
        <div className="flex flex-col gap-2">
          <div className="flex w-full divide-x items-start">
            <div className="w-1/2 p-2 sticky top-16">
              <div className="sticky top-16">
                <PdfViewer pdfUrl={`${fileUrl}`} />
              </div>
            </div>

            <div className="w-1/2 p-2 flex flex-col divide-y">
              {reportData?.atsContent && (
                <Ats
                  atsReport={reportData.atsContent}
                  spellingAndGrammar={reportData.spellingAndGrammar}
                />
              )}

              {reportData?.jobMatch &&
                reportData.jobMatch.overallScore !== 0 && (
                  <JobMatch jobMatch={reportData.jobMatch} />
                )}

              {reportData?.jobRecommendation && (
                <Recommendations
                  jobRecommendation={reportData.jobRecommendation}
                />
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
                  <RecommendedListings jobListings={jobListings.jobListings} />
                )}

              <Disclaimer />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
