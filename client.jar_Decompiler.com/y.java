import vn.me.core.BaseCanvas;

public final class y extends eh {
   private byte b;
   public int a;
   public byte a;
   dz[] a;

   public y(int var1, byte var2) {
      this.b = var2;
      this.a = var1;
   }

   public final void b(int var1, int var2) {
      switch (this.b) {
         case 0:
            if (this.b != null) {
               BaseCanvas.g.drawImage(this.b, var1 - (this.b.getWidth() >> 1), var2 - this.b.getHeight(), 0);
               return;
            }

            return;
         case 1:
            if (this.a != null) {
               this.a[this.a].a(BaseCanvas.g, var1, var2);
               return;
            }

            return;
         default:
      }
   }

   public final void a(int var1, int var2) {
      this.b(this.i - var1, this.j - var2);
   }

   public final gy a() {
      this.a.a = this.k + this.i;
      this.a.b = this.l + this.j;
      return this.a;
   }
}
