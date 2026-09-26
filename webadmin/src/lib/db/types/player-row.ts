/** Cột `gopettae_tae2.player` web dùng tới (bigint đọc dạng string). */
export interface PlayerRow {
  ID: number;
  user_id: number;
  isAdmin: number;
  name: string;
  gender: number;
  gold: string;
  coin: string;
  lua: string;
  star: number;
  pkPoint: number;
  clanId: number;
  avatarPath: string;
  AccumulatedPoint: number;
  EventPoint: number;
  loginDate: string;
  LastTimeOnline: string;
}
