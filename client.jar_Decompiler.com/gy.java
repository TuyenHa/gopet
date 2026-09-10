public final class gy {
   public int a;
   public int b;
   public gx a;

   public gy(int var1, int var2, int var3, int var4) {
      this.a = var1;
      this.b = var2;
      this.a = new gx(var3, var4);
   }

   public final boolean a(int var1, int var2, int var3, int var4) {
      var1 = a(this.a, var1, var1 + var3) || a(var1, this.a, this.a + this.a.a);
      var2 = a(this.b, var2, var2 + var4) || a(var2, this.b, this.b + this.a.b);
      return var1 && var2;
   }

   public final boolean a(gy var1) {
      return this.a(var1.a, var1.b, var1.a.a, var1.a.b);
   }

   private static boolean a(int var0, int var1, int var2) {
      return var0 >= var1 && var0 <= var2;
   }
}
