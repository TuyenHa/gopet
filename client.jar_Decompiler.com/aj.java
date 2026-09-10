import java.io.IOException;
import java.io.InputStream;
import javax.microedition.io.Connector;
import javax.microedition.io.HttpConnection;

public final class aj implements Runnable {
   private final gz a;
   private final gz b;

   public aj(gz var1, gz var2) {
      this.a = var1;
      this.b = null;
   }

   public final void run() {
      try {
         HttpConnection var1;
         (var1 = (HttpConnection)Connector.open(cx.d + "?" + System.currentTimeMillis())).setRequestMethod("GET");
         var1.setRequestProperty("Content-Type", "//text plain");
         var1.setRequestProperty("Connection", "close");
         if (var1.getResponseCode() == 200) {
            String var2 = "";
            InputStream var3 = var1.openInputStream();
            int var5;
            if ((var5 = (int)var1.getLength()) != -1) {
               byte[] var6 = new byte[var5];
               var3.read(var6);
               var2 = new String(var6);
            }

            cx.c(var2);
            fb.f();
            if (this.a != null) {
               this.a.a(cx.a);
               return;
            }

            return;
         }
      } catch (IOException var4) {
      }

      cg.c(a.a(553));
      if (this.b != null) {
         this.b.a((Object)null);
      }

   }
}
