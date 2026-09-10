import java.io.IOException;
import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class ex extends fw {
   private l a;
   private do[] a;
   private String[] a;
   private r a;
   private do a;
   private q a;
   private q b;
   private int a = 96;
   private int b;
   private int c;

   public ex() {
      super(true);
      this.b = BaseCanvas.w - 30 - 30 - 15;
      this.c = BaseCanvas.w - 30 - 15;
      this.f = true;
      this.a = cp.c();
      this.n = new cd(1, gw.a(2), this);
      this.a = new l(114, BaseCanvas.w - 30, BaseCanvas.h - 100 - 15 - 9 - gs.m, 30);
      this.b.a(this.a);
      this.m = new cd(2, gw.a(7), this);
      Image var2 = null;

      try {
         var2 = Image.createImage("/pet/left.png");
      } catch (IOException var4) {
         var4.printStackTrace();
      }

      this.a = new q(var2);
      this.a.a(this.b, this.a, 30, 20);
      this.a.a(new cd(17, "", this));
      var2 = null;

      try {
         var2 = Image.createImage("/pet/right.png");
      } catch (IOException var3) {
         var3.printStackTrace();
      }

      this.b = new q(var2);
      this.b.a(this.c, this.a, 30, 20);
      this.b.a(new cd(18, "", this));
   }

   public final void a(do[] var1) {
      this.b.b(this.a);
      this.b.b(this.b);
      if (var1 != null && var1.length > 0) {
         this.a = var1;

         for(int var3 = 0; var3 < this.a.length; ++var3) {
            this.a[var3].b = this;
         }

         this.a.a(var1);
         this.a.a(0);
         if (this.a.a > 1) {
            this.b.a(this.a);
            this.b.a(this.b);
         }

      } else {
         gj var2;
         (var2 = new gj(gw.a(27), gv.a)).g = false;
         this.a.o();
         this.a.a(var2);
      }
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
      dj.a.a(0);
      gs.a(10, 20, BaseCanvas.w - 20, 80);
      if (this.a != null) {
         int var1 = 25;

         for(int var2 = 0; var2 < this.a.length; ++var2) {
            gv.a.a(BaseCanvas.g, this.a[var2], 20, var1, 0);
            var1 += gv.a.a() + 2;
         }
      }

      gs.a(10, 100, BaseCanvas.w - 20, BaseCanvas.h - 100 - 5 - gs.m);
      int var3 = 20;

      for(int var4 = 0; var4 < this.a.a; ++var4) {
         if (var4 == this.a.b) {
            BaseCanvas.g.setColor(16777215);
         } else {
            BaseCanvas.g.setColor(10000536);
         }

         BaseCanvas.g.fillArc(var3, 107, 5, 5, 0, 360);
         var3 += 10;
      }

   }

   public final void a(Object var1) {
      cd var2;
      if ((var2 = (cd)((Object[])var1)[0]) == null) {
         gn var11 = (gn)((Object[])var1)[1];
         boolean var14 = false;

         for(int var15 = 0; var15 < this.a.length; ++var15) {
            if (var11 == this.a[var15]) {
               var14 = true;
            }
         }

         if (!var14) {
            this.a = null;
         } else {
            do var16;
            if ((var16 = (do)var11).c != null) {
               this.a = gv.a.a(var16.c, BaseCanvas.w - 40);
            }
         }
      } else {
         switch (var2.a) {
            case 1:
               this.t();
               return;
            case 2:
               gn var10;
               if ((var10 = this.a.a(true)) != null && var10 != this.a) {
                  this.a.a(true);
                  Vector var13;
                  (var13 = new Vector()).addElement(new cd(3, gw.a(28), this));
                  var13.addElement(new cd(4, gw.a(29), this));
                  var13.addElement(new cd(5, gw.a(2), this));
                  this.a(var13, 2);
                  return;
               }

               return;
            case 3:
               gn var3;
               do var9;
               if ((var3 = this.a.a(true)) != null && var3 != this.a && (var9 = (do)this.a.a(true)) != null) {
                  this.a = new r(2);
                  this.a.a(0, var9.a, var9.b, var9.c, var9.b);
                  this.a(this.a, false);
                  this.a = var9;
                  return;
               }

               return;
            case 4:
               gn var7;
               if ((var7 = this.a.a(true)) != null && var7 != this.a && (var7 = (do)this.a.a(true)) != null) {
                  this.a = new r(3);
                  this.a.a(0, var7.a, var7.b, var7.c, var7.b);
                  this.a(this.a, false);
                  this.a = var7;
                  return;
               }

               return;
            case 5:
               gn var4;
               if ((var4 = this.a.a(true)) != null && var4 != this.a && (var4 = (do)this.a.a(true)) != null) {
                  cg.f();
                  int var6 = var4.a;
                  en var12;
                  (var12 = new en(81)).a(83);
                  var12.b(var6);
                  cx.a.a(var12);
                  var12.a();
                  return;
               }

               return;
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            default:
               super.a(var1);
               return;
            case 17:
               this.a.a(this.a.b == 0 ? 0 : this.a.b - 1);
               return;
            case 18:
               this.a.a(this.a.b == this.a.a - 1 ? this.a.a - 1 : this.a.b + 1);
         }
      }
   }

   public final void a(en var1) {
      if (this.a != null) {
         try {
            int var2 = var1.a().readInt();
            String var3 = var1.a().readUTF();
            String var4 = var1.a().readUTF();
            int var5 = var1.a().readInt();
            switch (this.a.a) {
               case 2:
                  byte var7 = 1;
                  if (var5 == 12) {
                     var7 = 2;
                  }

                  this.a.a(var7, var2, var3, var4, (byte)0);
                  break;
               case 3:
                  this.a.a(1, var2, var3, var4, (byte)var1.a().readInt());
            }

            this.a(this.a, true);
         } catch (Exception var6) {
            var6.printStackTrace();
         }
      }
   }

   public final void b(en var1) {
      try {
         int var2 = var1.a().readInt();
         int var3 = var1.a().readInt();
         String var4 = var1.a().readUTF();
         int var11 = var1.a().readInt();
         if (this.a.a == 2) {
            for(int var5 = 0; var5 < this.a.length; ++var5) {
               if (this.a[var5].a == var2) {
                  this.a[var5].c = var4;
                  this.a[var5].b = String.valueOf(var3);
                  this.a[var5].a((byte)var11);
                  do[] var10000 = this.a;
               }
            }
         } else {
            do var12;
            (var12 = new do()).a(this.a);
            var12.a = var2;
            var12.b = String.valueOf(var3);
            var12.c = var4;
            var12.a((byte)var11);
            var12.b = this;
            do[] var6 = new do[this.a.length - 1];
            int var7 = 0;

            for(int var8 = 0; var8 < this.a.length; ++var8) {
               if (this.a[var8].a != this.a.a[0] && this.a[var8].a != this.a.a[1]) {
                  int var9 = var7++;
                  var6[var9] = this.a[var8];
               }
            }

            var6[var7] = var12;
            this.a = new do[var6.length];
            System.arraycopy(var6, 0, this.a, 0, this.a.length);
            this.b.b(this.a);
            this.a.a(this.a);
            this.a.a(0);
            this.b.a(this.a);
         }

         this.x();
         s var13 = new s(true);
         do var14;
         (var14 = new do()).a = var2;
         var14.b = String.valueOf(var3);
         var14.a((byte)var11);
         var13.a(var14, var4);
         this.a(var13, false);
      } catch (Exception var10) {
         var10.printStackTrace();
      }
   }

   public final void c(en var1) {
      try {
         int var7 = var1.a().readInt();
         do[] var2 = new do[this.a.length - 1];
         int var3 = 0;

         for(int var4 = 0; var4 < this.a.length; ++var4) {
            if (this.a[var4].a != var7) {
               int var5 = var3++;
               var2[var5] = this.a[var4];
            }
         }

         this.a = new do[var2.length];
         System.arraycopy(var2, 0, this.a, 0, this.a.length);
         this.b.b(this.a);
         this.a.a(this.a);
         this.a.a(0);
         this.b.a(this.a);
         this.x();
      } catch (Exception var6) {
         var6.printStackTrace();
      }
   }

   public final void d(en param1) {
      // $FF: Couldn't be decompiled
   }
}
