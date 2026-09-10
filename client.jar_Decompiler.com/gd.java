import vn.me.core.BaseCanvas;

public class gd extends go {
   public boolean a;
   public boolean b;
   public gn a;
   public boolean c;
   private boolean m;

   public gd() {
      this(0, 0, 0, 0);
   }

   public gd(int var1, int var2, int var3, int var4) {
      super(var1, var2, var3, var4);
      this.a = true;
      this.c = false;
      this.m = false;
      this.k = true;
      this.x = 3;
   }

   public final void i() {
      BaseCanvas.getCurrentScreen().a(this);
   }

   public final void a(boolean var1) {
      if (var1 && fw.a != null) {
         gd var10000 = fw.a;
         BaseCanvas.currentScreen.w();
      }

      BaseCanvas.currentScreen.a(this);
   }

   public final void j() {
      fw var10000 = BaseCanvas.currentScreen;
      fw.b(this);
   }

   public void e() {
      super.e();
      gs.a(this.t, this.u);
   }

   public void a() {
      gs.a(this);
   }

   public void c() {
      super.c();
      if (this.a && this.r >= BaseCanvas.w) {
         this.b = true;
      }
   }

   public final boolean a(int var1, int var2) {
      super.a(var1, var2);
      return true;
   }

   public static void a(String var0, cd var1, cd var2) {
      a(var0, var1, (cd)null, var2, false);
   }

   public static void a(String var0, cd var1, cd var2, cd var3, boolean var4) {
      (new gm(var0, var1, var2, var3, 0)).a(var4);
   }

   public static void a(String var0, cd var1, boolean var2) {
      (new gm(var0, (cd)null, var1, (cd)null, 1)).a(var2);
   }

   public static void a(String var0) {
      a(var0, false);
   }

   public static void a(String var0, boolean var1) {
      a(var0, (cd)null, cd.a, (cd)null, var1);
   }

   public static void k() {
      (new gm(gw.a(22), (cd)null, cd.b, (cd)null, 2)).a(true);
   }

   public static void b(String var0, cd var1, cd var2) {
      a(var0, (cd)null, var1, var2, true);
   }

   public static void b(String var0) {
      a(var0, cd.a, true);
   }

   public static void l() {
      if (fw.a != null && fw.a instanceof gm && ((gm)fw.a).a == 2) {
         fw.a.j();
      }

   }

   public static gi a(String var0, cd var1, cd var2) {
      gi var3;
      (var3 = new gi(var0, var1, var2 == null ? cd.b : var2, 0)).a(true);
      return var3;
   }

   public final void m() {
      if (this.a) {
         this.v = BaseCanvas.w;
      } else {
         this.b = true;
      }
   }
}
