import java.util.Vector;
import vn.me.core.BaseCanvas;

public final class fk extends fw implements gz {
   public static Vector a = new Vector();
   public static Vector b = new Vector();
   public static Vector c = new Vector();
   private go a;
   private go c;
   private go d;
   private cd a;
   private cd b;
   private aq a;
   private int a = -1;
   private int b = -1;
   private int c = -1;
   private cd c;

   public fk() {
      super(true);
      this.c = "MESSAGE";
      this.n = cg.e;
      this.c = new cd(100, a.a(275), this);
      this.l = this.c;
   }

   public final void a(int var1, boolean var2) {
      super.a(var1, (boolean)var2);
      var1 = a.size();
      var2 = b.size();
      int var3 = c.size();
      byte var4 = -1;
      this.a = new aq(this.b.t, BaseCanvas.h - gs.m);
      this.a.a.l = true;
      this.b.a(this.a, false);
      if (this.a == null) {
         this.a = new go(0, gs.l, this.b.t, BaseCanvas.h - 2 * gs.l);
         this.a.i = true;
      }

      if (a.isEmpty()) {
         gj var5;
         (var5 = new gj(a.a(307))).q = 17;
         this.a.a(var5);
      } else {
         var4 = 0;
      }

      for(int var9 = 0; var9 < var1; ++var9) {
         gk var6;
         (var6 = new gk((cz)a.elementAt(var9), 0, 0, BaseCanvas.w, gs.k)).d = new cd(3, a.a(560), this);
         var6.c = cp.c;
         var6.d = cp.c;
         var6.v = var6.r;
         this.a.a(var6, false);
      }

      if (this.c == null) {
         this.c = new go(0, gs.l, this.b.t, BaseCanvas.h - 2 * gs.l);
         this.c.i = true;
      }

      if (b.isEmpty()) {
         gj var10;
         (var10 = new gj(a.a(307))).q = 17;
         this.c.a(var10);
      } else if (var4 == -1) {
         var4 = 1;
      }

      for(int var11 = 0; var11 < var2; ++var11) {
         gk var14;
         (var14 = new gk((cz)b.elementAt(var11), 0, 0, BaseCanvas.w, gs.k)).d = new cd(5, a.a(560), this);
         var14.c = cp.c;
         var14.d = cp.c;
         var14.v = var14.r;
         this.c.a(var14, false);
      }

      if (this.d == null) {
         this.d = new go(0, gs.l, this.b.t, BaseCanvas.h - 2 * gs.l);
         this.d.i = true;
      }

      if (c.isEmpty()) {
         gj var12;
         (var12 = new gj(a.a(307))).q = 17;
         this.d.a(var12);
      } else if (var4 == -1) {
         var4 = 2;
      }

      for(int var13 = 0; var13 < var3; ++var13) {
         gk var15;
         (var15 = new gk((cz)c.elementAt(var13), 0, 0, BaseCanvas.w, gs.k)).d = new cd(8, a.a(560), this);
         var15.c = cp.c;
         var15.d = cp.c;
         var15.v = var15.r;
         this.d.a(var15, false);
      }

      this.a.y = gs.p;
      this.a.b(1);
      this.c.y = gs.p;
      this.c.b(1);
      this.d.y = gs.p;
      this.d.b(1);
      this.a.a(a.a(159), this.a);
      this.a.a("Admin", this.c);
      this.a.a(a.a(391), this.d);
      this.a.a = new fl(this);
      if (var4 != -1) {
         this.a.a(var4);
      } else {
         this.a.f();
      }
   }

   private void f() {
      cz var1;
      if (this.a.c() != -1 && this.a.c() < a.size()) {
         this.a = this.a.c();
         cz var4;
         var1 = var4 = (cz)a.elementAt(this.a.c());
         var4.a = true;
      } else if (this.c.c() != -1 && this.c.c() < b.size()) {
         this.b = this.c.c();
         cz var3;
         var1 = var3 = (cz)b.elementAt(this.c.c());
         var3.a = true;
      } else {
         if (this.d.c() == -1 || this.d.c() >= c.size()) {
            return;
         }

         this.c = this.d.c();
         cz var2;
         var1 = var2 = (cz)c.elementAt(this.d.c());
         var2.a = true;
         switch (var1.a) {
            case 1:
               gd.a(var1.b, (cd)null, cg.b);
               return;
            case 5:
               gd.a(a.a(9) + " " + var1.a + " " + a.a(221), new cd(101, a.a(580), new Integer(var1.a), this), cg.b);
               return;
         }
      }

      gd.a(var1.b, (cd)null, cg.b);
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 0:
            switch (this.a.a()) {
               case 0:
                  if (this.a >= 0 && this.a < a.size()) {
                     a.removeElementAt(this.a);
                     this.a.b(this.a.a(this.a));
                     return;
                  }

                  return;
               case 1:
                  if (this.b >= 0 && this.b < b.size()) {
                     b.removeElementAt(this.b);
                     this.c.b(this.c.a(this.b));
                     return;
                  }

                  return;
               case 2:
                  if (this.c >= 0 && this.c < c.size()) {
                     c.removeElementAt(this.c);
                     this.d.b(this.d.a(this.c));
                     return;
                  }

                  return;
               default:
                  return;
            }
         case 1:
            switch (this.a.a()) {
               case 0:
                  a.removeAllElements();
                  this.a.o();
                  return;
               case 1:
                  b.removeAllElements();
                  this.c.o();
                  return;
               case 2:
                  c.removeAllElements();
                  this.d.o();
                  return;
               default:
                  return;
            }
         case 3:
            this.f();
            return;
         case 5:
            this.f();
            return;
         case 8:
            this.f();
            return;
         case 100:
            this.a = this.a.c();
            this.b = this.c.c();
            this.c = this.d.c();
            Vector var2 = new Vector();
            this.a = new cd(0, a.a(95), this);
            this.b = new cd(1, a.a(94), this);
            var2.addElement(this.a);
            var2.addElement(this.b);
            this.a(var2, 0);
            return;
         default:
      }
   }
}
