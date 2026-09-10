import java.io.IOException;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class cb extends n {
   private final ca a;

   public cb(ca var1, int var2) {
      super((Image)null);
      this.a = var1;
      this.C = (byte)var2;
   }

   public final void b() {
      BaseCanvas.g.translate(this.r, this.s);
      BaseCanvas.g.setColor(gs.b);
      BaseCanvas.g.fillRoundRect(0, 0, this.t, this.u, 10, 10);
      switch (ca.a(this.a)[this.C]) {
         case -1:
            if (ca.a(this.a) == null) {
               try {
                  ca.a(this.a, Image.createImage("/lock.png"));
               } catch (IOException var2) {
                  var2.printStackTrace();
               }
            }

            BaseCanvas.g.drawImage(ca.a(this.a), -1, -4, 0);
            break;
         case 1:
            Image var1;
            if ((var1 = dj.a.a(ca.a(this.a)[this.C])) != null) {
               BaseCanvas.g.drawImage(var1, -1, this.u >> 1, 6);
               if (ca.b(this.a)[this.C] != null) {
                  gv.a.a(BaseCanvas.g, ca.b(this.a)[this.C], var1.getWidth() + 2, 3, 20);
               }
            }
      }

      BaseCanvas.g.translate(-this.r, -this.s);
   }

   public final void g() {
      super.g();
      ca.a(this.a, this.C);
      if (ca.a(this.a)[this.C] == 1) {
         ca.a(this.a, gv.a.a(ca.c(this.a)[this.C], BaseCanvas.w - 25));
      } else {
         ca.a(this.a, (String[])null);
      }
   }
}
