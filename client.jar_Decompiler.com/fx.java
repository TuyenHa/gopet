import javax.microedition.lcdui.Image;
import thong.sdk.ISoundManagerSDK;
import vn.me.core.BaseCanvas;

public final class fx extends fw implements Runnable {
   private int a;
   private int b = 40;
   private Image a;
   private byte[][] a;
   private byte[] a;
   private byte[] b;
   private byte a;
   private boolean a;
   private Image c;

   public fx(int var1) {
      super(true);
      this.a = var1;
      if (var1 == 0) {
         gu.a(gv.a);
         gv.b();
         this.a(var1);
      } else {
         if (var1 == 1) {
            this.a(var1);
            this.a = new byte[][]{{0, 16}, {17, 12}, {29, 14}, {43, 15}, {64, 10}, {75, 12}, {88, 5}, {97, 6}, {105, 6}, {113, 6}};
            this.a = new byte[10];
            this.b = new byte[]{0, -2, 0, 3};
         }

      }
   }

   public final void a(int var1, boolean var2) {
      super.a(var1, var2);
      this.a = false;
      this.run();
   }

   public final void c_() {
      switch (this.a) {
         case 0:
            if (this.b <= 0 && this.a) {
               (new fb()).a(0, false);
               return;
            }

            --this.b;
            return;
         case 1:
            for(int var1 = 0; var1 < 10; ++var1) {
               if (this.a[var1] > 0) {
                  byte[] var2 = this.a;
                  --var2[var1];
               }
            }

            if (this.a < 10) {
               this.a[this.a] = 3;
            }

            ++this.a;
            if (this.a > 15) {
               this.a = 0;
               return;
            }

            return;
         default:
      }
   }

   public final void u() {
      switch (this.a) {
         case 0:
            BaseCanvas.g.setColor(0);
            BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
            BaseCanvas.g.drawImage(this.c, BaseCanvas.Field157, BaseCanvas.Field158, 3);
            BaseCanvas.g.setColor(16736256);
            int var3 = BaseCanvas.ticks % 40;
            int var4 = BaseCanvas.Field158 + this.c.getHeight() / 2 + 5;
            if (var3 > 30) {
               BaseCanvas.g.fillRect(BaseCanvas.Field157 + 8, var4, 4, 4);
            }

            if (var3 > 20) {
               BaseCanvas.g.fillRect(BaseCanvas.Field157 - 2, var4, 4, 4);
            }

            if (var3 > 10) {
               BaseCanvas.g.fillRect(BaseCanvas.Field157 - 12, var4, 4, 4);
               return;
            }

            return;
         case 1:
            BaseCanvas.g.setColor(0);
            BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
            int var1 = this.a.getHeight();

            for(int var2 = 0; var2 < 10; ++var2) {
               BaseCanvas.g.drawRegion(this.a, this.a[var2][0], 0, this.a[var2][1], var1, 0, BaseCanvas.w - this.a.getWidth() - 10 + this.a[var2][0], BaseCanvas.h - this.b[this.a[var2]] - 5, 36);
            }

            return;
         default:
      }
   }

   public static void f() {
      gs.a();
      if (cx.a != null) {
         cx.a.b = false;
      }

      if (gv.b != null) {
         gv.b = new gg(gv.b);
      }

   }

   private void a(int var1) {
      try {
         if (var1 != 0) {
            if (var1 == 1) {
               gu.a("/common.dat");
               this.a = gu.a(15);
            }

            return;
         }

         this.c = Image.createImage("/meLogo.png");
      } catch (Exception var2) {
         var2.printStackTrace();
      }

   }

   public final void run() {
      switch (this.a) {
         case 0:
            cg.a = new cg();
            f();
            gv.a();
            gu.a(5);
            gu.a(6);
            gv.a = gu.a(0);
            gv.a = new gr(gu.a(7), 6);
            cp.b();
            cp.a();
            el.b();
            ISoundManagerSDK.loadMusicState();
            ISoundManagerSDK.saveMusicState();
            ISoundManagerSDK.playBgSound("s_login");

            try {
               int var1;
               if ((var1 = a.a(a.a)) >= 0) {
                  a.a = var1;
               }
            } catch (Exception var2) {
            }
         default:
            this.a = true;
      }
   }
}
