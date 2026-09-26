# Code review: Chợ trời (2026-09-26)

Status: DONE_WITH_CONCERNS. Build OK; packet byte layouts match between client and server; per-listing Sync lock blocks buy/cancel/expire races correctly.

## High
1. **Market saved faster than player data → crash dupe/loss.**
   - Market save is debounced to 10s; player save runs about every 10 minutes.
   - Listing: item removed from the bag, listing reaches the DB within 10s, crash → item is in both the bag and the market (dupe).
   - Buy: buyer is not saved → crash refunds the buyer, the item disappears, the seller keeps the coin.
   - Cancel/expire online: the returned item is lost on crash.
   - Fix: `playerData.save()` the involved players synchronously with every market mutation, then save the market immediately together with it.
2. **`market` table grows without bound.** `GopetManager.saveMarket` INSERTs the full JSON every save. Fix: prune old rows after the insert (keep the newest N) or upsert a single fixed row.
3. **Offline expire loses the item.** `Kiosk.Expire.cs:31-32` sets `hasRemoved` and removes the listing before the INSERT at `:54`; if the INSERT fails, the item is gone. The online path has the same problem if `addItemToInventory` throws. Fix: insert/return the item first, and only on success set the flag and remove; on failure, keep the listing for the next tick.
4. **NPC regression: stale `OBJKEY_COUNT_OF_ITEM_KIOSK`** (`MenuController.cs:1357-1361` + `Kiosk.List.cs:54`). Sell 5 potions (key = 5), then sell a hat → `count 5 > 1` → rejected. Fix: read the count only for `MENU_KIOSK_OHTER_SELECT` and remove the key after use.
5. **Non-stackable items with `count == 0` can't be sold** (e.g. a gem returned on unequip, `GameController.cs:4572`). Server rejects at `Kiosk.List.cs:54`; client `TryReadCount` yields 0 and the button is disabled with no reason shown. Fix: treat non-stackables as count = 1 on the server; `WriteSellableRow` sends `Math.Max(1, count)`.

## Medium
6. **Displayed price differs from the charged price for partly retail-sold listings.** Row sends `MathPrice` (`MarketPacketWriter.cs:25`) but `TryBuyWhole` charges `price - sumVal` (`Kiosk.Trade.cs:153`). Fix: send `Math.Max(0, price - sumVal)` and charge exactly that.
7. **`TryListItem` doesn't check item expiry** — a crafted packet or the NPC flow can list expired items. The partial-stack copy (`Kiosk.List.cs:75`) drops `expire` and `canTrade`. Fix: call `MarketItemCategory.BlockReason` inside `TryListItem`; copy `expire` and `canTrade` to the copy.
8. **Expire tick mutates player bags from the Runtime thread** (`Kiosk.Expire.cs:41`, and `PaySeller` calls `save`). Stack merging races with `subCountItem`, and there is a logout window between save and remove (`Player.cs:709-713`). Fix: marshal the return to the player thread, or re-save and re-check `isConnected`; if unsure, fall back to `kiosk_recovery`.
9. **Login `kiosk_recovery` race.** `Player.cs:532` DELETEs by `user_id` and can wipe a row the tick inserted after the SELECT at `:510`. Fix: delete only the ids that were SELECTed.
10. **`FlushMarketSaveIfDirty` clears the flag before saving** (`GopetManager.cs:1497`); a failed save is silently dropped. Fix: clear the flag only after a successful save.
11. **Client drops LIST_STATE when the server clamps the page** (`MarketPopupView.MarketTab.cs:76`). After buying the last item on the last page the list goes stale. Fix: compare only filter/sort, then adopt `state.Page`.
12. **Int overflow in "Thực nhận"** (`MarketSellDetailPane.cs:107`): `price * 95`. Fix: use `(long)`.
13. **A pet wearing equipment can be listed** (`TryListPet`), leaving the seller's items with a dangling `petEuipId`. Fix: reject when `pet.equip.Count > 0`.

## Low
14. **Assign name compare is case-sensitive.** `Kiosk.Assign.cs:39` (self check) and `:43` (change check, which re-charges the fee on a pure case change). Use `OrdinalIgnoreCase`.
15. **NPC `INPUT_ASSIGNED_NAME_KIOSK` doesn't validate the name** (exists / not self) but still charges the fee.
16. NPC list (`sendMenu.cs:829-851`) still shows assigned listings to everyone (pre-existing).
17. **`HandleSell` with an arbitrary `source` creates a junk inventory.** Whitelist {-1, 0, 1, 4}.
18. **Subs 47/53 have no rate limit.** Add a ~300ms cooldown per player.
19. **`removeSellItem` NRE** when `OBJKEY_TYPE_SHOW_KIOSK` is missing (pre-existing).
20. **`TryBuyWhole` holds `Sync` during DB I/O.** An offline `PaySeller` failure is not recorded or retried.
21. **Odd pixel widths:** `MarketFilterBar.cs:88` `chipWidth`; `MarketSellPopupView.cs:92` `ContentWidth * 0.44f`. Apply `Mathf.Floor`.
22. **`server_db.sql` not updated** (MEDIUMTEXT, drop `item_2`).
