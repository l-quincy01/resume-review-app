![Banner](./frontend/screenshots/v0.14/0.png?raw=true)

# Resume Reviewer

![Frontend](https://img.shields.io/badge/frontend-Next.js%2016-black)
![Backend](https://img.shields.io/badge/backend-ASP.NET%20Core%209-512BD4)
![AI](https://img.shields.io/badge/AI-OpenAI%20Responses%20API-412991)
![Tests](https://img.shields.io/badge/tests-xUnit%20%7C%20Playwright-2EAD33)
![Deployment](https://img.shields.io/badge/deployment-Azure%20Container%20Apps-0078D4)

## Overview

Resume Reviewer is a full-stack AI resume analysis and ATS simulation platform. A user uploads a PDF resume, optionally adds a job description, selects an OpenAI model, and supplies their own OpenAI API key.

The application combines:

- Progressively streamed AI resume feedback
- Deterministic ATS keyword and header scoring
- Spelling and grammar analysis
- Job-role recommendations
- GPT-5 web search for matching South African job listings

The application is stateless. It does not provide user accounts, save resumes, or persist OpenAI API keys.

## Key Features

- PDF resume upload and in-browser PDF.js preview
- Optional job-description comparison
- Server-Sent Events for progressive review results
- Structured AI output for reliable frontend rendering
- Deterministic keyword scoring with tier weights and contextual evidence
- Standard resume-header validation
- Keyword-stuffing protection for uncontextualised keyword mentions
- Dedicated job-search profile generation
- Current job discovery through GPT-5 with web search
- User-supplied OpenAI API keys held in browser memory only
- API versioning, health checks, rate limiting, and request-size limits
- Separate production containers for the web and API services
- Automated Azure Container Apps deployment through GitHub Actions

# User Interface
### Onboarding Screens

| Landing | Upload form |
|--------|--------|
| <img src="./screenshots/0.png" width="85%" /> | <img src="./screenshots/0b.png" width="85%" /> |



| Review Feedback | Review Feedback |
|--------|--------|
| <img src="./screenshots/1.png" width="85%" /> | <img src="./screenshots/2.png" width="85%" /> |

| Keyword Score | Keyword Score|
|--------|--------|
| <img src="./screenshots/3.png" width="85%" /> | <img src="./screenshots/4b.png" width="85%" /> |

| Job Recommendation | Job Recommendation  |
|--------|--------|
| <img src="./screenshots/5.png" width="85%" /> | <img src="./screenshots/2b.png" width="85%" /> |

## Architecture

```mermaid
flowchart LR
    USER[User]
    WEB[Next.js Web App]
    API[ASP.NET Core API]
    REVIEW[Streamed Resume Review]
    ATS[ATS Pipeline]
    JOBS[Job Search Service]
    OPENAI[OpenAI Responses API]

    USER -->|PDF, optional job description, API key| WEB
    WEB -->|Multipart request + X-OpenAI-Api-Key| API
    API --> REVIEW
    API --> ATS
    REVIEW --> OPENAI
    ATS --> OPENAI
    REVIEW -->|SSE sections + job-search profile| WEB
    WEB -->|Profile + X-OpenAI-Api-Key| API
    API --> JOBS
    JOBS -->|GPT-5 + web search| OPENAI
    JOBS -->|Structured listings| WEB
```

The system has two independently deployable applications:

- `apps/web`: Next.js frontend on port `3000`
- `apps/api`: ASP.NET Core API on port `5053`

There is no database in the current architecture. Resume files, API keys, job descriptions, and analysis results are processed transiently.

## End-to-End Review Flow

1. The user enters an OpenAI API key, selects a model, and uploads a PDF resume.
2. The frontend validates the required input and displays the PDF locally.
3. The streamed resume-review request and ATS pipeline start independently.
4. The backend validates request sizes, rate limits, CORS, and the user API key.
5. Resume-review sections are generated as structured OpenAI responses.
6. Completed sections are returned to the browser through Server-Sent Events.
7. The ATS pipeline extracts job-description keywords, analyses resume context, and calculates deterministic scores.
8. If job search is enabled, the streamed candidate profile is sent to the dedicated job-listings endpoint.
9. GPT-5 searches the web for current matching South African jobs.
10. The frontend renders feedback, ATS results, recommendations, and job listings.

### Resume Review Sequence

```mermaid
sequenceDiagram
    actor User
    participant FE as Next.js Frontend
    participant API as ASP.NET Core API
    participant OpenAI as OpenAI Responses API

    User->>FE: Enter API key and upload resume PDF
    FE->>API: POST /api/v1/resume-review/stream
    API->>API: Validate request and API key
    API->>OpenAI: Upload resume temporarily

    par Resume content analysis
        API->>OpenAI: Request ATS-oriented feedback
    and Grammar analysis
        API->>OpenAI: Request spelling and grammar feedback
    and Career analysis
        API->>OpenAI: Request recommendations and search profile
    end

    OpenAI-->>API: Structured section results
    API-->>FE: SSE section_completed events
    FE-->>User: Render each completed section
    API->>OpenAI: Delete temporary resume file
    API-->>FE: SSE review_completed event
```



## ATS Scoring Pipeline

```mermaid
flowchart LR
    PDF[Resume PDF] --> TEXT[PDF Text Extraction]
    JD[Job Description] --> KEYWORDS[Keyword Extraction]
    KEYWORDS --> ENRICH[Filter, Classify, and Tier]
    TEXT --> CONTEXT[Contextual Keyword Analysis]
    ENRICH --> CONTEXT
    CONTEXT --> SCORE[Deterministic Keyword Scoring]
    TEXT --> HEADERS[Header Validation]
    SCORE --> FINAL[Final ATS Assessment]
    HEADERS --> FINAL
```

AI assists with keyword extraction and contextual evidence classification. The numerical score is calculated by application code.

Keywords are grouped into Tiers 0-3. Higher-priority and repeated job-description keywords receive greater weight. Each keyword is scored using:

- Tier baseline points
- Achievement context
- Metrics
- Action verbs
- Work-experience, project, and summary placement
- Must-have or nice-to-have multipliers

Missing keywords receive zero. A keyword that is present without supporting context also receives zero as a keyword-stuffing safeguard.

The final assessment is calculated as:

```text
ATS readiness = (overall keyword score * 70%) + (header quality score * 30%)
```

### ATS Scoring Sequence

```mermaid
sequenceDiagram
    participant FE as Next.js Frontend
    participant API as ASP.NET Core API
    participant PDF as PdfPig
    participant OpenAI as OpenAI Responses API
    participant Engine as Deterministic Scoring Engine

    par Header validation
        FE->>API: Upload resume for header validation
        API->>PDF: Extract resume text
        PDF-->>API: Resume text
        API->>Engine: Score standard and aliased headers
    and Keyword extraction
        FE->>API: Send job description
        API->>OpenAI: Extract and classify ATS keywords
        OpenAI-->>API: Tiered keyword set
        API->>Engine: Filter, count, and promote keywords
    end

    FE->>API: Upload resume with extracted keywords
    API->>PDF: Extract resume text
    API->>OpenAI: Analyse keyword context in batches
    OpenAI-->>API: Presence, context flags, and evidence
    API->>Engine: Calculate keyword and tier scores
    API->>Engine: Combine 70% keyword and 30% header scores
    API-->>FE: Final ATS assessment
```

## Job Search Flow

The resume-review pipeline generates a focused profile containing:

- Target job titles
- Technical keywords
- Seniority
- South African and remote locations
- Roles or levels to exclude

The frontend waits until job search is enabled and this profile exists. A review run ID and stable request key prevent duplicate searches, while stale requests are aborted or ignored.

The backend passes the profile unchanged to GPT-5 with the OpenAI `web_search` tool enabled. Provider failures return an error instead of being represented as a successful empty result.

### Job Search Sequence

```mermaid
sequenceDiagram
    participant Review as Resume Review Stream
    participant FE as Next.js Frontend
    participant API as ASP.NET Core API
    participant OpenAI as GPT-5 Web Search

    Review-->>FE: section_completed job_search_profile
    FE->>FE: Validate profile and build request key

    alt Job search enabled and profile not requested
        FE->>API: POST /api/v1/job-listings
        API->>API: Validate API key and profile
        API->>OpenAI: Search using profile with web_search
        OpenAI-->>API: Structured current listings
        API-->>FE: Listings, genuine empty result, or provider error
        FE->>FE: Ignore stale responses from older review runs
    else Search disabled or duplicate request
        FE->>FE: Skip job-search request
    end
```

## OpenAI API Key Security

Resume Reviewer uses a bring-your-own-key model:

- The key is entered through a masked field.
- It is kept in an in-memory Zustand store.
- It is sent to the API as `X-OpenAI-Api-Key`.
- The backend applies it only to the current outbound OpenAI request.
- It is not placed in JSON bodies, form data, URLs, cookies, local storage, or session storage.
- The sensitive header is redacted before application request logging.

Production deployments must use HTTPS. The user's own browser can display the outgoing header in developer tools because the browser must send it, but the application does not intentionally persist it.

### API Key Request Flow

```mermaid
sequenceDiagram
    actor User
    participant FE as Next.js Frontend
    participant API as ASP.NET Core API
    participant Logs as Request Logging
    participant OpenAI as OpenAI API

    User->>FE: Enter OpenAI API key
    FE->>FE: Hold key in memory only
    FE->>API: X-OpenAI-Api-Key over HTTPS
    API->>API: Store key transiently in request context
    API->>API: Replace request header with REDACTED
    API->>Logs: Record safe request metadata
    API->>OpenAI: Authorization Bearer user-key
    OpenAI-->>API: Structured response
    API-->>FE: Application result
    FE->>FE: Key disappears when page memory is cleared
```

The application does not implement user accounts or session authentication. The user API key authorises only the outbound OpenAI operation for the current request.



## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Next.js 16, React 19, TypeScript |
| State | Zustand |
| Styling | Tailwind CSS, Radix UI, shadcn |
| PDF viewer | PDF.js |
| Backend | ASP.NET Core 9, C# |
| PDF extraction | PdfPig |
| AI | OpenAI Responses API and structured outputs |
| Logging | Serilog |
| Frontend tests | Playwright |
| API tests | xUnit |
| Containers | Docker, Docker Compose |
| Cloud | Azure Container Apps, Azure Container Registry |
| Infrastructure | Bicep |
| CI/CD | GitHub Actions |
| DNS | Cloudflare |

## Project Structure

```text
.
├── apps
│   ├── api
│   │   ├── Controllers
│   │   ├── Dtos
│   │   ├── Services
│   │   ├── Options
│   │   └── Dockerfile
│   ├── api.Tests
│   └── web
│       ├── app
│       ├── components
│       ├── service
│       ├── stores
│       ├── tests
│       └── Dockerfile
├── infra
│   └── azure-container-apps
├── packages
│   ├── eslint-config
│   ├── typescript-config
│   └── ui
├── scripts
│   └── security
├── docker-compose.yml
├── docker-compose.dev.yml
└── resume-tracker.sln
```




