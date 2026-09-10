import java.util.Vector;
import javax.microedition.lcdui.Image;
import thong.sdk.ISoundManagerSDK;
import vn.me.core.BaseCanvas;

public class ew extends fw implements gz {
   public ef a;
   public u a = new u();
   protected Vector a = new Vector();
   private Vector b = new Vector();
   private Vector c = new Vector();
   public ee a = null;
   public eh a = null;
   private int b;
   private int c;
   public al a = new al();
   public al b = new al();
   public int a;
   private boolean[] a = new boolean[4];
   private boolean d = false;
   private int d;
   private Vector f = new Vector();
   private boolean g = false;
   private cd b;
   public cd a;
   private cd c;
   public boolean a;
   private int e = 27;
   private go a;
   public boolean b = true;
   private static String a = "";
   private static String b = "";
   public dv a;
   private boolean h = true;
   public gz a;
   protected boolean c = true;
   private int f = -1000;
   private int g = -1000;
   private int h = 50;
   private int i = 0;
   private long a;
   private static String[] a = new String[]{"s_outMap_0", "s_outMap_1"};

   public ew(dv var1) {
      super(true);
      this.a = var1;
      this.f = true;
      this.a = cp.c();
      this.a = new go(0, 0, BaseCanvas.w, BaseCanvas.h);
      this.a.k = true;
      this.a.h = true;
      this.b = BaseCanvas.w;
      this.c = BaseCanvas.h;
      this.b = new cd(1, a.a(419), this);
      this.a = new ge(0, BaseCanvas.h - 2 * gs.m, BaseCanvas.w, gs.m);
      this.a.d = new cd(-2, gw.a(1), this);
      this.a.c = new cd(-3, gw.a(16), this);
      this.a.f = false;
      this.a.a = this;
      this.l = new cd(100, a.a(275), this);
      this.a = new cd(201, a.a(58), this);
      this.n = new cd(200, a.a(3), this);
      this.n.a = a.a(3);
      this.c = new cd(2, "", this);
   }

   public static void c() {
      ISoundManagerSDK.playBgSound(a[BaseCanvas.ticks % a.length]);
   }

   public final void d() {
      if (BaseCanvas.instance != null && BaseCanvas.instance.hasPointerEvents()) {
         BaseCanvas.g.setColor(10264217);
         int var1 = this.f - (this.h >> 1);
         int var2 = this.g - (this.h >> 1);
         int var3 = this.h >> 1;
         BaseCanvas.g.drawArc(var1, var2, this.h, this.h, 0, 360);
         BaseCanvas.g.setColor(0);
         BaseCanvas.g.drawLine(var1 + var3 - 2, var2 + var3, var1 + var3 + 2, var2 + var3);
         BaseCanvas.g.drawLine(var1 + var3, var2 + var3 - 2, var1 + var3, var2 + var3 + 2);
         Image var4 = cp.b[2];
         if (!this.d) {
            BaseCanvas.g.drawImage(var4, this.f - (var4.getWidth() >> 1), this.g - (var4.getHeight() >> 1), 0);
         } else if (dv.a.f == 0) {
            BaseCanvas.g.drawImage(var4, this.f - (var4.getWidth() >> 1), this.g - (var4.getHeight() >> 1) - var3, 0);
         } else if (dv.a.f == 1) {
            BaseCanvas.g.drawImage(var4, this.f - (var4.getWidth() >> 1), this.g - (var4.getHeight() >> 1) + var3, 0);
         } else if (dv.a.f == 2) {
            BaseCanvas.g.drawImage(var4, this.f - (var4.getWidth() >> 1) - var3, this.g - (var4.getHeight() >> 1), 0);
         } else {
            BaseCanvas.g.drawImage(var4, this.f - (var4.getWidth() >> 1) + var3, this.g - (var4.getHeight() >> 1), 0);
         }
      }
   }

   protected void i_() {
   }

   public final void f() {
      for(int var1 = 0; var1 < this.a.length; ++var1) {
         this.a[var1] = false;
      }

      this.i = 0;
   }

   public final void a(String var1) {
      cx.a(var1);
      dv.a.b(var1);
      this.k();
   }

   public final boolean a(int var1, int var2) {
      if (super.a(var1, var2)) {
         return true;
      } else {
         switch (var2) {
            case -4:
               this.a[3] = var1 == 0;
               return true;
            case -3:
               this.a[2] = var1 == 0;
               return true;
            case -2:
               this.a[1] = var1 == 0;
               return true;
            case -1:
               this.a[0] = var1 == 0;
               return true;
            default:
               return super.a(var1, var2);
         }
      }
   }

   public final void a(ee var1) {
      synchronized(this.c) {
         this.a.b(var1);
         this.c.removeElement(var1);
      }
   }

   protected void g() {
   }

   protected void h() {
   }

   public static void i() {
      if (cg.a != null) {
         for(int var0 = 0; var0 < cg.a.length; ++var0) {
            if (cg.a.b == cg.a[var0]) {
               a = cg.b[var0];
               break;
            }
         }
      }

      b = String.valueOf(cg.a.c);
   }

   public static void j() {
      int var0 = BaseCanvas.w - 28;
      BaseCanvas.g.drawImage(cp.h, var0, 1, 0);
      cp.c().a(BaseCanvas.g, b, var0 + 12, 1, 17);
      cp.d().a(BaseCanvas.g, a, var0 - 5, 4, 24);
   }

   public final void b() {
      int var1 = this.a.b;
      int var2 = this.b.b;
      int var3 = this.a.a.length;
      this.a.a(var1, var2, false);
      synchronized(this.c) {
         for(int var5 = 0; var5 < this.c.size() - 1; ++var5) {
            for(int var6 = var5 + 1; var6 < this.c.size(); ++var6) {
               eh var7 = (eh)this.c.elementAt(var5);
               eh var8 = (eh)this.c.elementAt(var6);
               if (var7.j > var8.j) {
                  this.c.setElementAt(var8, var5);
                  this.c.setElementAt(var7, var6);
               }
            }
         }
      }

      int var4 = 0;
      int var12 = 0;
      this.c.elementAt(0);

      while(var4 < var3 && var12 < this.c.size()) {
         eh var13 = (eh)this.c.elementAt(var12);
         if (this.a.b[var4] < var13.j) {
            y var17;
            gy var21 = (var17 = this.a.a[this.a.a[var4]]).a();
            short var9 = this.a.a[var4];
            var21.a += var9;
            int var10 = this.a.b[var4] - this.a.c[var4];
            var21.b += var10;
            if (var17.g && var21.a(var1, var2, this.b, this.c)) {
               var17.a = this.a.b[var4];
               var17.b(var9 - var1, var10 - var2);
            }

            ++var4;
         } else {
            gy var18 = var13.a();
            if (var13.g && var18.a(var1, var2, this.b, this.c)) {
               var13.a(var1, var2);
            }

            ++var12;
         }
      }

      for(int var14 = var4; var14 < var3; ++var14) {
         y var19;
         gy var22 = (var19 = this.a.a[this.a.a[var14]]).a();
         short var24 = this.a.a[var14];
         var22.a += var24;
         int var25 = this.a.b[var14] - this.a.c[var14];
         var22.b += var25;
         if (var19.g && var22.a(var1, var2, this.b, this.c)) {
            var19.a = this.a.b[var14];
            var19.b(var24 - var1, var25 - var2);
         }
      }

      for(int var15 = var12; var15 < this.c.size(); ++var15) {
         eh var20;
         gy var23 = (var20 = (eh)this.c.elementAt(var15)).a();
         if (var20.g && var23.a(var1, var2, this.b, this.c)) {
            var20.a(var1, var2);
         }
      }

      if (this.a != null) {
         this.a.a_(var1, var2);
      }

      if (this.a != null) {
         this.a.a_(var1, var2);
      }

      int var16 = this.a.a.length;

      while(true) {
         --var16;
         if (var16 < 0) {
            this.h();
            return;
         }

         this.a.a[var16].a(var1, var2);
      }
   }

   public void a(int var1, int var2) {
      if (a == null && this.a == null && !this.a) {
         super.a(var1, var2);
         this.f = var1;
         this.g = var2;
      } else {
         super.a(var1, var2);
         this.d = false;
      }
   }

   public final void b_(int var1, int var2) {
      if (a == null && this.a == null && !this.a) {
         super.b_(var1, var2);
         this.i = 0;
         int var3 = this.h >> 1;
         byte var4 = dv.a.c;
         if (var1 != this.f || var2 != this.g) {
            int var5;
            var5 = (var5 = Math.abs(var1 - this.f)) > var3 ? var3 : var5;
            int var6 = Math.abs(var2 - this.g);
            var5 = Math.max(var5, var6 > var3 ? var3 : var6);
            this.i = var4 - var5 * var4 / var3;
            var1 -= this.f;
            var2 -= this.g;
            this.d = Math.abs(var1) > Math.abs(var2) ? (var1 < 0 ? 2 : 3) : (var2 < 0 ? 0 : 1);
            this.d = true;
            if (var5 < 10) {
               this.d = false;
            }

         }
      } else {
         super.b_(var1, var2);
         this.d = false;
      }
   }

   public final void c(int var1, int var2) {
      if (a == null && this.a == null && !this.a) {
         super.c(var1, var2);
         this.f();
         this.f = -1000;
         this.g = -1000;
         this.d = false;
      } else {
         super.c(var1, var2);
      }
   }

   public void c_() {
      int var1 = dv.a.a.c;
      this.b = var1 < BaseCanvas.w ? var1 : BaseCanvas.w;
      var1 = dv.a.a.d;
      this.c = var1 < BaseCanvas.h ? var1 : BaseCanvas.h;
      ee var13;
      int var2 = (var13 = dv.a).e;
      long var5 = System.currentTimeMillis();

      for(int var3 = 0; var3 < this.a.a(); ++var3) {
         ee var4;
         if ((var4 = this.a.b(var3)) != dv.a) {
            var4.a(var5);
         }
      }

      for(int var14 = 0; var14 < this.b.size(); ++var14) {
         ((eh)this.b.elementAt(var14)).a(var5);
      }

      int var15 = this.d;
      if (!this.d) {
         var15 = -1;
      }

      boolean var17 = false;
      if ((this.a[0] || var15 == 0) && this.b) {
         this.b(0);
         var17 = true;
         if (dv.a.a()) {
            this.a(0);
         } else {
            this.a(1);
         }
      } else if ((this.a[1] || var15 == 1) && this.b) {
         this.b(1);
         var17 = true;
         if (dv.a.a()) {
            this.a(0);
         } else {
            this.a(1);
         }
      } else if ((this.a[2] || var15 == 2) && this.b) {
         this.b(2);
         var17 = true;
         this.a(0);
      } else if ((this.a[3] || var15 == 3) && this.b) {
         this.b(3);
         var17 = true;
         this.a(1);
      }

      this.a = null;
      var15 = 1000000000;
      int var7 = dv.a.a() ? dv.a.i - 5 : dv.a.i + 5;
      int var8 = -1;

      for(int var9 = 0; var9 < this.a.a(); ++var9) {
         ee var10;
         if ((var10 = this.a.b(var9)) != dv.a && var10.i) {
            if (!var10.e) {
               int var11;
               if ((var11 = (var10.i - var7) * (var10.i - var7) + (var10.j - dv.a.j) * (var10.j - dv.a.j)) < var15) {
                  var15 = var11;
                  var8 = var9;
               }
            } else if (dv.a.a.a(var10.a)) {
               this.a = var10;
            }
         }
      }

      if (var15 <= 1225 && var8 >= 0 && var8 < this.a.a()) {
         this.a = this.a.b(var8);
      } else {
         this.a = null;
      }

      if (this.a == null) {
         for(int var18 = 0; var18 < this.b.size(); ++var18) {
            eh var22;
            if ((var22 = (eh)this.b.elementAt(var18)).i && var22.a != null && dv.a.a.a(var22.a)) {
               this.a = var22;
               break;
            }
         }
      }

      if (this.a != null && this.a != null) {
         this.m = this.b;
      } else if (this.a != null) {
         this.m = this.a.a;
      } else if (this.a != null) {
         this.m = this.a.a;
      } else {
         this.m = this.c;
      }

      if (var17) {
         for(int var19 = 0; var19 < this.b.size(); ++var19) {
            eh var23 = (eh)this.b.elementAt(var19);
            if (dv.a.a.a(var23.a)) {
               if (var23.h && !var23.f) {
                  cd[] var26 = new cd[]{var23.a};
                  this.f();
                  var23.a.a.a(var26);
                  var23.f = true;
                  break;
               }
            } else if (var23.h) {
               var23.f = false;
            }
         }

         if (var13.c) {
            var13.c = false;
            dc.a(false);
            ((df)var13).a.a = false;
         }
      } else {
         var13.b(-1L);
         if (var13.a()) {
            this.a(0);
         } else {
            this.a(1);
         }
      }

      if (var17) {
         if (var2 == 0 && !this.g) {
            this.q();
         } else if (this.g) {
            if (var5 - this.a >= 2000L) {
               this.r();
               this.q();
            } else {
               az var20 = new az(dv.a.i, dv.a.j);
               if (this.f.size() >= 2) {
                  az var24 = (az)this.f.elementAt(this.f.size() - 2);
                  az var27 = (az)this.f.elementAt(this.f.size() - 1);
                  if (ed.a(var24, var27, var20)) {
                     this.f.removeElement(var27);
                  }
               }

               this.f.addElement(var20);
            }
         }
      } else if (this.g && var5 - this.a > 2000L) {
         this.r();
      }

      var13.a(var5);
      this.a.a();
      this.b.a();

      for(int var21 = 0; var21 < this.a.size(); ++var21) {
         aa var25;
         if ((var25 = (aa)this.a.elementAt(var21)).a) {
            this.a.removeElement(var25);
         } else {
            var25.a();
         }
      }

      el.a();
      super.c_();
   }

   private void q() {
      this.a = System.currentTimeMillis();
      this.g = true;
      this.f.removeAllElements();
   }

   private void r() {
      this.g = false;
      this.f.addElement(new az(dv.a.i, dv.a.j));
      if (!this.f.isEmpty()) {
         int[] var1 = new int[this.f.size() << 1];

         for(int var2 = 0; var2 < this.f.size(); ++var2) {
            az var3 = (az)this.f.elementAt(var2);
            var1[var2 << 1] = var3.a;
            var1[(var2 << 1) + 1] = var3.b;
         }

         cx.a(this.a, dv.a.c, (byte)(dv.a.a() ? 0 : 1), var1);
      }
   }

   public final void a(ef var1) {
      this.a = var1;
      this.b.removeAllElements();

      for(int var2 = 0; var2 < this.c.size(); ++var2) {
         if (this.c.elementAt(var2) instanceof eg) {
            this.c.removeElementAt(var2);
            --var2;
         }
      }

      if (var1.a != null) {
         for(int var3 = 0; var3 < var1.a.length; ++var3) {
            this.a((eh)var1.a[var3]);
         }
      }

   }

   public final void a(eh var1) {
      this.b.addElement(var1);
   }

   public final void b(eh var1) {
      this.b.removeElement(var1);
      synchronized(this.c) {
         this.c.removeElement(var1);
      }
   }

   public final void a(ee var1, int var2, int var3, boolean var4) {
      this.a.a(var1);
      if (var4) {
         this.a(1);
      } else {
         var1.b = true;
      }

      var1.i = var2;
      var1.j = var3;
      this.c(var1);
      var1.g = true;
   }

   public final void c(eh var1) {
      this.c.addElement(var1);
   }

   private int a(int var1, boolean var2) {
      int var3 = var1;
      if (var2) {
         if (var1 < 0) {
            var3 = 0;
         } else if (var1 > this.a.c - this.b) {
            var3 = this.a.c - this.b;
         }
      } else if (var1 < 0) {
         var3 = 0;
      } else if (var1 > this.a.d - this.c) {
         var3 = this.a.d - this.c;
      }

      return var3;
   }

   public final void a(int var1) {
      int var2 = 0;
      int var3 = 0;
      switch (var1) {
         case 0:
            var2 = dv.a.i - (this.b / 3 << 1);
            var3 = dv.a.j - (this.c / 3 << 1);
            break;
         case 1:
            var2 = dv.a.i - this.b / 3;
            var3 = dv.a.j - (this.c / 3 << 1);
      }

      var1 = this.a(var2, true);
      var2 = this.a(var3, false);
      this.a.a = var1;
      this.b.a = var2;
   }

   public void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 0:
            cx.f();
            return;
         case 1:
            gb[] var2;
            (var2 = new gb[]{new gb(a.a(419) + " " + this.a.e), new gb(a.a(419) + " " + this.a.e)})[1].d = new cd(102, a.a(419), this);
            dv var10000 = this.a;
            dv.a(dv.a, var2);
            return;
         case 2:
            if (this.a) {
               return;
            }

            this.m();
            this.a = true;
            return;
         case 15:
            cg.j();
            return;
         case 23:
            return;
         case 51:
            cg.f();
            cx.h();
            return;
         case 66:
            this.k();
            return;
         case 85:
            return;
         case 100:
            this.g();
            return;
         case 101:
            if (this.a != null && this.a.a != null) {
               this.a.a.a(new Object[]{this.a.a, this.a});
               return;
            }

            return;
         case 102:
            this.a.a.a(new Object[]{this.a.a, this.a});
            return;
         case 200:
            this.i_();
            return;
         case 201:
            if (this.a != null && a == null && this.a == null) {
               this.b(this.a);
               this.a.n();
               this.a.f = true;
               return;
            }

            return;
         case 202:
            (new fk()).a(1, true);
            return;
         case 10000:
            cg.c();
            return;
         default:
            super.a(var1);
      }
   }

   public final void k() {
      if (this.a) {
         if (BaseCanvas.instance.hasPointerEvents()) {
            this.n = new cd(200, a.a(3), this);
         } else {
            this.n = new cd(200, a.a(3), this);
            this.n.a = a.a(3);
         }

         this.b.b(this.a);
         this.a.o();
      }

      this.a = false;
   }

   public final void a(gh[] var1) {
      if (this.h) {
         this.n = new cd(66, a.a(64), this);
         int var2 = var1.length * 28 + (var1.length + 1) * 3;
         int var3 = 3;
         int var4 = 3;

         for(int var5 = 0; var5 < var1.length; ++var5) {
            if (var2 >= BaseCanvas.w - 10 && var5 < var1.length / 2) {
               var3 += 31;
            } else {
               var4 += 31;
            }
         }

         int var12 = dv.a.i - this.a.b;
         var2 = dv.a.j - this.b.b;
         int var6 = BaseCanvas.w - var3 >> 1;
         var4 = BaseCanvas.w - var4 >> 1;
         int var7;
         int var8 = (var7 = BaseCanvas.h - gs.m - this.e - 5 - 28 - 3) + 28 + 3;

         for(int var9 = 0; var9 < var1.length; ++var9) {
            var1[var9].r = var12;
            var1[var9].s = var2;
            if (var3 > 3 && var9 < var1.length / 2) {
               var1[var9].v = var6;
               var6 += 31;
               var1[var9].w = var7;
            } else {
               var1[var9].v = var4;
               var4 += 31;
               var1[var9].w = var8;
            }

            var1[var9].b(var9 == 0);
            this.a.a(var1[var9]);
         }

         this.a.b(true);
         this.b.a(this.a);
         this.a = true;
      }

   }

   private void b(int var1) {
      ee var2 = dv.a;
      int var3 = 0;
      int var4 = 0;
      int var5 = 0;
      switch (var1) {
         case 0:
            var3 = var2.i;
            var4 = var2.j - var2.c + this.i;
            var5 = 0;
            break;
         case 1:
            var3 = var2.i;
            var4 = var2.j + var2.c - this.i;
            var5 = 0;
            break;
         case 2:
            var3 = var2.i - var2.c + this.i;
            var4 = var2.j;
            var5 = var2.k;
            break;
         case 3:
            var3 = var2.i + var2.c - this.i;
            var4 = var2.j;
            var5 = var2.k + var2.a.a.a;
      }

      if (this.a.a(var3 + var5, var4 + var2.l)) {
         var2.d(var3, var4);
      }

   }

   public final void a(gd var1) {
      super.a(var1);
      this.f();
   }

   public final void l() {
      super.l();
      this.f();
   }

   public void m() {
   }

   public void n() {
   }

   public final void o() {
      this.c = false;
   }

   public final void p() {
      this.h = false;
   }
}
