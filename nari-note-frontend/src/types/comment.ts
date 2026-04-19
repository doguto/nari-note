/**
 * Comment type definitions
 */

export interface Comment {
  id: number;
  userId: string;
  userName: string;
  userProfileImage?: string;
  message: string;
  createdAt: string;
}
