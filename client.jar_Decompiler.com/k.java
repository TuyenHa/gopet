import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class k extends gd implements gz {
   private String[] a;
   private String a;
   public int a;
   private String b;
   private String[] b;
   private int e;
   private int f = 0;
   private cd a = new cd(1, a.a(664), this);
   private cd f = new cd(2, a.a(364), this);
   private cd g = new cd(3, "", this);

   public k(int var1, String var2, String[] var3) {
      this.a = var1;
      this.a = var2;
      this.a = var3;
      this.b = this.a[0];
      this.y = gs.p;
      this.d = cg.a;
      this.e = this.a;
      this.c = true;
      this.x = 3;
      this.t = BaseCanvas.w - (gs.p << 1);
      this.a.a = this.t - 2 * this.y;
      this.b = gv.a.a(this.b, this.t - (gs.p + this.x << 2));
      this.u = gv.a.a() * this.b.length + (this.y + this.x << 1);
      if (this.u < 60) {
         this.u = 60;
      }

      this.a(gs.p, BaseCanvas.h - this.u - this.y - gs.l, this.t, this.u);
      this.e = (this.u >> 1) - this.b.length * gv.a.a() / 2 - this.y - this.x;
   }

   public final void b() {
      super.b();
      Image var1 = cp.a(this.a, (byte)1);
      if (this.a != null && var1 != null) {
         BaseCanvas.g.drawImage(var1, var1.getWidth() >> 1, this.u - this.y - this.x >> 1, 3);
         int var4 = 0;

         for(int var5 = this.e; var4 < this.b.length; var5 += gv.a.a()) {
            gv.a.a(BaseCanvas.g, this.b[var4], var1.getWidth() + gs.p, var5, 20);
            ++var4;
         }

      } else {
         int var2 = 0;

         for(int var3 = this.e; var2 < this.b.length; var3 += gv.a.a()) {
            gv.a.a(BaseCanvas.g, this.b[var2], this.y + this.x << 1, var3, 20);
            ++var2;
         }

      }
   }

   private void c(String var1) {
      this.b = var1;
      Image var2 = cp.a(this.a, (byte)1);
      if (this.a != null && var2 != null) {
         this.b = gv.a.a(this.b, this.t - (gs.p + this.x << 2) - (var2 != null ? var2.getWidth() : 0));
         this.u = gv.a.a() * this.b.length + (this.y + this.x << 1);
         if (this.u < (var2 != null ? var2.getHeight() : 0) + 2 * (this.y + this.x)) {
            this.u = (var2 != null ? var2.getHeight() : 0) + 2 * (this.y + this.x);
         }

         if (this.u < 60) {
            this.u = 60;
         }

         this.a(gs.p, BaseCanvas.h - this.u - this.y - gs.l, this.t, this.u);
         this.e = (this.u >> 1) - this.b.length * gv.a.a() / 2 - this.y - this.x;
      }
   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 0:
            this.j();
            cg.b = 1;
            a.a("guide", cg.b);
            return;
         case 1:
            if (this.f < this.a.length - 1) {
               ++this.f;
               this.c = this.f;
               if (this.f == this.a.length - 1) {
                  this.e = this.g;
               }

               this.c(this.a[this.f]);
               return;
            }

            return;
         case 2:
            if (this.f > 0) {
               --this.f;
               this.e = this.a;
               if (this.f == 0) {
                  this.c = this.g;
               }

               if (this.f > 0 && this.f < this.a.length) {
                  this.e = this.a;
               }

               this.c(this.a[this.f]);
               return;
            }

            return;
         case 3:
         default:
      }
   }
}
