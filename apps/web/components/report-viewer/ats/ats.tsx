import React from "react";
import AtsHeader from "./AtsHeader";
import AtsContent from "./AtsContent";
import { atsContent, spellingAndGrammar } from "@/types/atsReport.type";

interface props {
  atsReport: atsContent;
  spellingAndGrammar: spellingAndGrammar;
}

export default function Ats({ atsReport, spellingAndGrammar }: props) {
  return (
    <div>
      <AtsHeader atsContent={atsReport} />
      <AtsContent
        atsContent={atsReport}
        spellingAndGrammar={spellingAndGrammar}
      />
    </div>
  );
}
