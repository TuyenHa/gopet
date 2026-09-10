import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class ao extends fw {
   public Image[] a = new Image[9];
   private int a;
   private int b;
   private int c;
   private int d;
   private int e;

   public ao() {
      super(true);
      this.f = true;
   }

   public final void a() {
      this.a = this.a[0].getWidth() + this.a[1].getWidth() + this.a[2].getWidth();
      this.b = this.a[0].getHeight() + this.a[3].getHeight() + this.a[6].getHeight();
      this.c = BaseCanvas.w - this.a >> 1;
      this.d = BaseCanvas.h - this.b >> 1;
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
      BaseCanvas.g.translate(this.c, this.d);
      int var1 = 0;
      int var2 = 0;

      for(int var3 = 0; var3 < this.a.length; ++var3) {
         BaseCanvas.g.drawImage(this.a[var3], var1, var2, 0);
         if (var3 % 3 == 2) {
            var2 += this.a[var3].getHeight();
            var1 = 0;
         } else {
            var1 += this.a[var3].getWidth();
         }

         var1 = var1;
      }

      BaseCanvas.g.translate(-this.c, -this.d);
   }

   public final boolean a(int var1, int var2) {
      if (a == null && var1 == 1) {
         this.f();
         return true;
      } else {
         return super.a(var1, var2);
      }
   }

   public final void a(int var1) {
      this.e = var1;
   }

   private void f() {
      if (this.e < 0) {
         this.t();
      } else {
         cg.f();
         en var1;
         (var1 = new en(81)).a(65);
         cx.a.a(var1);
         var1.a();
      }
   }

   public final void a(int var1, int var2) {
      if (a == null) {
         this.f();
      } else {
         super.a(var1, var2);
      }
   }
}
