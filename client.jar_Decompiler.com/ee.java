import java.util.Random;
import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public class ee extends eh {
   public boolean b;
   public int c;
   public String b;
   public String c;
   protected int d;
   public int e;
   private long b;
   private long c;
   private int a;
   private int b;
   public int f;
   public int g;
   private static Image a;
   private int o;
   private boolean a;
   public byte a;
   public dv a;
   public boolean c;
   private int p;
   private String a;
   private byte d;
   private int q;
   private byte e;
   private long d;
   at a;
   public String d = "";
   public boolean d = true;
   private Vector a = new Vector();
   public byte b = -1;
   public boolean e = false;
   public int h = 16777215;
   public byte c = 8;
   private gp[] a = new gp[0];

   public ee(int var1, byte var2, dv var3) {
      this.a = var3;
      this.c = var1;
      this.a = var2;
      this.a((gy)(new gy(-8, -5, 16, 5)));
      if (a == null) {
         a = gu.a("/common.dat", 10);
      }

      this.i = 0;
      this.j = 0;
      this.a = -100;
      this.b = -100;
      this.b(-1L);
      this.p = 0 - cp.g.getHeight() - 74 + 5;
      this.q = (new Random(System.currentTimeMillis())).nextInt() % 1000;
   }

   public final void a(String var1) {
      this.d = var1;
      if ("".equals(var1)) {
         this.d = true;
      } else {
         this.d = false;
      }
   }

   public void a(long var1) {
      super.a(var1);
      if (this.a != null && var1 - this.d > 5000L) {
         this.a = null;
      }

      this.a = false;
      switch (this.e) {
         case 0:
            t var5;
            if (this.b && this.c != -1L && var1 - this.b >= this.c && (var5 = this.a()) != null) {
               this.a(var5);
               return;
            }

            return;
         case 1:
            this.a = true;
            if ((this.a != -100 || this.b != -100) && (this.i != this.a || this.j != this.b)) {
               if (ed.a(this.a - this.i) > ed.a(this.b - this.j)) {
                  if (this.a > this.i) {
                     this.i += this.c;
                  } else {
                     this.i -= this.c;
                  }

                  if (this.a != this.i) {
                     if (this.a > this.i) {
                        this.j += this.c * (this.b - this.j) / (this.a - this.i);
                     } else {
                        this.j += this.c * (this.j - this.b) / (this.a - this.i);
                     }
                  }
               } else {
                  if (this.b > this.j) {
                     this.j += this.c;
                  } else {
                     this.j -= this.c;
                  }

                  if (this.b != this.j) {
                     if (this.b > this.j) {
                        this.i += this.c * (this.a - this.i) / (this.b - this.j);
                     } else {
                        this.i += this.c * (this.i - this.a) / (this.b - this.j);
                     }
                  }
               }

               this.o = (this.o + 1) % 100;
               if (ed.a(this.b - this.j) <= this.c && ed.a(this.a - this.i) <= this.c) {
                  this.i = this.a;
                  this.j = this.b;
                  this.a = -100;
                  this.b = -100;
                  t var4;
                  if (this.b && (var4 = this.a()) != null) {
                     this.a(var4);
                     return;
                  } else {
                     return;
                  }
               } else {
                  return;
               }
            } else {
               t var3;
               if (this.b && (var3 = this.a()) != null) {
                  this.a(var3);
                  return;
               } else {
                  return;
               }
            }
         default:
      }
   }

   private void a(int var1, int var2, boolean var3) {
      for(int var4 = 0; var4 < this.a.length; ++var4) {
         gp var5;
         if ((var5 = this.a[var4]).a == var3) {
            if (var5.a == null) {
               var5.a = dj.a.a(var5.a);
            }

            if (var5.a != null) {
               int var6 = var5.a.getWidth() / var5.a;
               switch (var5.a) {
                  case 0:
                     if (BaseCanvas.ticks % 5 == 0) {
                        ++var5.d;
                     }

                     int var7 = var5.d % var5.a;
                     BaseCanvas.g.drawRegion(var5.a, var6 * var7, 0, var6, var5.a.getHeight(), 0, this.i - var1 + var5.b - var6 / 2, this.j - var2 + var5.c - 74 - (cp.d().a() << 1) - var5.a.getHeight(), 0);
               }
            }
         }
      }

   }

   public void a(int var1, int var2) {
      if (this.g) {
         this.a(var1, var2, false);
         Image var3;
         if (this.a != null && !"".equals(this.a) && (var3 = dj.a.a(this.a)) != null) {
            int var4 = (ed.a(BaseCanvas.ticks + this.q) >> 3) % 2;
            int var5 = var3.getWidth() >> 1;
            BaseCanvas.g.drawRegion(var3, var5 * var4, 0, var5, var3.getHeight(), 0, this.i - var1 - var5, this.j - var2 - 74 + this.d, 0);
            BaseCanvas.g.drawRegion(var3, var5 * var4, 0, var5, var3.getHeight(), 2, this.i - var1, this.j - var2 - 74 + this.d, 0);
         }

         if (this.d) {
            v.a((byte)(this.a == 0 ? 1 : 0), this.i - var1, this.j - var2, this.g == 1 ? 1 : -1, this.a, this.q);
         } else {
            Image var7;
            if ((var7 = dj.a.b(this.d)) == null) {
               v.a((byte)(this.a == 0 ? 1 : 0), this.i - var1, this.j - var2, this.g == 1 ? 1 : -1, this.a, this.q);
            } else {
               BaseCanvas.g.drawImage(v.a, this.i - var1, this.j - var2 - 74 + 68, 17);
               v.a(var7, this.i - var1, this.j - var2, this.g, this.a);
            }
         }

         if (ed.a(this.b, 0)) {
            BaseCanvas.g.drawImage(a, this.i - var1 - 10 - (this.d >> 1), this.j - 74 - var2 + 5, 0);
         }

         gg var8 = ed.a(this.b, 1) ? cp.a() : (this.h == 16777215 ? cp.d() : cp.a(this.h));
         ed.a(this.b, 2);
         int var6 = this.j - 74 + 3 - var2;
         var8.b(BaseCanvas.g, this.c, this.i - var1 - (this.d >> 1), var6, 0);
         if (this.b != null && !"".equals(this.b)) {
            var6 -= cp.d().a() + 2;
            cp.d().b(BaseCanvas.g, this.b, this.i - var1, var6, 17);
         }

         if (this.e != 0) {
            cp.d().b(BaseCanvas.g, "PK " + this.e, this.i - var1, var6 - (cp.d().a() + 2), 17);
         }

         if (this.a != null) {
            this.c(var1, var2 + 10);
         }

         this.a(var1, var2, true);
      }

   }

   public final void d(int var1, int var2) {
      this.a = true;
      this.a = var1;
      this.b = var2;
      var1 -= this.i;
      var2 -= this.j;
      if (var1 > 0) {
         this.f = 3;
      } else if (var1 < 0) {
         this.f = 2;
      } else if (var2 < 0) {
         this.f = 0;
      } else {
         this.f = 1;
      }

      if (this.f == 3) {
         var1 = 1;
      } else if (this.f == 2) {
         var1 = 0;
      } else {
         var1 = (byte)(this.a() ? 0 : 1);
      }

      if (this.e != 1 || this.f == 3 && this.a() || this.f == 2 && !this.a()) {
         this.g = var1;
      }

      this.e = 1;
   }

   public final boolean a() {
      return this.g == 0;
   }

   public final void b(long var1) {
      this.c = var1;
      this.b = System.currentTimeMillis();
      this.e = 0;
      this.g = (byte)(this.a() ? 0 : 1);
   }

   public void a_(int var1, int var2) {
      if (this.g) {
         BaseCanvas.g.drawImage(cp.g, this.i - var1, -var2 + this.j - this.m + this.p, 17);
      }

   }

   public final void b(String var1) {
      this.d = System.currentTimeMillis();
      if (this.a == null) {
         this.a = new at();
      }

      this.a.a(var1);
   }

   protected void c(int var1, int var2) {
      this.a.a(BaseCanvas.g, this.i - var1, -var2 + this.j - 54 - 8);
   }

   private t a() {
      synchronized(this.a) {
         if (this.a.isEmpty()) {
            return null;
         } else {
            t var2 = (t)this.a.firstElement();
            this.a.removeElementAt(0);
            return var2;
         }
      }
   }

   private void a(t var1) {
      switch (var1.a) {
         case 0:
            this.b(var1.a);
            return;
         case 1:
            this.d(var1.a, var1.b);
            return;
         default:
      }
   }

   public final void a(int[] var1) {
      synchronized(this.a) {
         if (!this.a.isEmpty()) {
            int var3 = 0;

            while(var3 < this.a.size()) {
               t var4;
               if ((var4 = (t)this.a.elementAt(var3)).a == 0) {
                  this.a.removeElement(var4);
               } else {
                  ++var3;
               }
            }
         }

         for(int var7 = 0; var7 < var1.length >> 1; ++var7) {
            t var9;
            (var9 = new t((byte)1)).a = var1[var7 << 1];
            var9.b = var1[(var7 << 1) + 1];
            this.a.addElement(var9);
         }

         t var8;
         (var8 = new t((byte)0)).a = -1L;
         this.a.addElement(var8);
         t var6;
         if (this.e == 0 && (var6 = this.a()) != null) {
            this.a(var6);
         }

      }
   }

   public final void c(String var1) {
      this.e = var1;
      this.c = var1.toUpperCase();
      this.d = cp.d().a(this.c);
   }

   public void a(ew var1) {
   }

   public final void a(String var1, byte var2) {
      this.a = var1;
      this.d = var2;
   }

   public final void b(byte var1) {
      this.e = var1;
   }

   public final void a(gp[] var1) {
      this.a = var1;
   }
}
