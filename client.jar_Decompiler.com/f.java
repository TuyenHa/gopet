import vn.me.core.BaseCanvas;

public final class f extends c {
   private int b;
   private byte a;
   private String a;
   private int c;
   private boolean c = true;
   private int d;
   private int e;
   private int f;

   public final void a() {
      this.a = true;
      this.a = (byte)((int)(System.currentTimeMillis() % 3L));
   }

   public f(String var1) {
      this.a = var1;
      this.b = true;
      if (var1 != null && var1.length() > 0) {
         this.c = cp.b().a(var1) + 2 >> 1;
         this.e = -this.c;
         this.f = BaseCanvas.Field157;
      }
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      if (this.c) {
         switch (this.a) {
            case 0:
               BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, (BaseCanvas.h >> 1) - this.b);
               int var5 = (BaseCanvas.h >> 1) + this.b;
               BaseCanvas.g.fillRect(0, var5, BaseCanvas.w, BaseCanvas.h - var5);
               break;
            case 1:
               int var4 = 32 - (this.b << 1);

               for(int var6 = 0; var6 < (BaseCanvas.h >> 5) + 1; ++var6) {
                  for(int var7 = 0; var7 < (BaseCanvas.w >> 5) + 1; ++var7) {
                     BaseCanvas.g.setColor(0);
                     BaseCanvas.g.fillRect((var7 << 5) + this.b, (var6 << 5) + this.b, var4, var4);
                  }
               }
               break;
            case 2:
               int var2 = 46 - (this.b << 1);

               for(int var3 = 0; var3 < (BaseCanvas.h >> 5) + 1; ++var3) {
                  for(int var1 = 0; var1 < (BaseCanvas.w >> 5) + 1; ++var1) {
                     BaseCanvas.g.setColor(0);
                     BaseCanvas.g.fillArc((var1 << 5) + this.b - 7, (var3 << 5) + this.b - 7, var2, var2, 0, 360);
                  }
               }
         }
      }

      if (this.d > 1) {
         cp.b().a(BaseCanvas.g, this.a, this.e, BaseCanvas.Field158, 17);
      }

   }

   public final void a(long var1) {
      if (this.c) {
         switch (this.a) {
            case 0:
               this.b += 10;
               if (this.b > BaseCanvas.h >> 1) {
                  this.c = false;
                  return;
               }

               return;
            case 1:
               ++this.b;
               if (this.b > 16) {
                  this.c = false;
                  return;
               }

               return;
            case 2:
               ++this.b;
               if (this.b > 23) {
                  this.c = false;
                  return;
               }

               return;
            default:
         }
      } else {
         ++this.d;
         if (this.d >= 15 && this.d <= 17) {
            if (this.d == 15) {
               this.f = BaseCanvas.w + this.c;
            }
         } else {
            this.e = this.f + this.e >> 1;
         }

         if (this.d > 32) {
            this.a = false;
         }
      }
   }
}
