import Image from 'next/image';
import Link from 'next/link';


export function HeaderLogo() {
  return (
    <div className="bg-brand-bg-light border-b border-brand-border">
      <div className="w-11/12 mx-auto px-4 py-1">
        <Link href="/" className="flex items-center gap-2 hover:opacity-80 transition-opacity w-fit">
          <div className="w-7 h-7 bg-brand-bg-light rounded-lg flex items-center justify-center text-white font-bold text-sm">
            <Image src="/narinote_icon_circle.png" alt="Logo" width={28} height={28} />
          </div>
          <span className="text-sm font-bold text-brand-text" style={{ fontFamily: 'serif' }}>
            将棋ブログ投稿サイト ～なりノート～
          </span>
        </Link>
      </div>
    </div>
  );
}
