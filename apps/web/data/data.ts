import { atsContent, spellingAndGrammar } from "@/types/atsReport.type";
import { jobListing } from "@/types/jobListing.type";
import { jobMatch } from "@/types/jobMatch.type";
import { JobRecommendation } from "@/types/jobRecommendations.type";

export const spellingAndGrammerData: spellingAndGrammar = {
  score: 92,
  grammarSuggestions: [
    {
      type: "Low",
      current:
        "Engineering lead for meta.com, a multi-year effort involving 40+ engineers and 20+ XFN teams",
      suggestedCorrection:
        "Engineering lead for Meta.com, a multi-year effort involving 40+ engineers and 20+ cross-functional (XFN) teams",
      explanation:
        "Capitalisation of 'Meta.com' should be consistent, and 'XFN' should be clarified on first use.",
    },
    {
      type: "Low",
      current: "Tech lead for the commerce org’s web infrastructure team.",
      suggestedCorrection:
        "Tech lead for the commerce organisation’s web infrastructure team.",
      explanation:
        "Use of 'org' is informal; 'organisation' is more appropriate for a formal CV.",
    },
    {
      type: "Low",
      current:
        "Created a server-driven UI framework to allow XML-based CMS contents to be used in React websites",
      suggestedCorrection:
        "Created a server-driven UI framework to allow XML-based CMS content to be used in React websites",
      explanation:
        "'Content' is uncountable in this context and should not be pluralised.",
    },
  ],
  spellingSuggestions: [
    {
      type: "Low",
      current: "Front End Engineer",
      suggestedCorrection: "Front-End Engineer",
      explanation:
        "Hyphenation improves consistency and readability in compound adjectives.",
    },
    {
      type: "Low",
      current: "Front end",
      suggestedCorrection: "Front-end",
      explanation:
        "Consistent hyphenation is recommended when used as an adjective.",
    },
    {
      type: "Low",
      current: "Back end",
      suggestedCorrection: "Back-end",
      explanation:
        "Should be hyphenated when used adjectivally for consistency.",
    },
  ],
  personalPronounCheck: [
    {
      type: "Low",
      current: "Entire resume",
      suggestedCorrection: "No changes required",
      explanation:
        "The resume appropriately avoids personal pronouns, maintaining a professional and concise tone.",
    },
  ],
  passiveVoiceCheck: [
    {
      type: "Low",
      current:
        "Used by 10+ teams – meta.com, Facebook help center, Meta Horizon website, etc.",
      suggestedCorrection:
        "Adopted by 10+ teams, including Meta.com, Facebook Help Center, and Meta Horizon website.",
      explanation:
        "The phrase is slightly passive; rewording improves clarity and impact.",
    },
    {
      type: "Low",
      current: "Made official by NUS in 2018",
      suggestedCorrection: "NUS officially adopted it in 2018",
      explanation:
        "Rewriting to active voice improves readability and ownership.",
    },
  ],
};

export const atsReportData: atsContent = {
  resumeName: "Yangshun Tay",

  heading: {
    section: "ATS Resume Review",
    score: 92,
  },

  content: [
    {
      section: "ATS Compaitablity",
      score: 94,
      summary:
        "The resume is highly ATS-friendly with clear sections, strong keyword usage, and structured bullet points. Minor improvements could increase parsing accuracy.",
      strengths: [
        "Clear section headings",
        "Consistent bullet formatting",
        "Strong keyword coverage (React, TypeScript, GraphQL, etc.)",
        "Readable role chronology",
        "Well structured experience section",
      ],
      weaknesses: [
        "Some bullet points are long and dense",
        "Projects section lacks consistent formatting",
        "Mixed punctuation and symbols",
      ],
      suggestions: [
        {
          type: "Low",
          content: "Standardise bullet punctuation across sections",
        },
        {
          type: "Medium",
          content: "Shorten multi-line bullet points for better parsing",
        },
        {
          type: "High",
          content: "Add explicit job titles to projects for keyword matching",
        },
      ],
    },

    {
      section: "Professional summary",
      score: 88,
      summary:
        "Resume lacks a dedicated professional summary. Adding one would improve recruiter scanning and ATS keyword density.",
      strengths: [
        "Strong experience speaks for itself",
        "Clear seniority level implied",
        "Technical depth evident",
      ],
      weaknesses: [
        "No summary section present",
        "Missing career positioning",
        "No key technologies highlighted at top",
      ],
      suggestions: [
        {
          type: "High",
          content:
            "Add a 3–4 line professional summary highlighting Staff-level frontend experience",
        },
        {
          type: "Medium",
          content:
            "Include React, TypeScript, and platform engineering keywords",
        },
      ],
      suggestedRewrites: [
        {
          current: "No professional summary",
          suggestion:
            "Staff Frontend Engineer with 8+ years of experience building scalable web platforms, design systems, and developer tooling. Led frontend architecture initiatives at Meta and built high-traffic products reaching millions of users. Specialises in React, TypeScript, performance optimisation, and UI infrastructure.",
          expplanation:
            "Adds ATS keywords, clarifies seniority, and improves recruiter readability.",
        },
      ],
    },

    {
      section: "Work experience",
      score: 95,
      summary:
        "Work experience is strong, impact-driven, and includes leadership, scale, and measurable outcomes. Very strong alignment for senior frontend roles.",
      strengths: [
        "Strong company names (Meta, Grab)",
        "Leadership responsibilities included",
        "Metrics included (MAU, adoption, teams)",
        "Architecture ownership described",
        "Clear progression in seniority",
      ],
      weaknesses: [
        "Some bullets are too technical for ATS ranking",
        "Limited business outcome metrics",
        "Few action verbs repeated",
      ],
      suggestions: [
        {
          type: "Low",
          content: "Vary action verbs to improve readability",
        },
        {
          type: "Medium",
          content: "Add business impact metrics where possible",
        },
        {
          type: "Medium",
          content: "Add 'Frontend architecture' keyword explicitly",
        },
      ],
    },

    {
      section: "Education",
      score: 93,
      summary:
        "Education section is strong with First Class Honours and multiple awards. Well structured and ATS-friendly.",
      strengths: [
        "Clear university and degree",
        "Strong GPA included",
        "Awards listed",
        "Scholarships mentioned",
      ],
      weaknesses: ["Could be shortened", "Awards may be overly detailed"],
      suggestions: [
        {
          type: "Low",
          content: "Condense awards into one line",
        },
        {
          type: "Low",
          content: "Move GPA to same line as degree",
        },
      ],
    },

    {
      section: "Projects",
      score: 90,
      summary:
        "Projects are highly impressive and demonstrate leadership in open source and developer tooling. Could benefit from consistent formatting.",
      strengths: [
        "Open source leadership",
        "High GitHub stars",
        "Real-world adoption",
        "Technical depth",
        "Strong credibility",
      ],
      weaknesses: [
        "Inconsistent formatting",
        "Missing technology stack labels",
        "Some descriptions too long",
      ],
      suggestions: [
        {
          type: "Medium",
          content: "Standardise project bullet formatting",
        },
        {
          type: "Medium",
          content: "Add tech stack per project",
        },
        {
          type: "Low",
          content: "Shorten descriptions",
        },
      ],
    },

    {
      section: "Skills",
      score: 91,
      summary:
        "Skills section is comprehensive and relevant to senior frontend roles. Minor grouping improvements would enhance ATS scoring.",
      strengths: [
        "Strong frontend stack coverage",
        "Includes tooling and backend",
        "Modern frameworks listed",
        "Accessibility experience included",
      ],
      weaknesses: [
        "Skills grouped inconsistently",
        "Too many items in single lines",
        "Some tools less relevant",
      ],
      suggestions: [
        {
          type: "Medium",
          content: "Group skills by category",
        },
        {
          type: "Low",
          content: "Remove less relevant tools",
        },
        {
          type: "Low",
          content: "Add 'Frontend Architecture' keyword",
        },
      ],
    },

    {
      section: "Cerificates",
      score: 75,
      summary:
        "No certifications listed. This is acceptable for senior engineers but could slightly improve ATS ranking.",
      strengths: [
        "Experience outweighs certifications",
        "Strong academic credentials",
      ],
      weaknesses: [
        "No certifications listed",
        "Missed ATS keyword opportunity",
      ],
      suggestions: [
        {
          type: "Low",
          content: "Add optional certifications if available",
        },
        {
          type: "Low",
          content: "Add open-source recognitions as alternative",
        },
      ],
    },
  ],
};

export const jobMatchData: jobMatch = {
  name: "Senior Frontend Engineer – React / Web Platform",

  overallScore: 94,

  jobDescription:
    "We are looking for a Senior Frontend Engineer to lead development of scalable React-based web platforms, build design systems, improve developer experience, and collaborate across cross-functional teams. The role requires strong experience in TypeScript, performance optimisation, accessibility, and large-scale frontend architecture.",

  targetJob: [
    {
      score: 98,
      type: "Skills Match",
      content:
        "Strong alignment with required frontend technologies including React, TypeScript, Next.js, design systems, accessibility, and large-scale UI architecture. Experience building internal frameworks, CMS-driven UI, and performance optimisations directly matches senior frontend platform expectations.",
    },
    {
      score: 95,
      type: "Keywords & ATS Optimization",
      content:
        "Resume contains high-value ATS keywords such as React, Next.js, TypeScript, GraphQL, design systems, accessibility, infrastructure, and scalable architecture. Could be improved by explicitly adding 'frontend architecture', 'performance optimisation', and 'web platform engineering'.",
    },
    {
      score: 96,
      type: "Education & Qualifications",
      content:
        "BSc in Computer Science from National University of Singapore with First Class Honours and multiple academic awards strongly supports senior engineering roles and demonstrates strong theoretical foundation.",
    },
    {
      score: 97,
      type: "Industry/Domain Relevance",
      content:
        "Experience at Meta, Grab, and startup environment building large-scale consumer web platforms, developer tooling, and design systems directly aligns with modern frontend platform engineering roles.",
    },
    {
      score: 94,
      type: "Job Title Alignment",
      content:
        "Previous roles including Staff Frontend Engineer, Engineering Lead, and Co-founder building frontend infrastructure closely match Senior Frontend Engineer and Frontend Platform Engineer titles.",
    },
    {
      score: 99,
      type: "Seniority/Experience Level",
      content:
        "Over 8 years of frontend engineering experience including Staff Engineer level leadership, cross-team architecture ownership, and large-scale system design strongly matches senior and staff-level expectations.",
    },
    {
      score: 93,
      type: "Accomplishments & Metrics",
      content:
        "Resume includes strong measurable impact such as 1.5M monthly pageviews, 60k MAU, 40+ engineers collaboration, 10+ teams adoption, and open-source projects with 50k+ stars. Additional business metrics would further strengthen impact.",
    },
    {
      score: 90,
      type: "Cultural & Values Fit Signals",
      content:
        "Evidence of open source leadership, mentorship, cross-functional collaboration, and developer experience improvements aligns well with engineering culture focused on ownership and impact.",
    },
  ],
};

export const jobRecommendationData: JobRecommendation = {
  yearsExperience:
    "8+ years frontend engineering experience with Staff-level leadership",

  jobTitles: [
    "Senior Frontend Engineer",
    "Staff Frontend Engineer",
    "Frontend Platform Engineer",
    "UI Infrastructure Engineer",
    "Web Platform Engineer",
    "Principal Frontend Engineer",
    "Design Systems Engineer",
    "Frontend Architect",
    "Developer Experience Engineer",
    "Full Stack Engineer (Frontend-focused)",
  ],

  roles: [
    "Frontend platform architecture",
    "Design systems ownership",
    "Developer experience engineering",
    "Large-scale React application development",
    "Web performance optimisation",
    "UI infrastructure development",
    "Server-driven UI systems",
    "CMS-driven frontend architecture",
    "Cross-team frontend standards",
    "Technical leadership for frontend teams",
  ],

  responsibilities: [
    "Design scalable frontend architecture for large web platforms",
    "Build and maintain reusable component libraries and design systems",
    "Lead frontend technical direction across multiple teams",
    "Improve performance, accessibility, and maintainability of UI applications",
    "Develop developer tooling and frontend infrastructure",
    "Collaborate with product, design, and backend engineering teams",
    "Mentor engineers and drive frontend best practices",
    "Implement server-driven UI and CMS integrations",
    "Establish frontend coding standards and architecture guidelines",
    "Drive adoption of shared UI frameworks across teams",
  ],

  seniority: [
    "Senior Engineer",
    "Staff Engineer",
    "Senior Staff Engineer",
    "Principal Engineer",
    "Frontend Architect",
    "Technical Lead",
  ],

  industry: [
    "Big Tech",
    "Developer Tooling",
    "SaaS Platforms",
    "Frontend Infrastructure",
    "E-commerce Platforms",
    "Cloud Platforms",
    "Design Tools",
    "Productivity Software",
    "Open Source Platforms",
    "Developer Platforms",
  ],

  hardSkills: [
    "React",
    "TypeScript",
    "JavaScript",
    "Next.js",
    "GraphQL",
    "Node.js",
    "Design Systems",
    "Frontend Architecture",
    "Web Performance Optimisation",
    "Accessibility (A11y)",
    "State Management",
    "Server-driven UI",
    "Tailwind CSS",
    "CSS Architecture",
    "Monorepo Tooling",
    "Webpack",
    "Babel",
    "Testing (Jest)",
    "UI Component Libraries",
    "CMS Integrations",
  ],

  softSkills: [
    "Technical leadership",
    "Cross-functional collaboration",
    "Mentoring engineers",
    "Architecture decision making",
    "Communication across teams",
    "Ownership mindset",
    "Problem solving",
    "Product thinking",
    "Developer experience focus",
    "Open source collaboration",
  ],

  workAndTeamEnvironment: [
    "Frontend platform teams",
    "Design systems teams",
    "Developer experience teams",
    "Cross-functional product teams",
    "Infrastructure-focused engineering teams",
    "High-scale web platform teams",
    "Engineering-led organisations",
    "Collaborative product engineering teams",
    "Remote-first engineering teams",
    "Open-source friendly environments",
  ],

  companySizeFit: [
    "Large tech companies (Meta, Google, etc.)",
    "Mid-size SaaS companies",
    "Developer tooling startups",
    "High-growth startups",
    "Enterprise platform companies",
    "Product-led tech companies",
    "Open source focused companies",
  ],

  careerTrack: [
    "Senior Frontend Engineer → Staff Frontend Engineer",
    "Staff Engineer → Principal Engineer",
    "Frontend Architect",
    "Engineering Lead (Frontend Platform)",
    "Developer Experience Lead",
    "UI Infrastructure Lead",
    "Technical Director (Frontend)",
    "Distinguished Engineer (Frontend)",
    "Engineering Manager (Frontend Platform optional)",
    "Startup CTO (Frontend-heavy product)",
  ],
};

export const jobListingsData: jobListing[] = [
  {
    listingTitle: "Senior Frontend Engineer (React / Web Platform)",
    companyName: "Meta",
    locationName: "Singapore / Remote",
    whyItsGreat:
      "Matches Yangshun's Staff-level frontend architecture experience, design systems work, and large-scale platform ownership.",
    listingLink: "https://www.linkedin.com/jobs/",
    searchQuery:
      'site:linkedin.com/jobs "Senior Frontend Engineer" React Singapore Meta',
  },
  {
    listingTitle: "Staff Frontend Engineer – Design Systems",
    companyName: "Airbnb",
    locationName: "Remote / US",
    whyItsGreat:
      "Strong alignment with experience building scalable component libraries and design systems using React and TypeScript.",
    listingLink: "https://www.linkedin.com/jobs/",
    searchQuery:
      'site:linkedin.com/jobs "Staff Frontend Engineer" "design system" React Airbnb',
  },
  {
    listingTitle: "Frontend Platform Engineer",
    companyName: "Stripe",
    locationName: "Remote",
    whyItsGreat:
      "Platform engineering, developer experience, and UI infrastructure responsibilities closely match Yangshun's background.",
    listingLink: "https://www.indeed.com/",
    searchQuery:
      'site:indeed.com "Frontend Platform Engineer" React TypeScript Stripe',
  },
  {
    listingTitle: "Senior Software Engineer – Frontend",
    companyName: "Google",
    locationName: "Singapore",
    whyItsGreat:
      "Large-scale frontend architecture, performance optimisation, and cross-team collaboration align with Meta experience.",
    listingLink: "https://careers.google.com/",
    searchQuery:
      'site:careers.google.com "Senior Software Engineer" frontend React Singapore',
  },
  {
    listingTitle: "Staff UI Engineer",
    companyName: "Vercel",
    locationName: "Remote",
    whyItsGreat:
      "Next.js ecosystem, design systems, and frontend infrastructure experience make this a strong match.",
    listingLink: "https://vercel.com/careers",
    searchQuery: 'site:vercel.com "Staff Frontend Engineer" React Next.js',
  },
  {
    listingTitle: "Senior Frontend Engineer – Developer Experience",
    companyName: "Figma",
    locationName: "Remote",
    whyItsGreat:
      "Strong overlap with tooling, plugins, and frontend developer productivity improvements.",
    listingLink: "https://www.figma.com/careers/",
    searchQuery:
      'site:figma.com "Senior Frontend Engineer" React developer experience',
  },
  {
    listingTitle: "Principal Frontend Engineer",
    companyName: "Shopify",
    locationName: "Remote",
    whyItsGreat:
      "Leadership in frontend architecture and platform engineering fits staff/principal level background.",
    listingLink: "https://www.shopify.com/careers",
    searchQuery:
      'site:shopify.com "Principal Frontend Engineer" React platform',
  },
  {
    listingTitle: "Senior React Engineer",
    companyName: "Atlassian",
    locationName: "Remote / APAC",
    whyItsGreat:
      "Component systems, accessibility, and large-scale UI architecture strongly align.",
    listingLink: "https://www.atlassian.com/company/careers",
    searchQuery:
      'site:atlassian.com "Senior Frontend Engineer" React TypeScript',
  },
];
