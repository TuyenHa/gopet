import vn.me.core.BaseCanvas;

public final class bu extends gd {
   private boolean m = false;
   private int a;
   private int e;

   public final void a(int var1, int var2) {
      this.a = var1;
      this.e = var2;
      this.r = this.a - (this.t >> 1);
      this.s = this.e - this.u - 6;
      if (this.m && this.s < 0) {
         this.s = 0;
      }

      this.a(this.r, this.s, this.t, this.u);
   }

   public bu(int var1, int var2) {
      this.m = true;
      this.y = 0;
      this.d = 0;
      this.l = true;
      this.k = true;
      this.a = 0;
      this.e = 0;
      this.t = var1;
      this.u = var2;
      this.r = this.a - (this.t >> 1);
      this.s = this.e - this.u - 5;
      if (this.m && this.s < 0) {
         this.s = 0;
      }

      this.x = 3;
      this.a(this.r, this.s, this.t, this.u);
   }

   public final void e() {
      at.b(BaseCanvas.g, this.t, this.u - 5);
   }

   public final void a() {
      at.c(BaseCanvas.g, this.t, this.u - 5);
   }

   public final void d_() {
      super.d_();
      gx var1 = this.a;
      int var2 = this.u + 5;
      var1.b = var2;
      this.u = var2;
   }
}
