import { SettingsSidebarNav } from '@/features/global/organisms';

export default function SettingsLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="w-full mx-auto px-4 py-4 md:py-6 flex justify-center">
      <div className="flex-1 min-w-0 max-w-5xl flex flex-col md:flex-row gap-4 md:gap-8">
        <SettingsSidebarNav />
        <main className="flex-1 min-w-0">
          {children}
        </main>
      </div>
    </div>
  );
}
