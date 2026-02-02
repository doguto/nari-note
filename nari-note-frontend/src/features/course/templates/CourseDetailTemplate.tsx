'use client';

import { GetCourseContentResponse } from '@/lib/api/types';
import { CourseDetailHeader, CourseArticleList } from '../organisms';

interface CourseDetailTemplateProps {
  course: GetCourseContentResponse;
}

/**
 * CourseDetailTemplate - Template Component
 * 
 * 講座詳細ページのUI構成とレイアウトを担当
 * Organismを組み合わせてレスポンシブなUIを構築
 */
export function CourseDetailTemplate({ course }: CourseDetailTemplateProps) {
  return (
    <div className="max-w-4xl mx-auto">
      <CourseDetailHeader
        courseName={course.name || '無題の講座'}
        userId={course.userId || 0}
        userName={course.userName || '不明なユーザー'}
        likeCount={course.likeCount || 0}
        articleCount={course.articles?.length || 0}
        createdAt={course.createdAt}
      />
      
      <div className="bg-white rounded-lg shadow-lg p-6">
        <CourseArticleList articles={course.articles || []} />
      </div>
    </div>
  );
}
