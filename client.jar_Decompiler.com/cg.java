import java.util.Vector;
import javax.microedition.io.ConnectionNotFoundException;
import javax.microedition.lcdui.Command;
import javax.microedition.lcdui.CommandListener;
import javax.microedition.lcdui.Display;
import javax.microedition.lcdui.Displayable;
import javax.microedition.midlet.MIDlet;
import vn.me.core.BaseCanvas;

public final class cg extends fw implements gz, CommandListener {
   public static w a;
   public static int a;
   public static boolean a;
   private static String e;
   public static cg a;
   private String f;
   private String g;
   public static int b;
   public static String a;
   public static String b;
   public static cd a;
   public static cd b;
   public static cd c;
   public static cd d;
   public static cd e;
   public static cd f;
   public static cd g;
   public static cd h;
   private static cd o;
   private static cd p;
   private static cd q;
   private static cd r;
   public static cd i;
   public static cd j;
   private static cd s;
   private static cd t;
   public static cd k;
   public static String[] a;
   public static String[] b;
   public static byte[] a;
   public static byte[] b;
   private static String[] c = new String[]{a.a(559), a.a(129)};
   private static boolean b = true;

   private static void k() {
      a.a = a.a(337);
      b.a = a.a(41);
      c.a = a.a(40);
      d.a = a.a(63);
      e.a = a.a(64);
      f.a = a.a(222);
      g.a = a.a(249);
      h.a = a.a(503);
      p.a = a.a(337);
      q.a = a.a(123);
      i.a = a.a(276);
      o.a = a.a(495);
      t.a = a.a(18);
      k.a = a.a(284);
   }

   public static String a() {
      return BaseCanvas.instance.midlet.getAppProperty("MIDlet-Name");
   }

   public static String b() {
      return BaseCanvas.instance.midlet.getAppProperty("MIDlet-Version");
   }

   public cg() {
      a = new cd(0, a.a(337), this);
      b = new cd(1, a.a(41), this);
      c = new cd(3, a.a(40), this);
      d = new cd(4, a.a(63), this);
      e = new cd(5, a.a(64), this);
      f = new cd(6, a.a(222), this);
      g = new cd(7, a.a(249), this);
      h = new cd(8, a.a(503), this);
      p = new cd(10, a.a(337), this);
      ge.a = new cd(11, a.a(123), this);
      i = new cd(17, a.a(276), this);
      j = new cd(18, a.a(24), this);
      o = new cd(24, a.a(495), this);
      q = new cd(26, a.a(123), this);
      t = new cd(39, a.a(18), this);
      s = new cd(41, a.a(668), this);
      k = new cd(51, a.a(284), this);
      Integer var1;
      b = (var1 = a.a("guide")) == null ? 0 : var1;
      b = (var1 = a.a("vibrate")) == null || var1 != 0;
   }

   public static void a(MIDlet var0) {
      BaseCanvas.create(var0).Method0();
      cx.c = Integer.parseInt(BaseCanvas.instance.midlet.getAppProperty("ProviderId"));
      (new fx(0)).a(0, false);
   }

   public static void a() {
      BaseCanvas.instance.resetScreen();
   }

   public static void c() {
      BaseCanvas.isRunning = false;
      a = null;
      if (cx.a != null) {
         cx.a.b();
      }

      if (BaseCanvas.instance != null) {
         BaseCanvas.instance.midlet.notifyDestroyed();
      }

      BaseCanvas.instance = null;
   }

   public static void e_() {
      BaseCanvas.isPause = true;
   }

   public static void f_() {
      BaseCanvas.isPause = false;
      BaseCanvas.instance.resetScreen();
   }

   public static void a_(String var0) {
      a(var0, false);
   }

   public static void a(String var0, boolean var1) {
      gd.a(var0, (cd)null, a, (cd)null, var1);
   }

   public static void b(String var0) {
      b(var0, false);
   }

   public static void f() {
      a(false);
   }

   public static void a(boolean var0) {
      b(a.a(363), var0);
   }

   public static void b(String var0, boolean var1) {
      (new gm(var0, (cd)null, b, (cd)null, 2)).a(var1);
   }

   public static void a(String var0, cd var1, cd var2) {
      gd.a(var0, (cd)null, var1, var2, true);
   }

   public static void c(String var0, boolean var1) {
      gd.a(var0, a, var1);
   }

   public static void c(String var0) {
      c(var0, true);
   }

   public static void d(String var0) {
      try {
         BaseCanvas.getCurrentScreen();
         fw.b(fw.a);
         BaseCanvas.instance.midlet.platformRequest("sms:?body=" + var0);
      } catch (Exception var1) {
         ap var2 = new ap(a.a(222), var0, a, Display.getDisplay(BaseCanvas.instance.midlet));
         BaseCanvas.isPause = true;
         Display.getDisplay(BaseCanvas.instance.midlet).setCurrent(var2);
      }
   }

   public final void a(Object var1) {
      cd var13;
      switch ((var13 = (cd)((Object[])var1)[0]).a) {
         case -1:
            f_();
            return;
         case 0:
         case 1:
            BaseCanvas.getCurrentScreen();
            fw.b(fw.a);
            return;
         case 2:
            this.m();
            return;
         case 3:
            cx.a = new ch(this);
            cx.b = new ci(this);
            if (cx.a()) {
               cx.a.a((Object)null);
               return;
            }

            b(a.a(83), false);
            cx.a();
            return;
         case 4:
            String var24 = a.a(87);
            if (BaseCanvas.w <= 128) {
               String var25;
               var24 = (var25 = var24.trim()).equals(gw.a(31)) ? gw.a(31) : var25;
            } else {
               var24 = var24;
            }

            a(a.a(73), new cd(401, var24, this), new cd(402, a.a(103), this));
            return;
         case 5:
            BaseCanvas.getCurrentScreen().t();
            return;
         case 6:
            a(false);
            (new Thread(new bw())).start();
            return;
         case 7:
            try {
               BaseCanvas.instance.midlet.platformRequest(cx.f);
               return;
            } catch (ConnectionNotFoundException var12) {
               return;
            }
         case 8:
            cx.a = new cj(this);
            if (cx.a()) {
               cx.a.a((Object)null);
               cx.a = null;
               return;
            }

            b(a.a(83), false);
            cx.a();
            return;
         case 9:
            a(a.a(532), new String[]{a.a(299), a.a(376)}, new int[]{0, 1}, p, b);
            return;
         case 10:
            gi var21;
            String var29 = (var21 = (gi)fw.a).a(0);
            String var22 = var21.a(1);

            try {
               Object[] var32;
               (var32 = new Object[2])[0] = var29;
               int var23 = Integer.parseInt(var22.length() == 0 ? "0" : var22);
               var32[1] = new Integer(var23);
               gd.a(a.a(299) + " " + var29 + ". " + a.a(376) + " " + var23 + ".", new cd(1001, a.a(580), this), b);
               return;
            } catch (Exception var11) {
               return;
            }
         case 11:
            gn var31;
            if ((var31 = BaseCanvas.getCurrentScreen().a().a(true)) instanceof ge) {
               a((ge)var31);
               return;
            }

            return;
         case 16:
            String var20 = ((gi)fw.a).a.a().trim();
            if ("".equals(var20)) {
               return;
            }

            BaseCanvas.getCurrentScreen();
            fw.b(fw.a);
            a(false);
            cx.a(var20, i.a);
            return;
         case 17:
            a(false);
            en var19;
            (var19 = new en(121)).a(13);
            cx.a.a(var19);
            var19.a();
            return;
         case 18:
            a(false);
            cx.e();
            return;
         case 25:
            gi var18 = (gi)fw.a;
            if ("".equals(var18.a.a())) {
               BaseCanvas.getCurrentScreen();
               fw.b(fw.a);
               return;
            }

            cx.b(var18.a.a());
            a(a.a(504), true);
            return;
         case 26:
            a(BaseCanvas.getCurrentScreen().a);
            return;
         case 30:
            Vector var17 = new Vector();

            for(int var28 = 0; var28 < c.length; ++var28) {
               var17.addElement(new cd(var28 + 31, c[var28], this));
            }

            BaseCanvas.getCurrentScreen().a(var17, 2);
            return;
         case 31:
            try {
               ((gb)fw.a.a(true)).e = c[0];
               a.a("language", 0);
               k();
               n();
               return;
            } catch (Exception var10) {
               return;
            }
         case 32:
            try {
               ((gb)fw.a.a(true)).e = c[1];
               a.a("language", 1);
               k();
               n();
               return;
            } catch (Exception var9) {
               return;
            }
         case 39:
            a(false);
            cx.h();
            return;
         case 41:
            if (a == null) {
               cx.i();
               return;
            } else {
               if (a != null && a.length != 0) {
                  gd var27 = new gd(0, BaseCanvas.h, BaseCanvas.w, BaseCanvas.h);
                  gj var16;
                  (var16 = new gj("Bản đồ", gv.a)).a(0, 0, var27.t, var16.u);
                  var16.y = 0;
                  var16.x = 0;
                  var16.q = 17;
                  var27.a(var16);
                  var27.a(new ac());
                  int var30 = 0;
                  int var33 = 0;

                  for(int var36 = 0; var36 < a.length; ++var36) {
                     if (a[var36] != a.b) {
                        gb var37;
                        (var37 = new gb(var30 + 1 + "." + a[var36], gv.a)).a(0, 0, var27.t, var37.u);
                        var37.y = 0;
                        var37.x = 0;
                        var37.q = 17;
                        var37.d = new cd(42, a.a(419), a);
                        var37.d.a = new Integer[]{new Integer(a[var36]), new Integer(b[var36])};
                        var27.a(var37);
                        ++var30;
                        var33 += 4 + var37.u;
                     }
                  }

                  var27.b(1);
                  var27.u = var16.u + 2 + (gs.p << 1) + var33;
                  var27.w = BaseCanvas.Field158 - (var27.u >> 1);
                  var27.e = b;
                  var27.k = true;
                  var27.a(true);
                  return;
               }

               return;
            }
         case 42:
            Integer[] var2 = (Integer[])var13.a;
            BaseCanvas.currentScreen.x();
            if (var2 == null && var2.length != 2) {
               return;
            }

            dv.a(var2[0], var2[1], ef.a(var2[0]));
            a(false);
            return;
         case 43:
            int[] var15 = (int[])var13.a;
            a(true);
            cx.c(var15[0], var15[1]);
            return;
         case 44:
            int[] var3 = (int[])var13.a;
            a(true);
            cx.d(var3[0], var3[1]);
            return;
         case 45:
            BaseCanvas.currentScreen.x();
            return;
         case 46:
            if (!a(fw.a)) {
               c(a.a(473), false);
               return;
            } else {
               String[] var4;
               if ((var4 = a(fw.a)) != null) {
                  int[] var35;
                  cx.a((var35 = (int[])var13.a)[0], var35[1], var35[2], var4);
                  BaseCanvas.currentScreen.x();
                  return;
               }

               return;
            }
         case 47:
            try {
               gb var34;
               if ((var34 = (gb)a.a(true)) != null) {
                  var34.a = !var34.a;
                  b = var34.a;
                  return;
               }

               return;
            } catch (Exception var8) {
               return;
            }
         case 48:
            a.a("vibrate", b ? 1 : 0);
            BaseCanvas.getCurrentScreen();
            fw.b(fw.a);
            return;
         case 51:
            Vector var5;
            (var5 = new Vector()).addElement(s);
            var5.addElement(new cd(1103, gw.a(53) + " (" + a.c + ")", ((ew)BaseCanvas.currentScreen).a));
            BaseCanvas.currentScreen.a(var5, 1);
            return;
         case 52:
            Vector var6;
            (var6 = new Vector()).addElement(new cd(53, a.a(670), this));
            var6.addElement(new cd(54, a.a(671), this));
            BaseCanvas.currentScreen.a(var6, 1);
            return;
         case 100:
            g();
            return;
         case 101:
            c();
            return;
         case 131:
            try {
               int var14 = Integer.parseInt(((gi)fw.a).a(0));
               a(true);
               cx.a(3, var14);
               return;
            } catch (Exception var7) {
               return;
            }
         case 401:
            a.a();
            a(a.a(390), true);
            return;
         case 402:
            BaseCanvas.getCurrentScreen();
            fw.b(fw.a);
            return;
         default:
      }
   }

   private void m() {
      cx.a(this.f, "sms://" + this.g, new ck(this), new cl(this));
   }

   public static void g() {
      r = new cd(101, a.a(580), a);
      gd.a(a.a(604) + (cx.a.b + cx.a.a >> 10) + a.a(140), r, b);
   }

   public static void e(String var0) {
      try {
         if (var0 == null) {
            BaseCanvas.instance.midlet.platformRequest("tel:" + c());
         } else {
            BaseCanvas.instance.midlet.platformRequest("tel:" + var0);

            try {
               a.a("Hotline", var0.getBytes());
            } catch (Exception var1) {
            }

            e = var0;
         }
      } catch (ConnectionNotFoundException var2) {
      }
   }

   public static String c() {
      if (e == null) {
         byte[] var0;
         String var10000 = (var0 = a.a("Hotline")) == null ? null : new String(var0);
         String var1 = var10000;
         e = var10000;
         if (var1 == null) {
            e = BaseCanvas.instance.midlet.getAppProperty("Hotline");
         }
      }

      return e;
   }

   public final void a(byte var1, String var2, String var3) {
      switch (var1) {
         case 1:
            if (b != null && b.length() != 0) {
               a(a.a(435), true);
               cx.a(var2.substring(0, var2.indexOf("?")) + b, "sms://" + var3, new cm(this), new cn(this));
               b = null;
               return;
            }

            a(a.a(178), true);
            return;
         default:
      }
   }

   public final void a(String var1, boolean var2, String var3, String var4) {
      this.f = var3;
      this.g = var4;
      if (var2) {
         this.m();
      } else {
         a(var1 + " " + a.a(167), true);
      }
   }

   public static void a(String var0, String var1, String var2) {
      fw var3;
      (var3 = new fw(true)).d = var1;
      var3.c = var0;
      av var4 = new av();
      var3.m = e;
      var4.a(0, gs.m, BaseCanvas.w, BaseCanvas.h - (gs.m << 1));
      var3.b((gn)var4);
      var4.a(var2, gv.a);
      var3.a(1, true);
   }

   public static void a(Vector var0) {
      ev var1;
      ev var4;
      (var4 = var1 = new ev(a.a(384))).b.b(var4.a);
      int var3 = var0.size();
      var4.b = Math.max(cp.f.getHeight(), cp.f.getWidth()) + (gs.p << 1);
      var4.a = new bp(var4, var0);
      var4.a.a(0, gs.l, BaseCanvas.w, BaseCanvas.h - (gs.l << 1));
      var4.a.i = true;
      var4.a.k = true;
      var4.a.y = gs.p;
      var4.a.c = (var4.a.t - 2 * var4.a.y) / var4.b;
      var4.a.d = (var4.a.t - 2 * var4.a.y - var4.b * var4.a.c) / (var4.a.c + 1);
      var3 /= var4.a.c;
      if (var0.size() % var4.a.c != 0) {
         ++var3;
      }

      var4.a.a.a = var4.a.c * var4.b;
      var4.a.a.b = var3 * var4.b;
      var4.b.a(var4.a);
      var4.a.d = new cd(5, a.a(231), var0, var4);
      var4.a = 0;
      var4.c = (var4.a.u / var4.b + 2) * var4.a.c;
      var4.a.n();
      var1.a(1, true);
   }

   public static gi a(String var0, String[] var1, int[] var2, cd var3, cd var4) {
      gi var5;
      (var5 = new gi(var0, var1, var2, var3, var4)).i();
      return var5;
   }

   public static gi a(String var0, cd var1, cd var2, int var3) {
      gi var4;
      (var4 = new gi(var0, var1, var2 == null ? b : var2, var3)).a(false);
      return var4;
   }

   public static gi a(String[] var0, int[] var1, cd var2, int var3) {
      gi var4;
      ge var5 = (ge)(var4 = a(BaseCanvas.w <= 128 ? a.a(5) + " mGold: " + dj.b : a.a(5) + " mGold: " + dj.b, var0, var1, var2, b)).a(2);
      ge var6;
      (var6 = (ge)var4.a(4)).g = false;
      var6.a = 15;
      var5.a = new co(var5, var6, var3);
      return var4;
   }

   public static void b(Vector var0) {
      fn var1;
      (var1 = new fn()).a(var0);
      var1.a(1, true);
   }

   public static void h() {
      if (fw.a != null && fw.a instanceof gm && ((gm)fw.a).a == 2) {
         fw.a.j();
      }

   }

   public static void f(String var0) {
      (new gi(var0, new cd(16, a.a(337), a), b, 0)).a(false);
   }

   public static void i() {
      fb var0;
      (var0 = (fb)BaseCanvas.getCurrentScreen()).x();
      var0.b = false;
      (new i()).a(true);
   }

   private static void a(ge var0) {
      (new gf(var0)).a(false);
   }

   public final void commandAction(Command var1, Displayable var2) {
   }

   public static void j() {
      int var0 = BaseCanvas.w < 128 ? BaseCanvas.w - 4 : BaseCanvas.w - 20;
      gd var1 = new gd();
      c = new String[]{a.a(559), a.a(129)};
      gj var2;
      (var2 = new gj(a.a(459))).q = 17;
      ac var3 = new ac();
      gj var4;
      (var4 = new gj(a.a(247) + ":")).a(0, 0, var1.t, var4.a.a() + 8);
      var4.g = true;
      Integer var5;
      gb var10;
      (var10 = (var5 = a.a("language")) != null ? new gb(c[var5]) : new gb(c[0])).q = 17;
      var10.b = a;
      var1.i = true;
      var1.k = true;
      var1.d = 0;
      var1.a(var2);
      var1.a(var3);
      var1.a(var4);
      var1.a(var10);
      (var4 = new gj(a.a(611), gv.a)).a(0, 0, var1.t, var4.a.a() + 10);
      gb var6;
      (var6 = new gb(1)).a((String)a.a(558));
      var6.a(0, 0, var1.t - (var1.x + var1.y << 1), gv.a.a() + 10);
      var6.a(gv.a, new gx(gv.a.getWidth(), gv.a.getWidth()));
      Integer var7;
      b = (var7 = a.a("vibrate")) == null || var7 != 0;
      var6.a = b;
      var6.d = new cd(47, a.a(419), a);
      var1.a(var4);
      var1.a(var6);
      int var11 = BaseCanvas.w - var0 >> 1;
      int var8 = var2.u * 3 + (var10.u << 1) + var3.u + (var1.x + var1.y << 2) + 5 + var4.u + var6.u;
      var1.a(var11, BaseCanvas.h - var8 - gs.m - 10, var0, var8);
      var1.b(1);
      var1.e = new cd(48, a.a(41), a);
      var1.a(true);
   }

   private static void n() {
      gd var0 = new gd();
      c = new String[]{a.a(559), a.a(129)};
      gj var1;
      (var1 = new gj(a.a(459))).q = 17;
      ac var2 = new ac();
      gj var3 = new gj(a.a(247) + ":", cp.a);
      Integer var4;
      gb var7;
      (var7 = (var4 = a.a("language")) != null ? new gb(c[var4]) : new gb(c[0])).q = 17;
      int var5 = var1.u * 3 + (var7.u << 1) + var2.u + (var0.x + var0.y << 1) + 5;
      int var6 = BaseCanvas.w < 128 ? BaseCanvas.w - 4 : BaseCanvas.w - 20;
      var0.a(BaseCanvas.w - var6 >> 1, BaseCanvas.h - var5 >> 1, var6, var5);
      var0.d = 0;
      var0.a(var1);
      var0.a(var2);
      var0.a(var3);
      var0.a(var7);
      var0.b(1);
      var0.e = b;
      var0.a(true);
   }

   public static gd a(int var0, String var1, int[] var2, String[] var3, byte[] var4, cd[] var5) {
      gd var6;
      (var6 = new gd()).y = gs.p;
      var6.e = b;
      var6.t = BaseCanvas.w - (gs.p << 1);
      var6.a.a = var6.t - 2 * var6.y;
      av var7;
      (var7 = new av()).t = var6.t - (var6.y << 1);
      var7.b = 0;
      var7.y = 0;
      var7.a = 17;
      var7.a(var1, gv.a);
      var7.g = false;
      var7.a(0, 0, var6.d(), var7.a.b);
      var6.a(var7);
      var6.a(new ac(), true);
      int var16 = (var6.x + var6.y << 1) + var7.a.b + 4 + var6.d;
      go var8;
      (var8 = new go(0, 0, var6.d(), 60, 1)).y = 0;
      var8.x = 0;
      var8.i = true;
      int var9 = 0;

      for(int var10 = 0; var10 < var3.length; ++var10) {
         gb var13;
         if (var4[var10] == 0) {
            var13 = new gb(var3[var10], cp.a);
         } else {
            var13 = new gb(var3[var10]);
            (new int[]{var0, 0})[1] = var2[var10];
         }

         var13.d = var5[var10];
         var9 += var6.d + var13.u;
         var8.a(var13);
      }

      int var17;
      var0 = var17 = var16 + var9;
      if (var17 < 60) {
         var0 = 60;
      }

      int var14 = BaseCanvas.h - gs.m - (gs.p << 1);
      if (var0 > var14) {
         var0 = var14;
      }

      var8.a(0, 0, var8.t, var0 - var16);
      var6.a(gs.p, BaseCanvas.h - gs.m - gs.p - var0, var6.t, var0);
      var6.a(var8);
      var6.b(1);

      for(int var12 = 0; var12 < var8.b(); ++var12) {
         gn var15 = var8.a(var12);
         if (var4[var12] != 0) {
            var15.n();
            break;
         }
      }

      return var6;
   }

   public static gd a(int var0, int var1, int var2, String var3, String[] var4, byte[] var5, cd var6) {
      var0 = (var0 = gv.b.a("n") * 45 + (gs.o << 1)) < BaseCanvas.w ? var0 : BaseCanvas.w;
      gd var13 = new gd(gs.o + (BaseCanvas.w - var0 >> 1), gs.p, var0 - (gs.o << 1), BaseCanvas.h - (gs.o << 1));
      gj var14;
      (var14 = new gj(var3)).q = 17;
      var13.a(var14);
      var13.a(new ac(), true);
      var13.u = var4.length * 20 + var14.u + 2 + (gs.p << 1);
      go var15 = new go(0, var14.r + var14.u + gs.o, var13.t, var13.u - (gs.p + var14.u + gs.o));
      if (var13.u > BaseCanvas.h) {
         var13.u = BaseCanvas.h;
      }

      var15.a.a = var13.t;
      var15.a.b = var4.length * (20 + gs.p);
      int var16 = 0;

      for(int var7 = var4.length - 1; var7 >= 0; --var7) {
         if (var16 < gv.a.a(var4[var7])) {
            var16 = gv.a.a(var4[var7]);
         }
      }

      int var18 = var16 + gs.p;
      var16 = var4.length;

      for(int var8 = 0; var8 < var16; ++var8) {
         gj var9;
         (var9 = new gj(var4[var8])).a(0, var8 * 20, var18, var9.u + (gs.p << 1));
         ge var10 = new ge(var18 + gs.p, var8 * 20, var15.t - var18 - (gs.p << 2), 20);
         if (var5[var8] == 0) {
            var10.a(0);
         } else {
            var10.a(1);
         }

         var15.a(var9);
         var15.a(var10);
         if (var8 == 0) {
            var10.n();
         }
      }

      var15.b(0);
      var15.i = true;
      var13.a(var15);
      var13.s = BaseCanvas.h - var13.u - (gs.o << 1) - gs.n;
      var13.a(var13.r, var13.s, var13.t, var13.u);
      var13.c = b;
      if (var6 == null) {
         var13.d = new cd(46, a.a(337), new int[]{122, 7, var2}, a);
      } else {
         var13.d = var6;
      }

      var13.b(1);
      var13.a(true);
      return var13;
   }

   public static gd a(int var0, String var1, String[] var2, byte[] var3) {
      return a(122, 7, var0, var1, var2, var3, (cd)null);
   }

   private static boolean a(gd param0) {
      // $FF: Couldn't be decompiled
   }

   public static String[] a(gd var0) {
      try {
         go var5;
         String[] var1 = new String[(var5 = (go)var0.a(2)).b() >> 1];
         int var2 = var5.b();

         for(int var3 = 1; var3 < var2; var3 += 2) {
            if (var5.a(var3) instanceof ge) {
               var1[var3 >> 1] = ((ge)var5.a(var3)).a().trim();
            }
         }

         return var1;
      } catch (Exception var4) {
         return null;
      }
   }

   public static void a(int var0, String var1, String var2, byte var3) {
      cd var4 = null;
      cd var5 = null;
      cd var6 = null;
      int[] var7 = new int[]{var0, var3};
      switch (var3) {
         case 0:
            var4 = new cd(44, a.a(101), var7, a);
            var6 = b;
            break;
         case 1:
            var4 = new cd(44, a.a(4), var7, a);
            var6 = b;
            break;
         case 2:
            var5 = a;
            var6 = null;
      }

      a(var1, var2, var4, var5, var6);
   }

   public static void a(String var0, String var1, cd var2, cd var3, cd var4) {
      gd var5;
      (var5 = new gd()).y = gs.p;
      var5.c = var2;
      var5.d = var3;
      var5.e = var4;
      var5.t = BaseCanvas.w - (gs.p << 1);
      var5.a.a = var5.t - 2 * var5.y;
      av var8;
      (var8 = new av()).t = var5.t - (var5.y << 1);
      var8.b = 0;
      var8.y = 0;
      var8.a = 17;
      var8.a(var0, gv.a);
      var8.g = false;
      av var6;
      (var6 = new av()).t = var8.t;
      var6.b = 0;
      var6.y = 0;
      var6.a(var1, cp.c);
      var5.u = var8.a.b + var6.a.b + (var5.y + var5.x << 1) + 4 + (var5.d << 1);
      if (var5.u < 60) {
         var5.u = 60;
      }

      int var7 = BaseCanvas.h - gs.m - (gs.p << 1);
      if (var5.u > var7) {
         var5.u = var7;
      }

      var5.a(gs.p, BaseCanvas.h - gs.m - gs.p - var5.u, var5.t, var5.u);
      var8.a(0, 0, var5.d(), var8.a.b);
      var6.a(0, 0, var5.d(), var5.u - var8.u - (var5.y + var5.x << 1) - 4 - (var5.d << 1));
      var5.a(var8);
      var5.a(new ac(), true);
      var5.a(var6, true);
      var5.b(1);
      var5.a(true);
   }

   public static String d() {
      try {
         String var0;
         String var1 = var0 = System.getProperty("lac");
         if (var0 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("com.nokia.mid.lac");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("LocAreaCode");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("phone.lac");
         }

         return var1 == null ? "" : var1;
      } catch (Exception var2) {
         return "";
      }
   }

   public static String e() {
      try {
         String var0;
         String var1 = var0 = System.getProperty("mcc");
         if (var0 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("phone.mcc");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("com.nokia.mid.mcc");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("com.nokia.mid.countrycode");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("com.lge.cmcc");
         }

         return var1 == null ? "" : var1;
      } catch (Exception var2) {
         return "";
      }
   }

   public static String f() {
      try {
         String var0;
         String var1 = var0 = System.getProperty("mnc");
         if (var0 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("phone.mnc");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("com.nokia.mid.networkid");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("com.nokia.mid.mnc");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("com.lge.cmnc");
         }

         return var1 == null ? "" : var1;
      } catch (Exception var2) {
         return "";
      }
   }

   public static String g() {
      try {
         String var0;
         String var1 = var0 = System.getProperty("Cell-ID");
         if (var0 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("CellID");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("com.nokia.mid.cellid");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("phone.cid");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("com.samsung.cellid");
         }

         if (var1 == null || var1.equals("null") || var1.equals("")) {
            var1 = System.getProperty("com.siemens.cellid");
         }

         return var1 == null ? "" : var1;
      } catch (Exception var2) {
         return "";
      }
   }

   public static String h() {
      String[] var0 = new String[]{"com.nokia.mid.mnc", "IMSI", "phone.imsi", "com.nokia.mid.mobinfo.IMSI", "com.nokia.mid.imsi", "com.sonyericsson.sim.subscribernumber", "imsi", "com.sonyericsson.imsi", "com.siemens.imei", "com.samsung.imei", "com.samsung.IMEI", "com.nokia.mid.networkid", "com.siemens.mid.networkid", "com.sonyericsson.mid.networkid", "com.motorola.mid.networkid", "com.samsung.mid.networkid"};
      String var1 = "";

      for(int var2 = 0; var2 < var0.length; ++var2) {
         String var3;
         var1 = var3 = System.getProperty(var0[var2]);
         if (var3 != null && !var1.equals("null") && !var1.equals("")) {
            break;
         }
      }

      return var1;
   }

   public static bu a(gn[] var0) {
      bu var1 = new bu(BaseCanvas.Field157, BaseCanvas.Field158);

      for(int var2 = 0; var2 < var0.length; ++var2) {
         var0[var2].b = var1;
         if (var0[var2].d != null) {
            var0[var2].d.a = a.a(419);
         }

         if (var0[var2] instanceof gb) {
            ((gb)var0[var2]).a(gv.b, cp.c);
            ((gb)var0[var2]).q = 17;
         } else if (var0[var2] instanceof gj) {
            ((gj)var0[var2]).a((gg)cp.d, (gg)null);
         }
      }

      var1.a = var0;
      var1.l = true;
      var1.b(1);
      var1.e = b;
      return var1;
   }

   public static String a(cg var0) {
      return var0.f;
   }

   public static String b(cg var0) {
      return var0.g;
   }
}
