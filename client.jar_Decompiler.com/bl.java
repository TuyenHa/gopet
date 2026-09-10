import vn.me.core.BaseCanvas;

public final class bl extends gd {
   public bl() {
      super(BaseCanvas.w - 171 >> 1, BaseCanvas.h - 64 - gs.m - 10, 171, 64);
      this.e = cg.b;
   }

   public final void a(int[] var1, String[] var2, String[] var3, int[] var4, cd[] var5) {
      for(int var6 = 0; var6 < var2.length; ++var6) {
         p var7;
         (var7 = new p(var1[var6], var2[var6], var3[var6], var4[var6])).c(this.t - 8, 20);
         var7.b(17, var6 * 20);
         this.a(var7, false);
         var7.d = var5[var6];
      }

   }

   public final void a() {
      int var10000 = BaseCanvas.w;
      var10000 = BaseCanvas.h;
      BaseCanvas.g.setColor(16579281);
      BaseCanvas.g.fillRect(0, 0, this.t, this.u);
      BaseCanvas.g.setColor(3872520);
      BaseCanvas.g.drawRect(0, 0, this.t - 1, this.u - 1);
   }
}
