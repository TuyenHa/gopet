/** `gopettae_tae2.gift_code`. `gift_data` là JSON int[][], `usersOfUseThis` là JSON int[]. */
export interface GiftCodeRow {
  id: number;
  code: string;
  currentUser: number;
  maxUser: number;
  gift_data: string;
  expire: string;
  usersOfUseThis: string;
  isClanCode: number;
}
