using Gopet.Util;
using Gopet.Data.Mob;
using Gopet.Data.Collections;

static class GameplayTests
{
    public static void Search()
    {
        var source = new SinglePass();
        if (source.BinarySearch(7)?.Id != 7) throw new Exception("lookup failed");
        var list = new CopyOnWriteArrayList<Entry>(new Entry(1), new Entry(3), new Entry(7));
        if (list.BinarySearch(3)?.Id != 3 || list.BinarySearch(2) != null) throw new Exception("snapshot lookup wrong");
        var clone = list.clone(); clone.Remove(clone[0]);
        if (list.Count != 3 || clone.Count != 2) throw new Exception("clone mutations leaked");
    }
    public static void Deadline()
    {
        var mob = new Mob { hp = 0 };
        var now = new DateTime(2026, 9, 24);
        if (mob.IsRemovalDue(now)) throw new Exception("dead mob removed immediately");
        for (int i=1; i<10; i++) if (mob.IsRemovalDue(now.AddMilliseconds(500*i))) throw new Exception("removed early");
        if (!mob.IsRemovalDue(now.AddSeconds(5))) throw new Exception("deadline moved with each tick");
    }
    public static void Selection()
    {
        int calls = 0;
        if (BoundedSelection.TryChoose(new[] {1,2}, _ => { calls++; return false; }, _ => 0, out int _, 4))
            throw new Exception("selected an ineligible gift");
        if (calls != 8) throw new Exception("selection did not obey attempt budget");
        if (!BoundedSelection.TryChoose(new[] {1,2,3}, x => x != 2, count => count-1, out int selected) || selected != 3)
            throw new Exception("eligible candidates were not selected uniformly by index");
    }
    sealed class Entry(int id) : IBinaryObject<Entry>
    {
        public int Id = id;
        public int GetId() => Id;
        public void SetId(int value) => Id = value;
        public Entry Instance => this;
    }
    sealed class SinglePass : IEnumerable<IBinaryObject<Entry>>
    {
        int enumerations;
        public IEnumerator<IBinaryObject<Entry>> GetEnumerator()
        {
            if (++enumerations > 1) throw new Exception("lookup enumerated source repeatedly");
            return new IBinaryObject<Entry>[] {new Entry(1),new Entry(3),new Entry(7)}.AsEnumerable().GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
