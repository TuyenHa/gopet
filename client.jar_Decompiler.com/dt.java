public final class dt extends di {
   public final void a(cf var1) {
      int var2 = var1.a;
      this.a(0, 0);
      switch (var1.b) {
         case 1:
            this.a.addElement(new e(this, 0, var2, (Object)null));
            break;
         case 4:
            this.a.addElement(new e(this, 5, var2, new Integer(var1.a)));
      }

      var2 = var1.b.length;

      for(int var3 = 0; var3 < var2; ++var3) {
         int var4 = var1.a[var3];
         int var5 = var1.b[var3];
         int var6 = var1.c[var3];
         int var7 = var1.d[var3];
         if (var5 < 0) {
            this.a.addElement(new e(this, 4, var4, new int[]{var6, var7}));
         } else if (var5 >= 0 && var5 <= 2) {
            this.a.addElement(new e(this, 1, var4, new int[]{var5, var6}));
         } else if (var5 >= 101 && var5 < 125) {
            this.a.addElement(new e(this, 6, var4, new int[]{var5 - 101 + 8, var6, var7}));
         } else if (var5 >= 125) {
            this.a.addElement(new e(this, 11, var4, new int[]{var5, var6, var7}));
         }
      }

   }

   public final void a(boolean var1) {
      int var2 = var1 ? 1 : 0;
      this.a.addElement(new e(this, 10, var2, new gm(var1 ? gw.a(56) : gw.a(57), (cd)null, new cd(gw.a(25), new dc()), cd.b, 0)));
      this.a.addElement(new e(this, 7, var2, (Object)null));
   }
}
