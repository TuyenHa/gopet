public final class ej implements em {
   public static final ej a = new ej();
   public int[] a = new int[]{0, 0, 0};
   public int[] b = new int[]{0, 0, 0};
   private long a = System.currentTimeMillis();
   private long b = System.currentTimeMillis();

   public final void a() {
      if (dv.a != null) {
         fr var1;
         if (!(var1 = (fr)dv.a).a.containsKey(new Integer(dv.a.c))) {
            en var2;
            (var2 = new en(81)).a(22);
            cx.a.a(var2);
            var2.a();
            this.b = System.currentTimeMillis() + 1000L;
            return;
         }

         if (var1.b == 1 || this.a > System.currentTimeMillis() || this.b > System.currentTimeMillis()) {
            return;
         }

         this.a = System.currentTimeMillis() + 4000L;
         var1.a((Object)(new Object[]{new cd(318, (String)null, var1)}));
      }

   }

   public final void a(int var1) {
      if (dv.a != null && dv.a.c == var1) {
         for(int var2 = 0; var2 < this.a.length; ++var2) {
            int var10002 = this.a[var2]--;
         }
      }

   }

   public final boolean a() {
      if (dv.a != null && ((df)dv.a).a == null) {
         return false;
      } else {
         return cg.a != null && cg.a.b == 12 ? false : el.a;
      }
   }
}
