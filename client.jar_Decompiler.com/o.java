import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class o extends gj {
   public do a;

   private o() {
   }

   public final void a() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(2, 2, this.t - 4, this.u - 4);
   }

   public final void c() {
      super.c();
      if (this.a != null) {
         this.a.c();
      }

   }

   public final void e() {
      if (this.d) {
         BaseCanvas.g.setColor(gs.d);
         BaseCanvas.g.drawRect(0, 0, this.t - 1, this.u - 1);
         BaseCanvas.g.drawRect(1, 1, this.t - 3, this.u - 3);
      }

   }

   public final void b() {
      if (this.a != null) {
         Image var1;
         if (this.a.b != null && (var1 = dj.a.a(this.a.b)) != null) {
            BaseCanvas.g.drawImage(var1, this.t >> 1, this.u >> 1, 3);
         }

         if (this.a.b == 0 || this.a.d == null) {
            return;
         }

         cp.d().a(BaseCanvas.g, this.a.d, 0, 0, 0);
      }

   }

   public o(fu var1) {
      this();
   }
}
