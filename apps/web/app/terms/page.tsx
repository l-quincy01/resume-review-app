export default function TermsPage() {
  return (
    <main className="mx-auto flex w-full max-w-3xl flex-col gap-6 px-6 py-12 text-sm leading-6 text-muted-foreground">
      <div className="flex flex-col gap-2">
        <h1 className="text-3xl font-semibold text-foreground">Terms</h1>
        <p>
          By using this service, you agree to use AI-generated resume feedback
          as general guidance only.
        </p>
      </div>

      <section className="flex flex-col gap-2">
        <h2 className="text-xl font-semibold text-foreground">
          AI Analysis
        </h2>
        <p>
          The service sends your resume and optional job description to an AI
          provider for analysis. AI outputs may be incomplete, inaccurate, or
          unsuitable for a specific job application.
        </p>
      </section>

      <section className="flex flex-col gap-2">
        <h2 className="text-xl font-semibold text-foreground">
          Your Responsibility
        </h2>
        <p>
          You are responsible for reviewing all suggestions before using them.
          Do not upload resumes or job descriptions unless you have the right to
          share that information with this service and its AI provider.
        </p>
      </section>

      <section className="flex flex-col gap-2">
        <h2 className="text-xl font-semibold text-foreground">
          Data Handling
        </h2>
        <p>
          This app does not intentionally store uploaded resumes. Uploaded
          provider files are deleted after processing on a best-effort basis,
          and operational logs are designed to avoid resume contents and full AI
          responses.
        </p>
      </section>
    </main>
  );
}

