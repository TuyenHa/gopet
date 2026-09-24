-- database: gp_log
-- Apply to the LOG database before deploying the performance changes.
-- Existing rows remain NULL; all new queued events supply a unique ID.
ALTER TABLE `history`
  ADD COLUMN IF NOT EXISTS `eventId` CHAR(32) CHARACTER SET ascii COLLATE ascii_bin NULL;
CREATE UNIQUE INDEX IF NOT EXISTS `ux_history_eventId` ON `history` (`eventId`);
