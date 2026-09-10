import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class du extends c {
   private byte a;
   private int b;
   private int c;
   private int d;
   private int e;
   private int f;
   private String[] a;
   private long a;
   private Image a;
   private String a;

   public du(String var1) {
      this.a = gv.b.a(var1, BaseCanvas.w >> 1);
      if (this.a.length > 0) {
         this.e = gv.b.a(this.a[0]) + 4;

         for(int var3 = 1; var3 < this.a.length; ++var3) {
            int var2;
            if ((var2 = gv.b.a(this.a[var3]) + 4) > this.e) {
               this.e = var2;
            }
         }

         this.d = BaseCanvas.w - this.e;
         this.f = gv.b.a() * this.a.length + 2;
      }

      this.a = null;
      this.a = "";
   }

   public final void a() {
      this.a = true;
      this.a = 0;
      this.b = BaseCanvas.h - gs.m;
      this.c = BaseCanvas.h - gs.m - this.f;
      BaseCanvas.getCurrentScreen();
   }

   public final void b() {
      BaseCanvas.g.setColor(14328834);
      BaseCanvas.g.fillRect(this.d, this.b, this.e, this.f);

      for(int var1 = 0; var1 < this.a.length; ++var1) {
         gv.b.a(BaseCanvas.g, this.a[var1], this.d + 2, this.b + 1 + var1 * gv.b.a(), 0);
      }

      if (this.a != null) {
         BaseCanvas.g.drawImage(this.a, BaseCanvas.w - this.a.getWidth() >> 1, this.b + this.a.length * gv.b.a(), 0);
      } else {
         if (this.a.trim().length() > 0) {
            this.a = cp.a(this.a, (byte)3);
         }

      }
   }

   public final void a(long var1) {
      switch (this.a) {
         case 0:
            if (ed.a(this.b - this.c) >= 3) {
               this.b -= 3;
               return;
            }

            this.b = this.c;
            this.a = 1;
            this.a = var1;
            return;
         case 1:
            if (var1 - this.a >= 3000L) {
               this.a = 2;
               return;
            }

            return;
         case 2:
            if (ed.a(BaseCanvas.h - gs.m - this.b) >= 3) {
               this.b += 3;
               return;
            }

            this.a = false;
            return;
         case 3:
            if (ed.a(this.b - this.c) >= 6) {
               this.b += 6;
               return;
            }

            this.b = this.c;
            this.a = 4;
            this.a = var1;
            return;
         case 4:
            if (var1 - this.a >= 2500L) {
               this.a = 5;
               return;
            }

            return;
         case 5:
            this.a = null;
            this.a = false;
            return;
         default:
      }
   }
}
