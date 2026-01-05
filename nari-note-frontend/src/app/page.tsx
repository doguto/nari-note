import Link from "next/link";
import { Header } from "@/components/layout/Header";
import { Sidebar } from "@/components/layout/Sidebar";
import { Footer } from "@/components/layout/Footer";
import { HomeArticleListContainer } from "@/features/article/containers/HomeArticleListContainer";

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-[#f5f3e8] to-[#e8e4d0]">
      <Header />

      {/* ヒーローセクション */}
      <section className="max-w-7xl mx-auto px-4 py-16 text-center">
        <div className="mb-8">
          <h1 className="text-4xl md:text-5xl font-bold text-[#2d3e1f] mb-4" style={{ fontFamily: 'serif' }}>
            技術の記録、レビュー、自分の記。
          </h1>
          <p className="text-2xl md:text-3xl text-[#555] mb-8" style={{ fontFamily: 'serif' }}>
            知識を共有しよう。
          </p>
        </div>
        
        <Link href="/debug" className="inline-block px-8 py-3 bg-[#88b04b] text-white rounded-lg hover:bg-[#769939] transition-colors font-medium text-lg">
          新規記事を作成
        </Link>
      </section>

      {/* メインコンテンツ */}
      <section className="max-w-7xl mx-auto px-4 pb-16">
        <div className="flex gap-8">
          {/* 記事一覧 */}
          <div className="flex-1">
            <h2 className="text-2xl font-bold text-[#2d3e1f] mb-6">新着記事</h2>
            <HomeArticleListContainer />
          </div>

          <Sidebar />
        </div>
      </section>

      <Footer />
    </div>
  );
}
