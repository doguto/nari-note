import { Button } from '@/components/ui/button';

interface ProfileTabsProps {
  activeTab: string;
  tabContext: 'content' | 'follow';
  onTabChange: (tab: string) => void;
}

const CONTENT_TABS = [
  { key: 'articles', label: '記事' },
  { key: 'courses', label: '講座' },
  { key: 'likes', label: 'いいね' },
];

const FOLLOW_TABS = [
  { key: 'followers', label: 'フォロワー' },
  { key: 'followings', label: 'フォロー中' },
];

/**
 * ProfileTabs - Organism Component
 *
 * プロフィールのタブ（記事/講座/いいね、またはフォロワー/フォロー中）
 * スマホでは等幅、PC では左寄せで並べる
 */
export function ProfileTabs({ activeTab, tabContext, onTabChange }: ProfileTabsProps) {
  const tabs = tabContext === 'content' ? CONTENT_TABS : FOLLOW_TABS;

  return (
    <nav className="grid auto-cols-fr grid-flow-col border-t border-brand-bg-gradient-to sm:flex sm:gap-9 sm:px-8">
      {tabs.map((tab) => {
        const isActive = activeTab === tab.key;
        return (
          <Button
            key={tab.key}
            variant="ghost"
            onClick={() => onTabChange(tab.key)}
            aria-current={isActive ? 'page' : undefined}
            className={`h-auto rounded-none border-b-[3px] px-0 py-3.5 text-sm hover:bg-transparent sm:text-[15px] ${
              isActive
                ? 'border-brand-primary font-bold text-brand-text'
                : 'border-transparent font-normal text-gray-500 hover:text-brand-text'
            }`}
          >
            {tab.label}
          </Button>
        );
      })}
    </nav>
  );
}
