import vn.me.core.BaseCanvas;

public final class r extends gd implements gz {
   private do[] a;
   public int a;
   public int[] a;

   public r(int var1) {
      this.a = var1;
      int var2 = BaseCanvas.w - (gs.p << 1);
      int var3 = 90 + (gs.p << 2) + 6;
      this.a(gs.p, BaseCanvas.h - gs.m - var3 - this.y, var2, var3);
      var2 = 0;
      switch (var1) {
         case 0:
         case 2:
            var2 = 3;
            break;
         case 1:
         case 3:
            var2 = 2;
      }

      this.a = new do[var2];

      for(int var5 = 0; var5 < this.a.length; ++var5) {
         this.a[var5] = new do();
         this.a(this.a[var5], false);
         this.a[var5].b(20, gs.p + (30 + gs.p) * var5);
         if (var5 != 0) {
            this.a[var5].d = new cd(0, "Đổi", new Integer(var5), this);
         }
      }

      this.e = cg.b;
      if (var1 != 0 && var1 != 2) {
         this.c = new cd(1, gw.a(29), this);
      } else {
         this.c = new cd(1, gw.a(28), this);
      }
   }

   public final void a(int var1, int var2, String var3, String var4, byte var5) {
      this.a[var1].a = var2;
      this.a[var1].b = var3;
      this.a[var1].c = var4;
      this.a[var1].a(var5);
   }

   public final void a() {
      super.a();

      for(int var1 = 0; var1 < this.a.length; ++var1) {
         if (this.a[var1].c != null) {
            gv.a.a(BaseCanvas.g, this.a[var1].c, 60, gs.p + (30 + gs.p) * var1 + (30 - gv.a.a()) / 2, 0);
         }
      }

   }

   public final void a(Object var1) {
      cd var5;
      switch ((var5 = (cd)((Object[])var1)[0]).a) {
         case 0:
            cg.f();
            Integer var11 = (Integer)var5.a;
            switch (this.a) {
               case 0:
                  dc.a(this.a[0].a, var11 + 6);
                  return;
               case 1:
                  dc.a(this.a[0].a, 123);
                  return;
               case 2:
                  var11;
                  int var13 = this.a[0].a;
                  int var10000 = this.a[0].b;
                  en var22;
                  (var22 = new en(81)).a(80);
                  var22.b(var13);
                  cx.a.a(var22);
                  var22.a();
                  return;
               case 3:
                  int var12 = this.a[0].a;
                  en var24;
                  (var24 = new en(81)).a(81);
                  var24.b(var12);
                  cx.a.a(var24);
                  var24.a();
                  return;
               default:
                  return;
            }
         case 1:
            int var6 = 1;
            int var2 = 0;

            for(; var2 < this.a.length; ++var2) {
               if (this.a[var2].a == 0) {
                  var6 = 0;
               }
            }

            if (!var6) {
               cg.a_(gw.a(35));
               return;
            } else {
               cg.a(true);
               switch (this.a) {
                  case 0:
                     var2 = this.a[0].a;
                     int var21 = this.a[1].a;
                     var6 = this.a[2].a;
                     en var23;
                     (var23 = new en(81)).a(48);
                     var23.b(var2);
                     var23.b(var21);
                     var23.b(var6);
                     cx.a.a(var23);
                     var23.a();
                     this.a = new int[3];
                     break;
                  case 1:
                     var6 = this.a[0].a;
                     var2 = this.a[1].a;
                     en var20;
                     (var20 = new en(81)).a(49);
                     var20.b(var6);
                     var20.b(var2);
                     cx.a.a(var20);
                     var20.a();
                     this.a = new int[2];
                     break;
                  case 2:
                     var6 = this.a[0].a;
                     var2 = this.a[1].a;
                     int var19 = this.a[2].a;
                     en var4;
                     (var4 = new en(81)).a(76);
                     var4.b(var6);
                     var4.b(var2);
                     var4.b(var19);
                     cx.a.a(var4);
                     var4.a();
                     this.a = new int[3];
                     break;
                  case 3:
                     var6 = this.a[0].a;
                     var2 = this.a[1].a;
                     en var3;
                     (var3 = new en(81)).a(79);
                     var3.b(var6);
                     var3.b(var2);
                     cx.a.a(var3);
                     var3.a();
                     this.a = new int[2];
               }

               for(int var18 = 0; var18 < this.a.length; ++var18) {
                  this.a[var18] = this.a[var18].a;
               }

               return;
            }
         default:
      }
   }
}
