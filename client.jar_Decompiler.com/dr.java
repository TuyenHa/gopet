import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class dr extends gn {
   private Image a;
   private dn a;
   private String a;
   public boolean a = true;
   private final int a;

   public dr() {
      this.a = gv.a.a();
      this.u = 77;
      int var1 = gv.a.a("Level 10000");
      this.t = 43;
      if (var1 > this.t) {
         this.t = var1;
      }

      this.g = false;
   }

   public final void a(dn var1) {
      this.a = var1;
   }

   public final void a_() {
      BaseCanvas.g.translate(this.r, this.s);
      int var1 = this.a;
      if (this.a == null) {
         this.a = dj.a.a(this.a.a);
      } else {
         int var2 = this.t - this.a.getWidth() / this.a.c >> 1;
         dp var10000 = dj.a;
         dp.a(this.a, var2, var1, 0, this.a.c);
         String var3 = null;
         switch (this.a.b) {
            case 1:
               var3 = "(fire)";
               break;
            case 2:
               var3 = "(tree)";
               break;
            case 3:
               var3 = "(rock)";
               break;
            case 4:
               var3 = "(thunder)";
               break;
            case 5:
               var3 = "(water)";
               break;
            case 6:
               var3 = "(dark)";
               break;
            case 7:
               var3 = "(light)";
         }

         if (var3 != null) {
            gv.a.a(BaseCanvas.g, var3, var2 + 25, var1, 0);
         }
      }

      if (this.a) {
         int var7 = var1 + 37;
         gv.a.a(BaseCanvas.g, "Cấp " + this.a.b, this.t >> 1, var7, 17);
         int var9 = var7 + this.a;
         var1 = 50;
         if (50 > this.t) {
            var1 = this.t;
         }

         var7 = this.t - var1 >> 1;
         BaseCanvas.g.setColor(3872520);
         BaseCanvas.g.fillRect(var7, var9, var1, 1);
         BaseCanvas.g.fillRect(var7, var9 + 4, var1, 1);
         BaseCanvas.g.fillRect(var7 - 1, var9 + 1, 1, 3);
         BaseCanvas.g.fillRect(var7 + var1, var9 + 1, 1, 3);
         BaseCanvas.g.setColor(16039947);
         BaseCanvas.g.fillRect(var7, var9 + 1, var1, 3);
         var1 = (int)((this.a.a - this.a.c) * (long)var1 / (this.a.b - this.a.c));
         BaseCanvas.g.setColor(1740031);
         BaseCanvas.g.fillRect(var7, var9 + 1, var1, 3);
         var1 = var9 + 5;
         if (this.a == null) {
            this.a = ed.d(this.a.a - this.a.c) + "/" + ed.d(this.a.b - this.a.c);
         }

         cp.d().a(BaseCanvas.g, this.a, var7, var1, 0);
      }

      BaseCanvas.g.translate(-this.r, -this.s);
   }

   public final boolean a(int var1, int var2) {
      switch (var2) {
         case -4:
            return true;
         case -3:
            return true;
         default:
            return super.a(var1, var2);
      }
   }
}
