import vn.me.core.BaseCanvas;

public final class gm extends gd {
   public int a = 0;
   private String[] a;
   private int e;
   private int f;
   private int g;
   private byte a = 0;

   public gm(String var1, cd var2, cd var3, cd var4, int var5) {
      this.y = gs.p;
      this.c = var2 == null ? b : var2;
      this.d = var3 == null ? b : var3;
      this.e = var4 == null ? b : var4;
      this.a = var5;
      if (var5 == 2) {
         this.f = gv.a.a >> 2;
         this.g = gv.a.b;
      } else if (var5 == 1) {
         this.c = true;
      }

      this.t = BaseCanvas.w - (gs.p << 1);
      this.a.a = this.t - 2 * this.y;
      this.a = gv.a.a(var1, this.t - (var5 == 2 ? this.f + this.y + this.x : 2 * (this.y + this.x)));
      this.u = gv.a.a() * this.a.length + (this.y + this.x << 1);
      if (this.u < 60) {
         this.u = 60;
      }

      this.a(gs.p, BaseCanvas.h - gs.m - this.u - this.y, this.t, this.u);
      this.e = (this.u >> 1) - this.a.length * gv.a.a() / 2 - this.y - this.x;
   }

   public final void a() {
      if (this.a != 1) {
         super.a();
      } else {
         BaseCanvas.g.setColor(gs.e);
         BaseCanvas.g.fillRect(2, 2, this.t - 4, this.u - 4);
      }
   }

   public final void b() {
      super.b();
      if (this.a == 2) {
         gv.a.a(BaseCanvas.g, this.a, this.y, (this.u >> 1) - (this.g >> 1) - this.y, 0, 0);
      }

      int var1 = 0;

      for(int var2 = this.e; var1 < this.a.length; var2 += gv.a.a()) {
         gv.a.a(BaseCanvas.g, this.a[var1], (this.t + (this.a == 2 ? this.f + this.y : 0) >> 1) - (this.y << 1), var2, 17);
         ++var1;
      }

   }

   public final void c() {
      super.c();
      if (this.a == 2 && BaseCanvas.ticks % 2 == 0) {
         ++this.a;
         if (this.a == gv.a.c) {
            this.a = 0;
         }
      }

   }
}
