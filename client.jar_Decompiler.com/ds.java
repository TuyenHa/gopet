import java.io.IOException;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class ds extends fw {
   private int a;
   private String[] a;
   private int b;
   private String[] b;
   private String[] c;
   private int c;
   private bz a;
   private bz b;
   private Image a;
   private int d;
   private int e = 42;
   private int f;
   private int g;
   private int h;
   private int i;
   private int j;
   private int k;
   private int l;
   private int m;
   private int n = 0;
   private long a;
   private int o;

   public final void a(int var1, int var2, String var3, String[] var4) {
      switch (var1) {
         case 1:
            this.a = var2;
            this.a = var4;
            break;
         case 2:
            this.b = var2;
            this.b = var4;
            break;
         case 3:
            this.c = var4;
      }

      this.f();
      if (var1 != 3 && this.a != -1 && this.b != -1) {
         if (this.a == this.b) {
            cg.c(gw.a(41));
         } else {
            cg.f();
            var1 = this.a;
            var2 = this.b;
            en var7;
            (var7 = new en(81)).a(70);
            var7.b(var1);
            var7.b(var2);
            cx.a.a(var7);
            var7.a();
         }
      }
   }

   public ds() {
      super(true);

      try {
         this.a = Image.createImage("/unknow.png");
      } catch (IOException var2) {
         var2.printStackTrace();
      }

      this.f = true;
      this.a = cp.c();
      this.g();
      this.f();
   }

   private void f() {
      if (this.c != null) {
         this.f = 10 + gv.a.a() * this.c.length + (this.c.length - 1) * 3;
      } else {
         this.f = 10 + gv.a.a() * 3 + 6;
      }

      this.g = BaseCanvas.h - gs.n - this.f - this.e;
      int var1 = (this.g - 100) / 4;
      this.h = var1;
      this.i = 50 + var1 * 3;
      var1 = 0;

      for(int var2 = 0; var2 < this.a.length; ++var2) {
         int var3 = gv.a.a(this.a[var2]);
         if (var1 < var3) {
            var1 = var3;
         }
      }

      for(int var5 = 0; var5 < this.b.length; ++var5) {
         int var6 = gv.a.a(this.b[var5]);
         if (var1 < var6) {
            var1 = var6;
         }
      }

      this.j = BaseCanvas.Field157 - (var1 + 75 >> 1);
      this.a.a(this.j, this.h + this.e, 50, 50);
      this.b.a(this.j, this.i + this.e, 50, 50);
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
      dj.a.a();
      gs.a(10, 20, BaseCanvas.w - 20, 22);
      cp.c().a(BaseCanvas.g, gw.a(29), BaseCanvas.Field157, 22, 17);
      gs.a(10, this.e, BaseCanvas.w - 20, this.g);
      switch (this.c) {
         case 0:
            int var1 = this.e + this.h;
            int var15 = this.j;

            for(int var2 = 0; var2 < this.a.length; ++var2) {
               gv.a.a(BaseCanvas.g, this.a[var2], this.j + 70 + 5, var1 + (gv.a.a() + 3) * var2, 0);
            }

            int var5 = this.e + this.i;
            var15 = this.j;

            for(int var3 = 0; var3 < this.b.length; ++var3) {
               gv.a.a(BaseCanvas.g, this.b[var3], this.j + 70 + 5, var5 + (gv.a.a() + 3) * var3, 0);
            }
            break;
         case 1:
            int var9 = this.j;
            var9 = this.e;
            var9 = this.k;
            var9 = this.j;
            var9 = this.e;
            var9 = this.l;
            break;
         case 2:
            int var10000 = this.j;
            var10000 = this.e;
            var10000 = this.m;
      }

      int var4 = this.e + this.g;
      gs.a(10, var4, BaseCanvas.w - 20, this.f);

      for(int var6 = 0; var6 < this.c.length; ++var6) {
         gv.a.a(BaseCanvas.g, this.c[var6], BaseCanvas.Field157, var4 + 5 + (gv.a.a() + 3) * var6, 17);
      }

   }

   public final void c_() {
      super.c_();
      if (this.c == 1) {
         switch (this.n) {
            case 0:
               boolean var1 = false;
               boolean var2 = false;
               if (this.k < this.m) {
                  this.k += 2;
               }

               if (this.k >= this.m) {
                  this.k = this.m;
                  var1 = true;
               }

               if (this.l > this.m) {
                  this.l -= 2;
               }

               if (this.l <= this.m) {
                  this.l = this.m;
                  var2 = true;
               }

               if (var1 && var2) {
                  this.n = 1;
                  this.a = System.currentTimeMillis();
                  d var3;
                  (var3 = new d(this.j + 25, this.e + this.m + 25, false)).c();
                  fw.e.addElement(var3);
               }
               break;
            case 1:
               if (System.currentTimeMillis() - this.a >= 2000L) {
                  this.n = 0;
                  this.c = 2;
                  this.l = null;
                  this.m = new cd(3, gw.a(30), this);
               }
         }
      }

      if (BaseCanvas.ticks % 2 == 0) {
         ++this.o;
         if (this.o == gv.a.c) {
            this.o = 0;
         }
      }

   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 0:
            en var8;
            (var8 = new en(81)).a(72);
            cx.a.a(var8);
            var8.a();
            cg.f();
            return;
         case 1:
            cg.f();
            dc.e(1);
            return;
         case 2:
            cg.f();
            dc.e(2);
            return;
         case 3:
            this.g();
            return;
         case 4:
            this.d = 1;
            this.h();
            return;
         case 5:
            this.d = 2;
            this.h();
            return;
         case 6:
            cg var10000 = cg.a;
            String[] var6;
            if ((var6 = cg.a(fw.a)) != null && ((Object[])var6).length != 0 && !((Object[])var6)[0].equals("")) {
               var6 = ((Object[])var6)[0];
               cg.f();
               int var2 = this.a;
               int var3 = this.b;
               int var4 = this.d;
               en var5;
               (var5 = new en(81)).a(71);
               var5.b(var2);
               var5.b(var3);
               var5.a(var6);
               var5.a(var4);
               cx.a.a(var5);
               var5.a();
               return;
            }

            cg.c(gw.a(42));
            return;
         default:
      }
   }

   private void g() {
      this.n = cg.e;
      this.l = new cd(0, gw.a(29), this);
      this.a = -1;
      this.b = -1;
      Image var10000 = this.a;
      this.a = new String[]{gw.a(43), gw.a(44)};
      var10000 = this.a;
      this.b = new String[]{gw.a(43), gw.a(44)};
      var10000 = this.a;
      this.c = new String[]{"", ""};
      this.c = 0;
      if (this.a == null) {
         this.a = new bz(this);
         this.a.a(this.j, this.h + this.e, 50, 50);
         this.a.d = new cd(1, gw.a(43), this);
      }

      this.b.a(this.a);
      if (this.b == null) {
         this.b = new bz(this);
         this.b.a(this.j, this.i + this.e, 50, 50);
         this.b.d = new cd(2, gw.a(43), this);
      }

      this.b.a(this.b);
   }

   private void h() {
      cg.a(122, 7, 0, gw.a(45), new String[]{""}, new byte[]{0}, new cd(6, gw.a(6), this)).a(true);
   }

   public final void c() {
      this.x();
      this.c = 1;
      this.n = 0;
      this.k = this.h;
      this.l = this.i;
      this.m = (this.h + this.i) / 2;
      this.b.b(this.a);
      this.b.b(this.b);
   }

   public final void b(int var1, int var2) {
      gd var3 = cg.a(0, gw.a(46), new int[]{1, 2}, new String[]{var1 > 0 ? "1. " + var1 + " (vang)" : "1. Miễn phí", var2 > 0 ? "2. " + var2 + " (ngoc)" : "2. Miễn phí"}, new byte[]{1, 1}, new cd[]{new cd(4, "Chọn", this), new cd(5, "Chọn", this)});
      this.x();
      var3.a(true);
   }
}
