"use client";
import { useTheme } from "next-themes";
import { useEffect, useRef, useState } from "react";

interface props {
  pdfUrl: string;
}

export default function PdfViewer({ pdfUrl }: props) {
  const iframeRef = useRef<HTMLIFrameElement>(null);

  const { systemTheme, resolvedTheme } = useTheme();
  const [isDark, setIsDark] = useState<boolean>(systemTheme === "dark");

  useEffect(() => {
    setIsDark(systemTheme === "dark");
  }, [systemTheme]);

  useEffect(() => {
    const doc = iframeRef.current?.contentDocument;
    if (!doc) return;

    doc.documentElement.classList.toggle("dark", isDark);
  }, [isDark]);

  useEffect(() => {
    const iframe = iframeRef.current;
    if (!iframe) return;

    const applySavedTheme = (doc: Document) => {
      const win = doc.defaultView;
      try {
        const saved = win?.localStorage.getItem("pdfjs-theme");
        const isDarkStored = saved ? saved === "dark" : null;
        if (isDarkStored !== null) {
          doc.documentElement.classList.toggle("dark", isDarkStored);
        } else {
          doc.documentElement.classList.toggle("dark", isDark);
        }
      } catch {
        doc.documentElement.classList.toggle("dark", isDark);
      }
    };

    const onLoad = () => {
      const doc = iframe.contentDocument;
      if (!doc) return;

      // 1) Inject CSS overrides
      const id = "my-pdfjs-overrides";
      doc.getElementById(id)?.remove();
      const style = doc.createElement("style");
      style.id = id;
      style.textContent = `
/* ========== THEME TOKENS ========== */
/* Light (default) */
:root {
  --tb-bg: #f4f4f5;
  --tb-fg: #1b1b22;
  --tb-border: #e6e6ef;
  --tb-hover: #efeff6;
  --tb-active: #e9e9f2;
  --tb-sep: #e0e0ea;
  --tb-input-bg: #ffffff;
  --tb-input-border: #d8d8e3;
  --app-bg: #f4f4f5;
  --page-bg: #ffffff;
  --selection: rgba(0,0,0,.14);
  --thumb-filter: none;
  --page-filter: none;
}

/* Dark overrides */
.dark {
  --tb-bg: #0e0e10;
  --tb-fg: #e9e9ee;
  --tb-border: #1f1f25;
  --tb-hover: #17171b;
  --tb-active: #101014;
  --tb-sep: #26262c;
  --tb-input-bg: #0f0f12;
  --tb-input-border: #2a2a33;
  --app-bg: #0f0f0f;
  --page-bg: #171717;
  --selection: rgba(255,255,255,.25);
  --thumb-filter: invert(1) hue-rotate(180deg);
  --page-filter: invert(1) hue-rotate(180deg);
}

#editorHighlightParamsToolbar{
  background: var(--tb-bg) !important;
  color: var(--tb-fg) !important ;
}

/* ========== APP BACKGROUND ========== */

#outerContainer,
#mainContainer,
#viewerContainer { background: var(--app-bg) !important; color-scheme: light; }
.dark #outerContainer,
.dark #mainContainer,
.dark #viewerContainer { color-scheme: dark; }


/* ========== TOOLBAR  ========== */
/* The wrapper strip that sits above the document */
#mainContainer > .toolbar {
  background: var(--tb-bg) !important;
  border-bottom: 1px solid var(--tb-border) !important;
}

/* Inner containers */
#toolbarContainer { background: transparent !important; }
#toolbarViewer { color: var(--tb-fg) !important; }

/* Buttons */
#toolbarViewer .toolbarButton {
  color: var(--tb-fg) !important;
}
#toolbarViewer .toolbarButton:hover {
  background: var(--tb-hover) !important;
}
#toolbarViewer .toolbarButton.toggled,
#toolbarViewer .toolbarButton[aria-expanded="true"] {
  background: var(--tb-active) !important;
}

/* Inputs / selects */
#toolbarViewer .toolbarField,
#toolbarViewer .dropdownToolbarButton select {
  background: var(--tb-input-bg) !important;
  color: var(--tb-fg) !important;
  border-color: var(--tb-input-border) !important;
}

/* Separators */
#toolbarViewer .splitToolbarButtonSeparator,
#toolbarViewer .verticalToolbarSeparator,
#toolbarViewer .horizontalToolbarSeparator {
  background-color: var(--tb-sep) !important;
  border-color: var(--tb-sep) !important;
}

/* Secondary tools menu (opened from the right-most toggle) */
#secondaryToolbar.menu,
#secondaryToolbar .menuContainer {
  background: var(--tb-bg) !important;
  color: var(--tb-fg) !important;
  border-color: var(--tb-border) !important;
  box-shadow: 0 10px 30px rgba(0,0,0,.45) !important;
}

/* ========== PAGE AREA ========== */
.page { background: var(--page-bg) !important; box-shadow: none !important; }

.page ::-webkit-scrollbar {
  display: none;
}

/* Selection color */
.textLayer ::selection { background: var(--selection) !important; }

/* Only invert page + thumbnails in dark; light stays normal */
.page canvas { filter: var(--page-filter); }
.thumbnailImage { filter: var(--thumb-filter); }

/* ==========  THEME TOGGLE BUTTON (unchanged) ========== */
#themeToggleButton.toolbarButton { min-width: auto; }
#themeToggleButton {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: transparent !important;
  border-style: none !important;
}
#themeToggleButton .icon {
  width: 16px; height: 16px; flex: none; color: currentColor; pointer-events: none;
}
`;

      doc.head.appendChild(style);

      const right = doc.getElementById("toolbarViewerRight");
      if (right && !doc.getElementById("themeToggleButton")) {
        const MOON_SVG = `
    <svg class="icon" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M21 12.79A9 9 0 1 1 11.21 3a7 7 0 1 0 9.79 9.79z" fill="currentColor"/>
    </svg>`;
        const SUN_SVG = `
    <svg class="icon" viewBox="0 0 24 24" aria-hidden="true">
      <circle cx="12" cy="12" r="5" fill="currentColor"/>
      <g stroke="currentColor" stroke-width="2" stroke-linecap="round" fill="none">
        <path d="M12 1v3"/><path d="M12 20v3"/>
        <path d="M4.22 4.22l2.12 2.12"/><path d="M17.66 17.66l2.12 2.12"/>
        <path d="M1 12h3"/><path d="M20 12h3"/>
        <path d="M4.22 19.78l2.12-2.12"/><path d="M17.66 6.34l2.12-2.12"/>
      </g>
    </svg>`;

        // Button markup
        const btn = doc.createElement("button");
        btn.id = "themeToggleButton";
        btn.className = "toolbarButtonWithContainer";
        btn.type = "button";
        btn.tabIndex = 0;
        btn.setAttribute("aria-label", "Toggle theme");

        // label + icon
        const isCurrentlyDark = doc.documentElement.classList.contains("dark");
        btn.innerHTML = `${isCurrentlyDark ? SUN_SVG : MOON_SVG}`;

        right.prepend(btn);

        //  theme toggle
        btn.addEventListener("click", () => {
          const html = doc.documentElement;
          const nowDark = !html.classList.contains("dark");
          html.classList.toggle("dark", nowDark);
          btn.innerHTML = `${nowDark ? SUN_SVG : MOON_SVG}`;
          try {
            doc.defaultView?.localStorage.setItem(
              "pdfjs-theme",
              nowDark ? "dark" : "light",
            );
          } catch (err) {
            console.log("pdf viewer error:" + err);
          }
        });
      }

      // 3) Apply  initial theme
      applySavedTheme(doc);
    };

    iframe.addEventListener("load", onLoad);

    if (
      iframe.contentDocument &&
      iframe.contentDocument.readyState !== "loading"
    ) {
      onLoad();
    }

    return () => {
      iframe.removeEventListener("load", onLoad);
    };
  }, [isDark]);

  const viewerUrl = `/pdfjs/web/viewer.html?file=${encodeURIComponent(
    pdfUrl,
  )}#pagemode=thumbs&zoom=page-width`;

  return (
    <div style={{ position: "relative", height: "90vh" }}>
      <iframe
        ref={iframeRef}
        title="PDF.js"
        src={viewerUrl}
        style={{ width: "100%", height: "100%", border: 0 }}
      />
    </div>
  );
}
