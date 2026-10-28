export interface LinkPreview {
  title: string;
  description?: string;
  imageUrl?: string;
  siteName?: string;
}

export interface LinkPreviewResponse extends Partial<LinkPreview> {
  url: string;
  // URL 自体が画像（Content-Type が image/*）の場合は true
  isImage?: boolean;
}
