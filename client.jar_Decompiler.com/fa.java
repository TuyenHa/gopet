import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class fa extends fw {
   private static int a = 40;
   private static int b = 80;
   private int c;
   private String a;
   private byte a;
   private String b;
   private String[] a;
   private String e;
   private Image a;
   private int d;
   private long a;
   private boolean a;
   private int e = 2;

   public fa() {
      super(true);
      this.f = true;
      this.a = cp.c();
      this.n = cg.e;
   }

   public final void c() {
      this.a = null;
      this.a = false;
      this.m = new cd(2, gw.a(62), this);
   }

   public final void a(int var1, String var2, String var3, String var4, int var5, byte var6) {
      this.a = true;
      this.c = var1;
      this.a = var2;
      this.b = var3;
      this.a = gv.a.a(var3, BaseCanvas.w - 15 - a - 5 - 15);
      this.e = var4;
      this.d = var5;
      this.a = System.currentTimeMillis();
      this.e = var6;
      this.m = new cd(1, gw.a(63), this);
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
      dj.a.a();
      gs.a(10, 20, BaseCanvas.w - 20, 22);
      cp.c().a(BaseCanvas.g, gw.a(65), BaseCanvas.Field157, 22, 17);
      gs.a(10, 42, BaseCanvas.w - 20, BaseCanvas.h - gs.n - 42);
      if (!this.a) {
         gv.a.a(BaseCanvas.g, gw.a(66), 15, 47, 0);
      } else {
         int var1 = 52;
         if (this.a == null) {
            Image var2;
            if ((var2 = dj.a.a(this.a)) != null) {
               this.a = gq.a(var2, var2.getHeight() << 1);
               a = this.a.getWidth();
               b = this.a.getHeight();
               if (this.a == 4) {
                  a /= this.e;
               }

               this.a = gv.a.a(this.b, BaseCanvas.w - 15 - a - 5 - 15);
            }
         } else if (this.a == 4) {
            dp var10000 = dj.a;
            dp.a(this.a, 20, 52, 0, this.e);
         } else {
            BaseCanvas.g.setColor(0);
            BaseCanvas.g.fillRect(18, 50, a + 4, b + 4);
            BaseCanvas.g.drawImage(this.a, 20, 52, 0);
         }

         int var5 = gv.a.a() + 2;

         for(int var3 = 0; var3 < this.a.length; ++var3) {
            gv.a.a(BaseCanvas.g, this.a[var3], 20 + a + 10, var1, 0);
            var1 += var5;
         }

         gv.a.a(BaseCanvas.g, this.e, 20 + a + 10, var1, 0);
         int var6 = (int)((long)this.d - (System.currentTimeMillis() - this.a) / 1000L);
         var1 += var5 << 1;
         gv.a.a(BaseCanvas.g, gw.a(67), BaseCanvas.w >> 1, var1, 17);
         gv.a.a(BaseCanvas.g, ed.c((long)var6), BaseCanvas.w >> 1, var1 + var5, 17);
      }
   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 1:
            Vector var5;
            (var5 = new Vector()).addElement(new cd(20, gw.a(64), this));
            this.a(var5, 2);
            return;
         case 2:
            cg.f();
            byte var4 = this.a;
            en var6;
            (var6 = new en(81)).a(85);
            var6.a(var4);
            cx.a.a(var6);
            var6.a();
            return;
         case 20:
            cg.f();
            int var3 = this.c;
            en var2;
            (var2 = new en(81)).a(87);
            var2.b(var3);
            cx.a.a(var2);
            var2.a();
            return;
         case 21:
         default:
      }
   }

   public final void a(byte var1) {
      this.a = var1;
   }
}
