import java.util.Stack;
import java.util.Vector;
import javax.microedition.lcdui.Graphics;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public class fw implements gz {
   public boolean e;
   public Stack a;
   public String c;
   public cd l;
   public cd m;
   public cd n;
   private gn b;
   private gn c;
   private gn d;
   public static Image b;
   public ge a;
   public gl a;
   public go b;
   public boolean f;
   public gg a;
   public gn a;
   public static gd a;
   public static gj a;
   private boolean a;
   public String d;
   public static Vector d = new Vector();
   public static Vector e = new Vector();

   public fw() {
      this(false);
   }

   public fw(boolean var1) {
      this.e = false;
      this.a = new Stack();
      this.f = false;
      this.a = false;
      if (var1) {
         this.b = new go(0, 0, BaseCanvas.w, BaseCanvas.h);
         this.a = true;
         f();
         this.b = new gn();
         this.b.a(0, BaseCanvas.h - gs.m, BaseCanvas.w / 3, gs.m);
         this.c = new gn();
         this.c.a(BaseCanvas.w / 3, BaseCanvas.h - gs.m, BaseCanvas.w / 3, gs.m);
         this.d = new gn();
         this.d.a((BaseCanvas.w << 1) / 3, BaseCanvas.h - gs.m, BaseCanvas.w / 3, gs.m);
         this.b.d = new cd(0, "", this);
         this.c.d = new cd(1, "", this);
         this.d.d = new cd(2, "", this);
         this.b.g = false;
         this.c.g = false;
         this.d.g = false;
      }

      this.a = gv.a;
   }

   public final void s() {
      for(int var1 = 0; var1 < d.size(); ++var1) {
         gd var2;
         (var2 = (gd)d.elementAt(var1)).c();
         if (var2.b) {
            d.removeElement(var2);
            if (d.isEmpty()) {
               a = null;
               if (this.b != null) {
                  this.b.a().n();
               }
            } else {
               a = var2 = (gd)d.lastElement();
               var2.a().n();
            }
         }
      }

      if (a != null) {
         a.c();
      }

      int var3 = 0;

      while(var3 < e.size()) {
         c var5;
         if ((var5 = (c)e.elementAt(var3)) != null && var5.a) {
            var5.a(System.currentTimeMillis());
         }

         if (var5.a) {
            ++var3;
         } else {
            e.removeElement(var5);
         }
      }

   }

   public void e() {
      if (this.b != null) {
         this.b.t = BaseCanvas.w;
         this.b.u = BaseCanvas.h;
         if (this.a) {
            gn var1 = this.b;
            gn var2 = this.c;
            gn var3 = this.d;
            int var4 = BaseCanvas.Field159;
            var3.t = var4;
            var2.t = var4;
            var1.t = var4;
            var1 = this.b;
            var2 = this.c;
            var3 = this.d;
            var4 = BaseCanvas.Field159;
            var3.u = var4;
            var2.u = var4;
            var1.u = var4;
            this.c.r = BaseCanvas.Field159;
            this.d.r = BaseCanvas.Field160;
            var1 = this.b;
            var2 = this.c;
            var3 = this.d;
            var4 = BaseCanvas.h - gs.m;
            var3.s = var4;
            var2.s = var4;
            var1.s = var4;
         }
      }

      b = null;
      f();
   }

   public final void d(int var1) {
      this.a(0, false);
   }

   public void a(int var1, boolean var2) {
      if (BaseCanvas.getCurrentScreen() != this) {
         switch (var1) {
            case -1:
               if (this.b != null) {
                  this.b.r = -BaseCanvas.w;
               }
               break;
            case 1:
               this.b.r = BaseCanvas.w;
         }

         if (var2 && (this.a.empty() || this.a.peek() != BaseCanvas.getCurrentScreen() || ((fw)this.a.peek()).c.equals(BaseCanvas.getCurrentScreen().c))) {
            if (BaseCanvas.getCurrentScreen().a != null) {
               BaseCanvas.getCurrentScreen().v();
            }

            this.a.push(BaseCanvas.getCurrentScreen());
         }
      }

      gq.a();
      BaseCanvas.setCurrentScreen(this);
      if (!d.isEmpty()) {
         var1 = d.size();

         while(true) {
            --var1;
            if (var1 < 0) {
               return;
            }

            if (!((gd)d.elementAt(var1)).c) {
               b((gd)d.elementAt(var1));
               ((gd)d.elementAt(var1)).b = true;
            }
         }
      }
   }

   public final void t() {
      if (!this.a.empty()) {
         ((fw)this.a.pop()).a(-1, false);
      }
   }

   private static void f() {
      if (b == null) {
         Image var0;
         b = var0 = Image.createImage(BaseCanvas.w, gs.n);
         Graphics var1 = var0.getGraphics();
         gq.a = false;
         gq.a(var1, gs.b, gs.a, BaseCanvas.w, gs.n, BaseCanvas.w, 10 + gs.n);
         gq.a = true;
         var1.setColor(0);
         var1.drawLine(0, 0, BaseCanvas.w, 0);
         var1.setColor(gs.c);
         var1.drawLine(0, 1, BaseCanvas.w, 1);
      }

   }

   public void u() {
      BaseCanvas.g.translate(-BaseCanvas.g.getTranslateX(), -BaseCanvas.g.getTranslateY());
      BaseCanvas.g.setClip(0, 0, BaseCanvas.w, BaseCanvas.h);
      this.b();
      this.h_();
      if (this.d != null) {
         gs.a(this.d);
      }

      if (a != null) {
         a.a_();
      }

      if (this.a) {
         gs.a(this);
      }

      for(int var1 = 0; var1 < e.size(); ++var1) {
         c var2;
         if ((var2 = (c)e.elementAt(var1)) != null && var2.a) {
            if (var2.b) {
               BaseCanvas.g.setClip(0, 0, BaseCanvas.w, BaseCanvas.h);
            } else {
               BaseCanvas.g.setClip(0, 0, BaseCanvas.w, BaseCanvas.h - gs.m);
            }

            var2.b();
         }
      }

      BaseCanvas.g.setClip(0, 0, BaseCanvas.w, BaseCanvas.h);
   }

   public void h_() {
      if (this.b != null) {
         this.b.a_();
      }

      if (!d.isEmpty()) {
         int var1 = d.size();

         for(int var2 = 0; var2 < var1; ++var2) {
            if (var2 < d.size()) {
               ((gd)d.elementAt(var2)).a_();
            }
         }
      }

      if (this.a != null) {
         this.a.a_();
      }

   }

   public void c_() {
      if (this.b != null) {
         this.b.c();
      }

   }

   public boolean a(int var1, int var2) {
      if (a != null && a.d != null && var2 == (Integer)a.d.a) {
         if (var1 == 1) {
            a.d.a(new cd[]{a.d});
            return true;
         } else {
            return true;
         }
      } else if (var1 == 1 && var2 == -5 && this.a == null) {
         this.b(this.a());
         return true;
      } else if (var2 == -6) {
         if (var1 == 0) {
            this.a(this.a());
            return true;
         } else {
            return true;
         }
      } else if (var2 == -7) {
         if (var1 == 0) {
            this.c(this.a());
            return true;
         } else {
            return true;
         }
      } else {
         go var3;
         return (var3 = this.a()) != null ? var3.a(var1, var2) : false;
      }
   }

   public void b() {
      BaseCanvas.g.setColor(gs.a);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
   }

   public final void g(String var1) {
      this.d = var1;
   }

   public void a(int var1, int var2) {
      if (this.b != null && this.b.e(var1, var2)) {
         this.b.j = true;
      } else if (this.c != null && this.c.e(var1, var2)) {
         this.c.j = true;
      } else if (this.d != null && this.d.e(var1, var2)) {
         this.d.j = true;
      } else {
         go var3;
         if ((var3 = this.a()) != null) {
            var3.b(var1, var2);
            if (this.a == null || this.a.e(var1, var2)) {
               return;
            }

            gl var10000 = this.a;
            gl.i();
         }

      }
   }

   public void b_(int var1, int var2) {
      if (this.a != null) {
         this.a.d(var1, var2);
      } else {
         gn var3 = null;
         if (a != null) {
            var3 = a.a(true);
         } else if (this.a != null) {
            var3 = this.a.a(true);
         } else if (this.b != null) {
            var3 = this.b.a(true);
         }

         if (var3 != null) {
            var3.d(var1, var2);
         }

      }
   }

   public void c(int var1, int var2) {
      if (this.b != null && this.b.j && this.b.e(var1, var2)) {
         this.b.j = false;
         this.a(this.a());
      } else if (this.c != null && this.c.j && this.c.e(var1, var2)) {
         this.b(this.a());
      } else if (this.d != null && this.d.j && this.d.e(var1, var2)) {
         this.d.j = false;
         this.c(this.a());
      } else if (this.a == null) {
         this.a().a(true).c(var1, var2);
      } else {
         this.a.c(var1, var2);
         this.a = null;
      }
   }

   private void a(go var1) {
      if (var1 != null) {
         gn var2;
         if ((var2 = var1.a(true)) != null) {
            var2.j = false;
            cd var3;
            if ((var3 = var2.a()) != null) {
               var3.a(new Object[]{var3, var2});
               return;
            }
         } else if (var1.c != null) {
            var1.c.a(new Object[]{var1.c, var1});
            return;
         }
      }

      if (a == null && this.a == null && this.l != null) {
         this.l.a(new Object[]{this.l, this.b});
      }

   }

   private void b(go var1) {
      if (var1 != null) {
         gn var2;
         if ((var2 = var1.a(true)) != null) {
            var2.j = false;
            cd var3;
            if ((var3 = var2.c()) != null) {
               if (this.a != null) {
                  System.out.println(this.toString() + "Hide menu khi focus");
                  this.v();
               }

               var3.a(new Object[]{var3, var2});
               return;
            }
         } else if (var1.d != null) {
            if (this.a != null) {
               this.v();
            }

            var1.d.a(new Object[]{var1.d, var1});
            return;
         }
      }

      if (a == null && this.m != null) {
         this.m.a(new Object[]{this.m, this.c});
      }
   }

   private void c(go var1) {
      if (var1 != null) {
         gn var2;
         if ((var2 = var1.a(true)) != null) {
            var2.j = false;
            cd var3;
            if ((var3 = var2.b()) != null) {
               var3.a(new Object[]{var3, var2});
               return;
            }
         } else if (var1.e != null) {
            var1.e.a(new Object[]{var1.e, var1});
            return;
         }
      }

      if (a == null && this.n != null) {
         this.n.a(new Object[]{this.n, this.d});
      }
   }

   public final void a(Vector var1, int var2) {
      if (var1 != null && !var1.isEmpty()) {
         if (this.a != null) {
            this.a.a(var1, var2);
            this.a.a = true;
         } else {
            this.a = new gl();
            this.a.a = this.b.a(true);
            this.a.a(var1, var2);
            this.b.a(this.a);
            this.a.n();
         }
      }
   }

   public final void v() {
      this.b.b(this.a);
      if (this.a != null && this.a.a != null && this.a.a.f && a == null) {
         this.a.a.n();
      }

      this.a = null;
   }

   public final void a(gn var1) {
      if (var1 != null) {
         Object var10000 = a == null ? this.b : a;
         Object var2 = var10000;
         Object var3 = var10000;
         if (var2 != null) {
            gn var4;
            if ((var4 = ((go)var3).a(true)) != var3) {
               var4.c(false);
            }

            if (var1 != null) {
               var1.c(true);

               for(gn var6 = var1.b; var6 != null; var6 = var6.b) {
                  if (var6 instanceof go) {
                     ((go)var6).c = var1;
                  }
               }

               if (var4 != null) {
                  var4.b_();
               }

               if (var1 != null) {
                  gn var7 = var1;

                  for(gn var5 = var1.b; var5 != null; var5 = var5.b) {
                     if (var5.c()) {
                        if (var5 instanceof go) {
                           ((go)var5).d(var7);
                        }
                        break;
                     }

                     var7 = var5;
                  }

                  var1.g();
               }
            }
         }

      }
   }

   public final void a(gd var1, boolean var2) {
      var1.b = false;
      var1.a = var2;
      if (var2) {
         var1.r = -var1.t;
         var1.v = BaseCanvas.w - var1.t >> 1;
      }

      var1.a = this.b.a(true);
      if (var1.a != this.b) {
         var1.a.c(false);
      }

      d.addElement(var1);
      a = var1;
      if (var1.c != null) {
         var1.c.n();
      } else if (var1.a.length > 0) {
         var1.a[0].n();
      } else {
         var1.n();
      }
   }

   public void a(gd var1) {
      this.a(var1, true);
   }

   public final void w() {
      b(a);
   }

   public static void b(gd var0) {
      if (var0 != null && !d.isEmpty()) {
         var0.m();
      }
   }

   public final void x() {
      if (this.b != null && !d.isEmpty()) {
         for(int var1 = 0; var1 < d.size(); ++var1) {
            ((gd)d.elementAt(var1)).m();
         }

         d.removeAllElements();
         a = null;
         this.b.a().n();
      }
   }

   public void l() {
   }

   public final boolean a(int var1) {
      if (a != null) {
         gn var3;
         return (var3 = a.a(true)) instanceof ge ? var3.a(0, var1) : false;
      } else if (this.b != null) {
         gn var2;
         if ((var2 = this.b.a(true)) instanceof ge) {
            return var2.a(0, var1);
         } else {
            return this.a != null && a == null && this.a == null && var1 > 0 ? this.a.a(0, var1) : false;
         }
      } else {
         return false;
      }
   }

   public static boolean a() {
      return false;
   }

   public void g_() {
      if (a != null) {
         a.d = false;
         a.n();
      }

   }

   public final void b(gn var1) {
      if (this.b != null) {
         this.b.a(var1);
      }

   }

   public final void c(gn var1) {
      if (this.b != null) {
         this.b.b(var1);
      }

   }

   public static void h(String var0) {
      if (gs.a == 0) {
         a = new gj(var0);
      } else {
         a = new gj(var0, gv.a);
      }

      a.y = gs.p;
      a.d = 1;
      a.G = 1;
      a.a(0, 0, BaseCanvas.w, gs.l);
      a.s = -a.u;
      a.h();
   }

   public void a(String var1) {
   }

   public void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case -6:
            if (this.a.a().length() == 0 && this.b.a(this.a)) {
               this.a.f = false;
               this.c((gn)this.a);
               return;
            } else {
               if (this.a.a().length() > 0 && !this.b.a(this.a)) {
                  this.a.f = true;
                  this.b((gn)this.a);
                  this.a.n();
                  if ("*".equals(this.a.a)) {
                     (new gf(this.a)).a(false);
                     this.a.h();
                     return;
                  }

                  return;
               }

               return;
            }
         case -5:
            (new gf(this.a)).a(false);
            return;
         case -3:
            Vector var3;
            (var3 = new Vector(2)).addElement(new cd(-5, gw.a(17), this));
            var3.addElement(new cd(-4, gw.a(2), this));
            this.a(var3, 0);
            return;
         case -2:
            String var2;
            if ((var2 = this.a.a().trim()).length() > 0) {
               this.a(var2);
            }
         case -4:
            this.c((gn)this.a);
            this.a.b("");
            this.a.f = false;
            return;
         case -1:
         default:
            return;
         case 0:
            this.a(1, -6);
            return;
         case 1:
            this.a(1, -5);
            return;
         case 2:
            this.a(1, -7);
      }
   }

   public final go a() {
      gl var1 = null;
      if (this.a != null) {
         var1 = this.a;
      } else {
         if (a != null) {
            return a;
         }

         if (this.b != null) {
            return this.b;
         }
      }

      return var1;
   }

   public final gn a() {
      return this.b.a(true);
   }

   public static void y() {
      for(int var0 = 0; var0 < e.size(); ++var0) {
         c var1;
         if ((var1 = (c)e.elementAt(var0)).a == 1) {
            e.removeElement(var1);
            return;
         }
      }

   }
}
