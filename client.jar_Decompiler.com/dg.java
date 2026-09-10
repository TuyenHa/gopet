import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class dg extends ee implements gz {
   private int o;
   private String[] a;
   private int p;
   public int a = 128;
   private Vector a = new Vector();
   private Vector b = new Vector();
   private gd a;
   public int b = 1;
   public String a;
   private at b;
   public boolean a;
   private long b;
   private long c;
   private long d;
   private boolean j;
   private boolean k;
   private boolean l;

   public dg(int var1, dv var2) {
      super(var1, (byte)0, var2);
      this.b = true;
      this.e = "NPC";
      this.a = new cd(0, a.a(419), this);
      this.a = new gm(a.a(363), (cd)null, new cd(1, a.a(41), this), (cd)null, 2);
      this.f = 3;
      this.d = (long)(ed.b(5) * 1000 + 3000);
   }

   public final void a(String var1, k var2) {
      this.a.addElement(var1);
      this.b.addElement(var2);
      var2.d = new cd(3, a.a(337), this);
   }

   public final void a(String[] var1, int var2) {
      this.o = var2;
      this.a = var1;
      this.a = System.currentTimeMillis();
      if (this.b == null) {
         this.b = new at();
      }

      this.a = this.b;
      this.p = 0;
      this.a.a(var1[this.p]);
   }

   public final void a(int var1, int var2) {
      if (this.b == null) {
         if (this.a != null) {
            this.b = cp.a(this.a, (byte)2);
            if (this.b != null) {
               Image var5 = this.b;
               this.b = var5;
               var5.getWidth();
               int var10000 = this.b;
               this.a = var5.getHeight();
            }
         }
      } else {
         BaseCanvas.g.drawImage(cp.i, this.i - 13 - var1, this.j - 6 - var2, 0);
         int var3 = this.f == 2 ? 2 : 0;
         if (this.j) {
            int var4 = this.b.getHeight() - (this.b.getHeight() >> 2);
            BaseCanvas.g.drawRegion(this.b, 0, 0, this.b.getWidth(), var4, var3, this.i - var1, this.j - this.a - var2 + 1, 17);
            BaseCanvas.g.drawRegion(this.b, 0, var4, this.b.getWidth(), this.b.getHeight() - var4, var3, this.i - var1, this.j - this.a - var2 + var4, 17);
         } else if (var3 == 0) {
            BaseCanvas.g.drawImage(this.b, this.i - var1, this.j - this.a - var2, 17);
         } else {
            BaseCanvas.g.drawRegion(this.b, 0, 0, this.b.getWidth(), this.b.getHeight(), 2, this.i - var1, this.j - this.a - var2, 17);
         }

         if (this.a) {
            cp.d().a(BaseCanvas.g, this.c, this.i - var1 - (this.d >> 1), this.j - this.a - var2 - 8, 0);
         }

         this.c(var1, var2);
      }
   }

   public final void a_(int var1, int var2) {
      BaseCanvas.g.drawImage(cp.g, this.i - var1, -(this.b == null ? var2 - 3 : var2 + this.a - 3) + this.j - cp.g.getHeight() - this.m, 17);
   }

   protected final void c(int var1, int var2) {
      if (this.a != null) {
         this.a.a(BaseCanvas.g, this.i - var1, -var2 + this.j - this.a - 9);
      }

   }

   public final void a(Object var1) {
      cd var2;
      if ((var2 = (cd)((Object[])var1)[0]).a >= 100) {
         this.a.a(true);
         cx.b(this.c, var2.a - 100);
      } else {
         switch (var2.a) {
            case 0:
               this.a.a(true);
               cx.d(this.c);
               return;
            case 1:
               this.a.j();
               return;
            case 2:
            default:
               return;
            case 3:
               this.a();
               return;
            case 4:
               ((k)var2.a).a(true);
         }
      }
   }

   public final void a(String[] var1, int[] var2) {
      BaseCanvas.currentScreen.x();
      gb[] var3 = new gb[var1.length];

      for(int var4 = 0; var4 < var1.length; ++var4) {
         var3[var4] = new gb(new cd(var2[var4] + 100, var1[var4], this));
      }

      dv var10000 = this.a;
      dv.a(this, var3);
   }

   public final void a() {
      BaseCanvas.currentScreen.x();
      if (!this.b.isEmpty()) {
         gb[] var1 = new gb[this.b.size()];

         for(int var2 = 0; var2 < this.b.size(); ++var2) {
            var1[var2] = new gb(new cd(4, (String)this.a.elementAt(var2), (k)this.b.elementAt(var2), this));
         }

         dv var10000 = this.a;
         dv.a(this, var1);
      }
   }

   public final void b() {
      this.b.removeAllElements();
      this.a.removeAllElements();
   }

   public final void a(long var1) {
      super.a(var1);
      if (this.a) {
         if (this.a != null) {
            if (this.a != null) {
               if (var1 - this.a > 5000L) {
                  this.a = null;
                  this.a = var1;
               }
            } else if (var1 - this.a > (long)(this.o * 1000)) {
               this.p = (this.p + 1) % this.a.length;
               this.a = this.b;
               this.a.a(this.a[this.p]);
               this.a = var1;
            }
         }

         if (this.k && var1 - this.b > 400L) {
            this.b = var1;
            this.j = !this.j;
         }

         if (this.l && var1 - this.c > this.d) {
            this.c = var1;
            if (this.f == 2) {
               this.f = 3;
            } else {
               this.f = 2;
            }

            this.d = (long)(ed.b(5) * 1000 + 3000);
         }
      }

   }

   public final void a(byte var1) {
      switch (var1) {
         case 0:
            this.l = false;
            this.k = false;
            break;
         case 1:
            this.l = false;
            this.k = true;
            break;
         case 2:
            this.l = true;
            this.k = false;
            break;
         case 3:
            this.l = true;
            this.k = true;
      }

      this.a = var1 != 0;
   }
}
