'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';

const NAV_ITEMS = [
  {
    category: '一般',
    items: [
      { label: 'プロフィール編集', href: '/settings/general/profile' },
      { label: 'パスワード変更', href: '/settings/general/password' },
    ],
  },
];

export function SettingsSidebarNav() {
  const pathname = usePathname();

  return (
    <aside className="w-48 shrink-0">
      <nav className="space-y-6">
        {NAV_ITEMS.map(({ category, items }) => (
          <div key={category}>
            <h3
              className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2 px-3"
              style={{ fontFamily: 'serif' }}
            >
              {category}
            </h3>
            <ul className="space-y-1">
              {items.map(({ label, href }) => (
                <li key={href}>
                  <Link
                    href={href}
                    className={`block px-3 py-2 rounded-md text-sm transition-colors ${
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
