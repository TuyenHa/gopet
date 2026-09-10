import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.Vector;
import javax.microedition.lcdui.Graphics;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class fb extends fw implements gz {
   private String e;
   private String f = cg.b() + "beta";
   private String g = gw.a(23);
   private String h = "TAE.";
   public static boolean a = true;
   public static String a;
   public static String b;
   private ge b;
   private ge c;
   private ge d;
   private gb a;
   private int a;
   private int b;
   private int c;
   private int d;
   private int e;
   private byte a = 0;
   private cd a;
   private cd b;
   private cd c;
   private cd d;
   private cd e;
   private cd f;
   private cd g;
   private cd h;
   private gj b;
   private int f = 0;
   private final ef a;
   public long a;
   public boolean b = true;
   private gd b;
   private gd c;
   private Image a;
   private Image c;
   private int g;
   private int h = 2;

   public final void g_() {
      super.g_();
      this.j();
      cx.c = cx.b;
      cx.b = cx.a;
   }

   public fb() {
      super(true);
      this.f = true;
      this.a = cp.c();
      this.c = "LOGIN";
      this.g = gs.o;
      this.b = new ge(this.a, BaseCanvas.Field158 - gs.m - gs.p, this.b, gs.m);
      this.c = new ge(this.a, BaseCanvas.Field158, this.b, gs.m);
      this.d = new ge(this.a, BaseCanvas.Field158 + gs.m + gs.p, this.b, gs.m);
      this.a = new gb(1);
      this.b = new gj(a.a(331), gv.a);
      gu.a("/lg.dat");
      this.c = gu.a(0);
      this.a(0);
      this.a = new ef(11, new dv());
      fx.f();
      this.k();
      ek.a = false;
   }

   private void a(int var1) {
      String var3 = BaseCanvas.w > 240 ? a.a(331) : a.a(330) + " " + a.a(362);
      gj var4 = new gj("000000");
      int var10000 = gs.p;

      int var2;
      do {
         int var5;
         var2 = var5 = var4.a.a(var3 + var4.e) + (gs.p << 1);
         if (var5 >= BaseCanvas.w) {
            var2 = BaseCanvas.w - (gs.p << 1);
            break;
         }

         var4.e = var4.e.substring(0, var4.e.length() - 1);
      } while(var2 > BaseCanvas.w - (gs.p << 1) && var4.e.length() > 0);

      this.b = var2 - (this.g << 1);
      this.a = (BaseCanvas.w - var2 >> 1) + this.g;
      if (var1 == 0) {
         this.c = (gs.p << 2) + (gs.m << 1) + this.b.u + this.a.u;
      } else if (var1 == 1) {
         this.c = gs.p * 6 + gs.m * 3;
      }

      if (BaseCanvas.h >= 176) {
         this.e = gs.m + (gs.o << 1);
      } else {
         this.e = gs.m + gs.p;
      }

      int var12 = BaseCanvas.h - gs.m - this.c;
      if (BaseCanvas.w > 240 && BaseCanvas.h > 240) {
         if (var12 - gs.m - gs.p > this.c.getHeight() + this.e) {
            var12 -= var12 - gs.m - gs.p - this.c.getHeight() - this.e >> 1;
         }
      } else if (var12 - gs.m - gs.p > this.c.getHeight() + this.e - 20) {
         var12 -= var12 - gs.m - gs.p - this.c.getHeight() - this.e + 20 >> 1;
      }

      if (var12 - gs.m - gs.p * 3 < 0) {
         var12 = gs.m + gs.p * 3;
      }

      this.b.a(this.a, var12 - gs.m - gs.p, this.b, gs.m);
      this.b.a(a.a(298));
      this.b.a(3);
      this.c.a(this.a, var12, this.b, gs.m);
      this.d.a(this.a, var12 + gs.m + gs.p, this.b, gs.m);
      this.c.a(2);
      this.d.a(2);
      this.a.a((String)a.a(488));
      this.a.a(gv.a, gv.a);
      this.a.a((BaseCanvas.w >> 1) - (this.a.t >> 1) - gs.p, var12 + gs.m + gs.p, this.a.t + gv.a.getWidth() + gs.o, this.a.u);
      this.a.a(gv.a, new gx(gv.a.getWidth(), gv.a.getWidth()));
      if (BaseCanvas.w >= 240) {
         this.b.a(a.a(331));
         this.b.a(a.a(298));
         this.c.a(a.a(375));
         this.d.a(a.a(397));
         var1 = gv.a.a(a.a(397)) + gs.p * 5;
      } else {
         this.b.a(a.a(330) + " " + a.a(362));
         this.b.a(a.a(298));
         this.c.a(a.a(348));
         this.d.a(a.a(482));
         var1 = gv.a.a(a.a(482)) + gs.p * 5;
      }

      this.b.b(var1);
      this.c.b(var1);
      this.d.b(var1);
      this.b.a(0, this.a.s + this.a.u, BaseCanvas.w, this.b.u);
      this.b.q = 17;
      this.b.a(this.b);
      this.b = new cd(1, a.a(385), this);
      String var7 = a.a(266);
      if (BaseCanvas.w <= 128) {
         String var8;
         var7 = (var8 = var7.trim()).equals(a.a(266)) ? "Đ.nhập" : (var8.equals(a.a(385)) ? "Đ.ký" : var8);
      } else {
         var7 = var7;
      }

      cd var10 = new cd(2, var7, this);
      this.a = var10;
      this.m = var10;
      this.l = new cd(3, a.a(275), this);
      this.c = new cd(4, a.a(154), this);
      this.d = new cd(5, a.a(337), this);
      this.e = new cd(6, a.a(580), this);
      this.f = new cd(7, a.a(300), this);
      this.g = new cd(8, a.a(220), this);
      this.h = new cd(9, a.a(139), this);
      this.b = new go(0, 0, BaseCanvas.w, BaseCanvas.h);
      this.j();
      this.b.k = true;
      this.b.b(a.a("nick"));
      this.c.b(a.a("pass"));
      this.a.a = this.b.a().length() > 0;
      this.a.e = this.a;
      ge var11 = this.d;
      super.b.c(var11);
      this.e = a.a(205) + ": " + cg.c();
      this.b.n();
      this.b(this.b);
      this.b(this.c);
      this.b(this.d);
      this.b(this.a);
      this.b(this.b);
      this.b = new gd(BaseCanvas.w - var2 >> 1, 0, var2, 120);
      this.b.c = new cd(151, a.a(548), this);
      this.b.e = new cd(17, a.a(41), this);
      this.b.s = -100;
      this.c = new gd(BaseCanvas.w - var2 >> 1, 0, var2, 120);
      this.c.s = -100;
   }

   private void h() {
      this.a(1);
      this.m = this.b;
      this.a = 1;
      this.b.f = true;
      this.c.f = true;
      this.d.f = true;
      this.a.f = false;
      this.b.f = false;
      this.j();
   }

   private void i() {
      this.a = 0;
      this.a(0);
      this.b.c(this.d);
      this.a.f = true;
      this.b.f = true;
      this.b.f = true;
      this.c.f = true;
      this.m = this.a;
      this.j();
   }

   public final void c_() {
      super.c_();
      if (this.e != this.d) {
         this.d += this.e - this.d >> 1;
      }

      this.f += this.h;
      if (this.f < 0 || this.f >= this.a.b * 24 - BaseCanvas.w) {
         this.h = -this.h;
      }

   }

   public final void b() {
      this.a.a(this.f, 0, true);
   }

   public final void h_() {
      if (BaseCanvas.w >= 240 && BaseCanvas.h > 240) {
         this.a(BaseCanvas.g, BaseCanvas.Field157, this.d);
      } else {
         this.a(BaseCanvas.g, BaseCanvas.Field157, this.d - 20);
      }

      if (this.f != null) {
         int var1 = BaseCanvas.h - gs.m - cp.c().a() - 1;
         if (BaseCanvas.h >= 240) {
            cp.c().a(BaseCanvas.g, this.g, 1, BaseCanvas.h - gs.m - 2 * cp.c().a() - 1, 20);
            cp.c().a(BaseCanvas.g, this.h, 1, BaseCanvas.h - gs.m - cp.c().a() - 1, 20);
         } else {
            cp.c().a(BaseCanvas.g, this.g, 1, BaseCanvas.h - gs.m - 2 * cp.c().a() + 5, 20);
            cp.c().a(BaseCanvas.g, this.h, 1, BaseCanvas.h - gs.m - cp.c().a() + 1, 20);
         }

         cp.c().a(BaseCanvas.g, this.f, BaseCanvas.w - 1, var1, 24);
      }

      if (this.e != null) {
         cp.c().a(BaseCanvas.g, this.e, BaseCanvas.w - 2, 1, 24);
      }

      super.h_();
   }

   private void a(Graphics var1, int var2, int var3) {
      var1.drawImage(this.c, var2, var3, 17);
      if (this.b) {
         int var5 = BaseCanvas.w - this.b - (this.g << 1) >> 1;
         var2 = this.b.s - 5;
         var3 = this.b + (this.g << 1);
         int var4 = this.c;
         BaseCanvas.g.translate(var5, var2);
         BaseCanvas.g.setColor(gs.b);
         BaseCanvas.g.fillRect(3, 3, var3 - 6, var4 - 6);
         gs.a(var3, var4);
         BaseCanvas.g.translate(-var5, -var2);
      }

   }

   private void j() {
      this.b.r = -this.a;
      this.c.r = BaseCanvas.w + this.a;
      this.d.r = -this.a;
      this.a.r = -this.a;
      this.d = -this.c.getHeight();
   }

   public final void e() {
      super.e();
      if (this.b.f) {
         this.a(0);
         this.i();
      } else {
         this.a(1);
         this.h();
      }
   }

   public static void f() {
      try {
         ByteArrayOutputStream var0 = new ByteArrayOutputStream();
         DataOutputStream var1 = new DataOutputStream(var0);
         int var2 = cx.a.size();
         var1.writeInt(var2);

         for(int var3 = 0; var3 < var2; ++var3) {
            dw var4 = (dw)cx.a.elementAt(var3);
            var1.writeUTF(var4.a);
            var1.writeUTF(var4.b);
            var1.writeInt(var4.a);
         }

         var1.close();
         a.a("server_list");
         a.a("server_list", var0.toByteArray());
         ByteArrayOutputStream var8 = new ByteArrayOutputStream();
         DataOutputStream var9 = new DataOutputStream(var8);

         for(int var6 = 0; var6 < var2; ++var6) {
            dw var7 = (dw)cx.a.elementAt(var6);
            var9.writeBoolean(var7.a);
            var9.writeBoolean(var7.b);
         }

         var9.close();
         a.a("server_list_log_reg");
         a.a("server_list_log_reg", var8.toByteArray());
      } catch (Exception var5) {
         var5.printStackTrace();
      }
   }

   private static boolean b() {
      cx.a.removeAllElements();
      byte[] var0;
      if ((var0 = a.a("server_list")) == null) {
         dw var7 = new dw("test", "160.30.136.115", 19180);
         cx.a.addElement(var7);
         return false;
      } else {
         try {
            DataInputStream var5;
            int var1 = (var5 = new DataInputStream(new ByteArrayInputStream(var0))).readInt();

            for(int var2 = 0; var2 < var1; ++var2) {
               cx.a.addElement(new dw(var5.readUTF(), var5.readUTF(), var5.readInt()));
            }

            var5.close();
            byte[] var8;
            if ((var8 = a.a("server_list_log_reg")) == null) {
               return true;
            } else {
               DataInputStream var6 = new DataInputStream(new ByteArrayInputStream(var8));

               for(int var9 = 0; var9 < var1; ++var9) {
                  dw var3;
                  (var3 = (dw)cx.a.elementAt(var9)).a = var6.readBoolean();
                  var3.b = var6.readBoolean();
               }

               var6.close();
               return true;
            }
         } catch (IOException var4) {
            return false;
         }
      }
   }

   public final void g() {
      b();
      if (this.a == null) {
         this.a = gu.a("/common.dat", 20);
      }

      this.b.o();
      gj var1;
      (var1 = new gj(a.a(428), gv.a)).a(0, 0, this.b.t, var1.u);
      var1.q = 17;
      ac var2;
      (var2 = new ac()).a(0, var1.u, this.b.t, var2.u);
      int var3 = var1.u + var2.u;
      go var4;
      (var4 = new go(0, 0, this.b.t, var3)).a(var1);
      var4.a(var2);
      go var8 = new go(0, var1.u + 2, this.b.t, this.b.u - var3 - 2);
      int var9 = cx.a.size();
      var3 = 0;

      for(int var5 = 0; var5 < var9; ++var5) {
         dw var6 = (dw)cx.a.elementAt(var5);
         boolean var7 = false;
         switch (this.a) {
            case 0:
               var7 = var6.a;
               break;
            case 1:
               var7 = var6.b;
         }

         if (var7) {
            gb var13;
            (var13 = new gb()).a((Image)this.a);
            var13.a(gv.a, gv.a);
            var13.e = var6.a;
            var13.d = new cd(16, a.a(337), this);
            var13.a(0, var3 * gs.m + 2, this.b.t - (this.b.x << 1), gs.m);
            var8.a(var13);
            ++var3;
         }
      }

      int var11;
      int var12 = var11 = 30 + 21 * var8.b();
      if (var11 < 93) {
         var12 = 93;
      }

      this.b.a(BaseCanvas.w - this.b.t >> 1, BaseCanvas.h - var12 >> 1, this.b.t, this.b.u);
      this.b.u = var12;
      this.b.a(var4);
      this.b.a(var8);
      var8.b(0);
      this.b.b(0);
      var8.i = true;
      this.b.a(true);
      if (var8.b() > 0) {
         var8.a(0).c(true);
      }

   }

   private void k() {
      try {
         Integer var1;
         if ((var1 = a.a(a.a)) != null && var1 >= 0) {
            return;
         }
      } catch (Exception var8) {
      }

      if (this.a == null) {
         this.a = gu.a("/common.dat", 20);
      }

      this.c.o();
      gj var9;
      (var9 = new gj(gw.a(143), gv.a)).a(0, 0, this.c.t, var9.u);
      var9.q = 17;
      ac var2;
      (var2 = new ac()).a(0, var9.u, this.c.t, var2.u);
      int var3 = var9.u + var2.u;
      go var4;
      (var4 = new go(0, 0, this.c.t, var3)).a(var9);
      var4.a(var2);
      go var10 = new go(0, var9.u + 2, this.c.t, this.c.u - var3 - 2);
      int var11 = 0;
      String[] var12 = new String[]{gw.a(145), gw.a(144)};

      for(int var5 = 0; var5 < var12.length; ++var5) {
         gb var6;
         (var6 = new gb()).a(gv.a, gv.a);
         var6.e = var12[var5];
         var6.d = new cd(153, a.a(337), new fc(this, var5));
         var6.a(0, var11 * gs.m + 2, this.c.t - (this.c.x << 1), gs.m);
         var10.a(var6);
         ++var11;
      }

      int var13;
      int var14 = var13 = 30 + 21 * var10.b();
      if (var13 < 93) {
         var14 = 93;
      }

      this.c.a(BaseCanvas.w - this.b.t >> 1, BaseCanvas.h - var14 >> 1, this.b.t, this.b.u);
      this.c.u = var14;
      this.c.a(var4);
      this.c.a(var10);
      var10.b(0);
      this.c.b(0);
      var10.i = true;
      this.c.a(true);
      if (var10.b() > 0) {
         var10.a(0).c(true);
      }

   }

   public final void a(int var1, boolean var2) {
      super.a(var1, var2);
      this.k();
   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 0:
         case 10:
            fw.b(fw.a);
            return;
         case 1:
            if (this.a != 1) {
               this.h();
               return;
            } else {
               a = this.b.a().toLowerCase().trim();
               b = this.c.a();
               String var9 = this.d.a();
               if (a.equals("")) {
                  this.b.n();
                  cg.a_(a.a(132));
                  return;
               } else if (b.equals("")) {
                  this.c.n();
                  cg.a_(a.a(133));
                  return;
               } else {
                  if (b.equals(var9)) {
                     this.g();
                     return;
                  }

                  this.d.n();
                  cg.a_(a.a(322));
                  return;
               }
            }
         case 2:
            if (this.a != 0) {
               this.i();
               return;
            } else {
               a = this.b.a().toLowerCase().trim();
               b = this.c.a();
               if (a.equals("")) {
                  this.b.n();
                  cg.a_(a.a(393));
                  return;
               } else {
                  if (!b.equals("")) {
                     this.g();
                     return;
                  }

                  this.c.n();
                  cg.a_(a.a(394));
                  return;
               }
            }
         case 3:
            Vector var8;
            (var8 = new Vector()).addElement(this.a == 0 ? this.b : this.a);
            var8.addElement(cg.h);
            var8.addElement(this.c);
            var8.addElement(new cd(18, gw.a(24), this));
            var8.addElement(this.h);
            this.a(var8, 0);
            return;
         case 4:
            this.a(new gi(a.a(519) + ":", this.d, cg.b, 0));
            return;
         case 5:
            String var7;
            cg.b = var7 = ((gi)fw.a).a.a();
            if (var7 != null && cg.b.length() != 0) {
               gd.a(a.a(356), this.e, (cd)null, this.f, true);
               return;
            }

            cg.a(a.a(178), true);
            return;
         case 6:
            cg.a(true);
            cx.a = new fd(this);
            if (cx.a()) {
               cx.a.a((Object)null);
               return;
            }

            cg.b(a.a(83), true);
            cx.a();
            return;
         case 7:
            cg.a(a.a(174), true);
            return;
         case 8:
            cg.a_("\n" + a.a(371) + ": " + cg.a() + '\n' + a.a(557) + ": " + cg.b() + a.a(353) + "\n\n");
            return;
         case 9:
            cg.c();
            return;
         case 16:
            String var2;
            dw var4;
            cx.c = var2 = (var4 = (dw)cx.a.elementAt(((go)this.b.a(1)).c())).b;
            cx.b = var2;
            int var5;
            cx.b = var5 = var4.a;
            cx.a = var5;
            cx.b();
            if (this.a != 0) {
               if (this.a == 1) {
                  cx.a = new fe(this);
                  if (cx.a()) {
                     cx.a.a((Object)null);
                     return;
                  }

                  cg.b(a.a(83), true);
                  cx.a();
                  return;
               }

               return;
            } else {
               String var6 = a;
               var2 = b;
               cg.a = this.a.a;
               a = var6;
               b = var2;
               cx.a = new ff(this);
               if (cx.a()) {
                  cx.a.a((Object)null);
                  return;
               }

               cg.b(a.a(83), true);
               cx.a();
               return;
            }
         case 17:
            this.b.j();
            return;
         case 18:
            Vector var3;
            (var3 = new Vector()).addElement(cg.c);
            var3.addElement(cg.d);
            var3.addElement(cg.f);
            var3.addElement(cg.g);
            var3.addElement(this.g);
            this.a(var3, 0);
            return;
         case 19:
            return;
         case 151:
            cx.a = new fg(this);
            if (cx.a()) {
               cx.a.a((Object)null);
               return;
            }

            cg.b(a.a(83), true);
            cx.a();
            return;
         case 152:
            cx.a((gz)(new fh(this)));
            return;
         default:
            super.a(var1);
      }
   }
}
