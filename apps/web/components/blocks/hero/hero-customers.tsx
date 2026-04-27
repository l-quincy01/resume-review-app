import { ChevronRight } from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import React from "react";

const customers = [
  {
    name: "Google",
    logo: "https://companiesmarketcap.com/img/company-logos/64/GOOG.webp",
  },
  {
    name: "Ozow",
    logo: "https://d24ndt2yiijez0.cloudfront.net/uploads/image/asset/22768/Ozow-Logo.png",
  },

  {
    name: "Takealot",
    logo: "https://play-lh.googleusercontent.com/Zy-BRdWCKBqJeDCcoFnrEoGeCqQnSEfs3qBaC7_nWHLMm3-Nfs1HRul7cBJqjLiDH2h4",
  },
  {
    name: "Naspers",
    logo: "https://play-lh.googleusercontent.com/9KvU7PzJsX71lcT1V7n1KSt9bTZATsQc6RDtYjts93w3xbk9K8fLU0ATjUGAv9q-Szx7s3hLH6oOP1Gd44cx",
  },
  {
    name: "Yoco",
    logo: "https://a.storyblok.com/f/111633/2400x1260/b3756f78aa/yoco-logo.jpg",
  },
  {
    name: "Lesaka",
    logo: "https://companiesmarketcap.com/img/company-logos/64/LSAK.webp",
  },
];

export default function HeroCustomers() {
  return (
    <section className="bg-background pb-16 pt-16 md:pb-32">
      <div className="group relative m-auto max-w-5xl px-6">
        <div className="absolute inset-0 z-10 flex scale-95 items-center justify-center opacity-0 duration-500 group-hover:scale-100 group-hover:opacity-100">
          <Link
            href="/"
            className="block text-sm duration-150 hover:opacity-75"
          >
            <span> Meet Our Customers</span>

            <ChevronRight className="ml-1 inline-block size-3" />
          </Link>
        </div>
        <div className="group-hover:blur-xs mx-auto mt-12 grid max-w-2xl grid-cols-4 gap-x-12 gap-y-8 transition-all duration-500 group-hover:opacity-50 sm:gap-x-16 sm:gap-y-14">
          {customers.map((customer, index) => (
            <div className="flex" key={index}>
              <Image
                className="mx-auto h-5 w-auto "
                src={customer.logo}
                alt={customer.name}
                width={100}
                height={20}
              />
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
