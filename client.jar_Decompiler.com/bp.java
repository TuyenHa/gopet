import java.util.Vector;
import vn.me.core.BaseCanvas;

public final class bp extends go {
   private int a;
   private final Vector a;
   private final ev a;

   public bp(ev var1, Vector var2) {
      super(2);
      this.a = var1;
      this.a = var2;
      this.a = this.a.size();
   }

   public final void b() {
      BaseCanvas.g.translate(-this.A, -this.B);
      int var1;
      int var10000 = (var1 = this.B / this.a.b * this.c) < 0 ? 0 : var1;
      var1 = var10000;
      int var2 = var10000;
      int var7;
      int var3 = var7 = var1 + ev.a(this.a);
      if (var7 > this.a) {
         var3 = this.a;
      }

      for(int var8 = var2; var8 < var3; ++var8) {
         var2 = var8 % this.c;
         int var4 = var8 / this.c;
         var2 = var2 * this.a.b + (var2 + 1) * this.d;
         var4 *= this.a.b;
         if (this.d && var8 == this.a.a) {
            gq.a(BaseCanvas.g, this.j ? gs.j : gs.b, this.j ? gs.i : gs.a, var2 + 1, var4 + 1, this.a.b - 2, this.a.b - 2, false);
            BaseCanvas.g.setColor(gs.d);
            BaseCanvas.g.drawRoundRect(var2, var4, this.a.b - 1, this.a.b - 1, gs.q, gs.q);
         }

         ad var5 = (ad)this.a.elementAt(var8);
         BaseCanvas.g.drawImage(cp.f, var2 + (this.a.b >> 1), var4 + (this.a.b >> 1), 3);
         cp.d().a(BaseCanvas.g, "" + var5.a, var2 + (this.a.b >> 1) - 10, var4 + 15, 17);
         cp.b.a(BaseCanvas.g, var5.a(), var2 + (this.a.b >> 1) + 10, var4 + 11, 17);
      }

      BaseCanvas.g.translate(this.A, this.B);
   }

   public final boolean a(int var1, int var2) {
      if (var1 != 0) {
         return false;
      } else {
         var1 = 0;
         if (var2 == -2) {
            ev var10000 = this.a;
            var10000.a += this.c;
            if (this.a.a >= this.a.size()) {
               this.a.a = 0;
            }

            var1 = 1;
         } else if (var2 == -1) {
            ev var5 = this.a;
            var5.a -= this.c;
            if (this.a.a < 0) {
               this.a.a = this.a.size() - 1;
            }

            var1 = 1;
         } else if (var2 == -4) {
            ++this.a.a;
            if (this.a.a >= this.a.size()) {
               this.a.a = 0;
            }

            var1 = 1;
         } else if (var2 == -3) {
            --this.a.a;
            if (this.a.a < 0) {
               this.a.a = this.a.size() - 1;
            }

            var1 = 1;
         }

         var2 = this.a.a % this.c;
         this.b(var2 * this.a.b + (var2 + 1) * this.d, this.a.a / this.c * this.a.b, this.a.b, this.a.b);
         return (boolean)var1;
      }
   }

   public final boolean b(int var1, int var2) {
      super.b(var1, var2);

      for(int var3 = 0; var3 < this.a; ++var3) {
         int var4 = var3 % this.c;
         int var5 = var3 / this.c;
         var4 = var4 * this.a.b + (var4 + 1) * this.d;
         var5 *= this.a.b;
         int var6 = var1 - this.r - this.y + this.A;
         int var7 = var2 - this.s - this.y + this.B;
         if (var6 > var4 && var6 < var4 + this.a.b && var7 > var5 && var7 < var5 + this.a.b) {
            this.a.a = var3;
            return true;
         }
      }

      return false;
   }
}
