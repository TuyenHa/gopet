public final class gc extends go {
   public int a = -1;
   public gz a;

   public gc(int var1, int var2) {
      super(0, 0, var1, var2);
   }

   private void i() {
      if (this.a != -1) {
         if (this.a < this.b()) {
            ((gb)this.a(this.a)).a = false;
            ((gb)this.a(this.a)).j = false;
            ((gb)this.a(this.a)).d = false;
         }

         this.a = -1;
      }

   }

   public final void a(gb var1) {
      if (var1 == null) {
         this.i();
      } else {
         int var2 = this.a.length;

         do {
            --var2;
            if (var2 < 0) {
               return;
            }
         } while(var1 != this.a[var2]);

         this.a(var2);
      }
   }

   public final void a(int var1) {
      if (this.a != var1) {
         this.i();
         ((gb)this.a(var1)).a = true;
         this.a = var1;
         this.c = (gb)this.a(var1);
         if (this.a != null) {
            this.a.a(this);
         }

      }
   }
}
