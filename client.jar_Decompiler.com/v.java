import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class v extends eh {
   public static Image a;
   private static int a;
   private static int b;
   private static int c;
   private static int d;
   private static int e;
   private static int f;
   private static int g;
   private static Image[][] a = new Image[2][2];
   private static Image[][] b = new Image[2][2];
   private static Image[][] c = new Image[2][2];
   private static Image[][] d = new Image[2][2];
   private static Image[][] e = new Image[2][2];
   private static Image[][] f = new Image[2][2];
   private static Image[] a = new Image[2];
   private static Image[] b = new Image[2];
   private static final int[] a;
   private static final byte[][] a;

   public static void a(byte var0, int var1, int var2, int var3, boolean var4, int var5) {
      var5 = BaseCanvas.ticks + var5;
      boolean var6;
      boolean var7 = (boolean)(var6 = var4 && var5 % 8 < 4);
      var6 = var6 ? 2 : 1;
      var4 = var4 ? 0 : (var5 >> 5) % 2;
      var1 -= 27;
      var2 -= 74;
      BaseCanvas.g.translate(var1, var2);
      BaseCanvas.g.drawImage(a, 27, 68, 17);
      if (var3 == 1) {
         BaseCanvas.g.drawRegion(a[0], a[0][0], a[0][1], a[0][2], a[0][3], 0, 10, var4 + 22, 0);
         BaseCanvas.g.drawRegion(a[0], a[var6][0], a[var6][1], a[var6][2], a[var6][3], 0, a[var6], 60, 0);
      } else {
         BaseCanvas.g.drawRegion(a[1], 33 - a[0][0] - a[0][2], a[0][1], a[0][2], a[0][3], 0, 11, var4 + 22, 0);
         BaseCanvas.g.drawRegion(a[1], 33 - a[var6][0] - a[var6][2], a[var6][1], a[var6][2], a[var6][3], 0, 54 - a[var6] - a[var6][2], 60, 0);
      }

      Vector var16;
      a(var16 = new Vector(), var0);
      cp.a();

      for(int var10 = 0; var10 < var16.size(); ++var10) {
         ab var8 = (ab)var16.elementAt(var10);
         Image var9 = var3 == 1 ? var8.a : var8.b;
         a = var8.b;
         b = var8.c;
         c = var9.getWidth();
         d = var9.getHeight();
         if (var8.a == 4 && var5 / 5 % 16 == 13) {
            if (var3 == 1) {
               BaseCanvas.g.drawImage(b[0], 21, var4 + 38, 0);
            } else {
               BaseCanvas.g.drawRegion(b[1], 0, 0, 18, 11, 0, 15, var4 + 38, 0);
            }
         } else if (var8.a != 8 && var8.a != 9) {
            if (var3 == 1) {
               BaseCanvas.g.drawImage(var9, a, b + var4, 0);
            } else {
               BaseCanvas.g.drawImage(var9, 54 - a - c, b + var4, 0);
            }
         } else {
            e = 54 - a;
            if (var8.a == 9) {
               f = c - e - 3;
            } else {
               f = c - e + 1;
            }

            g = c - f;
            if (var3 == 1) {
               if (var7) {
                  BaseCanvas.g.drawRegion(var9, f, 0, g, d, 0, 0, b, 0);
               } else {
                  BaseCanvas.g.drawRegion(var9, 0, 0, e, d, 0, a, b, 0);
               }
            } else if (var7) {
               BaseCanvas.g.drawRegion(var9, 0, 0, e, d, 0, a + e - g, b, 0);
            } else {
               BaseCanvas.g.drawRegion(var9, f, 0, g, d, 0, e - g, b, 0);
            }
         }
      }

      BaseCanvas.g.translate(-var1, -var2);
   }

   private static void a(Vector var0, int var1) {
      if (var0 != null) {
         if (var1 == -1) {
            var1 = 0;
         }

         boolean[] var2 = new boolean[]{false, false, false, false, false, false};
         int var3 = var0.size();

         for(int var4 = 0; var4 < var3; ++var4) {
            switch (((ab)var0.elementAt(var4)).a) {
               case 2:
                  var2[1] = true;
                  break;
               case 3:
                  var2[4] = true;
                  break;
               case 4:
                  var2[0] = true;
               case 5:
               case 6:
               default:
                  break;
               case 7:
                  var2[3] = true;
                  break;
               case 8:
                  var2[2] = true;
                  break;
               case 9:
                  var2[5] = true;
            }
         }

         if (!var2[0]) {
            var0.addElement(a(4, (byte)var1));
         }

         if (!var2[1]) {
            var0.addElement(a(2, (byte)var1));
         }

         if (!var2[2]) {
            var0.addElement(a(8, (byte)var1));
         }

         if (!var2[3]) {
            var0.addElement(a(7, (byte)var1));
         }

         if (!var2[4]) {
            var0.addElement(a(3, (byte)var1));
         }

         if (!var2[5]) {
            var0.addElement(a(9, (byte)var1));
         }

         int var9 = var0.size();

         for(int var6 = 0; var6 < var9 - 1; ++var6) {
            for(int var7 = var6 + 1; var7 < var9; ++var7) {
               ab var8 = (ab)var0.elementAt(var6);
               ab var5;
               if ((var5 = (ab)var0.elementAt(var7)).d < var8.d) {
                  var0.setElementAt(var5, var6);
                  var0.setElementAt(var8, var7);
               }
            }
         }

      }
   }

   private static ab a(int var0, byte var1) {
      switch (var0) {
         case 2:
            return new ab(2, var1 == 0 ? 0 : 7, 15, -5, b[var1][0], b[var1][1]);
         case 3:
            return new ab(3, var1 == 0 ? -1 : 0, 14, -4, c[var1][0], c[var1][1]);
         case 4:
            return new ab(4, 18, var1 == 0 ? 35 : 36, -4, a[var1][0], a[var1][1]);
         case 5:
         case 6:
         default:
            return null;
         case 7:
            return new ab(7, var1 == 0 ? 22 : 18, var1 == 0 ? 51 : 50, -2, e[var1][0], e[var1][1]);
         case 8:
            return new ab(8, var1 == 0 ? 22 : 21, 59, -3, d[var1][0], d[var1][1]);
         case 9:
            return new ab(9, 21, 65, -4, f[var1][0], f[var1][1]);
      }
   }

   public static void a() {
      gu.a(cp.a);
      a[0] = gu.a(0);
      a[1] = ed.a(a[0]);
      b[0] = gu.a(1);
      b[1] = ed.a(b[0]);
      a = gu.a(15);
      a[0][0] = gu.a(8);
      a[0][1] = ed.a(a[0][0]);
      a[1][0] = gu.a(7);
      a[1][1] = ed.a(a[1][0]);
      b[0][0] = gu.a(14);
      b[0][1] = ed.a(b[0][0]);
      b[1][0] = gu.a(13);
      b[1][1] = ed.a(b[1][0]);
      d[0][0] = gu.a(12);
      d[0][1] = ed.a(d[0][0]);
      d[1][0] = gu.a(11);
      d[1][1] = ed.a(d[1][0]);
      e[0][0] = gu.a(4);
      e[0][1] = ed.a(e[0][0]);
      e[1][0] = gu.a(3);
      e[1][1] = ed.a(e[1][0]);
      c[0][0] = gu.a(10);
      c[0][1] = ed.a(c[0][0]);
      c[1][0] = gu.a(9);
      c[1][1] = ed.a(c[1][0]);
      f[0][0] = gu.a(6);
      f[0][1] = ed.a(f[0][0]);
      f[1][0] = gu.a(5);
      f[1][1] = ed.a(f[1][0]);
   }

   public static void a(Image var0, int var1, int var2, int var3, boolean var4) {
      BaseCanvas.g.drawRegion(var0, (var4 && (BaseCanvas.ticks + 1000) % 8 < 4 ? 1 : 0) * (var0.getWidth() >> 1), 0, var0.getWidth() >> 1, var0.getHeight(), var3 == 0 ? 2 : 0, var1, var2, 33);
   }

   static {
      short[][][] var10000 = new short[][][]{{{26, 42, 22}, {24, 42, 24}}, {{22, 5, 27}, {0, 20, 67}}, {{23, 43, 26}, {23, 43, 26}}, {{9, 0, 47}, {23, 42, 26}}, {{26, 41, 23}}, {{24, 43, 25}}, {{25, 41, 23}}};
      a = new int[]{0, 21, 20};
      a = new byte[][]{{0, 0, 33, 42}, {0, 42, 11, 14}, {11, 42, 16, 14}};
   }
}
