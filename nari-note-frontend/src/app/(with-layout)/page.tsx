import { HeroSection, MainContentSection } from "@/features/global/organisms";
import { HomeArticleListPage } from "@/features/article/pages";

export default function Home() {
  return (
    <>
      <HeroSection />
      <MainContentSection title="新着記事">
        <HomeArticleListPage />
      </MainContentSection>
    </>
  );
}
