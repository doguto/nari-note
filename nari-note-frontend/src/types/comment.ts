/**
 * Comment type definitions
 */

export interface Comment {
  id: number;
  userId: string;
  userName: string;
  userIconImageUrl?: string;
  message: string;
  createdAt: string;
}
