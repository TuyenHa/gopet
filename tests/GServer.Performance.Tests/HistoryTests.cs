using Gopet.Manager;

static class HistoryTests
{
    public static void ByteBoundedBatch()
    {
        string root = Path.Combine(Path.GetTempPath(), "gopet-history-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var sizes = new List<int>();
            var queue = new HistoryDeliveryQueue(root, batch => sizes.Add(batch.Count), maxBatchBytes: 1400);
            for (int i=0; i<3; i++) queue.Add(new HistoryRecord(Guid.NewGuid().ToString("N"), i, new string('x',1000), "null", "test"));
            for (int i=0; i<3; i++) if (!queue.DeliverBatch()) throw new Exception("bounded batch failed");
            if (!sizes.SequenceEqual(new[] {1,1,1}) || queue.PendingCount != 0)
                throw new Exception("batch exceeded byte budget or failed to drain");
        }
        finally { Directory.Delete(root, true); }
    }
    public static void SplitRejectedBatch()
    {
        string root = Path.Combine(Path.GetTempPath(), "gopet-history-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            int delivered = 0;
            var queue = new HistoryDeliveryQueue(root, batch => {
                if (batch.Count > 1) throw new HistoryBatchTooLargeException("packet limit");
                delivered++;
            });
            for (int i=0; i<3; i++) queue.Add(new HistoryRecord(Guid.NewGuid().ToString("N"), i, "event", "null", "test"));
            if (!queue.DeliverBatch() || delivered != 3 || queue.PendingCount != 0)
                throw new Exception("size rejection did not split and acknowledge smaller batches");
        }
        finally { Directory.Delete(root, true); }
    }
    public static void Run()
    {
        string root = Path.Combine(Path.GetTempPath(), "gopet-history-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            int attempts = 0;
            var delivered = new HashSet<string>();
            var queue = new HistoryDeliveryQueue(root, records => {
                attempts++;
                foreach (var record in records) delivered.Add(record.EventId);
                if (attempts == 1) throw new IOException("acknowledgement lost after commit");
            });
            var entry = new HistoryRecord(Guid.NewGuid().ToString("N"), 7, "trade", "{\"gold\":5}", "test");
            queue.Add(entry);
            if (queue.DeliverBatch()) throw new Exception("delivery failure reported as success");
            if (queue.PendingCount != 1) throw new Exception("failed record was lost");
            // A restart must reuse the same idempotency key and data.
            var restarted = new HistoryDeliveryQueue(root, records => {
                if (records[0] != entry) throw new Exception("persisted snapshot changed");
                foreach (var record in records) delivered.Add(record.EventId);
            });
            if (!restarted.DeliverBatch() || restarted.PendingCount != 0 || delivered.Count != 1)
                throw new Exception("recovery did not drain exactly one logical event");
            var small = new HistoryDeliveryQueue(root, _ => {}, maxBytes: 1);
            try { small.Add(entry); throw new Exception("accepted record beyond disk budget"); }
            catch (IOException) { }
        }
        finally { Directory.Delete(root, true); }
    }
}
