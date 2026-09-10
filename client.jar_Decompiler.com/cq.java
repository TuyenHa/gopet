import java.util.Hashtable;
import java.util.Vector;
import vn.me.core.BaseCanvas;

public final class cq implements cy {
   public ak a;
   protected static cq a;
   public dv a;
   private fi a;
   private af a = new af();
   private Hashtable a = new Hashtable();

   public static cq a() {
      if (a == null) {
         a = new cq();
      }

      return a;
   }

   public final void a() {
      af var10000 = this.a;
      cx.c();
      cx.g();
      fb.a = false;
   }

   public final void b() {
      this.a.a();
   }

   public final void c() {
      af var10000 = this.a;
      af.b();
   }

   public final void a(en param1) {
      // $FF: Couldn't be decompiled
   }

   private void a(boolean var1, en var2) {
      try {
         int var3 = var2.a().readInt();
         String var4 = var2.a().readUTF();
         byte var5 = var2.a().readByte();
         byte var6 = var2.a().readByte();
         byte var7 = var2.a().readByte();
         var2.a().readByte();
         byte var8 = -1;
         if (var1) {
            var8 = var2.a().readByte();
         }

         var1 = var2.a().readInt();
         int var15 = var2.a().readInt();
         if (var8 >= 0 && dv.a != null) {
            ef var9 = dv.a.a;

            for(int var10 = 0; var10 < var9.a.length; ++var10) {
               if (var9.a[var10].a == var8) {
                  var1 = var9.a[var10].a;
                  var15 = var9.a[var10].b;
                  break;
               }
            }
         }

         dv var19 = this.a;
         ee var14;
         if (dv.a == null) {
            var14 = null;
         } else {
            ee var16;
            ee var18 = var16 = dv.a(var3);
            if (var16 == null) {
               var18 = var16 = a.a(var3, var5, var19);
               var16.c(var4);
               dv.a.a(var18, var1, var15, false);
               var18.i = var1;
               var18.j = var15;
               dv.a.a.a(dv.a);
               var18.a = new cd(900, a.a(419), var19);
               var18.b(-1L);
               var18.b = var6;
            }

            var14 = var18;
         }

         if (var14 != null) {
            var14.c = var7;
         }

      } catch (Exception var11) {
         var11.printStackTrace();
      }
   }

   public static byte[] a(en var0) {
      try {
         int var1;
         if ((var1 = var0.a().readInt()) <= 0) {
            return null;
         } else {
            byte[] var3 = new byte[var1];
            var0.a().read(var3);
            return var3;
         }
      } catch (Exception var2) {
         var2.printStackTrace();
         return null;
      }
   }

   public static void d() {
      dv.b();
      a().a = null;
      a().a = null;
   }

   private static boolean a(int var0, Vector var1, byte var2, String var3) {
      fw var4;
      if ((var4 = BaseCanvas.getCurrentScreen()) instanceof fj) {
         ((fw)var4).x();
         if ((var4 = var4).b == var0) {
            var4.c(var1);
            var4.b(var0);
            return true;
         }
      }

      (var4 = new fj()).a = var2;
      var4.c(var1);
      var4.g(var3);
      var4.b(var0);
      var4.a(1, true);
      return false;
   }
}
