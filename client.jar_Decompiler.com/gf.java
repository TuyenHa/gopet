import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class gf extends gd implements gz {
   private ge a;

   public gf(ge var1) {
      this.a = var1;
      this.e = new cd(-1, gw.a(0), this);
      this.d = new cd(-2, gw.a(7), this);

      for(int var3 = 0; var3 < gg.b.length; ++var3) {
         gb var2;
         (var2 = new gb(0)).y = 1;
         this.x = 1;
         var2.a(0, 0, gg.a + 4, gg.a + 4);
         var2.a((Image)Image.createImage(gg.b, var3 * gg.a, 0, gg.a, gg.a, 0));
         var2.e = gg.b[var3];
         var2.c = true;
         var2.d = this.d;
         this.a(var2, false);
      }

      this.l = true;
      this.c = 5;
      this.b(2);
      this.r = 1;
      this.v = 1;
      this.w = BaseCanvas.getCurrentScreen().b.u - this.u - gs.m;
      this.s = BaseCanvas.getCurrentScreen().b.u - gs.m;
      this.i = true;
      this.k = true;
      BaseCanvas.getCurrentScreen().a(this.a(0));
   }

   public final void a() {
      BaseCanvas.g.setColor(gs.a);
      BaseCanvas.g.fillRect(2, 2, this.t - 4, this.u - 4);
   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case -2:
            this.j();
            if (this.c == null) {
               return;
            } else {
               if (this.a == null) {
                  if (BaseCanvas.getCurrentScreen().a != null) {
                     BaseCanvas.getCurrentScreen().a.f = true;
                  }

                  this.a = BaseCanvas.getCurrentScreen().a;
               }

               this.a.b(this.a.a() + ((gb)this.c).e);
               BaseCanvas.getCurrentScreen().a((gn)this.a);
               if (this.a.f) {
                  return;
               }

               this.a.f = true;
               BaseCanvas.getCurrentScreen().b((gn)this.a);
               return;
            }
         case -1:
            this.j();
            return;
         default:
      }
   }
}
