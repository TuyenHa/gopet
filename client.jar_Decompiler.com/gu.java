import java.io.DataInputStream;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class gu {
   private static String a;
   private static int[] a;
   private static int a = 0;

   public static void a(String var0) {
      a = null;
      a = var0;
   }

   public static Image a(String var0, int var1) {
      String var2 = a;
      a(var0);
      Image var3 = a(var1);
      a(var2);
      return var3;
   }

   private static void a() {
      if (a == null) {
         try {
            DataInputStream var0;
            if (BaseCanvas.iPlatformSDK == null) {
               var0 = new DataInputStream(gv.a(a));
            } else {
               var0 = new DataInputStream(BaseCanvas.iPlatformSDK.getAssetSDK().load(a));
            }

            int var1;
            a = new int[var1 = var0.readInt()];
            int var2 = (var1 << 2) + 4;

            for(int var3 = 0; var3 < var1; ++var3) {
               a[var3] = var0.readInt() + var2;
            }

            var0.close();
            return;
         } catch (Exception var4) {
         }
      }

   }

   private static byte[] a(int var0) {
      a();
      byte[] var1 = new byte[1];

      try {
         DataInputStream var2;
         if (BaseCanvas.iPlatformSDK == null) {
            var2 = new DataInputStream(gv.a(a));
         } else {
            var2 = new DataInputStream(BaseCanvas.iPlatformSDK.getAssetSDK().load(a));
         }

         var1 = new byte[a[var0 + 1] - a[var0]];
         var2.skip((long)a[var0]);
         var2.read(var1);
         var2.close();
      } catch (Exception var3) {
      }

      ++a;
      return var1;
   }

   public static Image a(int var0) {
      a();
      Image var1 = Image.createImage(1, 1);

      try {
         byte[] var3;
         var1 = Image.createImage(var3 = a(var0), 0, var3.length);
      } catch (Exception var2) {
         var2.printStackTrace();
      }

      ++a;
      return var1;
   }
}
