import java.util.Hashtable;
import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class ei extends eh {
   boolean a;
   long b;
   int a;
   int b;
   public int c;
   private Image a;
   public int d;
   public int e;
   public int f;
   boolean b;
   private be a;
   boolean c;
   boolean d;
   int g;
   int h;
   int o;
   boolean e;
   String a;
   int p;
   int q;
   bf a;
   public dn a;
   private be b;
   private bd a;
   private be c;
   private bd b;
   private bd c;
   private bd d;
   private dz a;
   private bk a;
   private Vector a = new Vector();
   public int r = 0;
   private final Hashtable a = new Hashtable(6);

   public ei(bf var1, int var2, int var3, dn var4, int var5) {
      this.a = var1;
      this.a = var4;
      this.a((gy)(new gy(10, -30, 21, 30)));
      this.d = var5;
      this.i = var2;
      this.j = var3;
      this.b = new be(new bd[]{new bd(this, 5, (Object)null)});
      this.a = new bd(this, 7, (Object)null);
      this.c = new be(new bd[]{this.a});
      int var6 = var5 == 2 ? 1 : -1;
      this.b = new bd(this, 1, new Integer(var6 * 20));
      this.c = new bd(this, 1, new Integer(var6 * -20));
      this.d = new bd(this, 4, new int[]{6, var2 + var6 * 20, var3 - 10});
   }

   private void d() {
      if (this.a.isEmpty()) {
         this.a = null;
      } else {
         be var1 = (be)this.a.firstElement();
         this.a.removeElementAt(0);
         this.a = var1;
      }
   }

   public final void a(int var1, int var2) {
      BaseCanvas.g.translate(-var1, -var2);
      if (this.a == null) {
         this.a = dj.a.a(this.a.a);
      } else {
         BaseCanvas.g.drawImage(cp.i, this.i - 11, this.j - 6, 0);
         if (this.r == 0) {
            dp var33 = dj.a;
            Image var34 = this.a;
            int var10001 = this.i - this.a.getWidth() / this.a.c / 2;
            int var10002 = this.j - this.a.getHeight();
            byte var30 = this.a.c;
            int var29 = this.d;
            int var25 = var10002;
            int var21 = var10001;
            boolean var16 = false;
            dp.a(var34, var21, var25, var29, 0, var30);
         } else {
            dp var10000 = dj.a;
            Image var5 = this.a;
            int var3 = this.i - this.a.getWidth() / this.a.c;
            int var4 = this.j - this.a.getHeight();
            int var32 = this.d;
            int var7 = this.r;
            int var8 = var5.getWidth() / this.a.c;
            int var9 = var5.getHeight() / 4;

            for(int var14 = 0; var14 < var9; ++var14) {
               int var6 = var14 << 2;
               BaseCanvas.g.drawRegion(var5, dp.b * var8, var6, var8, 4 - var7, var6, var3, var4 + (var14 << 2) + this.a.a, 0);
            }

            long var31;
            if ((var31 = System.currentTimeMillis()) - dp.b >= 200L) {
               dp.b = var31;
               dp.b = (dp.b + 1) % this.a.c;
            }
         }

         dz var17;
         if (this.c && (var17 = this.a) != null) {
            switch (this.g) {
               case 6:
                  var17.a(BaseCanvas.g, this.h, this.o, (this.d + 2) % 4);
                  break;
               case 7:
               case 9:
               case 10:
               case 11:
               case 12:
               case 13:
               case 14:
               case 16:
               case 17:
               case 19:
               default:
                  var17.a(BaseCanvas.g, this.h, this.o, 0);
                  break;
               case 8:
               case 15:
               case 18:
               case 20:
               case 21:
                  var17.a(BaseCanvas.g, this.h, this.o, this.d);
            }
         }

         bk var18;
         if (this.d && (var18 = this.a) != null) {
            var18.a(BaseCanvas.g);
            var18.c();
         }

         if (this.e) {
            switch (this.p) {
               case 0:
                  cp.e.a(BaseCanvas.g, this.a, this.i, this.j - this.q - 20, 17);
                  break;
               case 1:
                  cp.d.a(BaseCanvas.g, this.a, this.i, this.j - this.q - 20, 17);
            }
         }

         if (this.b) {
            int var22 = this.e;
            int var26 = this.a.j;
            int var19 = this.f;
            dj.a(var22, var26, var19, this.a.k, this.i - 10, this.j - this.a.getHeight() - 8);
         } else {
            int var23 = this.a.h;
            int var27 = this.a.j;
            int var20 = this.a.i;
            dj.a(var23, var27, var20, this.a.k, this.i - 10, this.j - this.a.getHeight() - 8);
         }

         if (this.a && this.c == 0 && this.b > 0) {
            int var24;
            int var28 = var24 = this.a - (int)(System.currentTimeMillis() - this.b);
            if (var24 < 0) {
               var28 = 0;
            }

            BaseCanvas.g.setColor(16711680);
            BaseCanvas.g.fillArc(this.i - 5, this.j - 50, 10, 10, 0, var28 * 360 / this.b);
         }
      }

      BaseCanvas.g.translate(var1, var2);
   }

   public final void a(long var1) {
      super.a(var1);
      if (this.a != null) {
         boolean var3 = true;

         for(int var4 = 0; var4 < this.a.a.length; ++var4) {
            this.a.a[var4].b();
            if (!this.a.a[var4].a) {
               var3 = false;
            }
         }

         if (var3) {
            this.d();
         }
      } else {
         this.d();
      }

      dz var5;
      if (this.c && (var5 = this.a) != null) {
         var5.a(var1);
      }
   }

   public final void a() {
      this.c = 1;
      this.b.a();
      this.a.addElement(new be(new bd[]{this.b}));
      this.d.a();
      this.a.addElement(new be(new bd[]{this.d}));
      this.c.a();
      this.a.addElement(new be(new bd[]{this.c}));
      this.a.addElement(this.b);
   }

   public final void c(int var1, int var2) {
      this.c = 1;
      if (var1 != 0) {
         this.a.addElement(new be(new bd[]{new bd(this, 6, new Object[]{String.valueOf(var1), null})}));
      }

      if (var2 != 0) {
         this.a.addElement(new be(new bd[]{new bd(this, 6, new Object[]{String.valueOf(var2), new Integer(1)})}));
      }

      this.a.a();
      this.a.a = new int[]{this.a.h, this.a.h + var1, this.a.i, this.a.i + var2};
      this.a.addElement(this.c);
      this.a.addElement(this.b);
   }

   public final void a(int var1) {
      this.c = 1;
      this.a.addElement(new be(new bd[]{new bd(this, 4, new int[]{5, this.i, this.j - 10}), new bd(this, 3, new Integer(500)), new bd(this, 6, new Object[]{String.valueOf(var1), null})}));
      this.a.a();
      this.a.a = new int[]{this.a.h, this.a.h + var1, this.a.i, this.a.i};
      this.a.addElement(this.c);
      this.a.addElement(this.b);
   }

   public final void b() {
      this.c = 1;
      int var1 = this.d == 2 ? -1 : 1;
      this.a.addElement(new be(new bd[]{new bd(this, 1, new Integer(var1 * 20)), new bd(this, 4, new int[]{5, this.i, this.j - 10})}));
      this.a.addElement(new be(new bd[]{new bd(this, 1, new Integer(var1 * -20))}));
      this.a.addElement(this.b);
   }

   public final void b(int var1) {
      this.c = 1;
      this.a.addElement(new be(new bd[]{new bd(this, 4, new int[]{7, this.i, this.j - 15})}));
      if (this.a.i + var1 >= 0) {
         this.a.a();
         this.a.a = new int[]{this.a.h, this.a.h, this.a.i, this.a.i + var1};
         this.a.addElement(this.c);
      }

      this.a.addElement(this.b);
   }

   public final void c(int var1) {
      this.c = 1;
      this.a.addElement(new be(new bd[]{new bd(this, 4, new int[]{4, this.i, this.j - 10}), new bd(this, 3, new Integer(500)), new bd(this, 6, new Object[]{String.valueOf(var1), null})}));
      this.a.a();
      this.a.a = new int[]{this.a.h, this.a.h + var1, this.a.i, this.a.i};
      this.a.addElement(this.c);
      this.a.addElement(this.b);
   }

   public final void a(int[] var1) {
      int var2 = var1[0];
      int var3 = var1[1];
      int var4 = var1[2];
      this.c = 1;
      bd var5 = new bd(this, 4, new int[]{var2, this.i, this.j - 15});
      if (var3 < 0) {
         this.a.addElement(new be(new bd[]{var5, new bd(this, 3, new Integer(500))}));
      } else {
         this.a.addElement(new be(new bd[]{var5}));
      }

      if (var3 != 0) {
         this.a.addElement(new be(new bd[]{new bd(this, 6, new Object[]{String.valueOf(var3), null})}));
      }

      if (var4 != 0) {
         this.a.addElement(new be(new bd[]{new bd(this, 6, new Object[]{String.valueOf(var4), new Integer(1)})}));
      }

      this.a.a();
      this.a.a = new int[]{this.a.h, this.a.h + var3, this.a.i, this.a.i + var4};
      this.a.addElement(this.c);
      this.a.addElement(this.b);
   }

   public final void b(int[] var1) {
      int var2 = var1[0];
      int var3 = var1[1];
      int var4 = var1[2];
      this.c = 2;
      bd var5 = new bd(this, 9, new int[]{var2, this.i, this.j});
      if (var3 < 0) {
         this.a.addElement(new be(new bd[]{var5, new bd(this, 3, new Integer(500))}));
      } else {
         this.a.addElement(new be(new bd[]{var5}));
      }

      if (var3 != 0) {
         this.a.addElement(new be(new bd[]{new bd(this, 6, new Object[]{String.valueOf(var3), null})}));
      }

      if (var4 != 0) {
         this.a.addElement(new be(new bd[]{new bd(this, 6, new Object[]{String.valueOf(var4), new Integer(1)})}));
      }

      this.a.a();
      this.a.a = new int[]{this.a.h, this.a.h + var3, this.a.i, this.a.i + var4};
      this.a.addElement(this.c);
      this.a.addElement(this.b);
   }

   public final void c() {
      this.c = 1;
      this.a.addElement(new be(new bd[]{new bd(this, 8, (Object)null)}));
      this.a.addElement(this.b);
   }

   public final bk a() {
      Integer var1 = new Integer(this.g);
      bk var2;
      bk var3 = var2 = (bk)this.a.get(var1);
      if (var2 == null) {
         a var5 = null;
         String var6 = "/pet/battle/skills/" + this.g + ".anu";
         System.out.println(var6);

         try {
            var5 = a.a(var6, (bm)bj.a());
         } catch (Exception var4) {
            var4.printStackTrace();
         }

         bk var7 = new bk(var5, this.h, this.o);
         this.a.put(var1, var7);
         var3 = var7;
      }

      var3.a();
      var3.b();
      return var3;
   }

   public static bk a(ei var0, bk var1) {
      var0.a = var1;
      return var1;
   }

   public static bk a(ei var0) {
      return var0.a;
   }

   public static dz a(ei var0, dz var1) {
      var0.a = var1;
      return var1;
   }

   public static dz a(ei var0) {
      return var0.a;
   }
}
