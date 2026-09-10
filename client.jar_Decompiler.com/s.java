import vn.me.core.BaseCanvas;

public final class s extends gd {
   private boolean m;
   private gj a;
   private String[] a;

   public s(boolean var1) {
      int var2 = BaseCanvas.w - (gs.p << 1);
      int var3 = 90 + (gs.p << 2) + 6;
      this.a(gs.p, BaseCanvas.h - gs.m - var3 - this.y, var2, var3);
      this.e = cg.b;
      this.m = var1;
   }

   public final void a(gj var1, String var2) {
      this.a = var1;
      this.a = gv.a.a(var2, BaseCanvas.w - (gs.p << 1) - 50 - 5);
      this.a(this.a, false);
      this.a.b(10, 5 + gv.a.a());
   }

   public final void a() {
      super.a();
      if (this.m) {
         gv.a.a(BaseCanvas.g, gw.a(36), this.t >> 1, 5, 17);
      } else {
         gv.a.a(BaseCanvas.g, gw.a(37), this.t >> 1, 5, 17);
      }

      int var1 = 5 + gv.a.a() + 5;

      for(int var2 = 0; var2 < this.a.length; ++var2) {
         gv.a.a(BaseCanvas.g, this.a[var2], 50, var1 + (gv.a.a() + 2) * var2, 0);
      }

   }
}
