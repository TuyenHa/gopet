import vn.me.core.BaseCanvas;

public final class aq extends go {
   public gz a;
   public gc a;
   public go a;

   public aq(int var1, int var2) {
      this(0, 0, var1, var2, gs.m);
   }

   private aq(int var1, int var2, int var3, int var4, int var5) {
      super(0, 0, var3, var4);
      this.h = false;
      this.i = false;
      this.a = new gc(var3, var5);
      this.a = new go(0, var5 + 2, var3, var4 - var5 - 2);
      this.a(this.a, false);
      this.a(this.a, false);
      this.a.a = new ar(this);
      this.a.h = true;
   }

   public aq() {
      this(BaseCanvas.w, BaseCanvas.h);
   }

   public final void a(String var1, gn var2) {
      gg var3 = gv.b;
      int var4 = 0;
      if (this.a.b() > 0) {
         au var5;
         var4 = (var5 = (au)this.a.a[this.a.a.length - 1]).r + var5.t;
      }

      au var6;
      (var6 = new au(var1)).a = var3;
      var6.a(var4, 0, Math.max(var6.a.a(var1), var6.b.a(var1)) + (var6.y << 1), gs.m);
      this.c(0);
      if (var2 != null) {
         this.a.a(var6, false);
         var2.a(0, 0, this.a.t - (this.a.y << 1), this.a.u - (this.a.y << 1));
         this.a.a(var2, false);
         this.a.c = this.a.b() + 1;
         gx var10000 = this.a.a;
         var10000.a += var6.t;
         var2.f = false;
         var6.b = new as(this, var6);
      }

      this.h();
   }

   private void c(int var1) {
      if (var1 < 0 || var1 > this.a.b()) {
         throw new IndexOutOfBoundsException("Index: " + var1);
      }
   }

   public final int a() {
      return this.a != null ? this.a.a : -1;
   }

   public final void f() {
      this.c(0);
      this.a.a(0);
   }

   public final void a(int var1) {
      this.c(var1);
      BaseCanvas.getCurrentScreen().a(this.a.a(var1));
   }

   public final void e() {
      BaseCanvas.g.setColor(6990585);
      BaseCanvas.g.drawRect(0, this.a.u, this.a.t - 1, 1);
   }

   public final void g() {
      super.g();
      BaseCanvas.getCurrentScreen().a((gn)this.a);
   }

   public final void h() {
      if (this.a.l) {
         int var5 = 0;

         for(int var6 = 0; var6 < this.a.a.length; ++var6) {
            gn var7 = this.a.a[var6];
            this.a.a[var6].r = var5;
            var7.v = var5;
            var5 += this.a.a[var6].t;
         }

      } else {
         int var1 = 0;

         for(int var2 = 0; var2 < this.a.a.length; ++var2) {
            this.a.a[var2].t = (this.a.t - 2 * (this.x + this.y)) / this.a.a.length;
            gn var3 = this.a.a[var2];
            this.a.a[var2].r = var1;
            var3.v = var1;
            var1 += this.a.a[var2].t;
         }

      }
   }
}
