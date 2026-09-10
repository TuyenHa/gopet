import vn.me.core.BaseCanvas;

public class bo extends gn {
   public byte a;
   public byte b;

   public final void e() {
      if (this.d) {
         BaseCanvas.g.setColor(5592405);
         BaseCanvas.g.drawLine(0, 0, this.t - 1, 0);
         BaseCanvas.g.setColor(gs.d);
         BaseCanvas.g.drawRoundRect(0, 0, this.t - 1, this.u - 1, gs.q, gs.q);
      }

   }

   public final void a() {
      if (this.j) {
         gq.a(BaseCanvas.g, gs.j, gs.i, 0, 0, this.t - 1, this.u - 1, false);
      } else {
         if (this.d) {
            gq.a(BaseCanvas.g, gs.a, gs.b, 0, 0, this.t - 1, this.u - 1, false);
         }

      }
   }
}
