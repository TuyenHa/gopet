import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public class gj extends gn {
   public String e;
   public gr a;
   public int p;
   private byte a;
   private boolean a;
   public int q;
   private int a;
   private int b;
   private int c;
   private int d;
   private boolean k;
   private boolean l;
   public gg a;
   public gg b;
   public boolean b;
   public byte d;
   private long a;
   private long b;
   private long c;
   private long d;
   public boolean c;

   public gj() {
      this.c = false;
      this.p = 0;
      this.a = 0;
      this.a = false;
      this.q = 20;
      this.a = 8;
      this.b = 2;
      this.c = 0;
      this.d = 0;
      this.k = false;
      this.l = true;
      this.a = gv.a;
      this.b = gv.a;
      this.b = false;
      this.d = 0;
      this.b = -1L;
      this.d = -1L;
   }

   public gj(String var1) {
      this(var1, gs.a == 0 ? gv.b : gv.a);
   }

   public gj(String var1, gg var2) {
      this(var1, var2, var2);
   }

   private gj(String var1, gg var2, gg var3) {
      this.c = false;
      this.p = 0;
      this.a = 0;
      this.a = false;
      this.q = 20;
      this.a = 8;
      this.b = 2;
      this.c = 0;
      this.d = 0;
      this.k = false;
      this.l = true;
      this.a = gv.a;
      this.b = gv.a;
      this.b = false;
      this.d = 0;
      this.b = -1L;
      this.d = -1L;
      this.a = var2;
      this.b = var3 == null ? var2 : var3;
      this.y = gs.p;
      this.a(var1);
      this.g = false;
   }

   private void i() {
      this.a.b = Math.max(this.a.a(), this.b.a());
      if (this.e != null) {
         this.a.a = Math.max(this.a.a(this.e), this.b.a(this.e));
      }

      if (this.a != null) {
         gx var10000 = this.a;
         var10000.a += this.a.a + this.b;
         this.a.b = Math.max(this.a.b, this.a.b);
      }

      this.u = this.a.b + 2 * (this.y + this.x);
      this.t = this.a.a + 2 * (this.y + this.x);
   }

   public final void a(String var1) {
      this.e = var1;
      this.i();
   }

   public final void a(gg var1, gg var2) {
      this.a = var1;
      this.b = var2 == null ? var1 : var2;
      this.i();
   }

   public void a(Image var1) {
      if (var1 != null) {
         this.b(var1, new gx(var1.getWidth(), var1.getHeight()));
      }
   }

   public final void a(Image var1, gx var2) {
      if (var1 != null) {
         this.b(var1, var2);
      }
   }

   private void b(Image var1, gx var2) {
      if (var1 != null) {
         this.a = new gr(var1, var2.a, var2.b, false);
         this.p = 0;
         this.a = new gx(var2.a, var2.b);
      }
   }

   public void b() {
      if (this.a != null || this.e != null && this.e.length() != 0) {
         int var1 = BaseCanvas.g.getClipX();
         int var2 = BaseCanvas.g.getClipY();
         int var3 = BaseCanvas.g.getClipWidth();
         int var4 = BaseCanvas.g.getClipHeight();
         if (this.a != null) {
            if (this.e == null || "".equals(this.e) || this.c) {
               int var9 = (this.u >> 1) - this.y - this.x;
               this.a.a(BaseCanvas.g, this.p, (this.t >> 1) - this.y - this.x, var9, 0, 3);
               return;
            }

            if (this.a == 32) {
               int var5 = Math.max(this.a.a(), this.b.a());
               int var6 = this.u - (this.y + this.x << 1) - var5 >> 1;
               this.a.a(BaseCanvas.g, this.p, (this.t >> 1) - this.y - this.x, var6, 0, 3);
            } else {
               int var7 = this.a == 4 ? this.t - (this.y + this.x << 1) : 1;
               this.a.a(BaseCanvas.g, this.p, var7, 0, 0, this.a == 8 ? 20 : 24);
               if (this.a == 8) {
                  BaseCanvas.g.clipRect(this.a.a, 0, this.t - this.a.a - (this.y << 1) - this.b, this.u);
               } else if (this.a == 4) {
                  BaseCanvas.g.clipRect(0, 0, this.t - this.a.a - (this.y << 1) - this.b, this.u);
               }
            }
         }

         if (this.q == 24) {
            if (this.a == null) {
               (this.d ? this.b : this.a).a(BaseCanvas.g, this.e, this.t - (this.y << 1), (this.u >> 1) - (this.g ? this.b.a() >> 1 : this.a.a() >> 1) - gs.p, this.q);
            } else {
               (this.d ? this.b : this.a).a(BaseCanvas.g, this.e, this.a == 8 ? this.t - (this.y << 1) : this.t - (this.y << 1) - this.a.a + this.b, (this.u >> 1) - (this.g ? this.b.a() >> 1 : this.a.a() >> 1) - gs.p, this.q);
            }
         } else {
            int var8 = this.q == 17 ? (this.t >> 1) - this.y - this.x : (this.a == 8 ? this.c + (this.a == null ? 0 : this.a.a + this.b) : (this.t >> 1) - (this.d ? this.b : this.a).a(this.e) - gs.p);
            int var10 = Math.max(this.a.a(), this.b.a());
            var10 = this.a == 32 ? this.u - (this.x + this.y << 1) - var10 : (this.u >> 1) - ((this.d ? this.b : this.a).a() >> 1) - gs.p;
            (this.d ? this.b : this.a).a(BaseCanvas.g, this.e, var8, var10, this.q);
         }

         BaseCanvas.g.setClip(var1, var2, var3, var4);
      } else {
         (this.d ? this.b : this.a).a(BaseCanvas.g, "", this.q == 17 ? (this.t >> 1) - this.y : this.c, 0, this.q);
      }
   }

   public void a() {
      if (this.b && BaseCanvas.ticks % 10 > 3) {
         BaseCanvas.g.setColor(15597568);
      } else {
         if (this.d != 1) {
            return;
         }

         if (gs.a == 0) {
            BaseCanvas.g.setColor(gs.e);
         } else {
            BaseCanvas.g.setColor(802440);
         }
      }

      BaseCanvas.g.fillRect(0, 0, this.t, this.u);
   }

   public final void h() {
      int var1 = 0;
      if (this.e != null) {
         var1 = this.b.a(this.e);
      }

      if (this.d == 0) {
         this.d = var1 - (this.t - (this.y << 1) - (this.a != null && this.q != 1 ? this.b + this.a.a : 0));
      } else {
         this.d = var1;
      }

      if (this.d > 0 && this.t > 0) {
         this.a = System.currentTimeMillis();
         this.b = 1000L;
      }

   }

   private void a(int var1) {
      this.k = false;
      this.c = System.currentTimeMillis();
      this.d = (long)var1;
   }

   public void c() {
      super.c();
      long var1 = System.currentTimeMillis();
      if (this.b != -1L && var1 - this.a >= this.b) {
         this.b = -1L;
         this.k = true;
      }

      if (this.d != -1L && var1 - this.c >= this.d) {
         this.d = -1L;
         if (this.d == 1) {
            this.w = -this.u;
         } else {
            this.c = 0;
         }
      }

      if (this.k && this.s == this.w && this.r == this.v) {
         this.c -= 2;
         if (this.c < -this.d) {
            this.a(1000);
         }
      }

      if (this.s <= -this.u) {
         fw.a = null;
      }

      if (BaseCanvas.ticks % 3 == 0) {
         gr var10000 = this.a;
      }

   }

   public void g() {
      super.g();
      if (this.l) {
         this.h();
      }

   }

   public void b_() {
      super.b_();
      if (this.l && this.k) {
         this.a(0);
      }

   }
}
