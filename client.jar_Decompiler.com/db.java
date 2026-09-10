import vn.me.core.BaseCanvas;

public final class db extends go {
   public da a;
   private final fi a;

   public db(fi var1, da var2) {
      this.a = var1;
      this.c(BaseCanvas.w, 15 + 2 * cp.a((byte)0).a());
      this.a = var2;
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

   public final void b() {
      int var1 = cp.a((byte)0).a();
      cp.a((byte)0).a(BaseCanvas.g, this.a.a, 10, 5, 0);
      BaseCanvas.g.drawImage(this.a.a ? fi.a(this.a) : fi.b(this.a), this.t - 2, 5, 24);
      cp.a((byte)9).a(BaseCanvas.g, this.a.b, 10, var1 + 5 + 5, 0);
   }

   public final void e() {
      if (this.x > 0) {
         BaseCanvas.g.setColor(5592405);
         BaseCanvas.g.drawLine(0, 0, this.t - 1, 0);
         if (this.d) {
            BaseCanvas.g.setColor(gs.d);
            BaseCanvas.g.drawRoundRect(0, 0, this.t - 1, this.u - 1, gs.q, gs.q);
         }
      }

   }
}
