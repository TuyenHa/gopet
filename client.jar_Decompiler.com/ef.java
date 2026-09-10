import java.io.ByteArrayInputStream;
import java.io.DataInputStream;
import java.io.IOException;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class ef {
   public int a;
   public int b;
   private int e;
   public int c;
   public int d;
   private static byte[][][] a = new byte[][][]{{{0, 0}, {0, 1}}, {{0, 0}, {1, 0}}, {{0, 0}, {1, 1}}, {{0, 1}, {0, 0}}, {{0, 1}, {0, 1}}, {{0, 1}, {1, 0}}, {{0, 1}, {1, 1}}, {{1, 0}, {0, 0}}, {{1, 0}, {0, 1}}, {{1, 0}, {1, 0}}, {{1, 0}, {1, 1}}, {{1, 1}, {0, 0}}, {{1, 1}, {0, 1}}, {{1, 1}, {1, 0}}};
   private int f;
   private byte[][][] b;
   private byte[][] a;
   private Image[] a;
   private int[] a;
   private byte[] d;
   public z[] a;
   public short[] a;
   public short[] b;
   public byte[] a;
   public byte[] b;
   public byte[] c;
   public y[] a;
   public eg[] a;
   private gy a;

   public ef() {
      this.a = new gy(0, 0, BaseCanvas.w, BaseCanvas.h);
   }

   public ef(int var1, dv var2) {
      this.a = new gy(0, 0, BaseCanvas.w, BaseCanvas.h);
      this.a = var1;
      DataInputStream var3 = null;
      if ((a(var1) != b(var1) || !a(var1)) && (var1 < 2 || var1 > 5)) {
         byte[] var4;
         if ((var4 = a.a("mapDynamicData_" + var1)) != null) {
            var3 = new DataInputStream(new ByteArrayInputStream(var4));
         }
      } else {
         var3 = new DataInputStream(gv.a("/maps/" + var1 + ".dat"));
      }

      this.a(var3, var2);
   }

   public static int a(int var0) {
      Integer var1;
      if ((var1 = a.a("mapVersion_" + var0)) == null) {
         return a(var0) ? b(var0) : -1;
      } else {
         return var1;
      }
   }

   private static boolean a(int var0) {
      try {
         (new DataInputStream(gv.a("/maps/" + var0 + ".dat"))).readByte();
         return true;
      } catch (Exception var1) {
         return false;
      }
   }

   public static void a(int var0, int var1) {
      String var2;
      a.a(var2 = "mapVersion_" + var0);
      a.a(var2, var1);
   }

   public static void a(byte[] var0, int var1) {
      try {
         a.a("mapDynamicData_" + var1);
         a(var1);
         a.a("mapDynamicData_" + var1, var0);
      } catch (Exception var2) {
      }
   }

   private static void a(int var0) {
      try {
         byte[] var5;
         if ((var5 = a.a("mapDynamicData_" + var0)) != null) {
            DataInputStream var6;
            int var1 = (var6 = new DataInputStream(new ByteArrayInputStream(var5))).readByte() + var6.readByte();

            for(int var2 = 0; var2 < var1; ++var2) {
               short var3 = var6.readShort();
               if (var6.readByte() == 1) {
                  a.a("ani11_" + var3);
               } else {
                  a.a("image10_" + var3);
               }
            }
         }

      } catch (Exception var4) {
      }
   }

   private void a(DataInputStream var1, dv var2) {
      try {
         int var3 = var1.readByte();
         this.a = new Image[var3];
         int var4 = var3 + var1.readByte();
         this.a = new int[var4];
         this.d = new byte[var4];

         for(int var5 = 0; var5 < var4; ++var5) {
            this.a[var5] = var1.readShort();
            this.d[var5] = var1.readByte();
         }

         for(int var16 = 0; var16 < var3; ++var16) {
            this.a[var16] = a("" + this.a[var16]);
         }

         this.b = var1.readByte();
         this.e = var1.readByte();
         this.c = this.b * 24;
         this.d = this.e * 24;
         this.f = var1.readByte();
         this.b = new byte[this.f][this.e][this.b];

         for(int var17 = 0; var17 < this.f; ++var17) {
            for(int var9 = 0; var9 < this.e; ++var9) {
               var1.read(this.b[var17][var9]);
            }
         }

         this.a = new byte[this.e][this.b];

         for(int var18 = 0; var18 < this.e; ++var18) {
            var1.read(this.a[var18]);
         }

         int var19 = var1.readInt();
         this.a = new short[var19];
         this.b = new short[var19];
         this.a = new byte[var19];
         this.b = new byte[var19];
         this.c = new byte[var19];
         this.a = new y[var4];

         for(int var10 = 0; var10 < var19; ++var10) {
            var4 = var1.readByte();
            this.a[var10] = (byte)var4;
            boolean var6 = false;
            if (this.a[var4] == null) {
               this.a[var4] = new y(this.a[var4], this.d[var4]);
               var6 = true;
            }

            y var7 = this.a[var4];
            this.a[var10] = var1.readShort();
            this.b[var10] = var1.readShort();
            this.c[var10] = var1.readByte();
            if (this.d[var4] == 1) {
               if (var6) {
                  var7.a(new gy(var1.readByte(), var1.readByte(), var1.readByte(), var1.readByte()));
                  var7.a = var1.readByte();
                  this.b[var10] = var7.a;
                  dy var13;
                  if ((var13 = a("" + var7.a)) != null) {
                     var7.a = new dz[var13.a.length];

                     for(int var20 = 0; var20 < var13.a.length; ++var20) {
                        var7.a[var20] = new dz(var13.a[var20]);
                     }
                  }
               } else {
                  for(int var14 = 0; var14 < 5; ++var14) {
                     this.b[var10] = var1.readByte();
                  }
               }
            } else if (var6 && var7.b == null) {
               var7.a(new gy(-64, -64, 127, 127));
               var7.b = a("" + var7.a);
            }
         }

         var3 = var1.readInt();
         this.a = new eg[var3];
         var4 = 0;

         for(int var21 = 0; var21 < var3; ++var21) {
            int var23 = var4++;
            this.a[var23] = eg.a(var2, var1);
         }

         byte var22 = var1.readByte();
         this.a = new z[var22];

         for(int var24 = 0; var24 < var22; ++var24) {
            this.a[var24] = new z(var1.readByte(), var1.readShort(), var1.readShort());
         }

         var1.close();
      } catch (Exception var8) {
         a(this.a, b(this.a));
      }
   }

   public final boolean a(int var1, int var2) {
      int var3 = var1 / 24;
      int var4 = var2 / 24;
      if (var3 >= 0 && var4 >= 0 && var3 < this.b && var4 < this.e) {
         if ((var3 = this.a[var4][var3]) == 0) {
            return true;
         } else if (var3 == 15) {
            return false;
         } else {
            return a[var3 - 1][var2 % 24 / 12][var1 % 24 / 12] == 0;
         }
      } else {
         return false;
      }
   }

   public final void a(int var1, int var2, boolean var3) {
      BaseCanvas.g.setColor(16777215);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
      int var4 = var1 / 24;
      int var5 = var2 / 24;
      if (var4 < 0) {
         var4 = 0;
      }

      if (var5 < 0) {
         var5 = 0;
      }

      int var6 = var5 + BaseCanvas.h / 24 + 2;
      int var7 = var4 + BaseCanvas.w / 24 + 2;
      if (var6 > this.e) {
         var6 = this.e;
      }

      if (var7 > this.b) {
         var7 = this.b;
      }

      BaseCanvas.g.translate(-var1, -var2);

      for(int var8 = 0; var8 < this.f; ++var8) {
         for(int var9 = var5; var9 < var6; ++var9) {
            for(int var10 = var4; var10 < var7; ++var10) {
               if (this.a[this.b[var8][var9][var10] >> 4] != null) {
                  BaseCanvas.g.drawRegion(this.a[this.b[var8][var9][var10] >> 4], ((this.b[var8][var9][var10] & 15) - 1) * 24, 0, 24, 24, 0, var10 * 24, var9 * 24, 0);
               } else {
                  this.a[this.b[var8][var9][var10] >> 4] = a("" + this.a[this.b[var8][var9][var10] >> 4]);
               }
            }
         }
      }

      if (var3) {
         this.a.a = var1;
         this.a.b = var2;

         for(int var11 = 0; var11 < this.a.length; ++var11) {
            y var12;
            gy var13;
            gy var10000 = var13 = (var12 = this.a[this.a[var11]]).a();
            var10000.a += this.a[var11];
            var13.b += this.b[var11] - this.c[var11];
            if (this.a.a(var13)) {
               var12.a = this.b[var11];
               var12.b(this.a[var11], this.b[var11] - this.c[var11]);
            }
         }
      }

      BaseCanvas.g.translate(var1, var2);
   }

   private static int b(int var0) {
      switch (var0) {
         case 1:
         case 6:
         case 7:
         case 10:
            return 3;
         case 2:
         case 3:
         case 4:
         case 5:
         case 9:
         default:
            return 1;
         case 8:
            return 2;
      }
   }

   private static Image a(String var0) {
      try {
         return Image.createImage("/newMapData/" + var0 + ".png");
      } catch (IOException var1) {
         return null;
      }
   }

   private static dy a(String var0) {
      try {
         Image var1 = Image.createImage("/newMapData/" + var0 + "_a.png");
         DataInputStream var4 = new DataInputStream(gv.a("/newMapData/" + var0 + "_b"));
         dy var2;
         (var2 = new dy()).a(var4);

         for(int var5 = 0; var5 < var2.a.length; ++var5) {
            var2.a[var5].a = var1;
         }

         return var2;
      } catch (Exception var3) {
         return null;
      }
   }
}
