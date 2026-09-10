import java.io.IOException;
import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class fi extends fw {
   private static final Vector[] a = new Vector[]{new Vector(), new Vector(), new Vector()};
   private Image a;
   private Image c;
   private go[] a;
   private aq a;

   public fi() {
      super(true);
      this.c = "MESSAGE";
      this.n = cg.e;
      this.l = new cd(1, gw.a(63), this);
      this.a = new go[3];

      try {
         this.a = Image.createImage("/pet/thuchuadoc.png");
         this.c = Image.createImage("/pet/thudocroi.png");
      } catch (IOException var2) {
         var2.printStackTrace();
      }
   }

   public final void f() {
      this.b.o();

      for(int var1 = 0; var1 < 3; ++var1) {
         a[var1].removeAllElements();
         this.a[var1] = null;
      }

      this.a = null;
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
   }

   public final void a(int var1, boolean var2) {
      super.a(var1, var2);
      var1 = -1;

      for(int var10 = 0; var10 < 3; ++var10) {
         if (a[var10].size() > 0) {
            var1 = var10;
            break;
         }
      }

      this.a = new aq(this.b.t, BaseCanvas.h - gs.m);
      this.a.a.l = true;
      this.b.a(this.a, false);

      for(int var11 = 0; var11 < 3; ++var11) {
         this.a[var11] = new go(0, gs.l, this.b.t, BaseCanvas.h - 2 * gs.l);
         this.a[var11].i = true;
         go var3 = this.a[var11];
         Vector var4 = a[var11];
         if ((var3 = var3) == null) {
            go var5;
            var3 = var5 = new go(0, gs.l, this.b.t, BaseCanvas.h - 2 * gs.l);
            var5.i = true;
         }

         if (var4.isEmpty()) {
            gj var13;
            (var13 = new gj(a.a(307))).q = 17;
            var3.a(var13);
         }

         int var14 = var4.size();

         for(int var6 = 0; var6 < var14; ++var6) {
            da var7 = (da)var4.elementAt(var6);
            db var8;
            (var8 = new db(this, var7)).d = new cd(2, gw.a(72), var7, this);
            var3.a(var8, false);
         }

         this.a[var11].y = gs.p;
         this.a[var11].b(1);
      }

      this.a.a(gw.a(70), this.a[0]);
      this.a.a(gw.a(69), this.a[1]);
      this.a.a(gw.a(68), this.a[2]);
      if (var1 != -1) {
         this.a.a(var1);
      } else {
         this.a.f();
      }
   }

   public final void a(int var1, String var2, String var3, String var4, boolean var5) {
      a[0].addElement(new da(var1, var2, var3, var4, var5));
   }

   public final void b(int var1, String var2, String var3, String var4, boolean var5) {
      a[2].addElement(new da(var1, var2, var3, var4, var5));
   }

   public final void c(int var1, String var2, String var3, String var4, boolean var5) {
      a[1].addElement(new da(var1, var2, var3, var4, var5));
   }

   private void a(int var1) {
      int var2 = -1;
      int var3 = -1;

      for(int var4 = 0; var4 < 3; ++var4) {
         for(int var5 = 0; var5 < a[var4].size(); ++var5) {
            if (((da)a[var4].elementAt(var5)).a == var1) {
               var2 = var5;
               break;
            }
         }

         if (var2 != -1) {
            var3 = var4;
            break;
         }
      }

      if (var2 != -1) {
         da var9 = (da)a[var3].elementAt(var2);
         a[var3].removeElementAt(var2);
         go var6 = this.a[var3];

         for(int var7 = 0; var7 < var6.a.length; ++var7) {
            db var8;
            if ((var8 = (db)var6.a[var7]).a == var9) {
               System.out.println("removed");
               var6.b(var8);
               return;
            }
         }
      }

   }

   public final void a(Object var1) {
      cd var4;
      switch ((var4 = (cd)((Object[])var1)[0]).a) {
         case 1:
            Vector var8;
            (var8 = new Vector()).addElement(new cd(11, gw.a(3), this));
            var8.addElement(new cd(12, gw.a(71), this));
            this.a(var8, 0);
            return;
         case 2:
            da var7;
            cg.a((var7 = (da)var4.a).a, (String)var7.c, (cd)null, (cd)null, cg.b);
            int var10 = var7.a;
            en var11;
            (var11 = new en(121)).a(16);
            var11.b(var10);
            cx.a.a(var11);
            var11.a();
            var7.a = true;
            return;
         case 11:
            int var2;
            int var5;
            if ((var2 = this.a.a()) != -1 && (var5 = this.a[var2].c()) != -1) {
               da var6;
               var2 = (var6 = (da)a[var2].elementAt(var5)).a;
               en var3;
               (var3 = new en(121)).a(17);
               var3.b(var2);
               cx.a.a(var3);
               var3.a();
               this.a(var6.a);
               return;
            }

            return;
         case 12:
            (new m()).a();
            return;
         default:
      }
   }

   public static Image a(fi var0) {
      return var0.c;
   }

   public static Image b(fi var0) {
      return var0.a;
   }
}
