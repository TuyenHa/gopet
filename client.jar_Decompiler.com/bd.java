import thong.sdk.ISoundManagerSDK;

public final class bd {
   public boolean a;
   private long a;
   Object a;
   private int a;
   private int b;
   private byte a;
   private byte b;
   private final ei a;

   public bd(ei var1, int var2, Object var3) {
      this.a = var1;
      this.a = (byte)var2;
      this.a = var3;
   }

   public final void a() {
      this.a = false;
      this.a = 0L;
      this.a = 0;
      this.b = 0;
      this.b = 0;
   }

   public final void b() {
      dz var1 = null;
      switch (this.a) {
         case 1:
            switch (this.b) {
               case 0:
                  this.a = this.a.i + (Integer)this.a;
                  this.b = this.a.i;
                  this.b = 1;
                  return;
               case 1:
                  int var14 = this.a - this.b;
                  int var18 = this.a - this.a.i;
                  if (this.a > this.a.i) {
                     ei var23 = this.a;
                     var23.i += 4;
                  } else {
                     ei var24 = this.a;
                     var24.i -= 4;
                  }

                  if (var14 * var18 <= 0) {
                     this.a.i = this.a;
                     this.a = true;
                     return;
                  }

                  return;
               default:
                  return;
            }
         case 2:
            switch (this.b) {
               case 0:
                  this.a = System.currentTimeMillis();
                  this.a = (Integer)this.a;
                  this.b = 1;
                  return;
               case 1:
                  if (System.currentTimeMillis() - this.a > (long)this.a) {
                     this.a = true;
                     return;
                  }

                  return;
               default:
                  return;
            }
         case 3:
            switch (this.b) {
               case 0:
                  this.a = System.currentTimeMillis();
                  this.a = (Integer)this.a;
                  this.b = this.a.i;
                  this.b = 1;
                  return;
               case 1:
                  if (System.currentTimeMillis() - this.a > (long)this.a) {
                     this.a = true;
                  }

                  if (this.a.i == this.b) {
                     --this.a.i;
                     return;
                  } else {
                     if (this.a.i >= this.b) {
                        this.a.i = this.b;
                        return;
                     }

                     ei var22 = this.a;
                     var22.i += 2;
                     return;
                  }
               default:
                  return;
            }
         case 4:
            switch (this.b) {
               case 0:
                  this.a = System.currentTimeMillis();
                  this.b = 1;
                  int[] var13 = (int[])this.a;
                  this.a.g = var13[0];
                  this.a.h = var13[1];
                  this.a.o = var13[2];
                  this.a.c = true;
                  ei.a(this.a, (dz)null);
                  ei var17 = this.a;
                  ei var19;
                  if ((var19 = this.a).g >= 4) {
                     if (var19.g < 6) {
                        var1 = dj.a.a[var19.g - 4];
                     } else if (var19.g == 6) {
                        var1 = dj.a.b;
                        ISoundManagerSDK.playSoundEffect("s_hit");
                     } else if (var19.g == 7) {
                        var1 = dj.a.a;
                     } else if (var19.g >= 8) {
                        var1 = dj.a.a(var19.g - 8);
                        ISoundManagerSDK.playSoundEffect("s_hit");
                     }

                     ei.a(var17, var1);
                     if (ei.a(this.a) != null) {
                        ei.a(this.a).a();
                        return;
                     }

                     this.a = true;
                     this.a.c = false;
                     return;
                  } else {
                     ei.a(var17, (dz)null);
                     ei.a(this.a);
                  }
               case 1:
                  if (ei.a(this.a) != null) {
                     if ((var1 = ei.a(this.a)).a == var1.a.b - 1) {
                        this.a = true;
                        this.a.c = false;
                        return;
                     }

                     return;
                  }

                  return;
               default:
                  return;
            }
         case 5:
            this.a.c = 0;
            this.a = true;
            if (this.a.a != null) {
               this.a.a.a();
               return;
            }

            return;
         case 6:
            switch (this.b) {
               case 0:
                  this.a = System.currentTimeMillis();
                  this.b = 1;
                  this.a.q = 0;
                  this.a.e = true;
                  Object[] var12 = this.a;
                  this.a.a = (String)var12[0];
                  Integer var16;
                  if ((var16 = (Integer)var12[1]) == null) {
                     this.a.p = 0;
                     return;
                  }

                  this.a.p = var16;
                  return;
               case 1:
                  if (System.currentTimeMillis() - this.a > 1000L) {
                     this.a = true;
                     this.a.e = false;
                     this.a.p = 0;
                  }

                  ++this.a.q;
                  return;
               default:
                  return;
            }
         case 7:
            switch (this.b) {
               case 0:
                  int[] var10;
                  int var15 = (var10 = (int[])this.a)[0];
                  int var4 = var10[1];
                  int var7 = var10[2];
                  int var11 = var10[3];
                  this.a.e = var15;
                  this.a.f = var7;
                  this.a = var4;
                  this.b = var11;
                  this.b = 1;
                  this.a.b = true;
                  return;
               case 1:
                  boolean var5;
                  boolean var9 = var5 = ed.a(this.a - this.a.e) < 2;
                  if (var5) {
                     this.a.a.h = this.a;
                     var9 = true;
                  } else {
                     ei var20 = this.a;
                     var20.e += this.a - this.a.e >> 1;
                  }

                  boolean var3 = var5 = ed.a(this.b - this.a.f) < 2;
                  if (var5) {
                     this.a.a.i = this.b;
                     var3 = true;
                  } else {
                     ei var21 = this.a;
                     var21.f += this.b - this.a.f >> 1;
                  }

                  if (var9 && var3) {
                     this.a = true;
                     this.a.b = false;
                     return;
                  }

                  return;
               default:
                  return;
            }
         case 8:
            switch (this.b) {
               case 0:
                  this.a.r = 1;
                  this.b = 1;
                  return;
               case 1:
                  ++this.a.r;
                  if (this.a.r == 4) {
                     this.a = true;
                     return;
                  }

                  return;
               default:
                  return;
            }
         case 9:
            switch (this.b) {
               case 0:
                  this.a = System.currentTimeMillis();
                  this.b = 1;
                  int[] var2 = (int[])this.a;
                  this.a.g = var2[0];
                  this.a.h = var2[1];
                  this.a.o = var2[2];
                  this.a.d = true;
                  ei.a(this.a, (bk)null);
                  ei.a(this.a, this.a.a());
                  ei.a(this.a).a((byte)(this.a.d == 0 ? 0 : 1));
                  if (ei.a(this.a) != null) {
                     ei.a(this.a).a(0);
                     ISoundManagerSDK.playSoundEffect("s_attack_crit");
                     return;
                  }

                  this.a = true;
                  this.a.d = false;
                  return;
               case 1:
                  if (ei.a(this.a) != null && !ei.a(this.a).a()) {
                     this.a = true;
                     this.a.d = false;
                     return;
                  }

                  return;
               default:
                  return;
            }
         default:
      }
   }
}
