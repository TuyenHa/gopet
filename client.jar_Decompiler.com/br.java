import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class br implements ha {
   public byte a = -1;
   public String a;
   public String b;
   public String c;
   public String d;
   public byte b;
   private String e;

   public br(byte var1) {
      this.b = var1;
   }

   public final Image a() {
      return BaseCanvas.w <= 128 ? null : cp.d;
   }

   public final String a() {
      return this.a;
   }

   public final String b() {
      if (this.e == null) {
         switch (this.b) {
            case 0:
               this.e = a.a(344) + this.b + " " + cx.c + " " + BaseCanvas.instance.midlet.getAppProperty("RefCode") + a.a(441) + this.c;
               break;
            case 1:
               this.e = a.a(212);
               break;
            case 2:
               this.e = a.a(213);
            case 3:
            case 4:
            case 5:
            default:
               break;
            case 6:
               this.e = a.a(185);
               break;
            case 7:
               this.e = a.a(186);
               break;
            case 8:
               this.e = a.a(596);
               break;
            case 9:
               this.e = gw.a(40);
         }
      }

      return this.e;
   }
}
