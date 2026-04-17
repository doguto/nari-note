import { ReactNode } from 'react';
import { Sidebar } from './Sidebar';

interface PageWithSidebarProps {
  children: ReactNode;
  title?: string;
}

export function PageWithSidebar({ children, title }: PageWithSidebarProps) {
  return (
    <div className="sm:w-full md:w-5/6 w-4/5 mx-auto px-4 py-8 flex justify-center">
      <div className="flex gap-8">
        <main className="flex-1 max-w-3xl">
          {title && <h2 className="text-2xl font-bold text-brand-text mb-6">{title}</h2>}
          {children}
        </main>
        <Sidebar />
      </div>
    </div>
  );
}
