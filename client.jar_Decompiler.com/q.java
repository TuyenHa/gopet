import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class q extends gb {
   private Image a;

   public q(Image var1) {
      this.a = var1;
   }

   public final void b() {
      int var1 = BaseCanvas.g.getTranslateX();
      int var2 = BaseCanvas.g.getTranslateY();
      BaseCanvas.g.translate(-var1 + this.r, -var2 + this.s);
      BaseCanvas.g.drawImage(this.a, this.t - this.a.getWidth() >> 1, this.u - this.a.getHeight() >> 1, 0);
      BaseCanvas.g.translate(-(-var1 + this.r), -(-var2 + this.s));
   }
}
