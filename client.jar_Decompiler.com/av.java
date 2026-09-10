import vn.me.core.BaseCanvas;

public final class av extends gn {
   private gg a;
   private int c;
   public int a = 20;
   public int b = 3;
   private String[] a = new String[0];

   public av() {
      this.y = gs.p;
      this.i = true;
   }

   public final void a(String var1, gg var2) {
      if (var1 == null) {
         var1 = "";
      }

      this.a = var2;
      this.a = var2.a(var1, this.t - (this.y << 1));
      this.a.b = this.a.length * (var2.a() + this.b) - this.b;
      this.a.a = this.t - (this.y << 1);
      this.c = this.a.b - this.u;
   }

   public final void b() {
      if (this.a != null) {
         int var1 = 0;

         for(int var2 = 0; var2 < this.a.length; ++var2) {
            int var3 = this.a == 17 ? this.t >> 1 : (this.a == 24 ? this.t - this.y : 0);
            if (var1 - this.B + this.a.a() >= 0) {
               this.a.a(BaseCanvas.g, this.a[var2], var3 - this.A, var1 - this.B, this.a);
            }

            var1 = var3 = var1 + this.a.a() + this.b;
            if (var3 - this.B > this.u) {
               return;
            }
         }
      }

   }

   public final boolean a(int var1, int var2) {
      if (var2 == -2 && this.a.b > this.u - 2 * this.y && this.D < this.c) {
         this.b(this.r + this.y, this.D + this.u - 2 * this.y, this.t - 2 * this.y, this.a.a());
         return true;
      } else if (var2 == -1 && this.a.b > this.u - 2 * this.y && this.D > 0) {
         this.b(this.r + this.y, this.D - this.a.a(), this.t - 2 * this.y, this.a.a());
         return true;
      } else {
         return false;
      }
   }
}
