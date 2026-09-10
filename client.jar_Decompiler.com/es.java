import java.util.Hashtable;
import java.util.Vector;
import vn.me.core.BaseCanvas;

public final class es extends fw {
   private final int a;
   private final Vector a = new Vector();
   private final Hashtable a = new Hashtable();
   private final String a;
   private go a;
   private int b = 0;

   public es(int var1, String var2) {
      super(true);
      this.f = true;
      this.a = cp.c();
      this.a = var1;
      this.a = var2;
      this.c();
   }

   public final void c() {
      this.a.removeAllElements();
      this.a.clear();
      this.a = new go();
      this.a.a(15, 45, BaseCanvas.w - 30, BaseCanvas.h - gs.n - 3 - 45);
      this.a.i = true;
      this.a.k = true;
      this.b = 0;
   }

   public final void d() {
      this.a.d = 0;
      this.b.a(this.a);
      this.a.b(1);
      this.a.c(true);
      this.a.a(true, 0, 1);
   }

   public final bq a(boolean var1, String var2, int var3, boolean var4, int var5, int var6) {
      bq var7;
      (var7 = new bq(this)).a = 1;
      var7.a = var2;
      var7.b = (byte)var3;
      var7.a = var4;
      this.a.addElement(var7);
      var7.c(BaseCanvas.w - 30, 4);
      this.a.a(var7);
      var7.b(0, this.b);
      this.b += var7.u + 5;
      var7.g = var1;
      var7.a = var5;
      var7.b = var6;
      return var7;
   }

   public final void a(boolean var1, String var2, int var3, int var4) {
      bs var5;
      (var5 = new bs(this)).a = 0;
      var5.c = (byte)var4;
      var5.b = (byte)var3;
      var5.a = var2;
      var5.a = cp.a(var5.c).a(var2, BaseCanvas.w - 30);
      this.a.addElement(var5);
      var5.c(BaseCanvas.w - 30, var5.a.length * cp.a(var5.c).a() + 4);
      this.a.a(var5);
      var5.b(0, this.b);
      this.b += var5.u + 5;
      var5.g = var1;
   }

   public final void a(int var1, int var2, String var3, boolean var4, boolean var5) {
      bn var6;
      bn var7 = var6 = (bn)this.a.get(new Integer(var1));
      if (var6 == null) {
         var7 = new bn();
         this.a.put(new Integer(var1), var7);
      }

      bn.a(var7)[var2] = var4;
      bn.b(var7)[var2] = var5;
      if (var1 == -1) {
         switch (var2) {
            case 0:
               this.l = new cd(0, var3, new Integer(var1), this);
               return;
            case 1:
               this.m = new cd(1, var3, new Integer(var1), this);
               return;
            case 2:
               this.n = new cd(2, var3, new Integer(var1), this);
               return;
            default:
         }
      } else {
         bo var8 = (bo)this.a.elementAt(var1);
         switch (var2) {
            case 0:
               var8.c = new cd(0, var3, new Integer(var1), this);
               return;
            case 1:
               var8.d = new cd(1, var3, new Integer(var1), this);
               return;
            case 2:
               var8.e = new cd(2, var3, new Integer(var1), this);
               return;
            default:
         }
      }
   }

   public final void a(Object var1) {
      cd var7;
      Integer var2 = (Integer)(var7 = (cd)((Object[])var1)[0]).a;
      bn var3 = (bn)this.a.get(var2);
      int var4 = 0;
      boolean var5 = false;
      int var6 = var7.a;
      if (var3 != null) {
         var4 = bn.a(var3)[var6];
         var5 = bn.b(var3)[var6];
      }

      if (var4) {
         for(int var10 = 0; var10 < this.a.size(); ++var10) {
            bo var12;
            if ((var12 = (bo)this.a.elementAt(var10)).a == 1) {
               dj.a.a(((bq)var12).a);
            }
         }

         this.t();
      }

      if (var5) {
         int var11 = var7.a;
         var4 = this.a;
         int var8 = var2;
         en var9;
         (var9 = new en(81)).a(100);
         var9.b(var4);
         var9.b(var8);
         var9.b(var11);
         cx.a.a(var9);
         var9.a();
         cg.f();
      }

   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
      dj.a.a();
      gs.a(10, 20, BaseCanvas.w - 20, 22);
      cp.c().a(BaseCanvas.g, this.a, BaseCanvas.Field157, 22, 17);
      gs.a(10, 42, BaseCanvas.w - 20, BaseCanvas.h - gs.n - 42);
   }

   public static go a(es var0) {
      return var0.a;
   }
}
