final class co implements gz {
   private final ge a;
   private final ge b;
   private final int a;

   co(ge var1, ge var2, int var3) {
      this.a = var1;
      this.b = var2;
      this.a = var3;
   }

   public final void a(Object var1) {
      if (this.a.a().trim().equals("")) {
         this.b.b("0");
      } else {
         try {
            long var2;
            if ((var2 = Long.parseLong(this.a.a().trim())) <= dj.c) {
               this.b.b(ed.a(var2 * (long)this.a));
            } else {
               this.a.b("" + dj.c);
            }
         } catch (Exception var4) {
         }
      }
   }
}
