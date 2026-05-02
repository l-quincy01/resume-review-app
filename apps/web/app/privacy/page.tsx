export default function PrivacyPage() {
  return (
    <main className="mx-auto flex w-full max-w-3xl flex-col gap-6 px-6 py-12 text-sm leading-6 text-muted-foreground">
      <div className="flex flex-col gap-2">
        <h1 className="text-3xl font-semibold text-foreground">
          Privacy Policy
        </h1>
        <p>
          This service analyzes resumes to generate AI-assisted feedback. This
          page explains the intended data handling behavior for the production
          application.
        </p>
      </div>

      <section className="flex flex-col gap-2">
        <h2 className="text-xl font-semibold text-foreground">
          Resume Processing
        </h2>
        <p>
          Uploaded resumes are processed in memory by this app and sent to the
          AI provider for analysis. The app does not save uploaded resumes to a
          database, filesystem, or object storage.
        </p>
        <p>
          When the API uploads a resume file to the AI provider, it attempts to
          delete that provider file after analysis completes or fails.
        </p>
      </section>

      <section className="flex flex-col gap-2">
        <h2 className="text-xl font-semibold text-foreground">
          Logs And Diagnostics
        </h2>
        <p>
          Application logs are intended to exclude resume contents, uploaded
          file contents, full AI responses, and job description text. Logs may
          include operational metadata such as timestamps, request paths,
          status codes, model names, file identifiers, and error categories.
        </p>
      </section>

      <section className="flex flex-col gap-2">
        <h2 className="text-xl font-semibold text-foreground">
          Generated Results
        </h2>
        <p>
          Resume analysis results are rendered in the browser for the current
          session. The app does not intentionally persist generated results
          unless a future feature explicitly says otherwise.
        </p>
      </section>

      <section className="flex flex-col gap-2">
        <h2 className="text-xl font-semibold text-foreground">
          Provider Processing
        </h2>
        <p>
          AI provider processing is governed by the provider&apos;s API terms and
          data handling policies. Do not upload information you are not allowed
          to share with an external AI provider.
        </p>
      </section>
    </main>
  );
}
