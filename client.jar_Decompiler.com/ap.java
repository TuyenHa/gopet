import javax.microedition.io.Connector;
import javax.microedition.lcdui.Alert;
import javax.microedition.lcdui.AlertType;
import javax.microedition.lcdui.Command;
import javax.microedition.lcdui.CommandListener;
import javax.microedition.lcdui.Display;
import javax.microedition.lcdui.Displayable;
import javax.microedition.lcdui.Form;
import javax.microedition.lcdui.Image;
import javax.microedition.lcdui.TextField;
import javax.wireless.messaging.MessageConnection;
import javax.wireless.messaging.TextMessage;

public final class ap extends Form implements CommandListener {
   private TextField a = new TextField(gw.a(14) + ":", "", 15, 3);
   private TextField b;
   private Command a;
   private Command b;
   private Display a;
   private gz a;

   public ap(String var1, String var2, gz var3, Display var4) {
      super(var1);
      this.b = new TextField(gw.a(10) + ":", var2, 600, 0);
      this.a = new Command(gw.a(9), 4, 0);
      this.b = new Command(gw.a(8), 7, 1);
      this.a = var3;
      this.append(this.a);
      this.append(this.b);
      this.addCommand(this.a);
      this.addCommand(this.b);
      this.a = var4;
      this.setCommandListener(this);
   }

   public final void commandAction(Command var1, Displayable var2) {
      MessageConnection var8 = null;
      if (var1 == this.b) {
         this.a.a(new Object[]{new cd(-1, "", this.a), this});
      } else {
         if (var1 == this.a) {
            String var7 = this.a.getString();
            String var3 = this.b.getString();
            if (var7.equals("")) {
               Alert var11;
               (var11 = new Alert(gw.a(12))).setString(gw.a(15));
               var11.setTimeout(2000);
               this.a.setCurrent(var11);
               return;
            }

            try {
               var8 = (MessageConnection)Connector.open("sms://" + var7);
            } catch (Exception var5) {
               Alert var4;
               (var4 = new Alert("Alert")).setString(gw.a(11));
               var4.setTimeout(2000);
               this.a.setCurrent(var4);
            }

            try {
               TextMessage var10;
               (var10 = (TextMessage)var8.newMessage("text")).setAddress("sms://" + var7);
               var10.setPayloadText(var3);
               var8.send(var10);
               return;
            } catch (Exception var6) {
               Alert var9;
               (var9 = new Alert(gw.a(12), "", (Image)null, AlertType.INFO)).setTimeout(-2);
               var9.setString(gw.a(13));
               this.a.setCurrent(var9);
            }
         }

      }
   }
}
