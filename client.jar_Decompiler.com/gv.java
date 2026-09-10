import java.io.IOException;
import java.io.InputStream;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class gv {
   public static String a = "/mui.dat";
   public static gr a;
   public static Image a;
   public static gg a;
   public static gg b;

   public static void a() {
      gu.a(a);
      a = new gr(gu.a(7), 4);
      Image var0;
      gg.b = var0 = gu.a(2);
      gg.a = var0.getHeight();
      gu.a(1);
   }

   public static void b() {
      a = new gg(" 0123456789.,:!?()+*<>/-%abcdefghijklmnopqrstuvwxyzáàảãạăắằẳẵặâấầẩẫậéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵđABCDEFGHIJKLMNOPQRSTUVWXYZĐ$ĂÁÂ=", new byte[]{4, 6, 5, 6, 6, 7, 6, 6, 6, 6, 6, 3, 3, 3, 4, 5, 4, 4, 6, 5, 8, 8, 6, 6, 10, 6, 7, 5, 7, 6, 4, 7, 7, 3, 4, 6, 3, 9, 7, 7, 7, 7, 5, 5, 4, 7, 6, 9, 6, 7, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7, 6, 6, 3, 3, 3, 5, 3, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 7, 7, 7, 7, 8, 7, 7, 7, 7, 7, 6, 6, 7, 7, 3, 5, 7, 6, 10, 8, 7, 7, 7, 6, 7, 7, 7, 7, 9, 7, 7, 8, 8, 6, 7, 7, 7, 9}, 13, gu.a(4), 0);
      b = new gg(" 0123456789.,:!?()+*$#/-%abcdefghijklmnopqrstuvwxyzáàảãạăắằẳẵặâấầẩẫậéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵđABCDEFGHIJKLMNOPQRSTUVWXYZĐ@Á=", new byte[]{4, 6, 4, 6, 6, 6, 6, 6, 6, 6, 6, 2, 2, 2, 2, 6, 4, 3, 6, 5, 6, 7, 3, 3, 10, 6, 6, 5, 6, 6, 4, 6, 6, 2, 2, 6, 2, 10, 6, 6, 6, 6, 4, 6, 3, 6, 5, 9, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 3, 2, 3, 4, 2, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 6, 6, 6, 6, 6, 8, 8, 8, 8, 8, 8, 5, 5, 5, 5, 5, 7, 7, 7, 8, 8, 7, 6, 8, 8, 2, 5, 8, 7, 8, 8, 8, 7, 8, 8, 7, 7, 8, 7, 9, 7, 7, 7, 8, 9, 7, 7, 9}, 14, gu.a(3), 0);
   }

   public static InputStream a(String var0) {
      if (BaseCanvas.iPlatformSDK != null) {
         try {
            return BaseCanvas.iPlatformSDK.getAssetSDK().load(var0);
         } catch (IOException var2) {
            var2.printStackTrace();
         }
      }

      return (new byte[0]).getClass().getResourceAsStream(var0);
   }
}
