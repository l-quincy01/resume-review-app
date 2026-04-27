import React from "react";
import { HeaderExport } from "./header/header-export";
import Link from "next/link";

export default function ResumeHeader() {
  return (
    <div className="flex flex-row items-center justify-between border-b p-2 z-100000   ">
      <Link href={"/"} className="text-sm gap-0">
        Resume Review<span className="text-xs">ᴮᴱᵀᴬ </span>{" "}
      </Link>
      <HeaderExport />
    </div>
  );
}
