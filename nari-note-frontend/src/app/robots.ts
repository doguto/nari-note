import type { MetadataRoute } from "next";

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || "https://nari-note.com";

export default function robots(): MetadataRoute.Robots {
  return {
    rules: {
      userAgent: "*",
      allow: "/",
      disallow: [
        "/articles/new",
        "/articles/drafts",
        "/articles/my-articles",
        "/courses/new",
        "/courses/my-courses",
        "/settings/",
        "/debug/",
      ],
    },
    sitemap: `${siteUrl}/sitemap.xml`,
  };
}
