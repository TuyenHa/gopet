import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class ga extends n {
   private Image a;

   public ga(Image var1) {
      super(var1);
      this.a = var1;
   }

   public final void b() {
      BaseCanvas.g.drawImage(this.a, this.r, this.s, 0);
   }
}
