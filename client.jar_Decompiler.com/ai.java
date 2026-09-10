import java.io.IOException;
import javax.microedition.io.Connector;
import javax.wireless.messaging.MessageConnection;
import javax.wireless.messaging.TextMessage;
import vn.me.core.BaseCanvas;

public final class ai implements Runnable {
   private MessageConnection a = null;
   private final String a;
   private final String b;
   private final gz a;
   private final gz b;

   public ai(String var1, String var2, gz var3, gz var4) {
      this.a = var1;
      this.b = var2;
      this.a = var3;
      this.b = var4;
   }

   public final void run() {
      try {
         try {
            this.a = (MessageConnection)Connector.open(this.a);
            TextMessage var1;
            (var1 = (TextMessage)this.a.newMessage("text")).setAddress(this.a);
            var1.setPayloadText(this.b + " " + cx.c + " " + BaseCanvas.instance.midlet.getAppProperty("RefCode"));
            this.a.send(var1);
            this.a.a(new Object[]{null, "smsOK"});
            if (this.a == null) {
               return;
            }

            try {
               this.a.close();
            } catch (IOException var4) {
               return;
            }
         } catch (Throwable var5) {
            if (this.a != null) {
               try {
                  this.a.close();
               } catch (IOException var2) {
               }
            }

            var5.printStackTrace();
         }

         return;
      } catch (Exception var6) {
         this.b.a(new Object[]{null, "smsFail"});
         if (this.a != null) {
            try {
               this.a.close();
               return;
            } catch (IOException var3) {
            }
         }
      }

   }
}
