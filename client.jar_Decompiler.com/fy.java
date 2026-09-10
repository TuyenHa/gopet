import vn.me.core.BaseCanvas;

public final class fy extends fw {
   private go a = new go();
   private dj a;
   dn a;
   private dr a;
   public int a;
   byte[] a;
   int[] a;
   int[] b;
   private String[] b;
   String[] a;
   public int b;
   private String a;

   public fy(dj var1, int var2) {
      super(true);
      this.b = var2;
      this.f = true;
      this.a = cp.c();
      this.a = var1;
      this.n = cg.e;
      dj var10003 = this.a;
      this.a = new dr();
      this.a.b(10 + (64 - this.a.t) / 2, 37);
      this.b.a(this.a);
      this.b.a(this.a);
      this.a.a(74, 42, BaseCanvas.w - 64 - 20, 80);
      this.a.d(100, 100);
      this.a.i = true;
      this.a.b(1);
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
      dj.a.a(this.a.l);
      gs.a(10, 20, BaseCanvas.w - 20, 22);
      dj var10000 = this.a;
      dj.a(this.a.b, this.a.a, BaseCanvas.w >> 1);
      gs.a(10, 42, 64, 80);
      gs.a(74, 42, BaseCanvas.w - 20 - 64, 80);
      gs.a(10, 122, BaseCanvas.w - 20, BaseCanvas.h - 122 - 5 - gs.m);
      int var1 = 130;
      if (this.a != null) {
         gv.a.a(BaseCanvas.g, this.a, 16, 130, 0);
         var1 = 130 + gv.a.a() + 4;
      }

      if (this.a != null) {
         for(int var2 = 0; var2 < this.a.length; ++var2) {
            gv.a.a(BaseCanvas.g, this.a[var2], 16, var1, 0);
            var1 += gv.a.a();
         }
      }

   }

   public final void a(Object var1) {
      cd var2;
      switch ((var2 = (cd)((Object[])var1)[0]).a) {
         case 10:
            int var6 = this.a.c();
            int var7 = this.a.a;
            en var3;
            (var3 = new en(81)).a(19);
            var3.b(var7);
            var3.a(var6);
            cx.a.a(var3);
            var3.a();
            cg.f();
            return;
         case 11:
            int var5;
            if ((var5 = this.a.c()) < this.a.a.length) {
               cg.a(gw.a(133), new cd(12, gw.a(132), new int[]{var5}, this), cg.b);
               return;
            }

            cg.f();
            dc.c(-1);
            return;
         case 12:
            int var4 = ((int[])var2.a)[0];
            cg.f();
            dc.c(this.a.a[var4]);
            return;
         default:
            super.a(var1);
      }
   }

   public final void a(dn var1, int[] var2, int[] var3, String[] var4, byte[] var5) {
      this.a = var1;
      this.a.a(var1);
      this.a = var2;
      this.b = var3;
      this.a = var5;
      this.b = var4;
      this.a = "(str)" + this.a.c + " (agi)" + this.a.d + " (int)" + this.a.e;
      this.a.o();

      for(int var6 = 0; var6 < 3; ++var6) {
         fz var7;
         (var7 = new fz(this, var6)).c(BaseCanvas.w - 85 - 17, 20);
         var7.b(10, 5 + var6 * 25);
         this.a.a(var7);
         if (this.b == 0) {
            this.m = new cd(10, gw.a(6), this);
         } else {
            this.m = new cd(11, gw.a(132), this);
         }
      }

      this.a((gn)this.a.a(this.a));
   }

   public final void a(dn var1) {
      this.a(var1, (int[])null, (int[])null, (String[])null, (byte[])null);
   }

   public final void a(en var1) {
      try {
         if (var1.a().readInt() == this.a.a) {
            this.a.c = var1.a().readInt();
            this.a.d = var1.a().readInt();
            this.a.e = var1.a().readInt();
            int[] var2 = new int[3];
            int[] var3 = new int[3];
            byte[] var4 = new byte[3];
            String[] var5 = new String[3];
            byte[] var6 = new byte[3];

            for(int var7 = 0; var7 < 3; ++var7) {
               var2[var7] = var1.a().readInt();
               var3[var7] = var1.a().readInt();
               var4[var7] = var1.a().readByte();
               var5[var7] = var1.a().readUTF();
               var6[var7] = var1.a().readByte();
            }

            this.a(this.a, var2, var3, var5, var6);
            cg.a(gw.a(134) + " \n(str) " + this.a.c + "\n(agi) " + this.a.d + "\n(int) " + this.a.e, true);
            --this.a.l;
         }
      } catch (Exception var8) {
         var8.printStackTrace();
      }
   }

   public static String a(fy var0) {
      return var0.b[var0.a];
   }
}
