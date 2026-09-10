import java.util.Vector;
import vn.me.core.BaseCanvas;

public class fj extends fw implements gz {
   public byte a = -1;
   public int b;
   protected go a;
   private dd[] a;
   private gi a;

   public fj() {
      super(true);
      this.n = new cd(1, a.a(64), this);
   }

   public final void c(Vector var1) {
      this.b.b(this.a);
      this.a = new dd[var1.size()];
      if (this.a == null) {
         this.a = new go(0, gs.l, BaseCanvas.w, BaseCanvas.h - 2 * gs.l);
         this.a.i = true;
         this.a.k = true;
      }

      this.a.o();
      int var2 = var1.size();

      for(int var3 = 0; var3 < var2; ++var3) {
         dd var4 = (dd)var1.elementAt(var3);
         this.a[var3] = var4;
         bt var5;
         (var5 = new bt(var4, this.a.t, gs.k + (gs.p << 1))).c = cp.c;
         var5.d = cp.c;
         var5.a = gv.a;
         if (var4.a) {
            var5.d = new cd(2, a.a(419), var4, this);
         } else {
            var5.d = null;
         }

         this.a.a(var5, false);
      }

      this.a.d = 0;
      this.b.a(this.a);
      this.a.b(1);
      this.a.c(true);
   }

   public void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
   }

   public void a(Object var1) {
      cd var5;
      switch ((var5 = (cd)((Object[])var1)[0]).a) {
         case 1:
            this.t();
            return;
         case 2:
            dd var30;
            if ((var30 = (dd)var5.a).b) {
               cg.a(var30.a, (String)var30.b, (cd)null, new cd(3, var30.c, var30, this), cg.b);
               return;
            } else {
               cx.c(this.b, var30.a);
               if (var30.c) {
                  this.t();
                  return;
               }

               return;
            }
         case 3:
            fw.b(fw.a);
            dd var29 = (dd)var5.a;
            switch (this.a) {
               case 1:
               case 3:
                  cd[] var44 = new cd[var29.a.length];

                  for(int var46 = 0; var46 < var44.length; ++var46) {
                     if (var29.a[var46] != 0) {
                        var44[var46] = new cd(4, gw.a(6), new int[]{var29.a, var29.a[var46]}, this);
                     }
                  }

                  cg.a(0, gw.a(73), var29.a, var29.a, var29.a, var44).a(true);
                  return;
               case 2:
                  cx.c(this.b, var29.a);
                  if (var29.c) {
                     this.t();
                     return;
                  }

                  return;
               default:
                  return;
            }
         case 4:
            fw.b(fw.a);
            cg.f();
            int[] var42 = (int[])var5.a;
            int var45 = this.b;
            int var28 = var42[0];
            int var43 = var42[1];
            en var4;
            (var4 = new en(122)).a(9);
            var4.b(var45);
            var4.a(2);
            var4.b(var28);
            var4.b(var43);
            cx.a.a(var4);
            var4.a();
            return;
         case 5:
            en var27;
            (var27 = new en(81)).a(74);
            cx.a.a(var27);
            var27.a();
            cg.f();
            return;
         case 6:
            cg.f();
            en var26;
            (var26 = new en(81)).a(92);
            var26.a(5);
            cx.a.a(var26);
            var26.a();
            return;
         case 7:
            Integer var25 = (Integer)var5.a;
            Vector var41;
            (var41 = new Vector()).addElement(new cd(8, gw.a(74), var25, this));
            var41.addElement(new cd(9, gw.a(39), var25, this));
            this.a(var41, 2);
            return;
         case 8:
            int var24 = this.a[(Integer)var5.a].a;
            en var40;
            (var40 = new en(81)).a(92);
            var40.a(4);
            var40.b(var24);
            cx.a.a(var40);
            var40.a();
            cg.f();
            return;
         case 9:
            int var23 = this.a[(Integer)var5.a].a;
            en var39;
            (var39 = new en(81)).a(92);
            var39.a(6);
            var39.b(var23);
            cx.a.a(var39);
            var39.a();
            cg.f();
            return;
         case 10:
            int var21;
            if ((var21 = this.a.c()) != -1) {
               var21 = this.a[var21].a;
               en var38;
               (var38 = new en(81)).a(99);
               var38.b(var21);
               cx.a.a(var38);
               var38.a();
               cg.f();
               return;
            }

            return;
         case 11:
            int var20;
            if ((var20 = this.a.c()) != -1) {
               cg.a(gw.a(85), new cd(111, gw.a(47), new Integer(var20), this), cg.b);
               return;
            }

            return;
         case 12:
            this.a = gd.a(gw.a(71), new cd(121, gw.a(9), new Integer(this.a[(Integer)var5.a].a), this), cg.b);
            return;
         case 13:
            Vector var37;
            (var37 = new Vector()).addElement(new cd(131, gw.a(75), this));
            var37.addElement(new cd(132, gw.a(76), this));
            this.a(var37, 0);
            return;
         case 14:
            dd var18 = this.a[this.a.c()];
            cg.f();
            int var19 = var18.a;
            en var36;
            (var36 = new en(121)).a(5);
            var36.b(var19);
            cx.a.a(var36);
            var36.a();
            return;
         case 15:
            dd var16 = this.a[this.a.c()];
            cg.f();
            int var17 = var16.a;
            en var35;
            (var35 = new en(121)).a(12);
            var35.b(var17);
            cx.a.a(var35);
            var35.a();
            return;
         case 16:
            this.a = gd.a(gw.a(77), new cd(161, gw.a(6), this), cg.b);
            return;
         case 111:
            dd var14 = this.a[(Integer)var5.a];
            this.x();
            cg.f();
            int var15 = var14.a;
            en var34;
            (var34 = new en(121)).a(7);
            var34.b(var15);
            cx.a.a(var34);
            var34.a();
            return;
         case 121:
            this.a.j();
            int var13 = (Integer)var5.a;
            cg.f();
            String var33 = this.a.a(0);
            en var3;
            (var3 = new en(121)).a(14);
            var3.b(var13);
            var3.a(var33);
            cx.a.a(var3);
            var3.a();
            return;
         case 131:
            dd var11 = this.a[this.a.c()];
            cg.f();
            int var12 = var11.a;
            en var32;
            (var32 = new en(121)).a(6);
            var32.b(var12);
            cx.a.a(var32);
            var32.a();
            return;
         case 132:
            dd var9 = this.a[this.a.c()];
            cg.f();
            int var10 = var9.a;
            en var31;
            (var31 = new en(121)).a(9);
            var31.b(var10);
            cx.a.a(var31);
            var31.a();
            return;
         case 161:
            cg.f();
            String var8 = this.a.a(0).trim();
            en var2;
            (var2 = new en(121)).a(11);
            var2.a(var8);
            cx.a.a(var2);
            var2.a();
            this.a.j();
            return;
         case 162:
            en var7;
            (var7 = new en(81)).a(6);
            cx.a.a(var7);
            var7.a();
            return;
         case 163:
            en var6;
            (var6 = new en(81)).a(6);
            cx.a.a(var6);
            var6.a();
            return;
         default:
      }
   }

   public final void b(int var1) {
      this.b = var1;
      this.l = null;
      this.m = null;
      switch (this.b) {
         case 1049:
            this.l = new cd(162, "Lấy", this);
            return;
         case 1050:
            this.l = new cd(163, "Đưa", this);
            return;
         case 81004:
            this.l = new cd(5, gw.a(83), this);
            return;
         case 81028:
            this.l = new cd(10, gw.a(82), this);
            return;
         case 81040:
            this.l = new cd(6, gw.a(84), this);

            for(int var7 = 0; var7 < this.a.a.length; ++var7) {
               gn var10 = this.a.a[var7];
               cd var13 = new cd(7, gw.a(7), new Integer(var7), this);
               this.m = var13;
               var10.d = var13;
            }

            return;
         case 81087:
            this.l = new cd(11, gw.a(81), this);

            for(int var6 = 0; var6 < this.a.a.length; ++var6) {
               gn var9 = this.a.a[var6];
               cd var12 = new cd(12, gw.a(71), new Integer(var6), this);
               this.m = var12;
               var9.d = var12;
            }

            return;
         case 81088:
            this.l = new cd(13, gw.a(63), this);

            for(int var5 = 0; var5 < this.a.a.length; ++var5) {
               gn var8 = this.a.a[var5];
               cd var11 = new cd(14, gw.a(80), new Integer(var5), this);
               this.m = var11;
               var8.d = var11;
            }

            return;
         case 81089:
            this.l = new cd(16, gw.a(79), this);

            for(int var4 = 0; var4 < this.a.a.length; ++var4) {
               gn var2 = this.a.a[var4];
               cd var3 = new cd(15, gw.a(78), new Integer(var4), this);
               this.m = var3;
               var2.d = var3;
            }

            return;
         default:
      }
   }

   public final void l() {
      super.l();
      cp.d();
   }
}
