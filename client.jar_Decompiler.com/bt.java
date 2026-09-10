import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class bt extends gk {
   private static final Image[] a;

   public bt(ha var1, int var2, int var3) {
      super(var1, 0, 0, var2, var3);
   }

   public final void b() {
      dd var1;
      Image var2 = (var1 = (dd)this.a).a();
      int var10000 = this.y;
      if (var2 != null) {
         switch (var1.a) {
            case 1:
               BaseCanvas.g.drawImage(a[0], gs.p + this.b, 0, 24);
               break;
            case 2:
               BaseCanvas.g.drawImage(a[1], gs.p + this.b, 0, 24);
               break;
            case 3:
               BaseCanvas.g.drawImage(a[2], 1, 36, 0);
         }
      }

      ha var8 = this.a;
      int var3 = this.y;
      if (var2 != null) {
         BaseCanvas.g.drawImage(var2, gs.p + (this.a >> 1), this.u >> 1, 3);
      }

      if (this.d) {
         int var4 = BaseCanvas.g.getClipX();
         int var5 = BaseCanvas.g.getClipY();
         int var6 = BaseCanvas.g.getClipWidth();
         int var7 = BaseCanvas.g.getClipHeight();
         BaseCanvas.g.clipRect(gs.p + (var2 != null ? this.a : 0), 0, this.t - this.a, this.u);
         this.b.a(BaseCanvas.g, var8.a(), gs.p + (var2 != null ? this.a : 0), var3, 20);
         BaseCanvas.g.setClip(var4, var5, var6, var7);
         this.b.a();
      } else {
         this.a.a(BaseCanvas.g, var8.a(), gs.p + (var2 != null ? this.a : 0), var3, 20);
         this.a.a();
      }

      String var10;
      if ((var10 = var8.b()) != null && !this.d) {
         int var11 = BaseCanvas.g.getClipX();
         int var12 = BaseCanvas.g.getClipY();
         int var13 = BaseCanvas.g.getClipWidth();
         int var9 = BaseCanvas.g.getClipHeight();
         BaseCanvas.g.clipRect(gs.p + (var2 != null ? this.a : 0), this.u - this.d.a() - this.y - 10, this.t - 20 - gs.p - this.a, this.u);
         if (this.d) {
            this.d.a(BaseCanvas.g, var10, gs.p + (var2 != null ? this.a : 0), this.u - this.d.a() - this.y - 10, 20);
         } else if (!this.d) {
            this.c.a(BaseCanvas.g, var10, gs.p + (var2 != null ? this.a : 0), this.u - this.d.a() - this.y - 10, 20);
         }

         BaseCanvas.g.setClip(var11, var12, var13, var9);
      }

      if (this.d || this.j) {
         super.b();
      }

   }

   static {
      Image[] var0;
      a = var0 = new Image[3];
      var0[0] = gu.a(11);
      a[1] = gu.a(12);
      a[2] = gu.a(13);
   }
}
