"use client";

import React from "react";
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
        <div className="flex w-full justify-end text-muted-foreground  flex-row items-center gap-2">
          Search for Job Listings⁴
          <Checkbox
            checked={checkJobListings}
            onCheckedChange={(checked) => setCheckJobListings(checked === true)}
          />
        </div>
      </Field>

      <Button type="button" onClick={handleSubmit} disabled={isSubmitting}>
        {isSubmitting ? "Submitting..." : "Submit"}
      </Button>
    </div>
  );
}
