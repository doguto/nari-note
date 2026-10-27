import { ReactNode } from 'react';
import { Sidebar } from './Sidebar';

interface PageWithSidebarProps {
  children: ReactNode;
  title?: string;
  wide?: boolean;
}

export function PageWithSidebar({ children, title, wide = false }: PageWithSidebarProps) {
  return (
    <div className="w-full md:w-11/12 mx-auto px-4 py-8 flex justify-center">
      <div className="flex gap-8 w-full">
        <div className={`${wide ? 'lg:w-1/12' : 'lg:w-1/6'} hidden lg:block shrink-0`} aria-hidden="true" />
        <main className={`flex-1 ${wide ? 'max-w-5xl' : 'max-w-4xl'}`}>
          {title && <h2 className="text-2xl font-bold text-brand-text mb-6">{title}</h2>}
          {children}
        </main>
        <Sidebar />
      </div>
    </div>
  );
}
