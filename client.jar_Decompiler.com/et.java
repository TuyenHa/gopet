import java.util.Vector;
import vn.me.core.BaseCanvas;

public final class et extends fw {
   private final Vector a;
   private final gg b;
   private int a;

   public et(Vector var1, int var2) {
      super(true);
      this.b = gv.a;
      this.d = gw.a(26);
      this.a = var1;
      go var6;
      (var6 = new go(0, gs.l, BaseCanvas.w, BaseCanvas.h - 2 * gs.l)).i = true;
      var6.k = true;
      int var3 = 0;

      for(int var4 = 0; var4 < this.a.size(); ++var4) {
         ae var5 = (ae)this.a.elementAt(var4);
         bv var7;
         (var7 = new bv(this, var5.a, var5.a, var4)).b(2, var3);
         var6.a(var7);
         var3 += var7.u;
      }

      this.b.a(var6);
      var6.b(1);
      var6.c(true);
      this.n = cg.e;
      this.m = new cd(0, gw.a(25), new eu(this));
      this.a = var2;
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
   }

   public static gg a(et var0) {
      return var0.b;
   }

   public static int a(et var0) {
      return var0.a;
   }
}
