import { ReactNode } from 'react';

interface PageWithoutSidebarProps {
  children: ReactNode;
  title?: string;
}

export function PageWithoutSidebar({ children, title }: PageWithoutSidebarProps) {
  return (
    <div className="sm:w-full md:min-w-11/12 w-11/12 mx-auto px-2 py-6 flex justify-center">
      <main className="flex-1 max-w-5xl">
        {title && <h2 className="text-2xl font-bold text-brand-text mb-6">{title}</h2>}
        {children}
      </main>
    </div>
  );
}
