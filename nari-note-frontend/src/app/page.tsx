import Link from "next/link";
import { Header } from "@/components/layout/Header";
import { Sidebar } from "@/components/layout/Sidebar";
import { Footer } from "@/components/layout/Footer";

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-[#f5f3e8] to-[#e8e4d0]">
      <Header />

      {/* ヒーローセクション */}
      <section className="max-w-7xl mx-auto px-4 py-16 text-center">
        <div className="mb-8">
          <h1 className="text-4xl md:text-5xl font-bold text-[#2d3e1f] mb-4" style={{ fontFamily: 'serif' }}>
            持続の裁判、ソノビビュー、自我記。
          </h1>
          <p className="text-2xl md:text-3xl text-[#555] mb-8" style={{ fontFamily: 'serif' }}>
            知識分全全を譲しよ。う
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
            
            <div className="space-y-4">
              {/* 記事カード */}
              {[
                {
                  title: "角坊わ具隊陣記",
                  author: "【角弛別師全額】",
                  stats: "101 2:3 -490",
                  date: "202-1 1.105 11",
                  image: "🎮"
                },
                {
                  title: "第2細目読記・全一記工中一郭",
                  author: "【情河調續記】",
                  stats: "101 22.3 -430",
                  date: "202-1 4.105 12",
                  image: "🎮"
                },
                {
                  title: "序始の僧担盖術",
                  author: "【辯哪課墜坐小登含】",
                  stats: "101 2:3 -490",
                  date: "202-1 4.105 11",
                  image: "🎮"
                },
                {
                  title: "序皮章題担結柄金部",
                  author: "【関発祖墜低】",
                  stats: "101 22.3 -430",
                  date: "202-1 4.105 11",
                  image: "🎮"
                }
              ].map((article, index) => (
                <div key={index} className="bg-[#2d3e1f] rounded-lg p-4 text-white hover:bg-[#3d4e2f] transition-colors cursor-pointer">
                  <div className="flex gap-4">
                    <div className="w-24 h-24 bg-[#1a2515] rounded flex items-center justify-center text-4xl">
                      {article.image}
                    </div>
                    <div className="flex-1">
                      <h3 className="text-lg font-bold mb-2">{article.title}</h3>
                      <div className="flex items-center gap-2 text-sm text-gray-300 mb-2">
                        <span className="w-6 h-6 bg-gray-600 rounded-full"></span>
                        <span>{article.author}</span>
                      </div>
                      <div className="flex gap-4 text-sm text-gray-400">
                        <span>{article.stats}</span>
                        <span>{article.date}</span>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <Sidebar />
        </div>
      </section>

      <Footer />
    </div>
  );
}
