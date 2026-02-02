/**
 * HeroSection - Organism Component
 * 
 * ホームページのヒーローセクションを表示するコンポーネント
 * タイトルとサブタイトルを含む
 */
export function HeroSection() {
  return (
    <section className="w-11/12 mx-auto px-4 py-16 text-center">
      <h1 
        className="text-3xl md:text-4xl font-bold text-brand-text mb-4" 
        style={{ fontFamily: 'serif' }}
      >
        将棋の知識共有プラットフォーム
      </h1>
      <p 
        className="text-xl md:text-2xl text-brand-secondary-text mb-8" 
        style={{ fontFamily: 'serif' }}
      >
        あなたの将棋の知識を共有し、コミュニティと共に成長しましょう
      </p>
    </section>
  );
}
