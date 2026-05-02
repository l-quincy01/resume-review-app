"use client";

import React from "react";
import Link from "next/link";
import { Field, FieldLabel } from "@/components/ui/field";
import { Textarea } from "@/components/ui/textarea";
import { FileUpload } from "@/components/ui/file-upload";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";

interface ResumeReviewFormProps {
  jobDescription: string;
  setJobDescription: (value: string) => void;
  checkJobListings: boolean;
  setCheckJobListings: (value: boolean) => void;
  consentToAiProcessing: boolean;
  setConsentToAiProcessing: (value: boolean) => void;
  setResumeFile: (file: File | null) => void;
  resumeFile: File | null;
  handleSubmit: () => void;
  isSubmitting?: boolean;
}

export default function ResumeReviewForm({
  jobDescription,
  setJobDescription,
  checkJobListings,
  setCheckJobListings,
  consentToAiProcessing,
  setConsentToAiProcessing,
  setResumeFile,
  resumeFile,
  handleSubmit,
  isSubmitting = false,
}: ResumeReviewFormProps) {
  return (
    <div className="flex flex-col gap-2 items-end">
      <Field className="flex flex-col lg:grid  lg:grid-cols-2 gap-6">
        <div className="flex flex-col gap-2 items-start">
          <FieldLabel
            htmlFor="job-description"
            className="text-muted-foreground"
          >
            Paste A Job Description For Your Desired Job
          </FieldLabel>

          <Textarea
            id="job-description"
            placeholder="Enter your target job description."
            className="w-full h-[260px] scrollbar-hide rounded-md"
            value={jobDescription}
            onChange={(e) => setJobDescription(e.target.value)}
          />
        </div>

        <div className="flex flex-col gap-2">
          <FieldLabel htmlFor="resume-upload">Upload CV</FieldLabel>

          <FileUpload onChange={setResumeFile} />

          {resumeFile && (
            <div className="text-xs text-muted-foreground">
              Selected file: {resumeFile.name}
            </div>
          )}
        </div>
        <div></div>
        <div className="flex w-full justify-end text-muted-foreground flex-row items-center gap-2">
          Search for Job Listings⁴
          <Checkbox
            checked={checkJobListings}
            onCheckedChange={(checked) => setCheckJobListings(checked === true)}
          />
        </div>
      </Field>

      <label className="flex w-full items-start gap-3 text-sm text-muted-foreground">
        <Checkbox
          checked={consentToAiProcessing}
          onCheckedChange={(checked) =>
            setConsentToAiProcessing(checked === true)
          }
          aria-label="Consent to AI resume processing"
        />
        <span>
          I agree that my resume and job description will be sent to the AI
          provider for analysis. Resumes are not stored by this app. Read the{" "}
          <Link href="/privacy" className="underline underline-offset-4">
            Privacy Policy
          </Link>{" "}
          and{" "}
          <Link href="/terms" className="underline underline-offset-4">
            Terms
          </Link>
          .
        </span>
      </label>

      <Button
        type="button"
        onClick={handleSubmit}
        disabled={isSubmitting || !consentToAiProcessing}
      >
        {isSubmitting ? "Submitting..." : "Submit"}
      </Button>
    </div>
  );
}

/*



*/
