import { MainLayout } from '@/features/global/organisms/MainLayout';

/**
 * Header/Footerを含む共通レイアウト
 * 
 * このルートグループ内の全ページに適用されます。
 */
export default function WithLayoutGroup({
  children,
}: {
  children: React.ReactNode;
}) {
  return <MainLayout>{children}</MainLayout>;
}
