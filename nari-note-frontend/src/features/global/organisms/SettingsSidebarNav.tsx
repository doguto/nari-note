'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';

const NAV_ITEMS = [
  {
    category: '一般',
    items: [
      { label: 'プロフィール編集', href: '/settings/general/profile' },
    ],
  },
  {
    category: 'アカウント',
    items: [
      { label: 'パスワード変更', href: '/settings/account/password' },
      { label: '退会', href: '/settings/account/withdraw' },
    ],
  },
];

export function SettingsSidebarNav() {
  const pathname = usePathname();

  return (
    <aside className="w-full md:w-48 shrink-0">
      {/* モバイルではカテゴリ見出しを省き、全項目を横スクロールのタブとして並べる */}
      <nav className="flex gap-2 overflow-x-auto border-b border-gray-200 pb-2 md:block md:space-y-6 md:border-b-0 md:pb-0">
        {NAV_ITEMS.map(({ category, items }) => (
          <div key={category} className="contents md:block">
            <h3
              className="hidden md:block text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2 px-3"
              style={{ fontFamily: 'serif' }}
            >
              {category}
            </h3>
            <ul className="contents md:block md:space-y-1">
              {items.map(({ label, href }) => (
                <li key={href} className="shrink-0">
                  <Link
                    href={href}
                    className={`block whitespace-nowrap px-3 py-2 rounded-md text-sm transition-colors ${
                      pathname === href
                        ? 'bg-brand-primary/10 text-brand-primary font-medium'
                        : 'text-brand-text hover:bg-gray-100 hover:text-brand-text'
                    }`}
                    style={{ fontFamily: 'serif' }}
                  >
                    {label}
                  </Link>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </nav>
    </aside>
  );
}
