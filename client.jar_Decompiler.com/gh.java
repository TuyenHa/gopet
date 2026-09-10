import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class gh extends gb {
   private boolean k = false;
   private long a = 0L;
   private Image a;
   private final ew a;

   public gh(ew var1, Image var2, cd var3) {
      this.a = var1;
      this.x = 0;
      this.y = 0;
      this.a(0, 0, 28, 27);
      this.a = var2;
      this.d = var3;
      if (var2 == null) {
         throw new NullPointerException();
      }
   }

   public final void a(Image var1) {
      this.a = var1;
   }

   public final void e() {
   }

   public final void a() {
   }

   public final void b() {
      Image var1 = this.d ? this.a.a.a[1] : this.a.a.a[0];
      BaseCanvas.g.drawImage(var1, this.t - var1.getWidth() >> 1, this.u - var1.getHeight() >> 1, 0);
      if (!this.d) {
         this.k = false;
      }

      if (this.k) {
         BaseCanvas.g.drawImage(this.a, this.t - this.a.getWidth() >> 1, (this.u - this.a.getHeight() >> 1) - 2, 0);
      } else {
         BaseCanvas.g.drawImage(this.a, this.t - this.a.getWidth() >> 1, this.u - this.a.getHeight() >> 1, 0);
      }

      long var2 = System.currentTimeMillis();
      if (this.d && var2 - this.a >= 300L) {
         this.a = var2;
         this.k = !this.k;
      }
   }
}
