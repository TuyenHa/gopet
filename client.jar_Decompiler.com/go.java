import thong.sdk.ISoundManagerSDK;
import vn.me.core.BaseCanvas;

public class go extends gn {
   protected int b;
   public int c;
   public gn[] a;
   public boolean k;
   public int d;
   public gn c;
   public boolean l;

   public go(int var1, int var2, int var3, int var4) {
      this(var1, var2, var3, var4, 0);
   }

   public go(int var1, int var2, int var3, int var4, int var5) {
      super(var1, var2, var3, var4);
      this.c = 1;
      this.k = false;
      this.d = 1;
      this.l = false;
      this.b = var5;
      this.a = new gn[0];
   }

   public go() {
      this(0, 0, 1, 1, 0);
   }

   public go(int var1) {
      this(0, 0, 1, 1, 2);
   }

   public final void a(gn var1, boolean var2) {
      gn[] var3 = new gn[this.a.length + 1];
      System.arraycopy(this.a, 0, var3, 0, this.a.length);
      var3[var3.length - 1] = var1;
      this.a = var3;
      var1.b = this;
      if (this.c == null && var1.g) {
         this.c = var1;
      }

      if (var2) {
         this.h();
      }

   }

   public final void a(gn var1) {
      this.a(var1, false);
   }

   public final void b(gn var1) {
      if (var1 != null && this.a.length != 0) {
         if (var1 == this.c) {
            this.c = null;
         }

         gn[] var2 = new gn[this.a.length - 1];
         boolean var3 = false;

         for(int var4 = 0; var4 < var2.length; ++var4) {
            if (this.a[var4] == var1) {
               var3 = true;
            }

            var2[var4] = this.a[var3 ? var4 + 1 : var4];
         }

         if (var3 || var1 == this.a[this.a.length - 1]) {
            this.a = var2;
         }

         this.h();
      }

   }

   public final void o() {
      this.c = null;
      this.a = new gn[0];
   }

   public final void c(gn var1) {
      if (this.f) {
         if (var1.d && this.a.length > 1) {
            int var4 = this.c();
            if (this.k) {
               this.a(true, var4, 1);
            } else {
               this.k = true;
               this.a(true, var4, 1);
               this.k = false;
            }
         } else {
            boolean var3 = false;
            var1.d = var3;
         }

         var1.f = false;
      }

   }

   public final gn a(int var1) {
      return this.a.length != 0 && var1 >= 0 && var1 < this.a.length ? this.a[var1] : null;
   }

   public final void b(int var1) {
      this.b = var1;
      this.h();
   }

   public void h() {
      switch (this.b) {
         case 1:
            this.d_();
            return;
         case 2:
            go var1 = this;
            if (this.a.length != 0) {
               int var2 = 0;
               int var3 = 0;
               int var4 = 0;
               int var5;
               int var6 = var5 = this.a.length;

               while(true) {
                  --var6;
                  if (var6 < 0) {
                     if (var1.c > 0) {
                        if (var1.l && var1.t > 0) {
                           var3 = (var1.t - (var1.y + var1.x << 1)) / var1.c;
                           var1.a.a = var1.c * var3 + (var1.c + 1) * var1.d;
                           var1.t = var1.a.a + 2 * var1.y;
                        }

                        var1.a.a = var1.c * var3 + (var1.c + 1) * var1.d;
                     } else {
                        var6 = var1.t - 2 * var1.y;
                        var1.c = Math.max(var6 / var3, 1);
                        var2 = (var6 - var3 * var1.c) / (var1.c + 1);
                     }

                     var6 = var5 / var1.c;
                     if (var5 % var1.c != 0) {
                        ++var6;
                     }

                     var1.a.b = var6 * var4 + (var6 - 1) * var1.d;

                     for(int var15 = 0; var15 < var5; ++var15) {
                        var6 = var15 % var1.c;
                        int var8 = var15 / var1.c;
                        int var9 = var6 * var3;
                        int var10 = var8 * var4;
                        var1.a(var15).a(var9 + (var6 + 1) * var2, var10 + var8 * var1.d, var3, var4);
                     }

                     if (var1.l) {
                        var1.t = var1.a.a + 2 * var1.y;
                        var1.u = var1.a.b + 2 * var1.y;
                     }

                     return;
                  }

                  gn var7 = var1.a(var6);
                  var3 = Math.max(var3, var7.t);
                  var4 = Math.max(var4, var7.u);
               }
            }
         default:
      }
   }

   protected void d_() {
      this.a.b = 0;
      if (this.a.length != 0) {
         this.c = 1;
         int var1 = this.a.length;
         int var2 = 0;
         if (!this.l) {
            if (this.t > 0) {
               var2 = this.t - (this.y + this.x << 1);
            }
         } else {
            for(int var3 = 0; var3 < var1; ++var3) {
               if (var2 < this.a[var3].t) {
                  var2 = this.a[var3].t;
               }
            }

            this.t = this.a.a = var2 + (this.y + this.x << 1);
         }

         int var6 = 0;

         for(int var4 = 0; var4 < var1; ++var4) {
            gn var5;
            (var5 = this.a(var4)).a(0, var6, var2, var5.u);
            if (var5 instanceof go) {
               ((go)var5).h();
            }

            var6 = var5.s + var5.u + this.d;
         }

         this.a.b = var6 - this.d;
         if (this.l) {
            if (this.b != null && this.b instanceof go && ((go)this.b).l) {
               gn var10000 = this.b;
               var10000.u -= this.u;
            }

            this.u = this.a.b + (this.y + this.x << 1);
            if (this.b != null && this.b instanceof go && ((go)this.b).l) {
               gn var7 = this.b;
               var7.u += this.u;
            }

            return;
         }

         if (this.u == 0) {
            this.u = this.a.b + (this.y + this.x << 1);
         }
      }

   }

   public final int b() {
      return this.a == null ? 0 : this.a.length;
   }

   public final gn a(boolean var1) {
      while(this.a != null) {
         int var2 = this.a.length;

         gn var3;
         do {
            --var2;
            if (var2 < 0) {
               if (this.g) {
                  return this;
               }

               return null;
            }
         } while(!(var3 = this.a[var2]).d);

         if (!var1 || !(var3 instanceof go)) {
            return var3;
         }

         go var10000 = (go)var3;
         var1 = true;
         this = var10000;
      }

      return this;
   }

   public final int c() {
      if (this.a != null) {
         int var1 = this.a.length;

         while(true) {
            --var1;
            if (var1 < 0) {
               break;
            }

            if (this.a[var1].d) {
               return var1;
            }
         }
      }

      return -1;
   }

   public void c() {
      super.c();
      if (this.a != null) {
         int var1 = this.a.length;

         while(true) {
            --var1;
            if (var1 < 0) {
               break;
            }

            if (this.a[var1].f) {
               this.a[var1].c();
            }
         }
      }

   }

   public void b() {
      BaseCanvas.g.translate(-this.A, -this.B);
      if (this.a != null) {
         gn var1 = this.a(false);

         for(int var2 = 0; var2 < this.a.length; ++var2) {
            if (this.a[var2].f && var1 != this.a[var2] && !(this.a[var2] instanceof gl)) {
               this.a[var2].a_();
            }
         }

         if (var1 != this && var1 != null && !(var1 instanceof gl)) {
            var1.a_();
         }
      }

      BaseCanvas.g.translate(this.A, this.B);
   }

   public void a() {
      super.a();
   }

   public final boolean a(boolean var1, int var2, int var3) {
      while(true) {
         if (this.a != null && this.a.length != 0) {
            go var4 = this;
            int var5 = 0;

            boolean var10000;
            while(true) {
               if (var5 >= var4.a.length) {
                  var10000 = 0;
                  break;
               }

               if (var4.a[var5].g) {
                  var10000 = 1;
                  break;
               }

               ++var5;
            }

            if (!var10000) {
               return false;
            }

            int var10001 = var1 ? var3 : -var3;
            var10000 = var2 + (var1 ? var3 : -var3);
            int var8 = var2 + var10001;
            if (var10000 < 0) {
               if (this.k) {
                  var8 = this.a.length - 1;
               } else {
                  var8 = var2;
               }
            } else if (var8 >= this.a.length) {
               if (this.k) {
                  var8 = 0;
               } else {
                  var8 = var2;
               }
            }

            if (var2 != var8 && (var2 <= 0 || this.a[var2] != this.a[var8])) {
               gn var7;
               if ((var7 = this.a[var8]).f && var7.g) {
                  if (var7 instanceof go) {
                     gn var6 = ((go)var7).a();
                     BaseCanvas.getCurrentScreen().a(var6);
                  } else {
                     BaseCanvas.getCurrentScreen().a(var7);
                  }

                  return true;
               }

               var2 = var8;
               var1 = var1;
               this = this;
               continue;
            }

            return false;
         }

         return false;
      }
   }

   public final gn a() {
      if (this.c != null && this.c.f && this.c.g) {
         return this.c instanceof go ? ((go)this.c).a() : this.c;
      } else {
         return this;
      }
   }

   public boolean a(int var1, int var2) {
      boolean var3 = false;
      gn var4;
      if ((var4 = this.a(false)) != this && var4 != null && var4.a(var1, var2)) {
         return true;
      } else {
         if (var1 == 0 && var2 == -3 && this.b != 1) {
            var3 = this.a(false, this.c(), 1);
         } else if (var1 == 0 && var2 == -4 && this.b != 1) {
            var3 = this.a(true, this.c(), 1);
         } else if (var1 == 0 && var2 == -2) {
            var3 = this.a(true, this.c(), this.c);
         } else if (var1 == 0 && var2 == -1) {
            var3 = this.a(false, this.c(), this.c);
         }

         return var3;
      }
   }

   public boolean b(int var1, int var2) {
      gn var3;
      if ((var3 = this.a(var1, var2)) == this) {
         ISoundManagerSDK.playSoundEffect("s_button");
         return super.b(var1, var2);
      } else {
         return var3 != null ? var3.b(var1, var2) : false;
      }
   }

   private gn a(int var1, int var2) {
      label30:
      while(true) {
         for(int var3 = this.a.length - 1; var3 >= 0; --var3) {
            gn var4;
            if ((var4 = this.a(var3)).f && var4.e(var1, var2)) {
               if (var4 instanceof go) {
                  go var10000 = (go)var4;
                  var1 = var1;
                  this = var10000;
                  continue label30;
               }

               if (var4.g) {
                  return var4;
               }
            }
         }

         if (this.g && this.e(var1, var2)) {
            return this;
         }

         return null;
      }
   }

   public final boolean e() {
      return this.i;
   }

   public final boolean d() {
      return this.h;
   }

   public final void p() {
      int var1 = this.b();

      while(true) {
         --var1;
         if (var1 < 0) {
            this.c = null;
            return;
         }

         gn var2;
         (var2 = this.a(var1)).f = false;
         var2.d = false;
      }
   }

   public final void d(gn var1) {
      this.b(var1.r, var1.s, var1.t, var1.u);
   }

   public final void a(int var1, int var2, int var3, int var4) {
      super.a(var1, var2, var3, var4);
      this.h();
   }

   public final int d() {
      return this.t - (this.y + this.x << 1);
   }

   public final boolean a(gn var1) {
      for(int var2 = this.a.length - 1; var2 >= 0; --var2) {
         if (this.a[var2] == var1) {
            return true;
         }
      }

      return false;
   }
}
