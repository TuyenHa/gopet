import vn.me.core.BaseCanvas;

public final class gi extends gd {
   private String[] a;
   public ge a;
   private ge[] a;
   private gj[] a;
   private gj a;
   private int a;
   private boolean m;

   public gi(String var1, cd var2, cd var3, int var4) {
      super(8, BaseCanvas.h - gs.m - 69 - gs.p, BaseCanvas.w - 16, 69);
      this.a = 0;
      this.m = false;
      this.y = gs.p;
      this.m = true;
      this.a = new ge();
      this.a.a(0, this.u - gs.m - 2 * (this.y + this.x), this.t - 2 * (this.y + this.x), gs.m);
      this.a(this.a, false);
      this.d = var2;
      this.c = var3;
      this.a.a(var4);
      this.a.b("");
      this.a = gv.a.a(var1, this.t - 2 * (this.y + this.x));
      this.a.b = this.a.length * gv.a.a() + this.a.u + 2 * (this.y + this.x);
      if (this.a.b >= 70) {
         this.u = this.a.b;
      }

      this.c = this.a;
   }

   public final String a(int var1) {
      return this.a != null ? this.a.a() : this.a[var1].a();
   }

   public gi(String var1, String[] var2, int[] var3, cd var4, cd var5) {
      this(var1, var2, var3, var4, var5, (byte)0);
   }

   private gi(String var1, String[] var2, int[] var3, cd var4, cd var5, byte var6) {
      super(BaseCanvas.w / 10, 100, 4 * BaseCanvas.w / 5, 2 * gs.m * var2.length + gs.p + gs.l);
      this.a = 0;
      this.m = false;
      this.y = gs.p;
      this.a = var3.length;
      this.a = new ge[this.a];
      this.a = new gj[this.a];
      this.m = false;
      this.k = true;
      this.d = 2;
      if (var1 != null) {
         this.a = new gj(var1, gv.a);
         this.a.y = 0;
         this.a.a(0, 0, this.t, this.a.a.a() + 2);
         this.a.q = 17;
         this.a(this.a, false);
      }

      for(int var7 = 0; var7 < this.a; ++var7) {
         this.a[var7] = new ge(0, gs.m + this.y, this.t - (this.y + this.x << 1), gs.m);
         this.a[var7].b = var3[var7];
         this.a[var7] = new gj(var2[var7]);
         this.a[var7].y = 0;
         this.a[var7].a(0, this.y << 1, this.t - (this.y << 1), this.a[var7].a.a() + 2);
         this.a(this.a[var7], false);
         this.a(this.a[var7], false);
      }

      this.d = var4;
      this.c = var5;
      this.b(1);
      this.c = this.a[0];
      if (this.a.b < BaseCanvas.h - gs.l - gs.n) {
         this.u = this.a.b + (this.x + this.y << 1);
      } else {
         this.a.g = true;
         this.u = BaseCanvas.h - gs.l - gs.n - 10;
      }

      this.w = (BaseCanvas.h - this.u) / 2;
      if (this.a.b > this.u) {
         this.i = true;
      }

   }

   public final void b() {
      super.b();
      if (!this.m) {
         if (gs.a == 0) {
            gq.a(BaseCanvas.g, gs.a, -1, this.y, -this.B + gs.m - 6, (this.t >> 1) - this.y, 1, true);
            gq.a(BaseCanvas.g, -1, gs.a, (this.t >> 1) - this.y, -this.B + gs.m - 6, (this.t >> 1) - this.y, 1, true);
         } else {
            gq.a(BaseCanvas.g, 4945818, -1, this.y, -this.B + gs.m - 6, (this.t >> 1) - this.y, 1, true);
            gq.a(BaseCanvas.g, -1, 4945818, (this.t >> 1) - this.y, -this.B + gs.m - 6, (this.t >> 1) - this.y, 1, true);
         }
      } else {
         int var1 = 0;

         for(int var2 = (this.a.s >> 1) - (this.a.length * gv.a.a() >> 1); var1 < this.a.length; var2 += gv.a.a()) {
            gv.a.a(BaseCanvas.g, this.a[var1], (this.t >> 1) - this.y - this.x, var2, 17);
            ++var1;
         }

      }
   }
}
