import java.util.Hashtable;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class dv implements gz {
   public static ee a;
   public static ew a;
   public Image[] a;
   public Image a;
   private final Hashtable a = new Hashtable();

   public final e a(int var1) {
      return (e)this.a.get(new Integer(var1));
   }

   public static ee a(int var0) {
      ee var1 = null;
      if (a != null) {
         var1 = a.a.a(var0);
      }

      return var1;
   }

   public static void a(int var0) {
      ee var1;
      if (a != null && (var1 = a(var0)) != null && var1.b) {
         a.a(var1);
         var1.a(a);
      }
   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 101:
            (new fk()).a(1, true);
            return;
         case 900:
            ee var2;
            int var10000 = (var2 = a.a).c;
            String var3 = var2.e;
            ed.a(var2.b, 0);
            a.n();
            return;
         case 1103:
            cg.f();
            cx.c(a.a);
            return;
         case 1204:
            a(a.a, new gn[]{new gj(a.a(574)), new gb(new cd(1202, a.a(134), this)), new gb(new cd(1203, a.a(471), this))});
            return;
         case 2020:
            (new fk()).a(1, true);
            return;
         default:
      }
   }

   public static void a(int var0, int var1, int var2) {
      cg.f();
      cx.a(var0, var1, var2);
   }

   public static void a() {
      if (a != null && a.a != null) {
         a.a.a((Object)null);
      }

      (new fx(1)).d(0);

      try {
         Thread.sleep(50L);
      } catch (InterruptedException var0) {
      }

      b();
   }

   public static ew a(ew var0, ef var1, int var2, int var3, int var4) {
      var0.a = var2;
      var0.a(var1);

      for(int var5 = 0; var5 < 4; ++var5) {
         x var6;
         (var6 = new x()).i = ed.b(var0.a.c);
         var6.j = ed.b(var0.a.d);
         var6.c = var6.i;
         var6.d = var6.j;
         var6.a = var0.a.c;
         var6.b = var0.a.d;
         var0.a((eh)var6);
         var0.c(var6);
      }

      var0.a(a, var3, var4, true);
      var0.a(a.a() ? 0 : 1);
      var0.a.b = var0.a.a;
      var0.b.b = var0.b.a;
      var0.f();
      return var0;
   }

   public static void a(eh var0, gn[] var1) {
      var1 = cg.a((gn[])var1);
      if (var0 instanceof ee) {
         var1.a(var0.i - a.a.b, var0.j - 54 - a.b.b);
      } else {
         var1.a(var0.i - a.a.b, var0.j - a.b.b);
      }

      var1.a(true, -1, 1);
      BaseCanvas.currentScreen.a(var1, false);
   }

   public static void b() {
      if (dj.a != null) {
         dj.a.a.clear();
      }

      cp.d();
      cp.c();
      a = null;
      System.gc();
   }
}
