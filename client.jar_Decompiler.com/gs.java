import vn.me.core.BaseCanvas;

public final class gs {
   public static int a = 345451;
   public static int b = 551328;
   public static int c = 16777215;
   public static int d = 7564651;
   public static int e = 16711680;
   private static int r = 6710886;
   public static int f = 0;
   public static int g = 5378829;
   public static int h = 16777215;
   public static int i = 16748544;
   public static int j = 16433664;
   public static int k = 40;
   public static int l = 20;
   public static int m = 20;
   public static int n = 20;
   public static int o = 8;
   public static int p = 3;
   public static int q = 6;
   public static byte a;

   public static void a() {
      a = 1;
      d = 1160191;
      f = 16777215;
      g = 8955067;
      h = 0;
   }

   public static void a(String var0) {
      BaseCanvas.g.translate(-BaseCanvas.g.getTranslateX(), -BaseCanvas.g.getTranslateY());
      BaseCanvas.g.setClip(0, 0, BaseCanvas.w, l);
      gq.a(BaseCanvas.g, a, b, 0, 0, BaseCanvas.w, l, false);
      BaseCanvas.g.setColor(c);
      BaseCanvas.g.drawLine(0, l - 1, BaseCanvas.w, l - 1);
      gv.a.a(BaseCanvas.g, var0, BaseCanvas.Field157, l - gv.a.a() >> 1, 17);
   }

   public static void a(fw var0) {
      BaseCanvas.g.translate(-BaseCanvas.g.getTranslateX(), -BaseCanvas.g.getTranslateY());
      BaseCanvas.g.setClip(0, 0, BaseCanvas.w, BaseCanvas.h);
      if (fw.b != null && !var0.f) {
         BaseCanvas.g.drawImage(fw.b, 0, BaseCanvas.h - n + 1, 20);
      }

      gg var10000;
      cd var10001;
      cd var10002;
      cd var10003;
      if (var0.a != null) {
         var10000 = var0.a;
         var10001 = null;
         var10002 = var0.a.d;
         var10003 = var0.a.e;
      } else {
         if (fw.a != null) {
            gn var5;
            if ((var5 = fw.a.a(true)) == null) {
               a(var0.a, fw.a.c, fw.a.d, fw.a.e);
               return;
            }

            a(var0.a, var5.a(), var5.c(), var5.b());
            return;
         }

         if (var0.a() == null) {
            var10000 = var0.a;
            var10001 = var0.l;
            var10002 = var0.m;
            var10003 = var0.n;
         } else {
            gn var1;
            cd var2 = (var1 = var0.a()).a();
            cd var3 = var1.c();
            cd var4 = var1.b();
            var10000 = var0.a;
            var10001 = var0.a == null && var2 == null ? var0.l : var2;
            var10002 = var0.a == null && var3 == null ? var0.m : var3;
            var10003 = var0.a == null && var4 == null ? var0.n : var4;
         }
      }

      a(var10000, var10001, var10002, var10003);
   }

   private static void a(gg var0, cd var1, cd var2, cd var3) {
      if (var1 != null) {
         var0.a(BaseCanvas.g, var1.a, p, (n - gv.a.a() >> 1) + BaseCanvas.h - n, 20);
      }

      if (var2 != null) {
         var0.a(BaseCanvas.g, var2.a, BaseCanvas.Field157, (n - gv.a.a() >> 1) + BaseCanvas.h - n, 17);
      }

      if (var3 != null) {
         var0.a(BaseCanvas.g, var3.a, BaseCanvas.w - p, (n - gv.a.a() >> 1) + BaseCanvas.h - n, 24);
      }

   }

   public static void a(int var0, int var1) {
      BaseCanvas.g.translate(0, 0);
      BaseCanvas.g.setColor(16777215);
      BaseCanvas.g.fillRect(2, 1, var0 - 4, 1);
      BaseCanvas.g.fillRect(2, var1 - 2, var0 - 4, 1);
      BaseCanvas.g.fillRect(1, 2, 1, var1 - 4);
      BaseCanvas.g.fillRect(var0 - 2, 2, 1, var1 - 4);
      BaseCanvas.g.drawRect(2, 2, var0 - 5, var1 - 5);
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.drawRoundRect(0, 0, var0 - 1, var1 - 1, 10, 10);
      BaseCanvas.g.drawRect(3, 3, var0 - 7, var1 - 7);
      BaseCanvas.g.translate(0, 0);
   }

   public static void a(int var0, int var1, int var2, int var3) {
      BaseCanvas.g.translate(var0, var1);
      BaseCanvas.g.setColor(a);
      BaseCanvas.g.fillRect(3, 3, var2 - 6, var3 - 6);
      a(var2, var3);
      BaseCanvas.g.translate(-var0, -var1);
   }

   public static void a(gd var0) {
      gq.a(BaseCanvas.g, a, b, 3, 3, var0.t - 6, var0.u - 2, false);
   }

   public static void a(gb var0) {
      if (var0.b()) {
         if (var0.e >>> 24 == 0) {
            gq.a(BaseCanvas.g, j, i, 1, 1, var0.t - 2, var0.u - 2, false);
         } else {
            BaseCanvas.g.setColor(var0.e);
            BaseCanvas.g.fillRoundRect(0, 0, var0.t - 1, var0.u - 1, 6, 6);
         }
      } else {
         if (var0.d && var0.c == 0) {
            if (var0.e >>> 24 == 0) {
               gq.a(BaseCanvas.g, b, a, 1, 1, var0.t - 2, var0.u - 2, false);
               return;
            }

            BaseCanvas.g.setColor(var0.e);
            BaseCanvas.g.fillRoundRect(0, 0, var0.t - 1, var0.u - 1, 6, 6);
         } else if (var0.d >>> 24 != 0) {
            BaseCanvas.g.setColor(var0.d);
            BaseCanvas.g.fillRoundRect(0, 0, var0.t - 1, var0.u - 1, 6, 6);
            return;
         }

      }
   }

   public static void b(gb var0) {
      if (var0.d) {
         BaseCanvas.g.setColor(d);
         BaseCanvas.g.drawRoundRect(1, 1, var0.t - 3, var0.u - 3, 6, 6);
      }

   }

   public static void c(gb var0) {
      if ((var0.c == 1 || var0.c == 2) && var0.a && var0.a != null) {
         var0.a.a(BaseCanvas.g, 2, 1, var0.a.b - var0.a.b >> 1, 0, 20);
      }

   }

   public static void d(gb var0) {
      if (a == 0) {
         if (var0.a || var0.d) {
            if (var0.d) {
               BaseCanvas.g.setColor(var0.e);
            } else {
               BaseCanvas.g.setColor(r);
            }

            BaseCanvas.g.fillRect(2, 2, var0.t - 4, var0.u - 2);
            BaseCanvas.g.setColor(3289650);
            BaseCanvas.g.fillRect(2, 0, var0.t - 4, 1);
            BaseCanvas.g.fillRect(0, 2, 1, var0.u - 1);
            BaseCanvas.g.fillRect(var0.t - 1, 2, 1, var0.u - 1);
            BaseCanvas.g.fillRect(1, 1, 1, 1);
            BaseCanvas.g.fillRect(var0.t - 2, 1, 1, 1);
            return;
         }
      } else {
         if (!var0.a && !var0.d) {
            BaseCanvas.g.setColor(var0.d);
            BaseCanvas.g.fillRect(1, 3, var0.t - 2, var0.u - 2);
            BaseCanvas.g.setColor(6515815);
            BaseCanvas.g.fillRect(1, 2, var0.t - 2, 1);
            BaseCanvas.g.fillRect(0, 3, 1, var0.u - 2);
            BaseCanvas.g.fillRect(1, 3, 1, 1);
            BaseCanvas.g.fillRect(var0.t - 2, 3, 1, 1);
            BaseCanvas.g.setColor(3289650);
            BaseCanvas.g.fillRect(var0.t - 1, 3, 1, var0.u - 2);
            return;
         }

         if (var0.d) {
            BaseCanvas.g.setColor(var0.e);
         } else {
            BaseCanvas.g.setColor(14478591);
         }

         BaseCanvas.g.fillRect(2, 2, var0.t - 4, var0.u - 2);
         BaseCanvas.g.setColor(3289650);
         BaseCanvas.g.fillRect(2, 0, var0.t - 4, 1);
         BaseCanvas.g.fillRect(0, 2, 1, var0.u - 1);
         BaseCanvas.g.fillRect(var0.t - 1, 2, 1, var0.u - 1);
         BaseCanvas.g.fillRect(1, 1, 1, 1);
         BaseCanvas.g.fillRect(var0.t - 2, 1, 1, 1);
         BaseCanvas.g.setColor(16777215);
         BaseCanvas.g.fillRect(2, 1, var0.t - 4, 1);
         BaseCanvas.g.fillRect(1, 2, 1, var0.u - 1);
         BaseCanvas.g.fillRect(var0.t - 2, 2, 1, var0.u - 1);
         BaseCanvas.g.fillRect(2, 2, 1, 1);
         BaseCanvas.g.fillRect(var0.t - 3, 2, 1, 1);
      }

   }

   public static void a(ge var0) {
      if (var0.d) {
         BaseCanvas.g.setColor(d);
      } else {
         BaseCanvas.g.setColor(c);
      }

      if (var0.c == 0) {
         BaseCanvas.g.drawRoundRect(0, 0, var0.t - 1, var0.u - 1, q, q);
      } else {
         BaseCanvas.g.drawRoundRect(0 + var0.c, 0, var0.t - 1 - var0.c, var0.u - 1, q, q);
      }
   }

   public static void a(gk var0) {
      if (var0.b()) {
         gq.a(BaseCanvas.g, j, i, 0, 0, var0.t - 1, var0.u - 1, false);
      } else {
         if (var0.d) {
            gq.a(BaseCanvas.g, a, b, 0, 0, var0.t - 1, var0.u - 1, false);
         }

      }
   }

   public static void b(gk var0) {
      BaseCanvas.g.setColor(5592405);
      BaseCanvas.g.drawLine(0, 0, var0.t - 1, 0);
      if (var0.d) {
         BaseCanvas.g.setColor(d);
         BaseCanvas.g.drawRoundRect(0, 0, var0.t - 1, var0.u - 1, q, q);
      }

   }

   public static void a(gl var0) {
      BaseCanvas.g.setColor(16777215);
      BaseCanvas.g.fillRect(2, 1, var0.t - 4, 1);
      BaseCanvas.g.fillRect(2, var0.u - 2, var0.t - 4, 1);
      BaseCanvas.g.fillRect(1, 2, 1, var0.u - 4);
      BaseCanvas.g.fillRect(var0.t - 2, 2, 1, var0.u - 4);
      BaseCanvas.g.drawRect(2, 2, var0.t - 5, var0.u - 5);
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.drawRoundRect(0, 0, var0.t - 1, var0.u - 1, 10, 10);
      BaseCanvas.g.drawRect(3, 3, var0.t - 7, var0.u - 7);
   }

   public static void b(gl var0) {
      BaseCanvas.g.setColor(345451);
      BaseCanvas.g.fillRect(3, 3, var0.t - 6, var0.u - 6);
   }

   public static void a(gn var0) {
      if (var0.B != var0.D || var0.e) {
         BaseCanvas.g.setClip(BaseCanvas.g.getClipX() + var0.x, BaseCanvas.g.getClipY() + var0.x, BaseCanvas.g.getClipWidth() - (var0.x << 1), BaseCanvas.g.getClipHeight() - (var0.x << 1));
         BaseCanvas.g.setColor(201059015);
         BaseCanvas.g.fillRoundRect(var0.t - 4 - var0.x, var0.I, 4, var0.H, 4, 4);
         BaseCanvas.g.setColor(14582307);
         BaseCanvas.g.fillRect(var0.t - 3 - var0.x, var0.I + 2, 2, var0.H - 4);
         BaseCanvas.g.setClip(BaseCanvas.g.getClipX() - var0.x, BaseCanvas.g.getClipY() - var0.x, BaseCanvas.g.getClipWidth() + (var0.x << 1), BaseCanvas.g.getClipHeight() + (var0.x << 1));
      }

   }
}
