'use client';

import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { ArticleList, CourseList } from '../organisms';
import { ArticleDto, CourseDto } from '@/lib/api/types';

interface HomeListTemplateProps {
  activeTab: 'articles' | 'courses';
  onTabChange: (tab: 'articles' | 'courses') => void;
  articles: ArticleDto[];
  courses: CourseDto[];
  isLoadingArticles: boolean;
  isLoadingCourses: boolean;
  articlesError: Error | null;
  coursesError: Error | null;
  onRetryArticles: () => void;
  onRetryCourses: () => void;
}

/**
 * HomeListTemplate - Template Component
 * 
 * ホーム画面のUI構成とレイアウトを担当
 * タブを使って記事と講座を切り替える
 */
export function HomeListTemplate({
  activeTab,
  onTabChange,
  articles,
  courses,
  isLoadingArticles,
  isLoadingCourses,
  articlesError,
  coursesError,
  onRetryArticles,
  onRetryCourses,
}: HomeListTemplateProps) {
  return (
    <Tabs 
      value={activeTab} 
      onValueChange={(value) => onTabChange(value as 'articles' | 'courses')}
      className="w-full"
    >
      <TabsList className="grid w-full max-w-md mx-auto mb-6 grid-cols-2">
        <TabsTrigger value="articles">新着記事</TabsTrigger>
        <TabsTrigger value="courses">講座一覧</TabsTrigger>
      </TabsList>
      
      <TabsContent value="articles">
        <ArticleList
          articles={articles}
          isLoading={isLoadingArticles}
          error={articlesError}
          onRetry={onRetryArticles}
        />
      </TabsContent>
      
      <TabsContent value="courses">
        <CourseList
          courses={courses}
          isLoading={isLoadingCourses}
          error={coursesError}
          onRetry={onRetryCourses}
        />
      </TabsContent>
    </Tabs>
  );
}
