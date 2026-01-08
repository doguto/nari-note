import { Header } from "@/components/layout/Header";
import { Sidebar } from "@/components/layout/Sidebar";
import { Footer } from "@/components/layout/Footer";
import { HomeArticleList } from "@/features/article/organisms";

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-brand-bg-gradient-from to-brand-bg-gradient-to">
      <Header />

      {/* ヒーローセクション */}
      <section className="max-w-7xl mx-auto px-4 py-16 text-center">
        <h1 className="text-4xl md:text-5xl font-bold text-brand-text mb-4" style={{ fontFamily: 'serif' }}>
          技術の記録、レビュー、自分の記。
        </h1>
        <p className="text-2xl md:text-3xl text-brand-secondary-text mb-8" style={{ fontFamily: 'serif' }}>
          知識を共有しよう。
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

      <Footer />
    </div>
  );
}
