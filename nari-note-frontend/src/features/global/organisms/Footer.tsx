/**
 * フッターコンポーネント
 * 
 * 全ページ共通のフッター。
 */
export function Footer() {
  return (
    <footer className="bg-brand-text text-white py-8">
      <div className="max-w-7xl mx-auto px-4">
        <div className="flex flex-col md:flex-row justify-between items-center gap-4">
          <div className="text-center md:text-left">
            <p className="text-sm text-gray-400">
              © 2024 なりノート
            </p>
          </div>
          <div className="flex gap-6 text-sm">
            <a href="#" className="text-gray-400 hover:text-brand-primary transition-colors">
              利用規約
            </a>
            <a href="#" className="text-gray-400 hover:text-brand-primary transition-colors">
              プライバシーポリシー
            </a>
            <a href="#" className="text-gray-400 hover:text-brand-primary transition-colors">
              お問い合わせ
            </a>
          </div>
        </div>
      </div>
    </footer>
  );
}
