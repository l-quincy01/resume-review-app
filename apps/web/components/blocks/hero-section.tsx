"use client";

import React from "react";
import type { Variants } from "framer-motion";
import Image from "next/image";
import { Button } from "@/components/ui/button";
import { AnimatedGroup } from "@/components/ui/animated-group";
import {
  ArrowRight,
  CheckCircle2,
  FileText,
  ScanSearch,
  Target,
  Briefcase,
  PenTool,
} from "lucide-react";
import Link from "next/link";
// import HeroCustomers from "./hero/hero-customers";

const transitionVariants: { item: Variants } = {
  item: {
    hidden: {
      opacity: 0,
      filter: "blur(12px)",
      y: 16,
    },
    visible: {
      opacity: 1,
      filter: "blur(0px)",
      y: 0,
      transition: {
        type: "spring",
        bounce: 0.25,
        duration: 1.1,
      },
    },
  },
};

const animatedVariants: { container: Variants; item: Variants } = {
  container: {
    visible: {
      transition: {
        staggerChildren: 0.06,
        delayChildren: 0.15,
      },
    },
  },
  ...transitionVariants,
};

const atsTips = [
  {
    title: "Use standard section headings",
    description:
      'Stick to headings like "Work Experience", "Education", "Projects", and "Skills" so ATS software can identify your content correctly.',
  },
  {
    title: "Match important keywords naturally",
    description:
      "Reflect key requirements from the job description in your experience bullets, but only where they genuinely apply to your background.",
  },
  {
    title: "Keep the layout simple",
    description:
      "A clean single-column resume is easier for parsers to read. Tables, graphics, icons, and sidebars often cause issues.",
  },
  {
    title: "Use evidence-backed bullet points",
    description:
      "Strong resumes connect skills to actions and outcomes. Context and measurable results make your experience more convincing.",
  },
];

const reportFeatures = [
  {
    title: "ATS compatibility review",
    description:
      "See whether your formatting, structure, and wording are easy for applicant tracking systems to parse.",
    icon: ScanSearch,
  },
  {
    title: "Resume strength scoring",
    description:
      "Get a structured assessment of clarity, impact, relevance, and overall competitiveness.",
    icon: CheckCircle2,
  },
  {
    title: "Section-by-section feedback",
    description:
      "Understand what is working and what needs improvement across your summary, experience, projects, skills, and education.",
    icon: FileText,
  },
  {
    title: "Job match insights",
    description:
      "Compare your resume against a target role and spot the biggest gaps in alignment.",
    icon: Target,
  },
  {
    title: "Role recommendations",
    description:
      "Discover the kinds of roles your resume currently positions you for based on your experience and skills.",
    icon: Briefcase,
  },
  {
    title: "Actionable rewrite suggestions",
    description:
      "Get practical recommendations to strengthen weak bullets, improve phrasing, and present your experience more clearly.",
    icon: PenTool,
  },
];

const steps = [
  {
    number: "1",
    title: "Upload your resume",
    description:
      "Submit your resume in seconds and let the platform analyse your content and structure.",
  },
  {
    number: "2",
    title: "Review your report",
    description:
      "Get ATS checks, writing feedback, role-fit analysis, and practical improvement suggestions.",
  },
  {
    number: "3",
    title: "Improve before you apply",
    description:
      "Use targeted recommendations to refine your resume and apply with more confidence.",
  },
];

export function HeroSection() {
  return (
    <>
      <main className="overflow-hidden bg-background">
        <section className="relative">
          <div className="absolute inset-x-0 top-0 -z-10 h-128 bg-linear-to-b from-muted/40 via-background to-background" />

          <div className="space-y-12 sm:space-y-24 md:space-y-36  pt-12 md:pt-24 pb-0 flex flex-col items-center ">
            <div className="max-w-7xl grid items-center gap-14 pt-12 lg:grid-cols-2 lg:pt-20 px-12 xl:px-0 ">
              <AnimatedGroup variants={animatedVariants}>
                <div className="max-w-2xl ">
                  <div className=" text-center text-primary font-semibold inline-flex items-center rounded-full    text-md  ">
                    ATS checks, recruiter-style feedback, and practical resume
                    guidance
                  </div>

                  <h1 className="mt-6 text-balance text-4xl font-semibold tracking-tight sm:text-5xl lg:text-6xl">
                    Improve your resume with smarter feedback before you apply
                  </h1>

                  <p className="mt-6 max-w-xl text-lg leading-8 text-muted-foreground">
                    Understand how your resume reads to both applicant tracking
                    systems and recruiters. Get structured feedback on
                    formatting, clarity, impact, keyword alignment, and role
                    fit.
                  </p>

                  <div className="mt-8 flex flex-col gap-3 sm:flex-row">
                    <Link href="/resume">
                      <Button className="h-12 rounded-xl px-6 text-base shadow-sm">
                        Scan your resume
                        <ArrowRight className="ml-2 size-4" />
                      </Button>
                    </Link>

                    <Button
                      variant="outline"
                      className="h-12 rounded-xl px-6 text-base"
                    >
                      View sample report
                    </Button>
                  </div>

                  <div className="mt-6 flex flex-wrap items-center gap-x-6 gap-y-2 text-sm text-muted-foreground">
                    <span>Clear feedback</span>
                    <span>ATS-aware analysis</span>
                    <span>Practical recommendations</span>
                  </div>
                </div>
              </AnimatedGroup>

              <AnimatedGroup variants={animatedVariants}>
                <div className="relative">
                  <div className="absolute -inset-4 -z-10 rounded-[2rem] bg-muted/40 blur-2xl" />
                  <div className="relative overflow-hidden ">
                    <Image
                      className="hidden aspect-15/8 rounded-[1.25rem] object-cover dark:block"
                      src="/hero-banner-dark.png"
                      alt="Resume review dashboard preview"
                      width={2700}
                      height={1440}
                    />
                    <Image
                      className="aspect-15/8 rounded-[1.25rem]  object-cover dark:hidden"
                      src="/hero-banner.png"
                      alt="Resume review dashboard preview"
                      width={2700}
                      height={1440}
                    />
                  </div>
                </div>
              </AnimatedGroup>
            </div>

            <AnimatedGroup
              className="relative overflow-hidden   py-16  bg-accent w-full flex flex-col items-center "
              variants={animatedVariants}
            >
              <div className="max-w-7xl  grid gap-10 lg:grid-cols-[0.9fr_1.1fr] px-12 xl:px-0">
                <div>
                  <h2 className="max-w-xl text-balance text-3xl font-semibold md:text-4xl">
                    How do you make sure your resume is ATS-friendly?
                  </h2>
                  <p className="mt-4 max-w-md text-lg leading-8 text-muted-foreground">
                    A strong resume needs to do two things well: read clearly to
                    a recruiter and parse cleanly through ATS software.
                  </p>
                </div>

                <ul className="grid gap-4">
                  {atsTips.map((tip) => (
                    <li key={tip.title} className=" ">
                      <h3 className="text-lg font-semibold">{tip.title}</h3>
                      <p className="mt-2 text-base leading-7 text-muted-foreground">
                        {tip.description}
                      </p>
                    </li>
                  ))}
                </ul>
              </div>
            </AnimatedGroup>

            <AnimatedGroup
              className=" max-w-7xl flex flex-col gap-4 text-center px-12 xl:px-0"
              variants={animatedVariants}
            >
              <div className="mx-auto max-w-3xl">
                <h2 className="text-balance text-3xl font-semibold md:text-4xl">
                  What you get in the report
                </h2>
                <p className="mt-4 text-lg leading-8 text-muted-foreground">
                  A useful report should do more than give you a score. It
                  should show you where your resume is weak, why it matters, and
                  how to improve it.
                </p>
              </div>

              <div className="mt-6 grid gap-5 md:grid-cols-2 xl:grid-cols-3">
                {reportFeatures.map((feature) => {
                  const Icon = feature.icon;

                  return (
                    <div
                      key={feature.title}
                      className="group rounded-3xl border bg-background p-6 text-left shadow-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-lg"
                    >
                      <div className="flex h-12 w-12 items-center justify-center rounded-2xl border bg-muted/40">
                        <Icon className="size-5" />
                      </div>

                      <h3 className="mt-5 text-xl font-semibold tracking-tight">
                        {feature.title}
                      </h3>

                      <p className="mt-3 text-base leading-7 text-muted-foreground">
                        {feature.description}
                      </p>
                    </div>
                  );
                })}
              </div>
            </AnimatedGroup>

            <AnimatedGroup
              className="bg-accent w-full  flex flex-col items-center py-16  "
              variants={animatedVariants}
            >
              <div className="max-w-3xl px-12 xl:px-0">
                <div className=" w-full flex flex-col items-center ">
                  <h2 className="text-balance text-3xl font-semibold md:text-4xl text-center ">
                    See whether your resume is ready for real applications
                  </h2>

                  <p className="mt-4 text-lg leading-8 text-muted-foreground text-center ">
                    Review formatting, keyword alignment, writing quality, and
                    resume impact in one place. The aim is not just to “pass
                    ATS”, but to present your experience in a way that is easier
                    to read and stronger to evaluate.
                  </p>

                  <Button className="mt-8 h-12 rounded-xl px-6 text-base shadow-sm">
                    Get your free resume scan
                  </Button>
                </div>
              </div>
            </AnimatedGroup>

            <AnimatedGroup
              className="max-w-7xl grid items-center gap-10 lg:grid-cols-2 px-12 xl:px-0"
              variants={animatedVariants}
            >
              <div className="order-2 flex flex-col gap-5 lg:order-1">
                <h2 className="text-balance text-3xl font-semibold md:text-4xl">
                  What an ATS-friendly resume looks like
                </h2>

                <p className="text-lg leading-8 text-muted-foreground">
                  Strong resumes are easy to scan, easy to understand, and easy
                  to parse. They avoid unnecessary formatting tricks and keep
                  the focus on relevant experience, skills, and measurable
                  results.
                </p>

                <p className="text-lg leading-8 text-muted-foreground">
                  Clean typography, standard headings, a single-column layout,
                  and clear bullet points usually outperform visually complex
                  templates when software and recruiters review them.
                </p>
              </div>

              <div className="order-1 lg:order-2">
                <div className="overflow-hidden rounded-[1.75rem] border bg-background p-3 shadow-lg shadow-zinc-950/5">
                  <Image
                    src="https://resumeworded.com/assets/images/experienced-hire-v6.png"
                    alt="Example of an ATS-friendly resume layout"
                    className="mx-auto w-full rounded-[1.25rem] border"
                    width={1320}
                    height={1708}
                  />
                </div>
              </div>
            </AnimatedGroup>

            <AnimatedGroup
              className="w-full bg-accent py-16"
              variants={animatedVariants}
            >
              <div className="w-full flex flex-col items-center px-12 xl:px-0 ">
                <div className="max-w-7xl  w-full flex flex-col items-center justify-center gap-4 text-center">
                  <div className="max-w-2xl">
                    <h2 className="text-balance text-3xl font-semibold md:text-4xl">
                      Three steps to a better resume
                    </h2>

                    <p className="mt-4 text-lg leading-8 text-muted-foreground">
                      No complicated setup. Upload your resume, review the
                      report, and improve the parts that matter most.
                    </p>
                  </div>

                  <div className="mt-6 grid w-full gap-6 md:grid-cols-3">
                    {steps.map((step) => (
                      <div
                        key={step.number}
                        className="rounded-3xl border bg-background p-6 text-center shadow-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-md"
                      >
                        <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full border-2 border-primary text-lg font-semibold">
                          {step.number}
                        </div>

                        <h3 className="mt-4 text-xl font-semibold">
                          {step.title}
                        </h3>
                        <p className="mt-3 text-base leading-7 text-muted-foreground">
                          {step.description}
                        </p>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </AnimatedGroup>
          </div>
        </section>

        {/* <HeroCustomers /> */}
      </main>
    </>
  );
}
