public final class ad {
   public int a;
   public int b;
   public int c;
   private String a;

   public final String a() {
      if (this.a == null) {
         this.a = String.valueOf(this.b);
      }

      return this.a;
   }
}
