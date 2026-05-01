import { Footer } from "@/components/ui/footer";

export default function FooterViewer() {
  return (
    <div className="flex flex-col w-full items-center justify-center bg-accent pt-8 px-12 lg:px-0 ">
      <div className="max-w-7xl w-full">
        <Footer
          logo={<></>}
          brandName=""
          socialLinks={[]}
          mainLinks={[]}
          legalLinks={[
            { href: "#", label: "Privacy" },
            { href: "#", label: "Terms" },
          ]}
          copyright={{
            text: "© 2026 Resume Reviewᴮᴱᵀᴬ",
            license: "All rights reserved",
          }}
        />
      </div>
    </div>
  );
}

//   <Link href={"/"} className="text-sm gap-0">
//     Resume Review<span className="text-xs">ᴮᴱᵀᴬ </span>{" "}
//   </Link>
