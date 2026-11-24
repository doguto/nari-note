import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: '研究盤',
  robots: {
    index: false,
    follow: false,
  },
};

export default function BookResearchLayout({ children }: { children: React.ReactNode }) {
  return <>{children}</>;
}
