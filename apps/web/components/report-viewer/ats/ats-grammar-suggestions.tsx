import React from "react";

interface props {
  contentType: string;
  spellingAndGrammarContent: {
    type: "Low" | "Medium" | "High";
    current: string;
    suggestedCorrection: string;
    explanation: string;
  }[];
}

export default function AtsSuggestedCorrection({
  contentType,
  spellingAndGrammarContent,
}: props) {
  return (
    <div>
      <div className="flex flex-col gap-4 mb-2 ">
        <span
          className={`font-semibold text-xl rounded-2xl px-3 py-1  w-fit bg-gray-200 text-gray-950 dark:bg-gray-800 dark:text-gray-100`}
        >
          {" "}
          {contentType === "Grammar"
            ? " Grammar Suggestions"
            : contentType === "Spelling"
              ? "Spelling Check"
              : contentType === "PersonalPronouns"
                ? "Pronoun Check"
                : contentType === "PassiveVoiceCheck" &&
                  "Passive Usage Voice Check"}
        </span>

        {spellingAndGrammarContent.length === 0 && (
          <span className="text-green-500 font-semibold">
            No Corrections Needed
          </span>
        )}

        {spellingAndGrammarContent?.filter((item) => item.type === "High")
          .length !== 0 && (
          <span className="text-red-500 font-semibold">High</span>
        )}

        {spellingAndGrammarContent
          .filter((item) => item.type === "High")
          ?.map((suggested, index) => (
            <div key={index} className="flex flex-col ">
              <div className="grid grid-cols-2 border  divide-x">
                <div className="flex flex-col gap-2 p-2">
                  <span className=" uppercase text-orange-400">Current</span>
                  {suggested.current}
                </div>
                <div className="flex flex-col gap-2 p-2">
                  <span className="uppercase text-indigo-400">
                    Improved example
                  </span>
                  {suggested.suggestedCorrection}
                </div>
              </div>

              <div className="border border-t-0 flex flex-col gap-2 p-2">
                <span className="uppercase"> Explanation</span>
                <div>{suggested.explanation}</div>
              </div>
            </div>
          ))}

        {spellingAndGrammarContent.filter((item) => item.type === "Medium")
          .length !== 0 && (
          <span className="text-yellow-500  font-semibold">Medium</span>
        )}

        {spellingAndGrammarContent
          .filter((item) => item.type === "Medium")
          ?.map((suggested, index) => (
            <div key={index} className="flex flex-col ">
              <div className="grid grid-cols-2 border  divide-x">
                <div className="flex flex-col gap-2 p-2">
                  <span className=" uppercase text-orange-400">Current</span>
                  {suggested.current}
                </div>
                <div className="flex flex-col gap-2 p-2">
                  <span className="uppercase text-indigo-400">
                    Improved example
                  </span>
                  {suggested.suggestedCorrection}
                </div>
              </div>

              <div className="border border-t-0 flex flex-col gap-2 p-2">
                <span className="uppercase"> Explanation</span>
                <div>{suggested.explanation}</div>
              </div>
            </div>
          ))}

        {spellingAndGrammarContent.filter((item) => item.type === "Low")
          .length !== 0 && (
          <span className="text-blue-500 font-semibold">Low</span>
        )}

        {spellingAndGrammarContent
          .filter((item) => item.type === "Low")
          ?.map((suggested, index) => (
            <div key={index} className="flex flex-col ">
              <div className="grid grid-cols-2 border  divide-x">
                <div className="flex flex-col gap-2 p-2">
                  <span className=" uppercase text-orange-400">Current</span>
                  {suggested.current}
                </div>
                <div className="flex flex-col gap-2 p-2">
                  <span className="uppercase text-indigo-400">
                    Improved example
                  </span>
                  {suggested.suggestedCorrection}
                </div>
              </div>

              <div className="border border-t-0 flex flex-col gap-2 p-2">
                <span className="uppercase"> Explanation</span>
                <div>{suggested.explanation}</div>
              </div>
            </div>
          ))}
      </div>
    </div>
  );
}
