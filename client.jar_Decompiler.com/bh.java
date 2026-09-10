import vn.me.core.BaseCanvas;

final class bh extends eh {
   private byte a;
   private final fr a;

   public bh(fr var1, int var2) {
      this.a = var1;
      this.a = (byte)var2;
   }

   public final void a(int var1, int var2) {
      switch (this.a) {
         case 0:
            this.a.a.a(BaseCanvas.g, this.i - var1, this.j - var2 - 31);
            return;
         case 1:
            this.a.b.a(BaseCanvas.g, this.i - var1, this.j - var2 - 11);
            return;
         case 2:
            this.a.c.a(BaseCanvas.g, this.i - var1, this.j - var2 - 31);
            return;
         default:
      }
   }
}
