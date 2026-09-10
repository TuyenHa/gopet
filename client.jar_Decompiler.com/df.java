public final class df extends ee {
   public dh a;

   public df(int var1, byte var2, dv var3) {
      super(var1, var2, var3);
   }

   public final void a(long var1) {
      int var3 = this.i;
      int var4 = this.j;
      super.a(var1);
      int var5 = this.i - var3;
      int var2 = this.j - var4;
      if (this.a != null) {
         if (var5 == 0 && var2 == 0) {
            return;
         }

         dh var6;
         if ((var3 = (var6 = this.a).a.i - var6.i) < -30) {
            var6.i = var6.a.i + 30;
         } else if (var3 > 30) {
            var6.i = var6.a.i - 30;
         }

         if ((var4 = var6.a.j - var6.j) < -15) {
            var6.j = var6.a.j + 15;
         } else if (var4 > 15) {
            var6.j = var6.a.j - 15;
         }

         if (var3 < 0) {
            var6.a = 0;
         } else {
            var6.a = 2;
         }

         if (var5 != 0) {
            if (var6.j < var6.a.j) {
               var6.j += 2;
               if (var6.j > var6.a.j) {
                  var6.j = var6.a.j;
                  return;
               }
            } else if (var6.j > var6.a.j) {
               var6.j -= 2;
               if (var6.j < var6.a.j) {
                  var6.j = var6.a.j;
               }
            }
         }
      }

   }

   public final void a(int var1, int var2) {
      super.a(var1, var2);
   }

   public final void a(ew var1) {
      var1.b(this.a);
      di var2;
      if ((var2 = ((fr)dv.a).a(this.n)) != null) {
         var2.a.addElement(new e(var2, 9, 0, var2.a));
         var2.a.addElement(new e(var2, 2, 0, (Object)null));
      }

   }

   public final void a(dh var1) {
      if (this.a != null) {
         dv.a.b(this.a);
      }

      this.a = var1;
      dv.a.a((eh)this.a);
      dv.a.c(this.a);
   }

   public final void a(int var1) {
      if (this.a != null && this.a.b == var1) {
         dv.a.b(this.a);
      }

      this.a = null;
   }
}
