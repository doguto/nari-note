import type { Metadata } from 'next';
import { AuthGuard } from '@/features/global/organisms';
import { MyBooksPage } from '@/features/book/pages';

export const metadata: Metadata = {
  title: 'マイ定跡',
  robots: {
    index: false,
    follow: false,
  },
};

export default function BooksPage() {
  return (
    <AuthGuard redirectPath="/books">
      <MyBooksPage />
    </AuthGuard>
  );
}
