import type { Metadata } from "next";
import { CourseDetailPage } from "@/features/course/pages";
import { getCourseContent } from "@/lib/api/server";

interface CoursePageProps {
  params: Promise<{ id: string }>;
}

export async function generateMetadata({
  params,
}: CoursePageProps): Promise<Metadata> {
  const { id } = await params;
  const courseId = Number(id);

  try {
    const course = await getCourseContent({ id: courseId });
    const description = `${course.userName} による将棋コース。${course.articles.length}本の記事を収録。`;

    return {
      title: course.name,
      description,
      openGraph: {
        title: course.name,
        description,
        type: "article",
        publishedTime: course.publishedAt,
      },
      twitter: {
        card: "summary_large_image",
        title: course.name,
        description,
      },
    };
  } catch {
    return {
      title: "コース",
    };
  }
}

export default async function CourseDetailPageRoute({
  params,
}: CoursePageProps) {
  const { id } = await params;
  const courseId = Number(id);

  return <CourseDetailPage courseId={courseId} />;
}
