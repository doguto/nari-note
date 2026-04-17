
import { MainLayout } from '@/features/global/organisms/MainLayout';


export default function WithLayoutGroup({
  children,
}: {
  children: React.ReactNode;
}) {
  return <MainLayout>{children}</MainLayout>;
}
