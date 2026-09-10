import javax.microedition.lcdui.Image;

public class gb extends gj implements gz {
   public int c;
   public boolean a;
   public int d;
   public int e;

   public gb() {
      this(0);
   }

   public gb(int var1) {
      this("");
      this.c = var1;
      if (var1 == 1 || var1 == 2) {
         this.d = new cd(0, gw.a(7), this);
      }

   }

   public gb(Image var1) {
      this(0);
      this.a((Image)var1);
   }

   public gb(String var1, gg var2) {
      super(var1, var2);
      this.c = 0;
      this.a = false;
      this.d = 16777215;
      this.e = 16777215;
      this.g = true;
   }

   public gb(String var1) {
      super(var1);
      this.c = 0;
      this.a = false;
      this.d = 16777215;
      this.e = 16777215;
      this.g = true;
   }

   private gb(cd var1, gg var2) {
      this(var1 == null ? "" : var1.a, var2);
      this.d = var1;
   }

   public gb(cd var1) {
      this(var1, gv.a);
   }

   public final void a(cd var1) {
      this.e = var1 == null ? "" : var1.a;
      this.d = var1;
   }

   public void a() {
      gs.a(this);
   }

   public void b() {
      super.b();
      gs.c(this);
   }

   public void e() {
      gs.b(this);
   }

   public void g() {
      super.g();
      if ((this.c == 1 || this.c == 2) && this.a != null) {
         this.p = 1;
         this.x = 1;
      }

   }

   public final void b_() {
      super.b_();
      if ((this.c == 1 || this.c == 2) && this.a != null) {
         this.p = 0;
         this.x = 0;
      }

   }

   public final void a(Object var1) {
      if (this.c == 1) {
         this.a = !this.a;
      }

      if (this.c == 2 && !this.a) {
         ((gc)this.b).a(this);
      }
   }
}
