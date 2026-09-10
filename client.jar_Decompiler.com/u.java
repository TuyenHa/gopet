import java.util.Hashtable;
import java.util.Vector;

public final class u {
   private Vector a = new Vector();
   private Hashtable a = new Hashtable();

   public final void a(ee var1) {
      this.a.addElement(var1);
      this.a.put(new Integer(var1.c), var1);
   }

   public final ee a(int var1) {
      return (ee)this.a.get(new Integer(var1));
   }

   public final ee b(int var1) {
      return (ee)this.a.elementAt(var1);
   }

   public final int a() {
      return this.a.size();
   }

   public final void b(ee var1) {
      this.a.removeElement(var1);
      this.a.remove(new Integer(var1.c));
   }
}
