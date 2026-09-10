import javax.microedition.lcdui.Graphics;

public final class dz {
   public aw a;
   public int a;
   private boolean a = true;
   private boolean b = true;
   private long a;

   public dz(aw var1) {
      this.a = var1;
   }

   public final void a() {
      this.a = System.currentTimeMillis();
      this.a = 0;
   }

   public final void a(Graphics var1, int var2, int var3, int var4) {
      this.a(var1, this.a, var2, var3, var4);
   }

   public final void a(long var1) {
      if (this.a && var1 - this.a > (long)this.a.a[this.a]) {
         ++this.a;
         this.a = var1;
         if (this.a > this.a.a.length - 1) {
            this.a = 0;
            if (this.b) {
               return;
            }

            this.a = false;
         }

      }
   }

   public final void a(Graphics var1, int var2, int var3) {
      long var4;
      if (this.a && (var4 = System.currentTimeMillis()) - this.a > (long)this.a.a[this.a]) {
         ++this.a;
         this.a = var4;
         if (this.a > this.a.a.length - 1) {
            this.a = 0;
            if (!this.b) {
               this.a = false;
            }
         }
      }

      this.a(var1, this.a, var2, var3, 0);
   }

   private void a(Graphics var1, int var2, int var3, int var4, int var5) {
      ax var11 = this.a.a.a[this.a.a[var2]];

      for(int var6 = 0; var6 < var11.a.length; ++var6) {
         ay var7 = this.a.a.a[var11.a[var6]];
         int var8 = (var5 & 2) == 0 ? var11.a[var6] + var3 : -var11.a[var6] - var7.c + var3;
         int var9 = (var5 & 1) == 0 ? var11.b[var6] + var4 : -var11.b[var6] - var7.d + var4;
         byte var10 = var11.b[var6];
         if (var5 == 2) {
            var10 = (byte)((var11.b[var6] + var5) % 4);
         } else if (var5 == 1) {
            var10 = (byte)(var11.b[var6] ^ var5);
         }

         var1.drawRegion(this.a.a, var7.a, var7.b, var7.c, var7.d, var10, var8, var9, 0);
      }

   }
}
