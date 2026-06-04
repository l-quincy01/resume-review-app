(function () {
  const trustedOrigin = window.location.origin;

  async function openPdfFromBytes(bytes, filename) {
    const app = window.PDFViewerApplication;

    if (!app) {
      throw new Error("PDFViewerApplication is not available.");
    }

    await app.initializedPromise;
    await app.open({
      data: bytes,
      originalUrl: filename || "resume.pdf",
    });
  }

  window.addEventListener("message", (event) => {
    if (event.origin !== trustedOrigin) {
      return;
    }

    const message = event.data;
    if (!message || message.type !== "resume-review:open-pdf") {
      return;
    }

    const bytes = message.bytes;
    if (!(bytes instanceof Uint8Array)) {
      return;
    }

    openPdfFromBytes(bytes, message.filename).catch((error) => {
      window.parent.postMessage(
        {
          type: "resume-review:pdf-error",
          message: error instanceof Error ? error.message : "PDF load failed.",
        },
        trustedOrigin,
      );
    });
  });
})();
