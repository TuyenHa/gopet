import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class ca extends fw {
   private int a = 0;
   private int[] a = new int[3];
   private int[] b = new int[3];
   private String[] a = new String[3];
   private String[] b = new String[3];
   private String[] c = new String[3];
   private String[] d;
   private go a = new go();
   private int b;
   private int c;
   private Image a;

   public ca(int var1) {
      super(true);
      this.f = true;
      this.a = cp.c();
      this.n = cg.e;
      this.b = var1;
      this.b.a(this.a);
      this.a.a(74, 42, BaseCanvas.w - 64 - 20, 100);
      this.a.d(100, 100);
      this.a.i = true;
      this.a.b(1);
   }

   public final void a(int[] var1, int[] var2, String[] var3, String[] var4, String[] var5) {
      this.a = var1;
      this.b = var2;
      this.a = var3;
      this.b = var4;
      this.c = var5;
      this.a.o();

      for(int var6 = 0; var6 < 3; ++var6) {
         cb var7;
         (var7 = new cb(this, var6)).c(BaseCanvas.w - 85 - 17, 20);
         var7.b(10, 5 + (var6 << 5));
         this.a.a(var7);
         if (this.b == 1) {
            var4 = "";
            byte var9 = 0;
            switch (var1[var6]) {
               case -1:
                  var9 = 11;
                  var4 = gw.a(49);
                  break;
               case 0:
                  var9 = 12;
                  var4 = gw.a(50);
                  break;
               case 1:
                  var9 = 13;
                  var4 = gw.a(51);
            }

            var7.d = new cd(var9, var4, this);
         }
      }

      this.a((gn)this.a.a(0));
   }

   public final void a(int var1) {
      this.a = var1;
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
      dj.a.a();
      cp.d().a(BaseCanvas.g, dj.d, 17, 22, 0);
      BaseCanvas.g.drawRegion(cp.e, dp.a * 14, 0, 14, 14, 0, 2, 20, 0);
      cp.d().a(BaseCanvas.g, "(chien)" + String.valueOf(this.a), BaseCanvas.Field159, 22, 0);
      gs.a(10, 42, 64, 100);
      gs.a(74, 42, BaseCanvas.w - 20 - 64, 100);
      ee var1;
      if ((var1 = dv.a).d) {
         v.a((byte)(var1.a == 0 ? 1 : 0), 45, 132, 1, false, 0);
      } else {
         Image var2;
         if ((var2 = dj.a.b(var1.d)) == null) {
            v.a((byte)(var1.a == 0 ? 1 : 0), 45, 132, 1, false, 0);
         } else {
            BaseCanvas.g.drawImage(v.a, 20, 20, 17);
            byte var10000 = var1.a;
            v.a(var2, 45, 132, 1, false);
         }
      }

      gs.a(10, 142, BaseCanvas.w - 20, BaseCanvas.h - 142 - 5 - gs.m);
      int var4 = 150;
      if (this.d != null) {
         for(int var3 = 0; var3 < this.d.length; ++var3) {
            gv.a.a(BaseCanvas.g, this.d[var3], 16, var4, 0);
            var4 += gv.a.a();
         }
      }

   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 11:
            cg.f();
            en var2;
            (var2 = new en(81)).a(91);
            var2.a(25);
            cx.a.a(var2);
            var2.a();
            return;
         case 12:
            cg.f();
            dc.g(this.b[this.c]);
            return;
         case 13:
            cg.a(gw.a(48), new cd(14, gw.a(47), this), cg.b);
            return;
         case 14:
            cg.f();
            dc.g(this.b[this.c]);
            return;
         default:
      }
   }

   public static int[] a(ca var0) {
      return var0.a;
   }

   public static Image a(ca var0) {
      return var0.a;
   }

   public static Image a(ca var0, Image var1) {
      var0.a = var1;
      return var1;
   }

   public static String[] a(ca var0) {
      return var0.a;
   }

   public static String[] b(ca var0) {
      return var0.c;
   }

   public static int a(ca var0, int var1) {
      var0.c = var1;
      return var1;
   }

   public static String[] a(ca var0, String[] var1) {
      var0.d = var1;
      return var1;
   }

   public static String[] c(ca var0) {
      return var0.b;
   }
}
