"use client";

import { useEffect, useMemo, useState } from "react";

interface TypewriterProps {
  words: string[];
  speed?: number;
  delayBetweenWords?: number;
  cursor?: boolean;
  cursorChar?: string;
  className?: string;
}

export function Typewriter({
  words,
  speed = 100,
  delayBetweenWords = 2000,
  cursor = true,
  cursorChar = "|",
  className,
}: TypewriterProps) {
  const safeWords = useMemo(() => words.filter(Boolean), [words]);

  const [displayText, setDisplayText] = useState("");
  const [isDeleting, setIsDeleting] = useState(false);
  const [wordIndex, setWordIndex] = useState(0);
  const [charIndex, setCharIndex] = useState(0);
  const [showCursor, setShowCursor] = useState(true);

  const currentWord = safeWords[wordIndex] ?? "";

  useEffect(() => {
    if (!safeWords.length) return;

    const isComplete = !isDeleting && charIndex === currentWord.length;
    const isDeleted = isDeleting && charIndex === 0;

    const timeout = window.setTimeout(
      () => {
        if (isComplete) {
          setIsDeleting(true);
          return;
        }

        if (isDeleted) {
          setIsDeleting(false);
          setWordIndex((prev) => (prev + 1) % safeWords.length);
          return;
        }

        const nextIndex = isDeleting ? charIndex - 1 : charIndex + 1;
        setCharIndex(nextIndex);
        setDisplayText(currentWord.slice(0, nextIndex));
      },
      isComplete ? delayBetweenWords : isDeleting ? speed / 2 : speed,
    );

    return () => window.clearTimeout(timeout);
  }, [
    safeWords.length,
    currentWord,
    charIndex,
    isDeleting,
    speed,
    delayBetweenWords,
  ]);

  useEffect(() => {
    if (!cursor) return;

    const interval = window.setInterval(() => {
      setShowCursor((prev) => !prev);
    }, 500);

    return () => window.clearInterval(interval);
  }, [cursor]);

  return (
    <span className={className}>
      {displayText}
      {cursor && (
        <span
          className="ml-1 transition-opacity duration-75"
          style={{ opacity: showCursor ? 1 : 0 }}
        >
          {cursorChar}
        </span>
      )}
    </span>
  );
}
