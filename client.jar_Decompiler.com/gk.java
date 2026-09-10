import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public class gk extends go implements gz {
   protected int a = 43;
   public ha a;
   public gg a;
   public gg b;
   public gg c;
   public gg d;

   public gk(ha var1, int var2, int var3, int var4, int var5) {
      super(0, 0, var4, var5);
      this.a = gv.a;
      this.b = gv.a;
      this.c = gv.b;
      this.d = gv.b;
      gg var10000 = gv.a;
      this.a = var1;
      this.x = 1;
      this.a(0, 0, var4, var5);
   }

   public final void a() {
      gs.a(this);
   }

   public void b() {
      ha var1;
      Image var2 = (var1 = this.a).a();
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
         this.b.a(BaseCanvas.g, var1.a(), gs.p + (var2 != null ? this.a : 0), var3, 20);
         BaseCanvas.g.setClip(var4, var5, var6, var7);
         this.b.a();
      } else {
         this.a.a(BaseCanvas.g, var1.a(), gs.p + (var2 != null ? this.a : 0), var3, 20);
         this.a.a();
      }

      String var9;
      if ((var9 = var1.b()) != null && !this.d) {
         int var10 = BaseCanvas.g.getClipX();
         int var11 = BaseCanvas.g.getClipY();
         int var12 = BaseCanvas.g.getClipWidth();
         int var8 = BaseCanvas.g.getClipHeight();
         BaseCanvas.g.clipRect(gs.p + (var2 != null ? this.a : 0), this.u - this.d.a() - this.y - 10, this.t - 20 - gs.p - this.a, this.u);
         if (this.d) {
            this.d.a(BaseCanvas.g, var9, gs.p + (var2 != null ? this.a : 0), this.u - this.d.a() - this.y - 10, 20);
         } else if (!this.d) {
            this.c.a(BaseCanvas.g, var9, gs.p + (var2 != null ? this.a : 0), this.u - this.d.a() - this.y - 10, 20);
         }

         BaseCanvas.g.setClip(var10, var11, var12, var8);
      }

      if (this.d || this.j) {
         super.b();
      }

   }

   public final void e() {
      if (this.x > 0) {
         gs.b(this);
      }

   }

   public final void g() {
      super.g();
      if (this.a.b() != null) {
         gj var1;
         (var1 = new gj(this.a.b(), this.d)).y = 0;
         ha var10000 = this.a;
         int var2 = 0;
         if (gs.a == 1) {
            var2 = gs.p << 1;
         }

         var1.a(gs.p + (this.a.a() != null ? 40 : 0), this.u - this.d.a() - this.y - 10, this.t - (gs.p << 1) - 40, this.d.a() + var2);
         var1.g = false;
         this.o();
         this.a(var1, false);
         var1.h();
      }
   }

   public final void b_() {
      super.b_();
      this.o();
   }

   public final void a(Object var1) {
   }
}
