/** `gopettae_tae2.letter` — hàng đợi thư, KHÔNG có PK. targetId = player.user_id. */
export interface LetterRow {
  userId: number;
  targetId: number;
  time: string;
  Type: number; // 1 bạn bè, 2 admin, 3 sự kiện
  Title: string;
  ShortContent: string;
  Content: string;
}
