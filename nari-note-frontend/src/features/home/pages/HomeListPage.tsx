'use client';

import { useState, useMemo } from 'react';
import { useGetArticles, useGetCourses } from '@/lib/api';
import { MainContentSection } from '@/features/global/organisms';
import { HomeListTemplate } from '../templates';

/**
 * HomeListPage - Page Component
 * 
 * ホーム画面のビジネスロジックを担当するページコンポーネント
 * データフェッチング、状態管理、イベントハンドリングを行い、Templateにpropsを渡す
 */
export function HomeListPage() {
  const [activeTab, setActiveTab] = useState<'articles' | 'courses'>('articles');
  
  // クライアントサイドでデータをフェッチ
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

  // IDが存在する記事と講座のみをフィルタリング（メモ化して不要な再計算を防ぐ）
  const articlesWithId = useMemo(
    () => articlesData?.articles?.filter(
      (article) => article.id !== null && article.id !== undefined
    ) || [],
    [articlesData?.articles]
  );
  
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
