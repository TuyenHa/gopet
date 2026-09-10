import vn.me.core.BaseCanvas;

public class gn {
   public static cd b = new cd(-1, "", (gz)null);
   public int r;
   public int s;
   public int t;
   public int u;
   public boolean d = false;
   public boolean e = false;
   public boolean f = true;
   public cd c;
   public cd d;
   public cd e;
   public int v;
   public int w;
   public int x = 0;
   public int y = 0;
   public boolean g = true;
   public gn b;
   public int z;
   private int a;
   public int A;
   public int B;
   public int C;
   public int D;
   public int E;
   public int F;
   private int b;
   private int c;
   public boolean h;
   public boolean i;
   public gx a;
   public boolean j;
   public gz b;
   public int G = 3;
   public int H;
   public int I;
   private int d;
   private int e;
   private int f;
   private int g;
   private long a = 0L;
   private long b = 0L;
   private int h;

   public gn() {
      this.a = new gx();
   }

   public gn(int var1, int var2, int var3, int var4) {
      this.r = var1;
      this.s = var2;
      this.t = var3;
      this.u = var4;
      this.v = var1;
      this.w = var2;
      this.a = new gx(var3, var4);
   }

   public void b() {
   }

   public void a() {
   }

   public void a_() {
      int var1 = BaseCanvas.g.getClipWidth();
      int var2 = BaseCanvas.g.getClipHeight();
      int var3 = BaseCanvas.g.getClipX();
      int var4 = BaseCanvas.g.getClipY();
      if (this.t <= 0 || this.u <= 0 || var3 + var1 >= this.r && var3 <= this.r + this.t && var4 + var2 >= this.s && var4 <= this.s + this.u) {
         BaseCanvas.g.translate(this.r, this.s);
         if (this.t > 0 && this.u > 0) {
            BaseCanvas.g.clipRect(0, 0, this.t, this.u);
         }

         this.a();
         int var5 = BaseCanvas.g.getClipWidth();
         int var6 = BaseCanvas.g.getClipHeight();
         int var7 = BaseCanvas.g.getClipX();
         int var8 = BaseCanvas.g.getClipY();
         int var9 = this.y + this.x;
         BaseCanvas.g.clipRect(var9, var9, this.t - (var9 << 1), this.u - (var9 << 1));
         BaseCanvas.g.translate(var9, var9);
         this.b();
         BaseCanvas.g.translate(-var9, -var9);
         BaseCanvas.g.setClip(var7, var8, var5, var6);
         this.e();
         BaseCanvas.g.translate(-this.r, -this.s);
         BaseCanvas.g.setClip(var3, var4, var1, var2);
      }

   }

   public void c() {
      if (this.w != this.s) {
         this.e = this.w - this.s << this.G;
         this.d += this.e;
         this.s += this.d >> 4;
         this.d &= 15;
      }

      if (this.v != this.r) {
         this.g = this.v - this.r << this.G;
         this.f += this.g;
         this.r += this.f >> 4;
         this.f &= 15;
      }

      if (this.c()) {
         if (this.B != this.D) {
            this.E = this.D - this.B << 2;
            this.F += this.E;
            this.B += this.F >> 4;
            this.F &= 15;
            this.i();
         }

         if (this.A != this.C) {
            this.b = this.C - this.A << 2;
            this.c += this.b;
            this.A += this.c >> 4;
            this.c &= 15;
         }
      }

   }

   public boolean a(int var1, int var2) {
      if (var2 == -5) {
         if (var1 == 1) {
            this.j = false;
            if (this.d != null) {
               this.d.a(new Object[]{this.d, this});
               return true;
            }
         } else {
            this.j = true;
         }
      } else if (var1 == 1 && var2 == -6) {
         if (this.c != null) {
            this.c.a(new Object[]{this.c, this});
            return true;
         }
      } else if (var1 == 1 && var2 == -7 && this.e != null) {
         this.e.a(new Object[]{this.e, this});
         return true;
      }

      return false;
   }

   public boolean b(int var1, int var2) {
      this.a = System.currentTimeMillis();
      this.b = (long)var2;
      this.h();
      this.j = true;
      if (this.g) {
         BaseCanvas.getCurrentScreen().a(this);
      }

      return true;
   }

   public boolean c(int var1, int var2) {
      if (this.e) {
         this.j = false;
         this.e = false;
         int var3 = var1;
         boolean var5 = this.d();
         var1 = this.e();
         if (!(var5 && var1 ? Math.abs(BaseCanvas.instance.initialPressX - var3) > Math.abs(BaseCanvas.instance.initialPressY - var2) : var5)) {
            var1 = (int)((long)var2 - this.b);
            long var18;
            if ((var18 = System.currentTimeMillis() - this.a + 1L) != 1L) {
               int var14;
               long var9 = (long)((var14 = (int)((long)(var2 = (int)((long)(var1 << 11) / (var18 * var18))) * var18)) / ((var2 << 1) + 1));
               var1 = (int)((long)var14 * var9 - ((long)var2 * var9 * var9 >> 1) >> 8);
            } else {
               var1 = (int)(((long)var2 - this.b + 1L) * 250L / Math.abs((long)var2 - this.b + 1L));
            }

            if (this.B < 0) {
               this.D = 0;
               return true;
            }

            var2 = this.u - (this.y + this.x << 1);
            if (this.B > this.a.b - var2) {
               this.D = this.a.b - var2;
            } else {
               this.D -= var1;
            }

            if (this.D > this.a.b - var2) {
               this.D = this.a.b - var2;
            } else if (this.D < 0) {
               this.D = 0;
            }

            return true;
         }

         if (this.A < 0) {
            this.C = 0;
            return true;
         }

         var1 = this.t - 2 * this.y;
         if (this.A > this.a.a - var1) {
            this.C = this.a.a - var1;
            if (this.C < 0) {
               this.C = 0;
            }

            return true;
         }
      } else {
         if (this.j) {
            this.j = false;
            if (this.d != null) {
               this.d.a(new Object[]{this.d, this});
               return true;
            }
         }

         this.j = false;
      }

      return false;
   }

   public final boolean b() {
      return this.j;
   }

   public boolean d(int var1, int var2) {
      if (this.c()) {
         long var3;
         if ((var3 = System.currentTimeMillis()) - this.a > 500L) {
            this.a = var3;
            this.b = (long)var2;
         }

         if (!this.e) {
            this.h = var2;
            this.a = var1;
            this.e = true;
            BaseCanvas.getCurrentScreen().a = this;
            return true;
         } else {
            if (this.e()) {
               int var5 = this.B + (this.h - var2);
               this.D = this.B = var5;
               this.i();
            }

            if (this.d()) {
               int var6 = this.A + (this.a - var1);
               this.C = this.A = var6;
            }

            this.h = var2;
            this.a = var1;
            return true;
         }
      } else {
         if (this.b != null) {
            this.j = false;
            this.b.d(var1, var2);
         }

         return false;
      }
   }

   public final void b(boolean var1) {
      this.d = var1;
   }

   public final void c(boolean var1) {
      while(true) {
         this.d = var1;
         if (this.b == null || !this.b.g) {
            return;
         }

         this = this.b;
      }
   }

   public void g() {
      if (this.b != null) {
         this.b.a(new Object[]{null, this});
      }

   }

   public final boolean e(int var1, int var2) {
      int var3 = this.a() + this.A;
      int var4 = this.b() + this.B;
      return var1 >= var3 && var1 < var3 + this.t && var2 >= var4 && var2 < var4 + this.u;
   }

   private int a() {
      int var1 = this.r - this.A + this.x + this.y;
      if (this.b != null) {
         var1 += this.b.a();
      }

      return var1;
   }

   private int b() {
      int var1 = this.s - this.B + this.x + this.y;
      if (this.b != null) {
         var1 += this.b.b();
      }

      return var1;
   }

   public void b_() {
   }

   private void h() {
      while(true) {
         this.e = false;
         if (this.b == null) {
            return;
         }

         this = this.b;
      }
   }

   public final boolean c() {
      return this.d() || this.e();
   }

   public boolean d() {
      return this.h;
   }

   public boolean e() {
      return this.i;
   }

   public final cd a() {
      if (this.c != null) {
         return this.c;
      } else {
         return this.b != null ? this.b.a() : null;
      }
   }

   public final cd b() {
      if (this.e != null) {
         return this.e;
      } else {
         return this.b != null ? this.b.b() : null;
      }
   }

   public final cd c() {
      if (this.d != null) {
         return this.d;
      } else {
         return this.b != null ? this.b.c() : null;
      }
   }

   public final void b(int var1, int var2, int var3, int var4) {
      if (this.e()) {
         int var5 = this.u - (this.y + this.x << 1);
         if (var2 < this.D) {
            this.D = var2;
         } else if (var2 + var4 > this.D + var5) {
            this.D = var2 + var4 - var5;
            if (this.D >= this.a.b - var5) {
               this.D = this.a.b - var5;
            }
         }

         if (this.D < 0) {
            this.D = 0;
         }
      }

      if (this.d()) {
         int var6 = this.t - (this.y + this.x << 1);
         if (var1 < this.C) {
            this.C = var1;
         } else if (var1 + var3 > this.C + var6) {
            this.C = var1 + var3 - var6;
            if (this.C > this.a.a - var6) {
               this.C = this.a.a - var6;
            }
         }

         if (this.C < 0) {
            this.C = 0;
         }
      }

   }

   public final void n() {
      BaseCanvas.getCurrentScreen().a(this);
   }

   public void a(int var1, int var2, int var3, int var4) {
      this.t = var3;
      this.u = var4;
      this.b(var1, var2);
   }

   public final void b(int var1, int var2) {
      this.r = var1;
      this.s = var2;
      this.v = var1;
      this.w = var2;
   }

   public final void c(int var1, int var2) {
      this.t = var1;
      this.u = var2;
   }

   private void i() {
      if (this.a.b != 0) {
         this.I = this.B * (this.u - (this.x << 1)) / this.a.b + this.x;
         this.H = (this.u - (this.x << 1)) * (this.u - (this.x << 1)) / this.a.b;
      }

   }

   protected void e() {
      gs.a(this);
   }

   public final void d(int var1, int var2) {
      this.a = new gx(var1, var2);
   }
}
