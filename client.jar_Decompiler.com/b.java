import javax.microedition.lcdui.Graphics;
import javax.microedition.lcdui.Image;

public abstract class b {
   private a a;
   private int a;
   private int b;
   private int c;
   private int d;
   private int e;
   private int f;

   public b(a var1) {
      this.a = var1;
   }

   public final void a() {
      this.a = 0;
      this.c = this.a.a[1] - this.a.a[0] + 1;
      this.a(0);
      this.d();
   }

   public final void a(int var1) {
      this.b = var1;
      this.e = 0;
      this.f = this.a.b[this.a.a[0] + var1 << 2];
   }

   public final void b() {
      this.d = -1;
   }

   public final void c() {
      if (this.e < this.a.b[(this.a.a[0] + this.b << 2) + 1]) {
         ++this.e;
      } else {
         if (this.b >= this.c - 1) {
            if (this.d < 0) {
               this.e();
               return;
            }

            this.b = this.d - 1;
         }

         this.a(this.b + 1);
         int var1 = this.a.a[0] + this.b;
         short var2 = this.a.b[(var1 << 2) + 2];
         var1 = this.a.b[(var1 << 2) + 3];
         this.a(this.a() == 1 ? -var2 : var2, this.a() == 2 ? -var1 : var1);
         ++this.e;
      }
   }

   public final void a(Graphics var1) {
      byte var4 = 0;
      int var7 = this.a.f[this.f << 1];
      short var8 = this.a.f[(this.f << 1) + 1];
      int var9 = var1.getClipX();
      int var10 = var1.getClipY();
      int var11 = var1.getClipWidth();

      for(int var12 = var1.getClipHeight(); var7 < var8; var1.setClip(var9, var10, var11, var12)) {
         int var2 = (char)var7;
         int var3 = var7 + 1;
         short var5 = this.a.c[var2];
         var2 = var3 + 1;
         var3 = this.a.c[var3];
         int var6 = var2 + 1;
         var2 = this.a.c[var2];
         var7 = var6 + 1;
         if (((var6 = (byte)this.a.c[var6]) & 1) == 0) {
            byte var13 = (byte)((var6 & 248) >> 3);
            byte var14 = (byte)((byte)(var6 & 7) >> 1);
            short var15 = (short)var2;
            short var16 = (short)var3;
            int var17;
            int var18 = (var17 = var5 << 2) + 1;
            short var19 = this.a.d[var17];
            var6 = var18 + 1;
            short var20 = this.a.d[var18];
            short var21 = this.a.d[var6];
            short var22 = this.a.d[var6 + 1];
            var6 = this.a();
            if (var14 == var6) {
               var4 = 0;
            } else if (var14 != 0 && var6 != 0) {
               System.out.println("FLIP H and FLIP V, cannot be used at a same time, use your own implementation");
            } else {
               var4 = (byte)(var14 + var6);
            }

            if (var6 == 1) {
               var3 = (short)(-var3 - var21);
               var2 = var2;
            } else {
               var3 = var3;
               var2 = var2;
               if (var6 == 2) {
                  var2 = (short)(-var15 - var22);
                  var3 = var16;
               }
            }

            if (this.a.a) {
               Image var51 = ((Image[][])this.a.a.elementAt(var13))[var5 - this.a.e[var13]][0];
               var3 += this.a();
               var2 += this.b();
               if (var4 == 0) {
                  var1.drawImage(var51, var3, var2, 20);
               } else if (var4 == 1) {
                  var1.drawRegion(var51, 0, 0, var21, var22, 2, var3, var2, 20);
               } else if (var4 == 2) {
                  var1.drawRegion(var51, 0, 0, var21, var22, 2, var3, var2, 20);
               }
            } else {
               Image[] var52 = (Image[])this.a.a.elementAt(var13);
               if (var4 == 1) {
                  var6 = (short)(var52[0].getWidth() - var21 - var19);
                  var5 = var20;
               } else {
                  var6 = var19;
                  var5 = var20;
                  if (var4 == 1) {
                     var5 = (short)(var52[0].getHeight() - var22 - var20);
                     var6 = var19;
                  }
               }

               var3 += this.a();
               var2 += this.b();
               var1.clipRect(var3, var2, var21, var22);
               if (var4 == 0) {
                  var1.drawImage(var52[0], var3 - var6, var2 - var5, 20);
               } else if (var4 == 1) {
                  var1.drawRegion(var52[0], 0, 0, var52[0].getWidth(), var52[0].getHeight(), 2, var3 - var6, var2 - var5, 20);
               } else if (var4 == 2) {
                  var1.drawRegion(var52[0], 0, 0, var21, var22, 2, var3 - var6, var2 - var5, 20);
               }
            }
         } else if (var6 != 1 && var6 != 3) {
            if (var6 == 5) {
               int var54 = var5 * 3;
               int var58 = this.a.b[var54];
               int var62 = this.a.b[var54 + 1];
               int var66 = this.a.b[var54 + 2];
               int var70 = var62;
               int var74 = var58;
               short var78 = (short)var2;
               var6 = var3;
               byte var82;
               if ((var82 = this.a()) == 1) {
                  var3 = (short)(-var3);
                  var74 = -var58;
                  var2 = var2;
               } else {
                  var3 = var3;
                  var2 = var2;
                  if (var82 == 2) {
                     var2 = (short)(-var78);
                     var70 = -var62;
                     var3 = var6;
                  }
               }

               int var86 = var3 + this.a();
               int var90 = var74 + this.a();
               var1.setColor(var66);
               var1.drawLine(var86, var2 + this.b(), var90, var70 + this.b());
            } else if (var6 != 7 && var6 != 9) {
               if (var6 == 11 || var6 == 13) {
                  int var56 = var5 * 5;
                  int var60 = this.a.d[var56];
                  int var64 = this.a.d[var56 + 1];
                  int var68 = this.a.d[var56 + 2];
                  int var72 = this.a.d[var56 + 3];
                  int var76 = this.a.d[var56 + 4];
                  boolean var80 = var6 == 13;
                  var6 = var2;
                  short var84 = (short)var3;
                  byte var88;
                  if ((var88 = this.a()) == 1) {
                     var3 = (short)(-var3 - var60);
                     var2 = var2;
                  } else {
                     var3 = var3;
                     var2 = var2;
                     if (var88 == 2) {
                        var2 = (short)(-var6 - var64);
                        var3 = var84;
                     }
                  }

                  int var91 = var3 + this.a();
                  var6 = var2 + this.b();
                  var1.setColor(var76);
                  if (var80) {
                     var1.fillRoundRect(var91, var6, var60, var64, var68, var72);
                  } else {
                     var1.drawRoundRect(var91, var6, var60, var64, var68, var72);
                  }
               }
            } else {
               int var55 = var5 * 3;
               int var59 = this.a.c[var55];
               int var63 = this.a.c[var55 + 1];
               int var67 = this.a.c[var55 + 2];
               boolean var71 = var6 == 9;
               short var75 = (short)var2;
               short var79 = (short)var3;
               var1.setColor(var67);
               if ((var6 = this.a()) == 1) {
                  var3 = (short)(-var3 - var59);
                  var2 = var2;
               } else {
                  var3 = var3;
                  var2 = var2;
                  if (var6 == 2) {
                     var2 = (short)(-var75 - var63);
                     var3 = var79;
                  }
               }

               int var83 = var3 + this.a();
               int var87 = var2 + this.b();
               if (var71) {
                  var1.fillRect(var83, var87, var59, var63);
               } else {
                  var1.drawRect(var83, var87, var59, var63);
               }
            }
         } else {
            int var53 = var5 * 5;
            int var57 = this.a.a[var53];
            int var61 = this.a.a[var53 + 1];
            int var65 = this.a.a[var53 + 2];
            int var69 = this.a.a[var53 + 3];
            int var73 = this.a.a[var53 + 4];
            boolean var77 = var6 == 3;
            var6 = var2;
            short var81 = (short)var3;
            byte var85;
            if ((var85 = this.a()) == 1) {
               var3 = (short)(-var3 - var57);
               var2 = var2;
            } else {
               var3 = var3;
               var2 = var2;
               if (var85 == 2) {
                  var2 = (short)(-var6 - var61);
                  var3 = var81;
               }
            }

            int var89 = var3 + this.a();
            var6 = var2 + this.b();
            var1.setColor(var73);
            if (var77) {
               var1.fillArc(var89, var6, var57, var61, var65, var69);
            } else {
               var1.drawArc(var89, var6, var57, var61, var65, var69);
            }
         }
      }

   }

   protected abstract int a();

   protected abstract int b();

   protected abstract void a(int var1, int var2);

   protected abstract byte a();

   protected abstract void d();

   protected abstract void e();
}
