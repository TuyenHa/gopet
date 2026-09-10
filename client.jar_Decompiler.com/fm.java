import vn.me.core.BaseCanvas;

public final class fm extends fw {
   private boolean a;
   private dr a;
   private dj a;
   private go a;
   private dn a;

   public fm(dj var1, boolean var2) {
      super(true);
      this.f = true;
      this.a = cp.c();
      this.a = var1;
      this.a = var2;
      dj var10003 = this.a;
      this.a = new dr();
      this.a.b(10 + (64 - this.a.t) / 2, 37);
      this.b.a(this.a);
      this.a.n();
      this.a = new go(0, 132, BaseCanvas.w, BaseCanvas.h - gs.m - 5 - 132);
      this.a.i = true;
      this.a.k = true;
      this.a.d = 0;
      this.b.a(this.a);
      this.n = cg.e;
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
      dj.a.a(this.a.l);
      gs.a(10, 20, BaseCanvas.w - 20, 22);
      dj.a(this.a.b, this.a.a, BaseCanvas.w >> 1);
      gs.a(10, 42, 64, 80);
      gs.a(74, 42, BaseCanvas.w - 20 - 64, 80);
      gv.a.a(BaseCanvas.g, "(str) " + String.valueOf(this.a.c), 85, 46, 0);
      gv.a.a(BaseCanvas.g, "(agi) " + String.valueOf(this.a.d), 85, 64, 0);
      gv.a.a(BaseCanvas.g, "(int) " + String.valueOf(this.a.e), 85, 82, 0);
      int var1 = 85 + (BaseCanvas.w - 85 - 20 >> 1);
      gv.a.a(BaseCanvas.g, "(atk) " + String.valueOf(this.a.f), var1, 46, 0);
      gv.a.a(BaseCanvas.g, "(def) " + String.valueOf(this.a.g), var1, 64, 0);
      gv.a.a(BaseCanvas.g, "(hp) " + String.valueOf(this.a.h) + "/" + this.a.j, var1, 82, 0);
      gv.a.a(BaseCanvas.g, "(mp) " + String.valueOf(this.a.i) + "/" + this.a.k, var1, 100, 0);
      gs.a(10, 122, BaseCanvas.w - 20, BaseCanvas.h - 122 - 5 - gs.m);
   }

   public final void a(Object var1) {
      cd var2;
      switch ((var2 = (cd)((Object[])var1)[0]).a) {
         case 0:
            cg.a_(((p)this.a.a((Integer)var2.a)).a);
            return;
         case 1:
            cg.a(gw.a(86), new cd(2, gw.a(6), var2.a, this), cg.b);
            return;
         case 2:
            dc.a(this.a.a[(Integer)var2.a], 9);
            cg.f();
            return;
         default:
            super.a(var1);
      }
   }

   public final void a(dn var1) {
      this.a = var1;
      this.a.a(var1);
      this.a.o();
      int var4 = 0;

      for(int var2 = 0; var2 < this.a.a.length; ++var2) {
         p var3 = new p(gv.a, this.a.a[var2], this.a.a[var2], this.a.b[var2], this.a.b[var2]);
         var4 = var2 * 20;
         var3.b(17, var4);
         this.a.a(var3);
         var3.d = new cd(0, gw.a(72), new Integer(var2), this);
         if (this.a) {
            var3.c = new cd(1, gw.a(39), new Integer(var2), this);
         }
      }

      int var6 = var4 + 20;

      for(int var7 = 0; var7 < this.a.c.length; ++var7) {
         p var5;
         (var5 = new p(gv.a, 0, this.a.c[var7], this.a.c[var7], 0)).b(17, var6 + var7 * 20);
         this.a.a(var5);
      }

      this.a((gn)this.a.a(0));
      this.a.d(0, 250);
      this.a.c(true);
   }

   public final void a(int var1, int var2, String var3, String var4, int var5) {
      for(int var6 = 0; var6 < 3; ++var6) {
         p var7;
         if ((var7 = (p)this.a.a(var6)).a == var1) {
            var7.a = var2;
            var7.a(var3);
            var7.a = var4;
            var7.b = var5;
         }

         if (this.a.a[var6] == var1) {
            this.a.a[var6] = var2;
            this.a.b[var6] = var4;
            this.a.a[var6] = var3;
            this.a.b[var6] = var5;
         }
      }

   }
}
