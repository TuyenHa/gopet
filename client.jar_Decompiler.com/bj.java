import javax.microedition.lcdui.Image;

public final class bj implements bm {
   private static bj a;

   private bj() {
   }

   public static bj a() {
      if (a == null) {
         a = new bj();
      }

      return a;
   }

   public final Image[] a(String var1) {
      Image[] var2 = new Image[1];
      if (var1.equals("/pet/battle/skills/125.anu")) {
         var2[0] = a("/pet/battle/skills/125.png");
      } else if (var1.equals("/pet/battle/skills/126.anu")) {
         var2[0] = a("/pet/battle/skills/126.png");
      } else if (var1.equals("/pet/battle/skills/127.anu")) {
         var2[0] = a("/pet/battle/skills/127.png");
      } else if (var1.equals("/pet/battle/skills/128.anu")) {
         var2[0] = a("/pet/battle/skills/128.png");
      } else if (var1.equals("/pet/battle/skills/129.anu")) {
         var2[0] = a("/pet/battle/skills/129.png");
      } else if (var1.equals("/pet/battle/skills/130.anu")) {
         var2[0] = a("/pet/battle/skills/130.png");
      }

      return var2;
   }

   private static Image a(String var0) {
      try {
         return Image.createImage(var0);
      } catch (Exception var1) {
         System.out.println("Error loading Image " + var0);
         return null;
      }
   }
}
