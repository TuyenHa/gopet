import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public class do extends gj {
   public int a;
   public int b;
   public String a;
   public String b;
   public String c;
   public int c;
   public int d;
   public int e;
   public int f;
   public int g;
   public int h;
   public int i;
   public int j;
   public int k;
   public int l;
   public int m;
   public int n;
   public byte a;
   public byte b;
   public String d;
   public byte c;
   public long a;
   public int o = 0;
   public boolean a = true;

   public do() {
      this.t = 30;
      this.u = 30;
   }

   public final boolean a() {
      return this.c == 2;
   }

   public final void a(byte var1) {
      this.b = var1;
      this.d = String.valueOf(var1);
   }

   public final void a() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(2, 2, this.t - 4, this.u - 4);
   }

   public final void e() {
      if (this.d) {
         BaseCanvas.g.setColor(gs.d);
         BaseCanvas.g.drawRect(0, 0, this.t - 1, this.u - 1);
         BaseCanvas.g.drawRect(1, 1, this.t - 3, this.u - 3);
      }

   }

   public final void b() {
      Image var1;
      if (this.b != null && (var1 = dj.a.a(this.b)) != null) {
         BaseCanvas.g.drawImage(var1, this.t >> 1, this.u >> 1, 3);
      }

      if (!this.a) {
         BaseCanvas.g.setColor(16711680);
         BaseCanvas.g.drawRect(0, 0, this.t - 1, this.u - 1);
         BaseCanvas.g.drawRect(1, 1, this.t - 3, this.u - 3);
      }

      if (this.b != 0 && this.d != null) {
         cp.d().a(BaseCanvas.g, this.d, 0, 0, 0);
      }
   }

   public final void a(do var1) {
      this.a = var1.a;
      this.b = var1.b;
      this.a = var1.a;
      this.b = var1.b;
      this.c = var1.c;
      this.c = var1.c;
      this.d = var1.d;
      this.e = var1.e;
      this.f = var1.f;
      this.g = var1.g;
      this.h = var1.h;
      this.i = var1.i;
      this.j = var1.j;
      this.k = var1.k;
      this.l = var1.l;
      this.m = var1.m;
      this.n = var1.n;
      this.a = var1.a;
      this.a(var1.b);
      this.c = var1.c;
      this.a = var1.a;
      this.o = var1.o;
   }

   public final int a() {
      return (int)((long)this.o - (System.currentTimeMillis() - this.a) / 1000L);
   }

   public final void c() {
      super.c();
      if (this.c == 2 && this.a() < 0) {
         this.c = 0;
         int var1 = this.a;
         en var2;
         (var2 = new en(81)).a(82);
         var2.b(var1);
         cx.a.a(var2);
         var2.a();
      }
   }
}
