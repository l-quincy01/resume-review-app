"use client";
import React, { useState } from "react";
import Link from "next/link";
import { Menu, X } from "lucide-react";
import { Button } from "@/components/ui/button";

import { cn } from "@/lib/utils";
import { usePathname } from "next/navigation";

export const HeroHeader = () => {
  const [menuState, setMenuState] = useState(false);

  const location = usePathname();

  return (
    <header className="sticky top-0 z-50 bg-background/50 backdrop-blur  w-full  ">
      <nav
        data-state={menuState && "active"}
        className={cn(
          "group relative z-20 w-full  transition-colors duration-150 container mx-auto md:px-36 px-4 ",
        )}
      >
        <div className="mx-auto  transition-all duration-300">
          <div className="relative flex flex-wrap items-center justify-between gap-6 py-3 lg:gap-0 lg:py-4">
            <div className="flex w-full items-center justify-between gap-12 lg:w-auto">
              <button
                onClick={() => setMenuState(!menuState)}
                aria-label={menuState == true ? "Close Menu" : "Open Menu"}
                className="relative z-20 -m-2.5 -mr-4 block cursor-pointer p-2.5 lg:hidden"
              >
                <Menu className="group-data-[state=active]:rotate-180 group-data-[state=active]:scale-0 group-data-[state=active]:opacity-0 m-auto size-6 duration-200" />
                <X className="group-data-[state=active]:rotate-0 group-data-[state=active]:scale-100 group-data-[state=active]:opacity-100 absolute inset-0 m-auto size-6 -rotate-180 scale-0 opacity-0 duration-200" />
              </button>

              <div className="hidden lg:block">
                <ul className="flex gap-8 text-sm">
                  <li>
                    <Link
                      href={"/"}
                      className="text-primary text-lg  hover:text-accent-foreground block duration-150"
                    >
                      Resume Review
                      <span className="text-xs text-muted-foreground font-medium">
                        ᴮᴱᵀᴬ{" "}
                      </span>{" "}
                    </Link>
                  </li>
                </ul>
              </div>
            </div>

            <div className="bg-background group-data-[state=active]:block lg:group-data-[state=active]:flex mb-6 hidden w-full flex-wrap items-center justify-end space-y-8 rounded-3xl border p-6 shadow-2xl shadow-zinc-300/20 md:flex-nowrap lg:m-0 lg:flex lg:w-fit lg:gap-6 lg:space-y-0 lg:border-transparent lg:bg-transparent lg:p-0 lg:shadow-none dark:shadow-none dark:lg:bg-transparent">
              <div className="lg:hidden">
                <ul className="space-y-6 text-base">
                  <li>
                    <Link
                      href={"/"}
                      className="text-muted-foreground hover:text-accent-foreground block duration-150"
                    >
                      Resume Review<span className="text-xs">ᴮᴱᵀᴬ </span>{" "}
                    </Link>
                  </li>
                </ul>
              </div>
              <div className="flex w-full flex-col space-y-3 sm:flex-row sm:gap-3 sm:space-y-0 md:w-fit">
                {location !== "/resume" && (
                  <Button asChild size="sm">
                    <Link href="/resume">
                      <span>Scan your resume</span>
                    </Link>
                  </Button>
                )}
              </div>
            </div>
          </div>
        </div>
      </nav>
    </header>
  );
};
