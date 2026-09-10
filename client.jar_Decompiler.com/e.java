public class e {
   private byte a;
   private int a;
   private Object a;
   private final di a;

   public e(di var1, int var2, int var3, Object var4) {
      this.a = var1;
      this.a = (byte)var2;
      this.a = var3;
      this.a = var4;
   }

   public final void a() {
      switch (this.a) {
         case 0:
            this.a.a[this.a].a();
            return;
         case 1:
            int[] var10;
            switch ((var10 = (int[])this.a)[0]) {
               case 0:
                  this.a.a[this.a].a(var10[1]);
                  return;
               case 1:
                  this.a.a[this.a].b();
                  return;
               case 2:
                  this.a.a[this.a].c(var10[1]);
                  return;
               default:
                  return;
            }
         case 2:
            this.a.b = true;
            return;
         case 3:
         default:
            return;
         case 4:
            int[] var9 = (int[])this.a;
            this.a.a[this.a].c(var9[0], var9[1]);
            return;
         case 5:
            this.a.a[this.a].b((Integer)this.a);
            return;
         case 6:
            this.a.a[this.a].a((int[])this.a);
            return;
         case 7:
            this.a.a[this.a].c();
            return;
         case 8:
            dv.a.b = true;
            dj.b = false;
            df var6 = (df)dv.a.a.a(this.a.a[0]);
            if (di.a(this.a) > 0) {
               var6.a.a(di.a(this.a) + " (ngoc)", 3, System.currentTimeMillis());
            }

            if (di.b(this.a) > 0) {
               var6.a.a(di.b(this.a) + " EXP", 3, System.currentTimeMillis() + 1000L);
            }

            if (di.a(this.a) != null && di.a(this.a).length > 0) {
               du var7;
               (var7 = new du("Nhận được " + di.a(this.a)[0].a)).a();
               fw.e.addElement(var7);
            }

            if (this.a.c) {
               du var8;
               (var8 = new du(dv.a.a.a(di.c(this.a)).e + " thắng!")).a();
               fw.e.addElement(var8);
            } else if (di.c(this.a) != this.a.a[0] && dv.a.a.a != 12) {
               cg.a("Thua rồi, bạn có muốn về thành phố để điều trị?", new cd(5, a.a(337), this.a), cg.b);
            }

            this.a.a = false;
            return;
         case 9:
            for(int var4 = 0; var4 < this.a.a.length; ++var4) {
               dv.a.b(this.a.a[var4]);
            }

            for(int var5 = 0; var5 < this.a.a.length; ++var5) {
               df var11;
               if ((var11 = (df)dv.a.a.a(this.a.a[var5])) != null && var11.a != null) {
                  var11.a.g = true;
               }
            }

            if (!this.a.c && di.c(this.a) != this.a.a[0]) {
               if (this.a.a.a) {
                  return;
               }

               ((fr)dv.a).a((eh)this.a.a);
               ((fr)dv.a).c(this.a.a);
            }

            this.a.a = false;
            return;
         case 10:
            ((gd)this.a).a(true);
            this.a.a = false;
            return;
         case 11:
            this.a.a[this.a].b((int[])this.a);
            return;
         case 12:
            for(int var1 = 0; var1 < this.a.a.length; ++var1) {
               dv.a.b(this.a.a[var1]);
            }

            for(int var3 = 0; var3 < this.a.a.length; ++var3) {
               df var2;
               if ((var2 = (df)dv.a.a.a(this.a.a[var3])) != null && var2.a != null) {
                  var2.a.g = true;
               }
            }

            this.a.a = false;
      }
   }

   public e() {
   }
}
