import { useTheme } from "next-themes";
import React from "react";
import MarkdownPreview from "@uiw/react-markdown-preview";
export default function SummarySection() {
  const { systemTheme } = useTheme();

  return (
    <div>
      <MarkdownPreview
        source={`jobMatchDetails.jobDescription`}
        style={{
          padding: 16,
          background: "transparent",
          color: systemTheme === "dark" ? "white" : "black",
        }}
      />
    </div>
  );
}
