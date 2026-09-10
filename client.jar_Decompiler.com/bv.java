import java.util.Vector;
import vn.me.core.BaseCanvas;

public final class bv extends gb {
   private final String a;
   private final String[] a;
   private int a;
   private final et a;

   public bv(et var1, String var2, String[] var3, int var4) {
      this.a = var1;
      this.a = var2;
      this.a = var4;
      Vector var9 = new Vector();

      for(int var5 = 0; var5 < var3.length; ++var5) {
         String var6 = var3[var5];
         String[] var11;
         (var11 = et.a(var1).a(var6, BaseCanvas.w - 2 - 60))[0] = "- " + var11[0];

         for(int var7 = 0; var7 < var11.length; ++var7) {
            String var8 = var11[var7];
            var9.addElement(var8);
         }
      }

      this.a = new String[var9.size()];

      for(int var10 = 0; var10 < var9.size(); ++var10) {
         this.a[var10] = (String)var9.elementAt(var10);
      }

      this.c(BaseCanvas.w - 4, this.a.length * (et.a(var1).a() + 4) + 4);
      if (var4 == et.a(var1)) {
         this.b = true;
      }

   }

   public final void b() {
      gs.a(0, 0, 50, 20);
      et.a(this.a).a(BaseCanvas.g, this.a, 2, 2, 0);
      int var1 = 0;

      for(int var2 = 0; var2 < this.a.length; ++var2) {
         et.a(this.a).a(BaseCanvas.g, this.a[var2], 60, var1, 0);
         var1 += et.a(this.a).a() + 4;
      }

   }

   public final void a() {
      if (this.a == et.a(this.a)) {
         gq.a(BaseCanvas.g, gs.j, gs.i, 1, 1, this.t - 2, this.u - 2, false);
      } else {
         BaseCanvas.g.setColor(0);
         BaseCanvas.g.fillRect(0, 0, this.t, this.u);
      }
   }
}
