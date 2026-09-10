import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class dh extends eh {
   public df a;
   private Image a;
   private String a;
   private int d;
   private long b;
   public int a;
   public int b;
   public int c;
   private int e;
   private int f;
   private Vector a = new Vector();
   public boolean a;
   private int g;
   private int h = 2;
   private int o = 0;
   private gt[] a = new gt[0];

   public dh(df var1, int var2, String var3, int var4, int var5) {
      this.a(new gy(-10, -30, 20, 30));
      this.a = var1;
      this.b = var2;
      this.a = var3;
      this.i = var1.i - 10;
      this.j = var1.j;
      this.h = var4;
      this.o = var5;
   }

   public final void a(int var1, int var2) {
      BaseCanvas.g.translate(-var1, -var2);
      BaseCanvas.g.drawImage(cp.i, this.i - 11, this.j - 6, 0);
      if (this.a == null) {
         this.a = dj.a.a(this.a);
         if (this.a != null) {
            this.e = this.a.getWidth() / this.h;
            this.f = this.a.getHeight();
         }
      } else {
         this.a(this.i, this.j + this.o, true);
         BaseCanvas.g.drawRegion(this.a, this.d * this.e, 0, this.e, this.f, this.a, this.i, this.j - this.a.getHeight() + this.o, 17);
         long var3;
         if ((var3 = System.currentTimeMillis()) - this.b >= 200L) {
            this.b = var3;
            this.d = (this.d + 1) % this.h;
         }

         if (this.a) {
            ++this.g;
            if (this.g > 10) {
               this.g = 0;
            }

            BaseCanvas.g.drawImage(fr.a, this.i, this.j - this.g - this.a.getHeight(), 3);
         }

         this.a(this.i, this.j + this.o, false);
      }

      long var7 = System.currentTimeMillis();

      for(int var5 = 0; var5 < this.a.size(); ++var5) {
         dq var6;
         if ((var6 = (dq)this.a.elementAt(var5)).a <= var7 && var6.a + 1000L > var7) {
            switch (var6.a) {
               case 0:
                  cp.e.a(BaseCanvas.g, var6.a, this.i, this.j - var6.b - 30, 17);
                  break;
               case 1:
                  cp.d.a(BaseCanvas.g, var6.a, this.i, this.j - var6.b - 30, 17);
                  break;
               case 2:
                  gv.b.a(BaseCanvas.g, var6.a, this.i, this.j - var6.b - 30, 17);
                  break;
               case 3:
                  cp.d().a(BaseCanvas.g, var6.a, this.i, this.j - var6.b - 30, 17);
            }

            ++var6.b;
         } else if (var6.a + 1000L <= var7) {
            this.a.removeElement(var6);
         }
      }

      BaseCanvas.g.translate(var1, var2);
   }

   public final void a(String var1, int var2, long var3) {
      dq var5;
      (var5 = new dq()).a = var3;
      var5.b = 0;
      var5.a = var1;
      var5.a = var2;
      this.a.addElement(var5);
   }

   public final void a(gt[] var1) {
      this.a = var1;
   }

   public final void a(long var1) {
      super.a(var1);

      for(int var3 = 0; var3 < this.a.length; ++var3) {
         this.a[var3].a();
      }

   }

   private void a(int var1, int var2, boolean var3) {
      for(int var4 = 0; var4 < this.a.length; ++var4) {
         gt var5;
         if ((var5 = this.a[var4]).a == var3) {
            var5.a(BaseCanvas.g, var1, var2);
         }
      }

   }
}
