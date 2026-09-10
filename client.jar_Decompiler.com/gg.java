import java.util.Vector;
import javax.microedition.lcdui.Graphics;
import javax.microedition.lcdui.Image;

public final class gg {
   public static int a;
   public static String[] a;
   public static Image a;
   private Image c;
   private String a;
   private byte[] a;
   public int b;
   private int c;
   private boolean b = true;
   public static String[] b = new String[]{":D", ":P", ":)", ":@", "(c)", "/--", "(w)", "(b)", ":(", "(d)", "(s)", "8|", "(y)", "(n)", ":*", "U-", "(l)", ":S", "(?)", ":zZ", "(B)", "(h)", "(u)", "@^", "@-"};
   public static Image b = null;
   public static boolean a = true;

   public gg(gg var1) {
      this.a = var1.a;
      this.a = var1.a;
      this.b = var1.b;
      this.c = var1.c;
      this.c = gq.b(var1.c, -16777216);
   }

   public gg(gg var1, int var2, int var3) {
      this.a = var1.a;
      this.a = var1.a;
      this.b = var1.b;
      this.c = var1.c;
      this.c = gq.a(var1.c, var2, var3);
   }

   public gg(String var1, byte[] var2, int var3, Image var4, int var5) {
      this.a = var1;
      this.a = var2;
      this.b = var3;
      this.c = var5;
      this.c = var4;
   }

   public final void a(Graphics var1, String var2, int var3, int var4, int var5) {
      label68:
      while(true) {
         int var6 = var3;
         int var7 = var2.length();
         if ((var5 & 8) > 0) {
            var6 = var3 - this.a(var2);
         } else if ((var5 & 1) > 0) {
            var6 = var3 - (this.a(var2) >> 1);
         }

         if (a && b != null) {
            for(int var11 = 0; var11 < b.length; ++var11) {
               String var14 = b[var11];
               int var8;
               if ((var8 = var2.indexOf(var14)) >= 0) {
                  String var19 = var2.substring(0, var8);
                  var2 = var2.substring(var8 + var14.length(), var2.length());
                  this.a(var1, var19, var6, var4, 20);
                  int var15 = this.a(var19);
                  var1.drawRegion(b, var11 * a, 0, a, a, 0, var6 + var15, var4 + (this.b >> 1), 6);
                  int var10003 = a + var6 + var15;
                  var5 = 20;
                  var4 = var4;
                  var3 = var10003;
                  var2 = var2;
                  var1 = var1;
                  this = this;
                  continue label68;
               }
            }
         }

         if (a != null && a != null) {
            for(int var12 = 0; var12 < a.length; ++var12) {
               String var16 = a[var12];
               int var21;
               if ((var21 = var2.indexOf(var16)) >= 0) {
                  String var20 = var2.substring(0, var21);
                  var2 = var2.substring(var21 + var16.length(), var2.length());
                  this.a(var1, var20, var6, var4, 20);
                  int var17 = this.a(var20);
                  var1.drawRegion(a, var12 * 15, 0, 15, 15, 0, var6 + var17, var4 + (this.b >> 1), 6);
                  int var22 = a + var6 + var17;
                  var5 = 20;
                  var4 = var4;
                  var3 = var22;
                  var2 = var2;
                  var1 = var1;
                  this = this;
                  continue label68;
               }
            }
         }

         for(int var13 = 0; var13 < var7; ++var13) {
            if ((var5 = this.a.indexOf(var2.charAt(var13))) == -1) {
               var5 = 0;
            }

            if (var5 >= 0 && this.b) {
               var1.drawRegion(this.c, 0, var5 * this.b, this.a[var5], this.b, 0, var6, var4, 20);
            }

            var6 += this.a[var5] + (var13 < var7 - 1 ? this.c : 0);
         }

         return;
      }
   }

   public final void b(Graphics var1, String var2, int var3, int var4, int var5) {
      this.a(var1, var2, var3, var4, var5);
   }

   public final int a(String var1) {
      if (a && b != null && b != null) {
         for(int var2 = 0; var2 < b.length; ++var2) {
            String var3 = b[var2];
            int var4;
            if ((var4 = var1.indexOf(var3)) >= 0) {
               String var5 = var1.substring(0, var4);
               var1 = var1.substring(var4 + var3.length(), var1.length());
               return (var2 = 0 + this.a(var5)) + a + this.a(var1);
            }
         }
      }

      if (a != null && a != null) {
         for(int var9 = 0; var9 < a.length; ++var9) {
            String var12 = a[var9];
            int var14;
            if ((var14 = var1.indexOf(var12)) >= 0) {
               String var16 = var1.substring(0, var14);
               var1 = var1.substring(var14 + var12.length(), var1.length());
               return (var9 = 0 + this.a(var16)) + a + this.a(var1);
            }
         }
      }

      int var11 = 0;
      int var13 = var1.length();

      for(int var15 = 0; var15 < var13; ++var15) {
         int var17;
         if ((var17 = this.a.indexOf(var1.charAt(var15))) == -1) {
            var17 = 0;
         }

         var11 += this.a[var17] + (var15 < var13 - 1 ? this.c : 0);
      }

      return var11;
   }

   public final String[] a(String var1, int var2) {
      Vector var3 = new Vector();
      int var4 = 0;
      int var5 = var1.length();

      for(int var6 = 0; var4 < var5; var6 = var4) {
         int var7 = var4;

         int var8;
         for(var8 = -1; var7 < var5; ++var7) {
            if (this.a(var1.substring(var4, var7 + 1)) > var2) {
               if (var8 == -1) {
                  var8 = var7;
               }
               break;
            }

            if (var1.charAt(var7) == ' ') {
               var8 = var7;
            } else if (var1.charAt(var7) == '\n') {
               var8 = var7;
               break;
            }
         }

         if (var7 != var5 && var8 > var4) {
            var4 = var8;
         } else {
            var4 = var7;
         }

         var3.addElement(var1.substring(var6, var4).trim());
         if (var4 >= 0 && var4 < var5 && var1.charAt(var4) == '\n') {
            ++var4;
         }
      }

      String[] var10 = new String[var3.size()];
      var3.copyInto(var10);
      return var10;
   }

   public final int a() {
      return this.b;
   }
}
