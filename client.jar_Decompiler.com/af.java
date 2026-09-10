import javax.microedition.io.ConnectionNotFoundException;
import vn.me.core.BaseCanvas;

public final class af implements gz {
   public final void a() {
      gd.a(a.a(82), new cd(1, a.a(580), this), (cd)null, cg.b, true);
   }

   public static void b() {
      fx.f();
      if (cq.a.a != null) {
         dv var10000 = cq.a.a;
         dv.a();
      }

      (new fb()).d(0);

      try {
         Thread.sleep(100L);
      } catch (InterruptedException var1) {
         var1.printStackTrace();
      }

      gq.a();
      dv.a = null;
      cq.d();
      cx.a = null;
   }

   public final void a(String var1, String var2) {
      cg.a = var2;
      gd.a(var1, new cd(48, a.a(116), this), cg.b);
      fw.a.c = true;
   }

   public final void a(Object var1) {
      cd var4;
      switch ((var4 = (cd)((Object[])var1)[0]).a) {
         case 1:
            cg.a(true);
            (new Thread(new ag(this))).start();
            return;
         case 2:
            try {
               BaseCanvas.instance.midlet.platformRequest((String)var4.a);
               return;
            } catch (ConnectionNotFoundException var3) {
               return;
            }
         case 48:
            try {
               BaseCanvas.getCurrentScreen().w();
               BaseCanvas.instance.midlet.platformRequest(cg.a);
               return;
            } catch (ConnectionNotFoundException var2) {
               return;
            }
         default:
      }
   }
}
