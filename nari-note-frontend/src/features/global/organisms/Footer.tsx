import Link from 'next/link';


export function Footer() {
  return (
    <footer className="bg-brand-text text-white py-8">
      <div className="w-11/12 mx-auto px-4">
        <div className="flex flex-col md:flex-row justify-between items-center gap-4">
          <div className="text-center md:text-left">
            <p className="text-sm text-gray-400">
              © 2026 なりノート
            </p>
          </div>
          <div className="flex gap-6 text-sm">
            <Link href="/terms" className="text-gray-400 hover:text-brand-primary transition-colors">
              利用規約
            </Link>
            <Link href="/privacy-policy" className="text-gray-400 hover:text-brand-primary transition-colors">
              プライバシーポリシー
            </Link>
            {/* TODO: お問い合わせフォームの作成 */}
            {/* <a href="#" className="text-gray-400 hover:text-brand-primary transition-colors">
              お問い合わせ
            </a> */}
          </div>
        </div>
      </div>
    </footer>
  );
}
