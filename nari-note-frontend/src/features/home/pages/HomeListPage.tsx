'use client';

import { useState, useMemo } from 'react';
import { useGetArticles, useGetCourses } from '@/lib/api';
import { MainContentSection } from '@/features/global/organisms';
import { HomeListTemplate } from '../templates';

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

  // IDが存在しない記事をフィルタリング（メモ化して不要な再計算を防ぐ）
  const articlesWithId = useMemo(
    () => articlesData?.articles?.filter(
      (article) => article.id !== null && article.id !== undefined
    ) || [],
    [articlesData?.articles]
  );
  
  // IDが存在しない講座をフィルタリング（メモ化して不要な再計算を防ぐ）
  const coursesWithId = useMemo(
    () => coursesData?.courses?.filter(
      (course) => course.id !== null && course.id !== undefined
    ) || [],
    [coursesData?.courses]
  );

  return (
    <MainContentSection title="">
      <HomeListTemplate
        activeTab={activeTab}
        onTabChange={setActiveTab}
        articles={articlesWithId}
        courses={coursesWithId}
        isLoadingArticles={isLoadingArticles}
        isLoadingCourses={isLoadingCourses}
        articlesError={articlesError}
        coursesError={coursesError}
        onRetryArticles={refetchArticles}
        onRetryCourses={refetchCourses}
      />
    </MainContentSection>
  );
}
