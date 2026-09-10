import vn.me.core.BaseCanvas;

public final class i extends gd implements gz {
   private int e = 94;
   private int f = 126;
   private int g = 24;
   public static int a;
   private j a;
   private j b;
   private gj a;

   public i() {
      this.u = this.f;
      this.t = BaseCanvas.Field161;
      int var1 = BaseCanvas.w - this.t >> 1;
      this.r = var1;
      this.v = var1;
      this.w = (BaseCanvas.h - this.u >> 1) + 6;
      this.s = -this.u;
      a = 0;
      this.a = new gj(a.a(422));
      this.a = new j(this);
      this.a.a = 1;
      this.a.d = new cd(1, a.a(337), this);
      this.a.u = 60;
      this.a.t = 45;
      this.a.v = (this.t >> 1) - this.a.t >> 1;
      this.a.w = this.u + this.g - this.a.u >> 1;
      this.b = new j(this);
      this.b.a = 0;
      this.b.d = new cd(2, a.a(337), this);
      this.b.u = 60;
      this.b.t = 45;
      this.b.v = (this.t >> 1) + ((this.t >> 1) - this.b.t >> 1);
      this.b.w = this.u + this.g - this.b.u >> 1;
      j var2 = this.a;
      this.b.x = 1;
      var2.x = 1;
      this.a.r = 0;
      this.b.r = this.e;
      this.c = this.a;
      this.c = new cd(3, a.a(139), this);
      ac var3 = new ac();
      this.a(this.a, false);
      this.a(var3, false);
      this.a(this.a, false);
      this.a(this.b, false);
      this.c = 2;
      this.b(0);
      this.a.a(0, 4, this.t, gs.m);
      this.a.q = 17;
      var3.a(0, this.a.u + this.a.s + gs.p, this.t, 4);
      this.e = new cd(4, a.a(267), this);
   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 1:
            a = 0;
            cg.f(a.a(130) + ":");
            return;
         case 2:
            a = 1;
            cg.f(a.a(130) + ":");
            return;
         case 3:
            cg.g();
            return;
         case 4:
            cx.f();
            return;
         default:
      }
   }
}
