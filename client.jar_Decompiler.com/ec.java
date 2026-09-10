import vn.me.core.BaseCanvas;

public final class ec extends gd implements gz {
   private eb[] a;
   public int a;

   public ec() {
      int var1 = BaseCanvas.w - (gs.p << 1);
      int var2 = 120 + (gs.p << 2) + 6;
      this.a(gs.p, BaseCanvas.h - gs.m - var2 - this.y, var1, var2);
      this.a = new eb[3];

      for(int var3 = 0; var3 < this.a.length; ++var3) {
         this.a[var3] = new eb();
         this.a(this.a[var3], false);
         this.a[var3].b(20, gs.p + (40 + gs.p) * var3);
         if (var3 != 0) {
            this.a[var3].d = new cd(0, gw.a(38), new Integer(var3), this);
         }
      }

      this.e = cg.b;
      this.c = new cd(1, gw.a(39), this);
   }

   public final void a(int var1, int var2, String var3, String var4, byte var5) {
      this.a[var1].z = var2;
      this.a[var1].b = var3;
      this.a[var1].c = var4;
      this.a[var1].a(var5);
   }

   public final void a() {
      super.a();

      for(int var1 = 0; var1 < this.a.length; ++var1) {
         if (this.a[var1].c != null) {
            gv.a.a(BaseCanvas.g, this.a[var1].c, 70, gs.p + (40 + gs.p) * var1 + (40 - gv.a.a()) / 2, 0);
         }
      }

   }

   public final void a(Object var1) {
      cd var5;
      switch ((var5 = (cd)((Object[])var1)[0]).a) {
         case 0:
            cg.f();
            Integer var8 = (Integer)var5.a;
            this.a = var8;
            byte var9 = var8.byteValue();
            en var11;
            (var11 = new en(81)).a(90);
            var11.a(4);
            var11.a(var9);
            cx.a.a(var11);
            var11.a();
            return;
         case 1:
            int var6 = 1;

            for(int var2 = 0; var2 < this.a.length; ++var2) {
               if (this.a[var2].z == 0) {
                  var6 = 0;
               }
            }

            if (!var6) {
               cg.a_(gw.a(35));
               return;
            }

            cg.a(true);
            int var10 = this.a[0].z;
            var6 = this.a[1].z;
            int var3 = this.a[2].z;
            en var4;
            (var4 = new en(81)).a(90);
            var4.a(5);
            var4.b(var10);
            var4.b(var6);
            var4.b(var3);
            cx.a.a(var4);
            var4.a();
            return;
         default:
      }
   }
}
