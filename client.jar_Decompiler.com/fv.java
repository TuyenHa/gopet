import vn.me.core.BaseCanvas;

public final class fv extends fw {
   private final l a;
   private eb[] a;
   private String[] a;
   private eb a;
   private final cd a;
   private final cd b;
   private final cd c;
   private int a;
   private ec a;

   public fv() {
      super(true);
      this.f = true;
      this.a = cp.c();
      this.a = new l(110, BaseCanvas.w - 30, BaseCanvas.h - 110 - 10 - gs.m, 40);
      this.b.a(this.a);
      this.n = cg.e;
      this.a = new cd(1, gw.a(39), this);
      this.b = new cd(2, gw.a(3), this);
      this.c = new cd(3, gw.a(120), this);
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
      gs.a(10, 20, BaseCanvas.w - 20, 80);
      int var1 = 20 + this.a;
      int var2 = BaseCanvas.w >> 1;
      if (this.a != null) {
         for(int var3 = 0; var3 < this.a.length; ++var3) {
            gv.a.a(BaseCanvas.g, this.a[var3], var2, var1, 17);
            var1 += gv.a.a() + 2;
         }
      }

      gs.a(10, 100, BaseCanvas.w - 20, BaseCanvas.h - 100 - 5 - gs.m);
   }

   public final void a(eb[] var1) {
      this.a = var1;

      for(int var2 = 0; var2 < this.a.length; ++var2) {
         this.a[var2].b = this;
      }

      this.a.a(this.a);
      this.a.a(0);
   }

   public final void a(Object var1) {
      cd var2;
      if ((var2 = (cd)((Object[])var1)[0]) == null) {
         gn var5 = (gn)((Object[])var1)[1];
         this.a = null;

         for(int var7 = 0; var7 < this.a.length; ++var7) {
            if (var5 == this.a[var7]) {
               eb var8;
               if ((var8 = (eb)var5).c != null) {
                  this.a = gv.a.a(var8.c, BaseCanvas.w - 30);
               }

               this.a = var8;
               this.a = 80 - (this.a.length * gv.a.a() + (this.a.length - 1 << 1)) >> 1;
            }
         }

         if (this.a != null) {
            switch (this.a.z) {
               case -1:
                  this.m = null;
                  this.l = null;
                  return;
               case 0:
                  this.m = this.c;
                  this.l = null;
                  return;
               default:
                  this.m = this.a;
                  this.l = this.b;
            }
         }
      } else {
         switch (var2.a) {
            case 1:
               this.a = new ec();
               this.a.a(0, this.a.z, this.a.b, this.a.c, this.a.b);
               this.a.a(true);
               return;
            case 2:
               cg.a(gw.a(131), new cd(4, gw.a(3), this), cg.b);
               return;
            case 3:
               cg.f();
               en var4;
               (var4 = new en(81)).a(90);
               var4.a(2);
               cx.a.a(var4);
               var4.a();
               return;
            case 4:
               int var6 = this.a.z;
               en var3;
               (var3 = new en(81)).a(90);
               var3.a(3);
               var3.b(var6);
               cx.a.a(var3);
               var3.a();
               BaseCanvas.getCurrentScreen();
               fw.b(fw.a);
               cg.f();
            default:
               super.a(var1);
         }
      }
   }

   public final void a(int var1, String var2, String var3) {
      if (this.a != null) {
         this.a.a(this.a.a, var1, var2, var3, (byte)0);
         this.a.a(true);
      }

   }
}
