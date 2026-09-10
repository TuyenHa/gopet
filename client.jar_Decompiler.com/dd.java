import javax.microedition.lcdui.Image;

public final class dd implements ha {
   public int a;
   private String d;
   public String a;
   private String e;
   public boolean a;
   public boolean b;
   public String b;
   public String c;
   public byte a;
   public boolean c;
   public String[] a;
   public int[] a;
   public byte[] a;

   public dd(int var1, String var2, String var3, String var4, byte var5) {
      this.a = var1;
      this.a = "  " + var3;
      this.e = "  " + var4;
      this.d = var2;
      if (var5 == 0) {
         this.a = false;
      } else {
         this.a = true;
      }
   }

   public final Image a() {
      if (this.d == null) {
         return null;
      } else {
         return cp.a(this.d, (byte)3) != null ? cp.a(this.d, (byte)3) : cp.a();
      }
   }

   public final String a() {
      return this.a;
   }

   public final String b() {
      return this.e;
   }
}
