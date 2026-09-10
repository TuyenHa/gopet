import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class dp {
   public static int a;
   public static long a;
   static int b;
   static long b;
   public dj a;

   public dp(dj var1) {
      this.a = var1;
   }

   public final void a() {
      BaseCanvas.g.drawRegion(cp.c, a * 14, 0, 14, 14, 0, 2, 1, 0);
      cp.d().a(BaseCanvas.g, dj.b, 17, 4, 0);
      BaseCanvas.g.drawImage(this.a.a, BaseCanvas.Field159, 1, 0);
      cp.d().a(BaseCanvas.g, dj.c, BaseCanvas.Field159 + 15, 4, 0);
      long var1;
      if ((var1 = System.currentTimeMillis()) - a > 100L) {
         a = (a + 1) % 5;
         a = var1;
      }

   }

   public final void a(int var1) {
      this.a();
      BaseCanvas.g.drawImage(this.a.b, BaseCanvas.Field159 << 1, 1, 0);
      cp.d().a(BaseCanvas.g, String.valueOf(var1), (BaseCanvas.Field159 << 1) + 15, 4, 0);
   }

   public static void a(Image var0, int var1, int var2, int var3, int var4, int var5) {
      try {
         int var6 = var0.getWidth() / var5;
         BaseCanvas.g.drawRegion(var0, b * var6, 0, var6, var0.getHeight(), var3, var1, var2, var4);
         long var7;
         if ((var7 = System.currentTimeMillis()) - b >= 200L) {
            b = var7;
            b = (b + 1) % var5;
         }

      } catch (Exception var9) {
         System.out.println("defpackage.PetRenderer.drawPetWithFrame() " + var5);
      }
   }

   public static void a(Image var0, int var1, int var2, int var3, int var4) {
      a(var0, var1, var2, 0, 0, var4);
   }
}
