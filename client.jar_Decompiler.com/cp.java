import java.util.Hashtable;
import java.util.Vector;
import javax.microedition.lcdui.Graphics;
import javax.microedition.lcdui.Image;

public final class cp {
   public static Image[] a;
   public static Image[] b;
   private static gg f;
   public static gg a;
   public static gg b;
   public static gg c;
   public static gg d;
   public static gg e;
   private static gg g;
   private static gg h;
   private static gg i;
   public static Image a;
   public static Image b;
   public static Image c;
   public static Image d;
   public static Image e;
   public static Image f;
   public static Image g;
   public static Image h;
   public static Image i;
   public static Image j;
   public static Image k;
   private static Image l;
   public static String a = "/avatar.dat";
   private static Hashtable a = new Hashtable();
   private static Hashtable b = new Hashtable();
   private static Vector a = new Vector();

   public static void a() {
      gu.a("/common.dat");
      gr var0 = new gr(gu.a(18), 3);
      a = new Image[3];

      for(int var1 = 0; var1 < 3; ++var1) {
         a[var1] = var0.a(var1);
      }

      a = gu.a(24);
      b = gu.a(14);
      gu.a(21);
      gu.a(7);
      Image var3;
      c = var3 = gu.a(17);
      d = Image.createImage(var3, c.getWidth() - 14, 0, 14, 14, 0);
      e = gu.a(22);
      var0 = new gr(gu.a(23), 4);
      b = new Image[4];

      for(int var4 = 0; var4 < 4; ++var4) {
         b[var4] = var0.a(var4);
      }

      f = gu.a(3);
      g = gu.a(6);
      h = gu.a(16);
      i = gu.a(0);
      j = gu.a(1);
      v.a();
   }

   public static void b() {
      b = new gg(gv.b, 0, -12887656);
      a = new gg(gv.a, 0, -256);
      d = new gg(gv.a, 0, -12887656);
      c = new gg(gv.b, 0, -1);
      f = new gg(d(), -1, -1508019);
      e = new gg(gv.a, 0, -65536);
   }

   public static gg a() {
      if (f == null) {
         f = new gg(d(), -1, -1508019);
      }

      return f;
   }

   public static gg b() {
      if (i == null) {
         i = new gg(" 0123456789.,:!?()-'/ABCDEFGHIJKLMNOPQRSTUVWXYZÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬÉÈẺẼẸÊẾỀỂỄỆÍÌỈĨỊÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÚÙỦŨỤƯỨỪỬỮỰÝỲỶỸỴĐ", new byte[]{4, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 4, 4, 4, 4, 8, 6, 6, 6, 3, 7, 10, 10, 10, 10, 8, 8, 10, 10, 5, 8, 9, 8, 13, 11, 10, 10, 10, 10, 10, 9, 10, 10, 13, 11, 11, 9, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 5, 5, 5, 5, 5, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10}, 20, gu.a("/common.dat", 5), 0);
      }

      return i;
   }

   public static gg c() {
      if (h == null) {
         h = new gg(" 0123456789.,:!?()+*$#/-%abcdefghijklmnopqrstuvwxyzáàảãạăắằẳẵặâấầẩẫậéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵđABCDEFGHIJKLMNOPQRSTUVWXYZĐ~Ớ", new byte[]{4, 7, 6, 7, 7, 8, 7, 7, 7, 7, 7, 4, 4, 4, 5, 6, 5, 5, 7, 6, 9, 9, 7, 7, 11, 7, 8, 6, 8, 7, 5, 8, 8, 4, 5, 7, 4, 10, 8, 8, 8, 8, 6, 6, 5, 8, 7, 10, 7, 8, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 7, 7, 4, 4, 4, 6, 4, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 8, 8, 8, 8, 9, 8, 8, 8, 8, 8, 7, 7, 8, 8, 4, 6, 8, 7, 11, 9, 8, 8, 8, 7, 8, 8, 8, 8, 10, 8, 8, 9, 9, 7, 8}, 15, gu.a("/common.dat", 4), -1);
      }

      return h;
   }

   public static gg d() {
      if (g == null) {
         g = new gg(" 0123456789.+-%$:ABCDEFGHIJKLMNOPQRSTUVWXYZ/", new byte[]{3, 5, 3, 5, 5, 5, 5, 5, 5, 5, 5, 4, 5, 5, 7, 5, 3, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 7, 6, 5, 5, 5, 5, 5, 5, 5, 5, 7, 5, 5, 5, 7}, 8, gu.a("/common.dat", 9), -1);
      }

      return g;
   }

   public static gg a(int var0) {
      Integer var1 = new Integer(var0);
      gg var2;
      if ((var2 = (gg)a.get(var1)) != null) {
         return var2;
      } else {
         gg var3 = new gg(d(), -1, var0);
         a.put(var1, var3);
         return var3;
      }
   }

   public static void c() {
      a.clear();
   }

   public static Image a() {
      if (l == null) {
         Image var0;
         l = var0 = Image.createImage(1, 1);
         Graphics var1;
         (var1 = var0.getGraphics()).setColor(128, 128, 128);
         var1.fillRect(0, 0, 10, 10);
      }

      return l;
   }

   public static void d() {
      b.clear();
   }

   public static Image a(String var0, byte var1) {
      String var2 = "gui" + var1 + "_" + var0;
      Image var3;
      if ((var3 = (Image)b.get(var2)) != null) {
         return var3;
      } else if (a.contains(var2)) {
         return null;
      } else {
         cx.a(var0, var1);
         a.addElement(var2);
         return null;
      }
   }

   public static void a(String var0, Image var1, byte var2) {
      var0 = "gui" + var2 + "_" + var0;
      b.put(var0, var1);
      a.removeElement(var0);
   }

   public static gg a(byte var0) {
      switch (var0) {
         case 0:
            return gv.a;
         case 1:
            return gv.b;
         case 2:
            return a();
         case 3:
            return b();
         case 4:
            return d();
         case 5:
            return c();
         case 6:
            return b;
         case 7:
            return a;
         case 8:
            return d;
         case 9:
            return c;
         case 10:
            return e;
         default:
            return null;
      }
   }
}
