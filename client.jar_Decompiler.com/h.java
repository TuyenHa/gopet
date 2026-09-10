import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class h extends go implements gz {
   private static boolean b;
   private ge a;
   private static Image a;
   private Image b;
   private cd a;
   private cd f;
   private boolean c;
   private long a;
   private Image c;
   private int e;
   public static boolean a = false;
   public static int a = -1;
   private static Vector a = new Vector();
   private static Vector b = new Vector();
   private static final Object a = new Object();
   private int f;
   private int g;
   private al a;

   public h() {
      this.f = cp.d.a();
      this.g = 0;
      this.a = new al();
      a = -1;
      int var1 = BaseCanvas.h > 180 ? BaseCanvas.Field158 : BaseCanvas.h - gs.l;
      this.a(1, BaseCanvas.h - var1 - gs.m, BaseCanvas.w - 2, var1);
      this.a = new ge(gs.p << 2, this.u - gs.m - gs.p - 1, this.t - (gs.p << 3), gs.m);
      this.a(this.a, false);
      a = gu.a("/common.dat", 19);
      this.c = gu.a("/common.dat", 8);
      if (this.b == null) {
         int[] var5 = new int[900];
         int[] var2 = new int[3 * this.c.getWidth()];
         int var3;
         var3 = ((var3 = this.c.getWidth()) << 1) + (var3 >> 1);
         this.c.getRGB(var2, 0, this.c.getWidth(), 0, 0, this.c.getWidth(), 3);

         for(int var4 = 0; var4 < var5.length; ++var4) {
            var5[var4] = var2[var3];
         }

         this.b = Image.createRGBImage(var5, 30, 30, true);
      }

      this.a = new cd(1, gw.a(7), this);
      new cd(2, gw.a(34), this);
      this.f = new cd(3, gw.a(33), this);
      this.c = new cd(0, gw.a(2), this);
      this.d = this.f;
   }

   public static void a(String var0, String var1) {
      synchronized(a) {
         int var3;
         if ((var3 = a.size()) >= 50) {
            for(int var4 = 0; var4 < var3 - 1; ++var4) {
               g var5;
               (var5 = (g)a.elementAt(var4 + 1)).b = var4;
               a.setElementAt(var5, var4);
            }

            a.setElementAt(new g(var3 - 1, var0, var1), var3 - 1);
         } else {
            a.addElement(new g(var3, var0, var1));
         }

         b = true;
      }
   }

   public static void b(String var0, String var1) {
      synchronized(a) {
         int var3;
         if ((var3 = b.size()) >= 200) {
            for(int var4 = 0; var4 < var3 - 1; ++var4) {
               g var5;
               (var5 = (g)b.elementAt(var4 + 1)).b = var4;
               b.setElementAt(var5, var4);
            }

            b.setElementAt(new g(var3 - 1, var0, var1), var3 - 1);
         } else {
            b.addElement(new g(var3, var0, var1));
            if (b.size() == 1) {
               b.addElement(new g(var3, var0, var1));
            }
         }

      }
   }

   public final boolean a(int var1, int var2) {
      Vector var3 = a();
      if (var1 == 0) {
         switch (var2) {
            case -4:
            case -3:
               return true;
            case -2:
               ++this.g;
               if (this.g >= var3.size()) {
                  this.g = 0;
               }

               this.i();
               this.d = this.a;
               return true;
            case -1:
               --this.g;
               if (this.g < 0) {
                  this.g = var3.size() - 1;
               }

               this.i();
               this.d = this.a;
               return true;
         }
      }

      if (!this.a.d) {
         BaseCanvas.getCurrentScreen().a((gn)this.a);
      }

      if (this.d.a == this.a.a && var2 != -5 && var2 != -3 && var2 != -4 && var2 != -1 && var2 != -2) {
         this.d = this.f;
      }

      return super.a(var1, var2);
   }

   private void i() {
      Vector var1 = a();
      if (this.g >= 0 && this.g < var1.size()) {
         int var2 = 0;
         int var3 = ((g)var1.elementAt(this.g)).a;

         for(int var4 = 0; var4 <= this.g; ++var4) {
            var2 += ((g)var1.elementAt(var4)).a;
         }

         if (var2 + this.a.a <= 16) {
            this.a.a = -var2 + var3 + gs.p;
         } else {
            if (var2 + this.a.a > this.u - gs.m - 10) {
               this.a.a = -var2 + (this.u - gs.l - 10);
            }

         }
      }
   }

   private static Vector a() {
      switch (a) {
         case 1:
            return b;
         default:
            return a;
      }
   }

   public final void a() {
      int var1 = BaseCanvas.g.getClipX();
      int var2 = BaseCanvas.g.getClipY();
      int var3 = BaseCanvas.g.getClipWidth();
      int var4 = BaseCanvas.g.getClipHeight();
      BaseCanvas.g.clipRect(1, 1, this.t - 2, this.u - 2);
      int var5 = this.t / 30 + 1;

      while(true) {
         --var5;
         if (var5 < 0) {
            BaseCanvas.g.setClip(var1, var2, var3, var4);
            BaseCanvas.g.setColor(11315353);
            ed.a(0, 0, this.t, this.u, BaseCanvas.g);
            BaseCanvas.g.setColor(16777215);
            ed.a(1, 1, this.t - 2, this.u - 2, BaseCanvas.g);
            BaseCanvas.g.clipRect(gs.p << 1, gs.p, BaseCanvas.w - (gs.p << 2), this.u - (gs.p << 1));
            int var16 = this.a.b;
            Vector var15 = a;
            switch (a) {
               case 1:
                  var15 = b;
               default:
                  int var7 = var15.size();

                  for(int var8 = 0; var8 < var7; ++var8) {
                     g var9 = (g)var15.elementAt(var8);
                     if (var16 >= this.f && var16 < this.u) {
                        boolean var10 = this.g == var9.b;
                        int var11 = gs.p;
                        BaseCanvas.g.translate(var11, var16);
                        cp.c().a(BaseCanvas.g, var9.a, 20, gs.p, 20);
                        gv.b.a(BaseCanvas.g, var9.a[0], cp.c().a(var9.a) + gs.p + 20, gs.p, 20);
                        int var13 = gv.b.a() + 1;

                        for(int var14 = 1; var14 < var9.a.length; ++var14) {
                           gv.b.a(BaseCanvas.g, var9.a[var14], 20, var13, 20);
                           var13 += gv.b.a() + 1;
                        }

                        if (var10) {
                           BaseCanvas.g.drawImage(a, gs.p << 1, 5, 0);
                        }

                        BaseCanvas.g.translate(-var11, -var16);
                     }

                     var16 += var9.a;
                  }

                  BaseCanvas.g.setClip(var1, var2, var3, var4);
                  return;
            }
         }

         int var6 = this.u / 30 + 1;

         while(true) {
            --var6;
            if (var6 < 0) {
               break;
            }

            BaseCanvas.g.drawImage(this.b, var5 * 30, var6 * 30, 0);
         }
      }
   }

   public final void b() {
      super.b();
      if (this.c) {
         cp.e.a(BaseCanvas.g, gw.a(22) + (int)(5L - (System.currentTimeMillis() - this.a) / 1000L), BaseCanvas.Field157, this.a.s + gs.p, 17);
      }

   }

   public final void c() {
      Vector var1 = a();
      super.c();
      this.a.a();
      if (b) {
         b = false;
         this.g = var1.size() - 1;
         this.d = this.a;
         this.i();
      }

      if (this.c) {
         this.a.b("");
         if (System.currentTimeMillis() - this.a >= 5000L) {
            this.c = false;
         }
      }

   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 0:
            synchronized(a) {
               a = false;
               BaseCanvas.currentScreen.c((gn)this);
               a.removeAllElements();
               a.notifyAll();
               return;
            }
         case 1:
            if (this.c) {
               return;
            }

            int var8 = a.size();

            while(true) {
               --var8;
               if (var8 > 0 && ((g)a.elementAt(var8)).b == this.g) {
                  a.elementAt(var8);
               }
            }
         case 2:
            if (this.c) {
               return;
            }

            String var12 = cg.a.b;
            this.a.b("");
            this.d = this.a;
            return;
         case 4:
            BaseCanvas.currentScreen.w();
            this.d = this.f;
         case 3:
            switch (a) {
               case 1:
                  if (!this.c && this.a.a().length() > 0) {
                     String var6 = this.a.a();
                     en var9;
                     (var9 = new en(81)).a(10);
                     var9.a(var6);
                     cx.a.a(var9);
                     var9.a();
                     this.a.b("");
                     this.c = true;
                     this.a = System.currentTimeMillis();
                     return;
                  }
                  break;
               default:
                  if (!this.c && this.a.a().length() > 0) {
                     String var10000 = cg.a.b;
                     String var10 = this.a.a();
                     String var7 = var10000;
                     en var11;
                     (var11 = new en(81)).a(66);
                     var11.a(var7);
                     var11.a(var10);
                     cx.a.a(var11);
                     var11.a();
                     this.a.b("");
                     this.c = true;
                     this.a = System.currentTimeMillis();
                  }
            }

            return;
         case 5:
            if (!this.c && this.a.a().length() > 0) {
               int var5 = this.e;
               String var2 = this.a.a();
               en var3;
               (var3 = new en(81)).a(91);
               var3.a(21);
               var3.b(var5);
               var3.a(var2);
               cx.a.a(var3);
               var3.a();
               this.a.b("");
               this.c = true;
               this.a = System.currentTimeMillis();
               return;
            }

            return;
         default:
      }
   }

   public final void d() {
      BaseCanvas.currentScreen.b((gn)this);
      BaseCanvas.getCurrentScreen().a((gn)this.a);
      a = true;
   }

   public final void a(int var1) {
      this.f = new cd(5, gw.a(33), this);
      this.d = this.f;
      this.e = var1;
   }
}
