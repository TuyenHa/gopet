import vn.me.core.BaseCanvas;

public final class de extends eh {
   public int a;
   private String a;
   private int b;
   private int c;
   private int d;
   private long b;
   private long c;
   private long d;
   private int e;
   private StringBuffer a;
   private byte a;
   private int f;
   public boolean a = false;

   public de(int var1, String var2, String var3, int var4, int var5, int var6, byte var7, short var8, boolean var9) {
      this.a = var1;
      this.a = var2;
      this.e = var3;
      this.i = var5;
      this.j = var6;
      this.h = true;
      this.a(new gy(-10, -10, 20, 20));
      this.d = (long)(ed.b(5) * 1000 + 3000);
      this.a = (new StringBuffer("LV ")).append(String.valueOf(var4));
      this.a = var7;
      this.f = var8;
      this.a = var9;
   }

   public final void a(int var1, int var2) {
      BaseCanvas.g.drawImage(cp.i, this.i - var1 - 11, this.j - 6 - var2, 0);
      if (this.b == null) {
         this.b = dj.a.a(this.a);
         if (this.b != null) {
            this.b = this.b.getWidth() / this.a;
            this.c = this.b.getHeight();
         }
      } else {
         BaseCanvas.g.drawRegion(this.b, this.d * this.b, 0, this.b, this.c, this.e, this.i - var1, this.j - this.b.getHeight() - var2 + this.f, 17);
         long var3;
         if ((var3 = System.currentTimeMillis()) - this.b >= 200L) {
            this.b = var3;
            this.d = (this.d + 1) % this.a;
         }

         if (var3 - this.c >= this.d) {
            this.e = (this.e + 2) % 4;
            this.d = (long)(ed.b(5) * 1000 + 3000);
            this.c = var3;
         }

         cp.a().a(BaseCanvas.g, this.a.toString(), this.i - var1, this.j - this.c - 5 - var2, 17);
      }
   }
}
