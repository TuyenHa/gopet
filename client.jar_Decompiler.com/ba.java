import vn.me.core.BaseCanvas;

public final class ba extends c {
   private long a;
   private long b;
   private int b;
   private int c;
   private int d;
   private dj a;
   private int e = 0;
   private long[] a = new long[2];
   private byte[] a = new byte[2];

   public final void a(dj var1, int var2, int var3, long var4, long var6) {
      if (var4 != 0L || var6 != 0L) {
         super.a();
         this.a = var1;
         this.c = var2;
         this.d = var3;
         this.b = System.currentTimeMillis();
         this.a[0] = var4;
         this.a[1] = var6;
         this.b = 0;

         for(int var8 = 0; var8 < this.a.length; ++var8) {
            this.a[var8] = (byte)cp.c().a("+" + ed.a(this.a[var8]));
            if (this.a[var8] > this.b) {
               this.b = this.a[var8];
            }
         }

         this.b += 14;
         this.a = true;
      }
   }

   public final void a(long var1) {
      if (var1 - this.a > 100L) {
         this.e = (this.e + 1) % 5;
         this.a = var1;
      }

      if (var1 - this.b > 2000L) {
         this.a = false;
      } else {
         this.d -= 2;
      }
   }

   public final void b() {
      int var1 = this.c - (this.b >> 1);
      int var2 = 0;

      for(int var3 = 0; var3 < 2; ++var3) {
         if (this.a[var3] != 0L) {
            cp.c().a(BaseCanvas.g, this.a[var3] > 0L ? "+" + ed.a(this.a[var3]) : ed.a(this.a[var3]), var1, this.d + var2, 0);
            switch (var3) {
               case 0:
                  BaseCanvas.g.drawRegion(cp.c, this.e * 14, 0, 14, 14, 0, var1 + this.a[var3] + 1, this.d + var2, 0);
                  break;
               case 1:
                  BaseCanvas.g.drawImage(this.a.a, var1 + this.a[var3] + 1, this.d + var2, 0);
            }

            var2 += 20;
         }
      }

   }
}
