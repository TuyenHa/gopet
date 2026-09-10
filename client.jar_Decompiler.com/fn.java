import java.util.Vector;
import javax.microedition.io.ConnectionNotFoundException;
import vn.me.core.BaseCanvas;

public final class fn extends fw implements gz {
   private Vector a;
   private go a;
   private cd a;
   private cd b;
   private cd c;
   br a;
   private gi a;

   public fn() {
      super(true);
      this.d = a.a(24);
      this.n = new cd(0, a.a(64), this);
      this.c = "MONEY";
   }

   private void f() {
      cd var1 = new cd(111, a.a(337), this);
      a.a(25);
      cg.a((String[])(new String[]{a.a(184), a.a(25)}), (int[])(new int[]{1, 1}), var1, 0);
   }

   private void g() {
      cd var1 = new cd(121, a.a(337), this);
      a.a(400);
      cg.a((String[])(new String[]{a.a(184), a.a(400)}), (int[])(new int[]{1, 1}), var1, 0);
   }

   private static void h() {
      cg.a(a.a(184) + ": " + ed.a(dj.c) + "\nNgọc: " + ed.a(dj.d), true);
   }

   public final void a(Vector var1) {
      br var2;
      (var2 = new br((byte)-1)).a = a.a(55) + " " + a.a(184);
      br var3;
      (var3 = new br((byte)-1)).a = a.a(51) + " " + a.a(184);
      var1.insertElementAt(var2, 0);
      var1.addElement(var3);
      (var2 = new br((byte)9)).a = a.a(184) + " > Ngọc";
      (var3 = new br((byte)8)).a = a.a(184) + " " + a.a(595);
      var1.addElement(var2);
      var1.addElement(var3);
      this.a = var1;
      if (this.a == null) {
         this.a = new go(0, gs.l, BaseCanvas.w, BaseCanvas.h - (gs.l << 1));
         if (cg.a != null) {
            this.a.d = new cd(a.a(419), this);
         }

         this.a.i = true;
         this.a.d = 0;
      }

      this.a.o();
      int var6 = this.a.size();
      int var8 = (cp.c.a() << 1) + (gs.p << 1) + 2;

      for(int var10 = 0; var10 < var6; ++var10) {
         br var4 = (br)this.a.elementAt(var10);
         gk var5 = new gk(var4, 0, 0, BaseCanvas.w, var8);
         if (var4.b != -1) {
            var5.c = cp.c;
            var5.d = cp.c;
            var5.b = new fo(this, var5);
            this.a.a(var5, false);
         } else {
            gj var11 = new gj(var4.a);
            if (var10 == 0) {
               var11.g = true;
            }

            var11.b = new fp(this);
            var11.a(gv.a, gv.a);
            var11.a(0, 0, BaseCanvas.w, gv.a.a() + 5);
            var11.q = 20;
            this.a.a(var11, false);
         }
      }

      if (var6 > 0) {
         this.a.a(0).n();
      }

      this.b.a(this.a);
      this.a.c(true);
      this.a.b(1);
      this.a.k = true;
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
   }

   public final void a(Object var1) {
      cd var2;
      if ((var2 = (cd)((Object[])var1)[0]) == null) {
         BaseCanvas.getCurrentScreen();
         fw.b(fw.a);
         String var12 = (String)((Object[])var1)[1];
         if ("smsOK".equals(var12)) {
            cg.a(a.a(440), true);
            cx.b(this.a.b + cg.a.a, this.a.c);
         } else {
            if ("smsFail".equals(var12)) {
               cg.a(a.a(439), true);
            }

         }
      } else if (var2 == this.a.d) {
         try {
            this.a = (br)((gk)this.a.c).a;
            switch (this.a.b) {
               case -1:
                  return;
               case 0:
                  gd.a(this.a.d, new cd("Có", new fq(this)), (cd)null, cg.b, true);
                  return;
               case 1:
                  this.c = new cd(a.a(337), this);
                  cg.a((String)this.a.a, (cd)this.c, cg.b, 0);
                  return;
               case 2:
                  this.b = new cd(a.a(337), this);
                  this.a = cg.a(this.a.a, new String[]{this.a.b, this.a.c}, new int[]{0, 0}, this.b, cg.b);
                  return;
               case 3:
                  return;
               case 4:
                  this.a = new cd(a.a(337), this);
                  gd.a(this.a.b, this.a, cg.b);
                  return;
               case 5:
               default:
                  cg.a(a.a(496), true);
                  return;
               case 6:
                  this.f();
                  return;
               case 7:
                  this.g();
                  return;
               case 8:
                  h();
                  return;
               case 9:
                  cd var11 = new cd(122, a.a(337), this);
                  int var16 = cg.a;
                  a.a(25);
                  cg.a(new String[]{a.a(184), "Ngọc"}, new int[]{1, 1}, var11, var16);
            }
         } catch (Exception var3) {
         }
      } else if (var2 == this.a) {
         try {
            BaseCanvas.instance.midlet.platformRequest("tel:" + this.a.c);
         } catch (ConnectionNotFoundException var4) {
         }
      } else if (var2 == this.b) {
         BaseCanvas.getCurrentScreen();
         fw.b(fw.a);
         if (this.a != null) {
            cg.b(a.a(352));
            cx.a(this.a, this.a.a(0), this.a.a(1));
         }
      } else if (var2 == this.c) {
         gi var10 = (gi)fw.a;
         cg.b(a.a(352));
         cx.a((br)this.a, (String)var10.a.a(), (String)null);
      } else if (var2 == this.l) {
         Vector var9;
         (var9 = new Vector(2)).addElement(new cd(1, a.a(51) + " " + a.a(184), this));
         var9.addElement(new cd(2, a.a(184) + " " + a.a(595), this));
         this.a(var9, 0);
      } else {
         switch (var2.a) {
            case 0:
               BaseCanvas.getCurrentScreen().t();
               return;
            case 1:
               Vector var8;
               (var8 = new Vector(5)).addElement(new cd(11, a.a(184) + " > " + a.a(25), this));
               var8.addElement(new cd(12, a.a(184) + " > " + a.a(400), this));
               this.a(var8, 0);
               return;
            case 2:
               h();
               return;
            case 11:
               this.f();
               return;
            case 12:
               this.g();
               return;
            case 111:
               try {
                  int var15 = Integer.parseInt(((ge)((gi)fw.a).a(2)).a());
                  cg.a(true);
                  cx.a(1, var15);
                  return;
               } catch (Exception var7) {
                  return;
               }
            case 121:
               try {
                  int var14 = Integer.parseInt(((ge)((gi)fw.a).a(2)).a());
                  cg.a(true);
                  cx.a(2, var14);
                  return;
               } catch (Exception var6) {
                  return;
               }
            case 122:
               try {
                  int var13 = Integer.parseInt(((ge)((gi)fw.a).a(2)).a());
                  cg.a(true);
                  cx.b(var13);
                  return;
               } catch (Exception var5) {
                  return;
               }
            default:
         }
      }
   }

   public static go a(fn var0) {
      return var0.a;
   }
}
