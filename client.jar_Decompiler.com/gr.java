import javax.microedition.lcdui.Graphics;
import javax.microedition.lcdui.Image;

public final class gr {
   public int a;
   public int b;
   public int c;
   private Image a;
   private int[] a;
   private int d;
   private boolean a;

   private gr(Image var1, int var2, int var3) {
      this(var1, var2, var3, false);
   }

   public gr(Image var1, int var2, int var3, boolean var4) {
      this.a = false;
      this.a = var1;
      this.a = var2;
      this.b = var3;
      this.d = var1.getHeight();
      this.c = this.d / var3;
      this.a = new int[this.c];

      for(int var5 = 0; var5 < this.c; ++var5) {
         this.a[var5] = var5 * var3;
      }

      this.a = false;
   }

   public gr(Image var1, int var2) {
      this(var1, var1.getWidth(), var1.getHeight() / var2);
   }

   public final void a(Graphics var1, int var2, int var3, int var4, int var5, int var6) {
      if (var2 >= 0 && var2 < this.c && !this.a && this.a != null) {
         var1.drawRegion(this.a, 0, this.a[var2], this.a, this.b, 0, var3, var4, var6);
      } else {
         if (var2 >= 0 && var2 < this.c && this.a != null && this.a) {
            var1.drawRegion(this.a, this.a[var2], 0, this.a, this.b, 0, var3, var4, var6);
         }

      }
   }

   public final Image a(int var1) {
      int var2 = this.a ? var1 * this.a : 0;
      var1 = this.a ? 0 : var1 * this.b;
      return Image.createImage(this.a, var2, var1, this.a, this.b, 0);
   }
}
