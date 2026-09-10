import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class bq extends bo {
   public String a;
   private Image a;
   public boolean a;
   public int a;
   public int b;
   private int c;
   private long a;
   private final es a;

   public bq(es var1) {
      this.a = var1;
      this.a = 1;
   }

   public final void b() {
      if (this.a == null) {
         Image var7;
         Image var8 = var7 = dj.a.a(this.a);
         if (var7 != null) {
            if (this.a) {
               var8 = gq.a(var8, var8.getHeight() << 1);
            }

            this.a = var8;
            this.c(this.t, var8.getHeight() + 4);
            es.a(this.a).h();
         }
      } else {
         byte var1 = this.b;
         int var2 = this.a.getWidth() / this.a;
         byte var3 = 0;
         int var4 = 0;
         switch (var1) {
            case 17:
               var3 = 24;
               var4 = this.t;
               break;
            case 20:
               var3 = 0;
               var4 = 0;
               break;
            case 24:
               var3 = 17;
               var4 = this.t >> 1;
         }

         BaseCanvas.g.drawRegion(this.a, this.c * var2, 0, var2, this.a.getHeight(), 0, var4, 0, var3);
         long var5;
         if ((var5 = System.currentTimeMillis()) - this.a >= 200L) {
            this.a = var5;
            this.c = (this.c + 1) % this.a;
         }

      }
   }
}
