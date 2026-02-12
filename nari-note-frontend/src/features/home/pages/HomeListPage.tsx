'use client';

import { useState } from 'react';
import { MainContentSection } from '@/features/global/organisms';
import { HomeListTemplate } from '../templates';
import type { ArticleDto, CourseDto } from '@/lib/api/types';

interface HomeListPageProps {
  articles: ArticleDto[];
  courses: CourseDto[];
}

export function HomeListPage({ articles, courses }: HomeListPageProps) {
  const [activeTab, setActiveTab] = useState<'articles' | 'courses'>('articles');

  return (
    <MainContentSection title="">
      <HomeListTemplate
        activeTab={activeTab}
        onTabChange={setActiveTab}
        articles={articles}
        courses={courses}
      />
    </MainContentSection>
  );
}
