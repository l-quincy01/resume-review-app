"use client";
import { HeroSection } from "@/components/blocks/hero-section";
import FooterViewer from "@/components/footer/footer-viewer";

export default function Home() {
  return (
    <>
      <HeroSection />
      <FooterViewer />
    </>
  );
}

/*

        apps/api/appsettings.Production.json apps/api/appsettings.example.json apps/web/.env.example apps/web/lib/api.ts

        modified:   apps/api/Program.cs apps/api/appsettings.json apps/web/.gitignore apps/web/app/"(unauth)"/resume/page.tsx apps/web/components/home/resume-review-form.tsx apps/web/components/resume/disclaimer.tsx apps/web/service/resume-review.service.ts turbo.json

*/
