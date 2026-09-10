import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class cc extends gd implements gz {
   private int a;
   private String a;
   private int e;
   private int f;
   private long a;
   private int g;
   private Image a;

   public cc(int var1, String var2, int var3, int var4, int var5) {
      this.a = var1;
      this.a = var2;
      this.e = var4;
      this.f = var5;
      this.t = BaseCanvas.w - (gs.p << 1);
      if (this.u > 0) {
         this.u = var3 + (this.y + this.x << 1) + 10;
      } else {
         this.u = 150;
      }

      this.a(gs.p, BaseCanvas.h - gs.m - this.u - this.y, this.t, this.u);
      this.d = new cd(0, gw.a(6), this);
      this.e = new cd(1, gw.a(2), this);
   }

   public final void c() {
      super.c();
      long var1;
      if ((var1 = System.currentTimeMillis()) - this.a >= (long)this.f) {
         this.g = (this.g + 1) % this.e;
         this.a = var1;
      }

   }

   public final void a() {
      super.a();
      if (this.a == null) {
         this.a = dj.a.a(this.a);
      } else {
         BaseCanvas.g.translate(this.t >> 1, this.u >> 1);
         int var1 = this.a.getWidth() / this.e;
         BaseCanvas.g.drawRegion(this.a, var1 * this.g, 0, var1, this.a.getHeight(), 0, 0, 0, 3);
         BaseCanvas.g.translate(-(this.t >> 1), -(this.u >> 1));
      }
   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 0:
            en var2;
            (var2 = new en(122)).a(11);
            var2.b(this.a);
            cx.a.a(var2);
            var2.a();
            return;
         case 1:
            dj.a.a(this.a);
            this.j();
            return;
         default:
      }
   }
}
