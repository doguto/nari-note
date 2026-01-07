'use client';

import { ArticleForm } from '../components/ArticleForm';
import { useArticleForm } from '../hooks/useArticleForm';

export function ArticleFormContainer() {
  const {
    title,
    setTitle,
    body,
    setBody,
    tags,
    setTags,
    showPreview,
    togglePreview,
    handleSubmit,
    handleSaveDraft,
    isSubmitting,
  } = useArticleForm();

  return (
    <ArticleForm
      title={title}
      body={body}
      tags={tags}
      onTitleChange={setTitle}
      onBodyChange={setBody}
      onTagsChange={setTags}
      onSubmit={handleSubmit}
      onSaveDraft={handleSaveDraft}
      isSubmitting={isSubmitting}
      showPreview={showPreview}
      onTogglePreview={togglePreview}
    />
  );
}
