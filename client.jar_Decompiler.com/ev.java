import java.util.Vector;
import vn.me.core.BaseCanvas;

public final class ev extends fw {
   public int a;
   public go a;
   public int b;
   public int c;

   public ev(String var1) {
      super(true);
      this.d = var1;
      this.c = "BOARDLIST";
      this.n = cg.e;
      this.l = new cd(1000, a.a(189), this);
   }

   public final void b() {
      BaseCanvas.g.setColor(2425856);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
   }

   public final void a(Object var1) {
      cd var2;
      switch ((var2 = (cd)((Object[])var1)[0]).a) {
         case 5:
            cg.f();
            cx.a(new int[]{cg.a.b, ((ad)((Vector)var2.a).elementAt(this.a)).a, 100, ef.a(cg.a.b)});
            return;
         case 1000:
            cg.a((String)a.a(189), (cd)(new cd(1001, a.a(337), this)), cg.b, 1);
            return;
         case 1001:
            cg.f();
            cx.a(new int[]{cg.a.b, Integer.parseInt(((gi)fw.a).a(0)), dv.a.i, dv.a.j});
            return;
         default:
      }
   }

   public static int a(ev var0) {
      return var0.c;
   }
}
