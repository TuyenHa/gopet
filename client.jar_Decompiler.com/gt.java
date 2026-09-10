import javax.microedition.lcdui.Graphics;
import javax.microedition.lcdui.Image;

public final class gt {
   private int a;
   private String a;
   private int b;
   private int c;
   public boolean a;
   private long a;
   private long b = System.currentTimeMillis();
   private int d = 0;

   public gt(int var1, String var2, int var3, int var4, boolean var5, byte var6, long var7) {
      this.a = var1;
      this.a = var2;
      this.b = var3;
      this.c = var4;
      this.a = var5;
      this.a = var7;
   }

   public final void a() {
      long var1 = System.currentTimeMillis() - this.b;
      this.d = (int)(var1 / this.a % (long)this.a);
   }

   public final void a(Graphics var1, int var2, int var3) {
      Image var4;
      if ((var4 = dj.a.a(this.a)) != null) {
         int var5 = var4.getWidth() / this.a;
         var4.getHeight();
         var1.drawRegion(var4, var5 * this.d, 0, var5, var4.getHeight(), 0, var2 + this.b, var3 + this.c, 17);
      }

   }
}
