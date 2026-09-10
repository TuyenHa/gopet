import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public class n extends gb {
   private int a;
   private long a;

   public n(Image var1) {
      super(var1);
   }

   public final void a_() {
      this.b();
      if (this.d) {
         BaseCanvas.g.drawImage(dj.e, this.r - 6 + this.a, this.s + (this.u - 14) / 2, 0);
         long var1;
         if ((var1 = System.currentTimeMillis()) - this.a >= 200L) {
            this.a = var1;
            if (this.a == -2) {
               this.a = 0;
               return;
            }

            this.a = -2;
         }
      }

   }
}
