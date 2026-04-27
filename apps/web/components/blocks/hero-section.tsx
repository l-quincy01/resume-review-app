"use client";

import React from "react";
import Link from "next/link";
import { ChevronRight, Menu, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { AnimatedGroup } from "@/components/ui/animated-group";
import { cn } from "@/lib/utils";
import { useScroll } from "motion/react";
import { HeroHeader } from "./hero/hero-header";
import HeroCustomers from "./hero/hero-customers";

const transitionVariants = {
  item: {
    hidden: {
      opacity: 0,
      filter: "blur(12px)",
      y: 12,
    },
    visible: {
      opacity: 1,
      filter: "blur(0px)",
      y: 0,
      transition: {
        type: "spring",
        bounce: 0.3,
        duration: 1.5,
      },
    },
  },
};

export function HeroSection() {
  return (
    <>
      <HeroHeader />
      <main className="overflow-hidden">
        <section className="flex flex-col items-center gap-2">
          <div className="relative pt-24 flex flex-row gap-2">
            <div className="absolute inset-0 -z-10 size-full [background:radial-gradient(125%_125%_at_50%_100%,transparent_0%,var(--background)_75%)]"></div>
            <div className="mx-auto max-w-5xl px-6">
              <div className="sm:mx-auto lg:mr-auto">
                <AnimatedGroup
                  variants={{
                    container: {
                      visible: {
                        transition: {
                          staggerChildren: 0.05,
                          delayChildren: 0.75,
                        },
                      },
                    },
                    ...transitionVariants,
                  }}
                >
                  <h1 className="mt-8 max-w-2xl text-balance text-2xl font-medium md:text-4xl lg:mt-16">
                    Get resume help from ex-FAANG software engineering hiring
                    managers
                  </h1>
                  <p className="mt-8 max-w-2xl text-pretty text-md text-muted-foreground">
                    We have screened thousands of applications, interviewed
                    hundreds of candidates and seen the best and worst of them —
                    now our advice is available to you. Regardless of your
                    experience level, find resume and career help tailored for
                    you.
                  </p>
                </AnimatedGroup>
              </div>
            </div>
            <AnimatedGroup
              variants={{
                container: {
                  visible: {
                    transition: {
                      staggerChildren: 0.05,
                      delayChildren: 0.75,
                    },
                  },
                },
                ...transitionVariants,
              }}
            >
              <div className="relative -mr-56 mt-8 overflow-hidden px-2 sm:mr-0 sm:mt-12 md:mt-20">
                <div
                  aria-hidden
                  className="bg-gradient-to-b to-background absolute inset-0 z-10 from-transparent from-35%"
                />
                <div className="inset-shadow-2xs ring-background dark:inset-shadow-white/20 bg-background relative mx-auto max-w-5xl overflow-hidden rounded-2xl border p-4 shadow-lg shadow-zinc-950/15 ring-1">
                  <img
                    className="bg-background aspect-15/8 relative hidden rounded-2xl dark:block"
                    src="https://tailark.com/_next/image?url=%2Fmail2.png&w=3840&q=75"
                    alt="app screen"
                    width="2700"
                    height="1440"
                  />
                  <img
                    className="z-2 border-border/25 aspect-15/8 relative rounded-2xl border dark:hidden"
                    src="https://tailark.com/_next/image?url=%2Fmail2-light.png&w=3840&q=75"
                    alt="app screen"
                    width="2700"
                    height="1440"
                  />
                </div>
              </div>
            </AnimatedGroup>
          </div>

          <div className="mt-12 flex items-center gap-2">
            <Link href="/resume" className="bg-primary p-2 rounded-2xl">
              <span className="text-nowrap">Try it out</span>
            </Link>
          </div>
        </section>

        <HeroCustomers />
      </main>
    </>
  );
}
