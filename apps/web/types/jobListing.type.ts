export interface jobListing {
  listingTitle: string;
  companyName: string;
  locationName: string;
  whyItsGreat: string;
  listingLink: string;
  searchQuery: string;
}
export interface jobListings {
  jobListings: jobListing[];
}


export interface jobSearchProfile{

  titles:string[];
  keywords: string[];
  seniority: string;
  locations: string[];
  exclude: string[];

}
