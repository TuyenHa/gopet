import vn.me.core.BaseCanvas;

public final class x extends eh {
   public int a;
   public int b;
   private int e;
   private long b;
   public int c;
   public int d;
   private boolean a = true;

   public x() {
      this.a(new gy(-5, -4, 9, 8));
   }

   public final void a(int var1, int var2) {
      BaseCanvas.g.drawRegion(cp.j, 9 * this.e, 0, 9, 8, this.a ? 0 : 2, this.i - var1, this.j - var2 - 50, 0);
   }

   public final void a(long var1) {
      super.a(var1);
      if (var1 - this.b > 100L) {
         this.b = var1;
         this.e = (this.e + 1) % 2;
      }

      if (ed.a(this.d - this.j) <= 2 && ed.a(this.c - this.i) <= 2) {
         this.i = this.c;
         this.j = this.d;
         this.c += ed.b(40) - 20;
         this.d += ed.b(40) - 20;
         if (this.c < 0) {
            this.c = 0;
         }

         if (this.c > this.a) {
            this.c = this.a;
         }

         if (this.d - 40 < 0) {
            this.d = 40;
         }

         if (this.d > this.b) {
            this.d = this.b;
            return;
         }
      } else if (ed.a(this.c - this.i) <= ed.a(this.d - this.j)) {
         if (this.d > this.j) {
            this.j += 2;
         } else {
            this.j -= 2;
         }

         if (this.d != this.j) {
            if (this.d > this.j) {
               this.i += 2 * (this.c - this.i) / (this.d - this.j);
               return;
            }

            this.i += 2 * (this.i - this.c) / (this.d - this.j);
            return;
         }
      } else {
         if (this.c > this.i) {
            this.i += 2;
            this.a = true;
         } else {
            this.a = false;
            this.i -= 2;
         }

         if (this.c != this.i) {
            if (this.c > this.i) {
               this.j += 2 * (this.d - this.j) / (this.c - this.i);
               return;
            }

            this.j += 2 * (this.j - this.d) / (this.c - this.i);
         }
      }

   }
}
