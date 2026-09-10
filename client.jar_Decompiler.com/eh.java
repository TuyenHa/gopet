import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public class eh {
   public String e;
   public cd a;
   public int i;
   public int j;
   public int k;
   public int l;
   public Image b;
   public boolean f;
   public boolean g = true;
   public gy a = new gy(0, 0, 0, 0);
   public boolean h = false;
   public int m = 1;
   public int n = 1;
   public long a = System.currentTimeMillis();
   public boolean i = true;

   public final void a(gy var1) {
      this.k = var1.a;
      this.l = var1.b;
      this.a.a.a = var1.a.a;
      this.a.a.b = var1.a.b;
   }

   public void a(int var1, int var2) {
   }

   public void a(long var1) {
      this.a.a = this.k + this.i;
      this.a.b = this.l + this.j;
      if (var1 - this.a > 50L) {
         this.a = var1;
         this.m += this.n;
         if (this.m < 0 || this.m > 1) {
            this.n = -this.n;
         }
      }

   }

   public void a_(int var1, int var2) {
      BaseCanvas.g.drawImage(cp.g, this.i - var1, -var2 + this.j - cp.g.getHeight() - this.m, 17);
   }

   public gy a() {
      return this.a;
   }
}
