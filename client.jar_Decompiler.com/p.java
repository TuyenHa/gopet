import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class p extends n {
   public int a;
   public String a;
   public int b;
   private gg c;

   public p(int var1, String var2, String var3, int var4) {
      super((Image)null);
      this.c = gv.b;
      this.b = ed.a(var4);
      this.e = var2;
      this.a = var3;
      this.a = var1;
   }

   public p(gg var1, int var2, String var3, String var4, int var5) {
      this(var2, var3, var4, var5);
      this.c = var1;
   }

   public final void b() {
      if (this.b != 0) {
         this.c.a(BaseCanvas.g, this.e + " " + this.b + " (mp)", this.r + 2, this.s + 2, 20);
      } else {
         this.c.a(BaseCanvas.g, this.e, this.r + 2, this.s + 2, 20);
      }
   }
}
