'use client';

import { useState } from 'react';
import { useGetArticles, useGetCourses } from '@/lib/api';
import { LoadingSpinner, ErrorMessage } from '@/components/ui';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { MainContentSection } from '@/features/global/organisms';
import { HomeArticleListTemplate, HomeCourseListTemplate } from '../templates/HomeListTemplates';

/**
 * HomeListPage - Page Component
 * 
 * ホーム画面の記事・講座一覧のロジックを管理するコンポーネント
 * データフェッチング、状態管理、ビジネスロジックを担当
 * タブで記事と講座を切り替え可能
 */
export function HomeListPage() {
  const [activeTab, setActiveTab] = useState<'articles' | 'courses'>('articles');
  
  const { 
    data: articlesData, 
    isLoading: isLoadingArticles, 
    error: articlesError, 
    refetch: refetchArticles 
  } = useGetArticles({ limit: 20, offset: 0 });
  
  const { 
    data: coursesData, 
    isLoading: isLoadingCourses, 
    error: coursesError, 
    refetch: refetchCourses 
  } = useGetCourses({ limit: 20, offset: 0 });

  // IDが存在しない記事をフィルタリング
  const articlesWithId = articlesData?.articles?.filter(
    (article) => article.id !== null && article.id !== undefined
  ) || [];
  
  // IDが存在しない講座をフィルタリング
  const coursesWithId = coursesData?.courses?.filter(
    (course) => course.id !== null && course.id !== undefined
  ) || [];

  return (
    <MainContentSection title="">
      <Tabs 
        value={activeTab} 
        onValueChange={(value) => setActiveTab(value as 'articles' | 'courses')}
        className="w-full"
      >
        <TabsList className="grid w-full max-w-md mx-auto mb-6 grid-cols-2">
          <TabsTrigger value="articles">新着記事</TabsTrigger>
          <TabsTrigger value="courses">講座一覧</TabsTrigger>
        </TabsList>
        
        <TabsContent value="articles">
          {isLoadingArticles ? (
            <LoadingSpinner text="記事を読み込み中..." />
          ) : articlesError ? (
            <ErrorMessage 
              message="記事の取得に失敗しました" 
              onRetry={refetchArticles}
            />
          ) : (
            <HomeArticleListTemplate articles={articlesWithId} />
          )}
        </TabsContent>
        
        <TabsContent value="courses">
          {isLoadingCourses ? (
            <LoadingSpinner text="講座を読み込み中..." />
          ) : coursesError ? (
            <ErrorMessage 
              message="講座の取得に失敗しました" 
              onRetry={refetchCourses}
            />
          ) : (
            <HomeCourseListTemplate courses={coursesWithId} />
          )}
        </TabsContent>
      </Tabs>
    </MainContentSection>
  );
}
