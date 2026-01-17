import { Header } from "@/components/layout/Header";
import { Sidebar } from "@/components/layout/Sidebar";
import { Footer } from "@/components/layout/Footer";
import { HomeArticleList } from "@/features/article/organisms";

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-brand-bg-gradient-from to-brand-bg-gradient-to flex flex-col">
      <Header />

      <div className="flex-1">
        {/* ヒーローセクション */}
        <section className="max-w-7xl mx-auto px-4 py-16 text-center">
          <h1 className="text-4xl md:text-5xl font-bold text-brand-text mb-4" style={{ fontFamily: 'serif' }}>
            将棋の知識共有プラットフォーム
          </h1>
          <p className="text-2xl md:text-3xl text-brand-secondary-text mb-8" style={{ fontFamily: 'serif' }}>
            あなたの将棋の知識を共有し、コミュニティと共に成長しましょう
          </p>
        </section>

        {/* メインコンテンツ */}
        <section className="max-w-7xl mx-auto px-4 pb-16">
          <div className="flex gap-8">
            {/* 記事一覧 */}
            <div className="flex-1">
              <h2 className="text-2xl font-bold text-brand-text mb-6">新着記事</h2>
              <HomeArticleList />
            </div>

            <Sidebar />
          </div>
        </section>
      </div>

      <Footer />
    </div>
  );
}
