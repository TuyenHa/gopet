import vn.me.core.BaseCanvas;

public final class l extends go {
   private int a = 30;
   public byte a;
   public byte b;
   private byte c;
   private gj[] a;
   private int e;

   public l(int var1, int var2, int var3, int var4) {
      super(15, var1, var2, var3);
      this.e = var4;
      this.b(2);
      this.i = true;
      this.c = (byte)((var2 + 2) / (var4 + 2));
      var1 = (var3 + 2) / (var4 + 2);
      this.a = this.c * (var1 <= 0 ? 1 : var1);
   }

   public final void a(gj[] var1) {
      this.a = var1;
      this.a = (byte)(var1.length / this.a);
      if (var1.length % this.a != 0) {
         ++this.a;
      }

   }

   public final void a(int var1) {
      if (var1 < this.a && var1 >= 0) {
         this.b = (byte)var1;
         this.o();
         int var2;
         int var7;
         int var3 = var2 = (var7 = var1 * this.a) + this.a - 1;
         if (var2 >= this.a.length) {
            var3 = this.a.length - 1;
         }

         var2 = 0;
         int var4 = 0;
         int var5 = 0;

         for(int var6 = 0; var6 < this.a.length; ++var6) {
            if (var6 >= var7 && var6 <= var3) {
               this.a(this.a[var6], false);
               this.a[var6].b(var2, var4);
               this.a[var6].d = false;
               ++var5;
               if (var5 >= this.c) {
                  var5 = 0;
                  var2 = 0;
                  var4 += this.e + 2;
               } else {
                  var2 += this.e + 2;
               }
            } else {
               this.a[var6].a = null;
            }
         }

         BaseCanvas.getCurrentScreen().a(this.a(0));
      }
   }

   public final boolean a(int var1, int var2) {
      gn var3;
      if ((var3 = this.a(false)) != this && var3 != null && var3.a(var1, var2)) {
         return true;
      } else {
         int var11 = this.c();
         if (var1 == 0 && var2 == -3) {
            if (this.b > 0 && var11 % this.c == 0) {
               this.a(this.b - 1);
               return true;
            } else {
               var2 = var1 = var11 - 1;
               if (var1 < 0) {
                  var2 = 0;
               }

               BaseCanvas.getCurrentScreen().a(this.a(var2));
               return true;
            }
         } else if (var1 == 0 && var2 == -4) {
            if (this.b < this.a - 1 && var11 % this.c == this.c - 1) {
               this.a(this.b + 1);
               return true;
            } else {
               gn var6;
               if ((var6 = this.a(var11 + 1)) != null) {
                  BaseCanvas.getCurrentScreen().a(var6);
                  return true;
               } else {
                  return true;
               }
            }
         } else if (var1 == 0 && var2 == -2) {
            var2 = var1 = var11 + this.c;
            if (var1 >= this.a.length) {
               var2 = this.a.length - 1;
            }

            BaseCanvas.getCurrentScreen().a(this.a(var2));
            return true;
         } else if (var1 == 0 && var2 == -1) {
            var2 = var1 = var11 - this.c;
            if (var1 < 0) {
               var2 = 0;
            }

            BaseCanvas.getCurrentScreen().a(this.a(var2));
            return true;
         } else {
            return true;
         }
      }
   }
}
