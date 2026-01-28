/**
 * HeroSection - Organism Component
 * 
 * ホームページのヒーローセクションを表示するコンポーネント
 * タイトルとサブタイトルを含む
 */
export function HeroSection() {
  return (
    <section className="max-w-7xl mx-auto px-4 py-16 text-center">
      <h1 
        className="text-4xl md:text-5xl font-bold text-brand-text mb-4" 
        style={{ fontFamily: 'serif' }}
      >
        将棋の知識共有プラットフォーム
      </h1>
      <p 
        className="text-2xl md:text-3xl text-brand-secondary-text mb-8" 
        style={{ fontFamily: 'serif' }}
      >
        あなたの将棋の知識を共有し、コミュニティと共に成長しましょう
      </p>
    </section>
  );
}
