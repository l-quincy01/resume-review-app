import { jobListing } from "@/types/jobListing.type";
import { ArrowUpRight } from "lucide-react";
import React from "react";

interface props {
  jobListings: jobListing[];
}

export default function RecommendedListings({ jobListings }: props) {
  return (
    <div className="flex flex-col gap-4 p-4">
      <div className="flex items-center justify-between">
        <span className="text-lg font-semibold">Suggested Listings⁴</span>
      </div>

      {jobListings.map((listing, index) => (
        <div
          key={index}
          className="flex flex-col gap-2 items-start  text-accent-foreground  bg-accent/60 rounded-xl p-2"
        >
          <span className=" font-semibold text-md">{listing.listingTitle}</span>

          <div className="flex flex-col  text-xs">
            <span>Comapny: {listing.companyName}</span>
            <span>Location: {listing.locationName}</span>
          </div>

          <div>
            <span className="font-semibold">Why it&apos;s great</span>
            <div className="text-xs">{listing.whyItsGreat}</div>
          </div>

          <div className="flex flex-row items-center gap-1">
            <a
              href={`${listing.listingLink}`}
              target="_blank"
              rel="noopener noreferrer"
              className=" cursor-pointer hover:bg-accent-foreground/5 py-2 px-1 rounded-2xl underline decoration-dotted underline-offset-2 flex flex-row items-center gap-1 text-xs"
            >
              View Listing Here <ArrowUpRight size={14} />
            </a>
            <div className="text-[0.65rem]">|</div>

            <a
              href={`https://www.google.com/search?q=${encodeURIComponent(`${listing.searchQuery}`)}`}
              target="_blank"
              rel="noopener noreferrer"
              className=" cursor-pointer hover:bg-accent-foreground/5 py-2 px-1 rounded-2xl underline decoration-dotted underline-offset-2 flex flex-row items-center gap-1 text-xs"
            >
              {listing.searchQuery}

              <ArrowUpRight size={14} />
            </a>
          </div>
        </div>
      ))}
    </div>
  );
}
