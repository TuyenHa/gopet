namespace Gopet.Util;

public static class BoundedSelection
{
    // Preserve independent eligibility trials, followed by uniform selection from survivors.
    // A failed attempt consumes no currency; the caller handles a bounded failure explicitly.
    public static bool TryChoose<T>(IReadOnlyList<T> candidates, Func<T, bool> eligible,
        Func<int, int> nextIndex, out T choice, int attempts = 128)
    {
        var survivors = new List<T>(candidates.Count);
        for (int attempt = 0; attempt < attempts && candidates.Count > 0; attempt++)
        {
            survivors.Clear();
            foreach (var candidate in candidates) if (eligible(candidate)) survivors.Add(candidate);
            if (survivors.Count == 0) continue;
            choice = survivors[nextIndex(survivors.Count)];
            return true;
        }
        choice = default!;
        return false;
    }
}
