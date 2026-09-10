import vn.me.core.BaseCanvas;

public final class d extends c {
   private int b;
   private int c;
   private int d;
   private int e;
   private int f;
   private int g;
   private long a;
   private boolean c;
   private int h;

   public d(int var1, int var2, boolean var3) {
      this.a = 1;
      this.f = var1;
      this.g = var2;
      this.b = true;
      this.c = var3;
   }

   public final void c() {
      super.a();
      this.h = 2000;
      this.a = System.currentTimeMillis();
   }

   public final void a() {
      this.c();
   }

   public final void b() {
      BaseCanvas.g.setColor(16777215);
      int var3 = BaseCanvas.g.getTranslateX();
      int var4 = BaseCanvas.g.getTranslateY();
      int var1;
      int var2;
      if (this.c) {
         var1 = dv.a.a.b;
         var2 = dv.a.b.b;
      } else {
         var1 = 0;
         var2 = 0;
      }

      BaseCanvas.g.translate(-var1 + this.f, -var2 + this.g);
      BaseCanvas.g.fillTriangle(0, 0, 0 + this.b, 0 + this.c, 0 + this.b + this.d, 0 + this.c + this.e);
      BaseCanvas.g.translate(-var3, -var4);
   }

   public final void a(long var1) {
      if (var1 - this.a >= (long)this.h) {
         this.a = false;
      } else {
         this.b = ed.b(400) - 100;
         this.c = ed.b(400) - 100;
         this.d = ed.b(60) - 30;
         this.e = ed.b(60) - 30;
      }
   }
}
