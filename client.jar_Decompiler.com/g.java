import vn.me.core.BaseCanvas;

public final class g {
   public String a;
   public String[] a;
   public int a;
   public int b;

   public g(int var1, String var2, String var3) {
      this.b = var1;
      this.a = var2;
      this.a = gv.b.a(var3 != null ? this.a + ":_" + var3 : this.a + ":_", BaseCanvas.w - 40 - gs.p);
      if (this.a[0].length() > this.a.length()) {
         this.a[0] = this.a[0].substring(this.a.length(), this.a[0].length());
      }

      this.a = this.a.length * gv.b.a() - 1;
   }
}
