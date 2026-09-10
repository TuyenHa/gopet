import java.util.Vector;
import vn.me.core.BaseCanvas;

public final class gl extends go implements gz {
   public gn a;
   private Vector a;
   private gg a;
   private gg b;
   private int a;
   private int e;
   public boolean a;

   public gl() {
      this.a = gv.a;
      this.b = gv.a;
      this.a = Math.max(this.a.a(), this.b.a()) + 2 * gs.p;
      this.e = 0;
      this.a = false;
      this.y = 1;
      this.x = 3;
      this.e = new cd(0, gw.a(0), this);
      this.d = new cd(2, gw.a(7), this);
      this.k = true;
      this.i = true;
   }

   public final void a(Vector var1, int var2) {
      this.e = 0;
      this.j = false;
      if (this.a != null && this.a != var1) {
         this.a = true;
      }

      this.a = var1;
      int var3 = var1.size();
      this.a = Math.max(this.a.a(), this.b.a()) + 2 * gs.p;
      this.a.b = var3 * this.a;
      this.u = this.a.b + (this.x + this.y << 1) > BaseCanvas.Field162 ? BaseCanvas.Field162 : this.a.b + (this.x + this.y << 1);
      var3 = var3;

      while(true) {
         --var3;
         if (var3 < 0) {
            if (this.a.a < BaseCanvas.Field157) {
               this.a.a = BaseCanvas.Field157;
            }

            this.t = this.a.a + 2 * (this.y + this.x);
            this.w = BaseCanvas.h - gs.n - this.u - 1;
            this.s = BaseCanvas.h - gs.n;
            if (var2 == 0) {
               this.r = 1;
               this.v = 1;
               return;
            }

            if (var2 == 1) {
               int var7 = BaseCanvas.w - this.t;
               this.r = var7;
               this.v = var7;
               return;
            }

            int var6 = BaseCanvas.Field157 - (this.t >> 1);
            this.r = var6;
            this.v = var6;
            return;
         }

         int var4 = this.a.a(((cd)var1.elementAt(var3)).a);
         this.a.a = this.a.a > var4 ? this.a.a : var4;
      }
   }

   public final void b() {
      BaseCanvas.g.translate(-this.A, -this.B);
      if (this.a != null && !this.a.isEmpty()) {
         int var1 = this.a.size();

         for(int var2 = 0; var2 < var1; ++var2) {
            if (this.e == var2) {
               gq.a(BaseCanvas.g, this.j ? gs.j : gs.b, this.j ? gs.i : gs.a, 0, var2 * this.a, this.t - (this.y + this.x << 1) - 1, this.a - 1, false);
            }

            this.a.a(BaseCanvas.g, ((cd)this.a.elementAt(var2)).a, gs.p, var2 * this.a + gs.p, 20);
            if (this.e == var2) {
               BaseCanvas.g.setColor(gs.d);
               BaseCanvas.g.drawRect(0, var2 * this.a, this.t - (this.y + this.x << 1) - 1, this.a - 1);
            }
         }
      }

      BaseCanvas.g.translate(this.A, this.B);
   }

   public final boolean b(int var1, int var2) {
      int var3 = this.a(var1, var2);
      this.e = var3 >= 0 ? var3 : this.e;
      this.j = var3 >= 0;
      return super.b(var1, var2);
   }

   public final boolean d(int var1, int var2) {
      if (super.d(var1, var2)) {
         this.j = false;
         return true;
      } else {
         return false;
      }
   }

   public final boolean c(int var1, int var2) {
      int var3 = this.a(var1, var2);
      if (this.e == var3) {
         this.e = var3;
         if (this.j && !this.e) {
            this.j();
            return true;
         }

         this.j = false;
      }

      return super.c(var1, var2);
   }

   private int a(int var1, int var2) {
      if (this.a != null && !this.a.isEmpty()) {
         int var3 = this.a.size();

         for(int var4 = 0; var4 < var3; ++var4) {
            int var5 = var1 - this.r - this.y + this.A;
            int var6 = var2 - this.s - this.y + this.B;
            if (var5 > 0 && var5 < this.t && var6 > var4 * this.a && var6 < var4 * this.a + this.a) {
               return var4;
            }
         }

         return -1;
      } else {
         return -1;
      }
   }

   public final boolean a(int var1, int var2) {
      if (var2 != -3 && var2 != -4) {
         if (var2 == -5) {
            if (var1 != 1) {
               this.j = true;
               return true;
            } else {
               this.j();
               this.j = false;
               return true;
            }
         } else {
            boolean var3 = false;
            if (var2 == -2 && var1 == 0) {
               if (this.a != null && !this.a.isEmpty()) {
                  if (this.e < this.a.size() - 1) {
                     ++this.e;
                  } else {
                     this.e = 0;
                  }

                  var3 = true;
               }
            } else if (var2 == -1 && var1 == 0 && this.a != null && !this.a.isEmpty()) {
               if (this.e > 0) {
                  --this.e;
               } else {
                  this.e = this.a.size() - 1;
               }

               var3 = true;
            }

            if (var3) {
               this.b(0, this.e * this.a, this.t - (this.x + this.y << 1), this.a);
            }

            return var3;
         }
      } else {
         return true;
      }
   }

   public final void e() {
      super.e();
      gs.a(this);
   }

   public final void a() {
      gs.b(this);
   }

   public static void i() {
      BaseCanvas.getCurrentScreen().v();
   }

   private void j() {
      if (this.a != null && this.e >= 0 && this.e < this.a.size()) {
         if (this.a) {
            this.a = false;
         } else {
            BaseCanvas.getCurrentScreen().v();
         }

         ((cd)this.a.elementAt(this.e)).a(new cd[]{(cd)this.a.elementAt(this.e)});
      }
   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 0:
            BaseCanvas.getCurrentScreen().v();
            return;
         case 2:
            this.j();
            return;
         default:
      }
   }
}
