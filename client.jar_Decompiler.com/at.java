import javax.microedition.lcdui.Graphics;

public final class at {
   private String[] a;
   private int a;
   private int b;
   private int c;

   public final void a(String var1) {
      this.c = cp.b.a() - 1;
      this.a = cp.b.a(var1);
      if (this.a > 80) {
         for(int var2 = this.a; var2 >= 40; var2 -= 10) {
            if (var2 / (this.a * this.c / var2) <= 2) {
               this.a = var2;
               break;
            }
         }

         this.a = cp.b.a(var1, this.a);
         this.a = 0;

         for(int var3 = 0; var3 < this.a.length; ++var3) {
            int var4 = cp.b.a(this.a[var3]);
            if (this.a < var4) {
               this.a = var4;
            }
         }

         this.b = this.a.length * this.c + 8;
      } else {
         this.a = new String[1];
         this.a[0] = var1;
         this.b = this.c + 8;
      }

      this.a = this.a < 30 ? 36 : this.a + 6;
   }

   public final void a(Graphics var1, int var2, int var3) {
      if (this.a != null) {
         var2 -= this.a >> 1;
         var3 = var3 - this.b - 6;
         var1.translate(var2, var3);
         c(var1, this.a, this.b);

         for(int var4 = 0; var4 < this.a.length; ++var4) {
            if (var4 < this.a.length) {
               cp.b.a(var1, this.a[var4], this.a >> 1, 2 + var4 * this.c, 17);
            }
         }

         b(var1, this.a, this.b);
         var1.translate(-var2, -var3);
      }
   }

   public static void b(Graphics var0, int var1, int var2) {
      var0.setColor(0);
      var0.drawLine(6, 0, var1 - 6, 0);
      var0.fillRect(0, 6, 1, var2 - 12);
      var0.fillRect(6, var2 - 1, var1 - 12, 1);
      var0.fillRect(var1 - 1, 6, 1, var2 - 12);
      var0.drawRegion(cp.a, 0, 0, 6, 6, 0, 0, 0, 0);
      var0.drawRegion(cp.a, 7, 0, 6, 6, 0, var1 - 6, 0, 0);
      var0.drawRegion(cp.a, 0, 6, 6, 6, 0, 0, var2 - 6, 0);
      var0.drawRegion(cp.a, 7, 6, 6, 6, 0, var1 - 6, var2 - 6, 0);
      var0.drawRegion(cp.a, 2, 12, 9, 6, 0, var1 >> 1, var2 - 1, 17);
   }

   public static void c(Graphics var0, int var1, int var2) {
      var0.setColor(16777215);
      var0.fillRect(6, 0, var1 - 12, 3);
      var0.fillRect(6, var2 - 3, var1 - 12, 3);
      var0.fillRect(0, 6, 3, var2 - 12);
      var0.fillRect(var1 - 3, 6, 3, var2 - 12);
      var0.fillRect(3, 3, var1 - 6, var2 - 6);
   }
}
