import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class fz extends n {
   private final fy a;

   public fz(fy var1, int var2) {
      super((Image)null);
      this.a = var1;
      this.C = (byte)var2;
      if (this.a.a.a == null) {
         this.a.a.a = new String[0];
      }

   }

   public final void b() {
      BaseCanvas.g.translate(this.r, this.s);
      BaseCanvas.g.setColor(gs.b);
      BaseCanvas.g.fillRoundRect(0, 0, this.t, this.u, 10, 10);
      switch (this.a.b) {
         case 0:
            switch (this.C) {
               case 0:
                  gv.a.a(BaseCanvas.g, "(str) " + gw.a(135), 2, 2, 20);
                  break;
               case 1:
                  gv.a.a(BaseCanvas.g, "(agi) " + gw.a(136), 2, 2, 20);
                  break;
               case 2:
                  gv.a.a(BaseCanvas.g, "(int) " + gw.a(137), 2, 2, 20);
            }
         case 1:
            if (this.C < this.a.a.a.length) {
               gv.a.a(BaseCanvas.g, this.a.a.a[this.C], 2, 2, 20);
            }
         default:
            BaseCanvas.g.translate(-this.r, -this.s);
      }
   }

   public final void g() {
      super.g();
      this.a.a = this.C;
      switch (this.a.b) {
         case 0:
            String var1 = "";
            switch (this.C) {
               case 0:
                  var1 = var1 + "+" + this.a.a[this.C] + " (str) ";
                  break;
               case 1:
                  var1 = var1 + "+" + this.a.a[this.C] + " (agi) ";
                  break;
               case 2:
                  var1 = var1 + "+" + this.a.a[this.C] + " (int) ";
            }

            var1 = var1 + "-" + this.a.b[this.C] + " (ngoc) -" + this.a.a[this.C] + " (vang)\n" + fy.a(this.a);
            this.a.a = gv.a.a(var1, BaseCanvas.w - 25);
            return;
         case 1:
            if (this.C < this.a.a.a.length) {
               this.a.a = gv.a.a(this.a.a.b[this.C], BaseCanvas.w - 25);
               return;
            }

            return;
         default:
      }
   }
}
