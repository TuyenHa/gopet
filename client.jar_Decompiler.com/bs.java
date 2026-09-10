import vn.me.core.BaseCanvas;

public final class bs extends bo {
   public String a;
   public String[] a;
   public byte c;

   public bs(es var1) {
   }

   public final void b() {
      int var1 = 0;
      if (this.b == 17) {
         var1 = this.t >> 1;
      } else if (this.b == 24) {
         var1 = this.t;
      }

      gg var2 = cp.a(this.c);
      int var3 = 2;

      for(int var4 = 0; var4 < this.a.length; ++var4) {
         var2.a(BaseCanvas.g, this.a[var4], var1, var3, this.b);
         var3 += var2.a();
      }

   }
}
