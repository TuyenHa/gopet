import java.io.IOException;
import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class fu extends fw {
   private dr a;
   private dn a;
   private o[] a;
   private int[] a;
   private String a;
   private l a;
   private do[] a;
   private String[] a;
   private boolean a;
   private do a;
   private do b;
   private int a;
   private q a;
   private q b;
   private int b = 124;
   private int c;
   private int d;
   private r a;

   public fu(dj var1, boolean var2) {
      super(true);
      this.c = BaseCanvas.w - 30 - 30 - 15;
      this.d = BaseCanvas.w - 30 - 15;
      this.a = var2;
      this.f = true;
      this.a = cp.c();
      this.a = new dr();
      this.a.b(15 + (64 - this.a.t) / 2, 57);
      this.a.a = false;
      this.a.g = false;
      this.b.a(this.a);
      if (var2) {
         this.a = new l(172, BaseCanvas.w - 30, BaseCanvas.h - 172 - 10 - gs.m, 30);
         this.a.e = new cd(4, gw.a(0), this);
         this.a.d = new cd(5, gw.a(74), this);
         this.a.c = new cd(11, gw.a(63), this);
      }

      this.n = cg.e;
      this.a = new o[5];
      this.a = new int[5];

      for(int var6 = 0; var6 < 5; ++var6) {
         this.a[var6] = new o(this);
      }

      this.a[0] = 3;
      this.a[1] = 2;
      this.a[2] = 1;
      this.a[3] = 104;
      this.a[4] = 105;
      int[] var7 = new int[]{37, 16, 57, 16, 57};
      int[] var10 = new int[]{50, 90, 90, 120, 120};

      for(int var3 = 0; var3 < this.a.length; ++var3) {
         this.a[var3].a(var7[var3], var10[var3], 25, 25);
         if (this.a) {
            this.a[var3].d = new cd(0, gw.a(51), new Integer(var3), this);
         }

         this.b.a(this.a[var3]);
         this.a[var3].b = this;
      }

      this.a[0].n();
      this.b.b(this.a);
      this.b = BaseCanvas.h - 20 >> 1;
      Image var8 = null;

      try {
         var8 = Image.createImage("/pet/left.png");
      } catch (IOException var5) {
         var5.printStackTrace();
      }

      this.a = new q(var8);
      this.a.a(this.c, this.b, 30, 20);
      this.a.a(new cd(17, "", this));
      var8 = null;

      try {
         var8 = Image.createImage("/pet/right.png");
      } catch (IOException var4) {
         var4.printStackTrace();
      }

      this.b = new q(var8);
      this.b.a(this.d, this.b, 30, 20);
      this.b.a(new cd(18, "", this));
   }

   private o a(int var1) {
      for(int var2 = 0; var2 < 5; ++var2) {
         if (this.a[var2] == var1) {
            return this.a[var2];
         }
      }

      return null;
   }

   public final void b() {
      BaseCanvas.g.setColor(0);
      BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
      dj.a.a(this.a.l);
      gs.a(10, 20, BaseCanvas.w - 20, 22);
      dj.a(this.a.b, this.a.a, BaseCanvas.w >> 1);
      gs.a(10, 42, 80, 110);
      gs.a(90, 42, BaseCanvas.w - 20 - 80, 110);
      int var1 = 47;
      if (this.a != null) {
         for(int var2 = 0; var2 < this.a.length; ++var2) {
            gv.a.a(BaseCanvas.g, this.a[var2], 95, var1, 0);
            var1 += gv.a.a() + 2;
         }

         if (!this.b.a) {
            cp.e.a(BaseCanvas.g, "Pet khác đang mặc", 95, var1, 0);
         }
      }

      if (this.b != null && this.b.a()) {
         gv.a.a(BaseCanvas.g, "Đang tháo ngọc,", 95, var1, 0);
         gv.a.a(BaseCanvas.g, "còn " + ed.b((long)this.b.a()), 95, var1 + gv.a.a() + 2, 0);
      }

      int var4 = 25;
      gs.a(10, 152, BaseCanvas.w - 20, BaseCanvas.h - 152 - 5 - gs.m);
      if (this.a != null) {
         gv.a.a(BaseCanvas.g, this.a, 25, 157, 20);
         var4 = 25 + gv.a.a(this.a);
      }

      if (this.a != null && this.a.a > 1) {
         var1 = var4 + 10;

         for(int var5 = 0; var5 < this.a.a; ++var5) {
            if (var5 == this.a.b) {
               BaseCanvas.g.setColor(16777215);
            } else {
               BaseCanvas.g.setColor(10000536);
            }

            BaseCanvas.g.fillArc(var1, 162, 5, 5, 0, 360);
            var1 += 10;
         }

      }
   }

   public final void a(Object var1) {
      cd var2;
      if ((var2 = (cd)((Object[])var1)[0]) != null) {
         switch (var2.a) {
            case 0:
               switch ((Integer)var2.a) {
                  case 0:
                     this.a = "Nón";
                     this.e(3);
                     return;
                  case 1:
                     this.a = "Giáp";
                     this.e(2);
                     return;
                  case 2:
                     this.a = "Vũ khí";
                     this.e(1);
                     return;
                  case 3:
                     this.a = "Giày";
                     this.e(104);
                     return;
                  case 4:
                     this.a = "Bao tay";
                     this.e(105);
                     return;
                  default:
                     return;
               }
            case 1:
            case 2:
            case 3:
            default:
               super.a(var1);
               return;
            case 4:
               this.b.b(this.a);
               this.b.b(this.b);
               this.a[0].n();
               this.b.b(this.a);
               return;
            case 5:
               gn var14;
               if ((var14 = this.a.a(true)) != null && var14 != this.a) {
                  int var25 = ((do)this.a.a(true)).a;
                  en var28;
                  (var28 = new en(81)).a(29);
                  var28.b(var25);
                  cx.a.a(var28);
                  var28.a();
                  cg.f();
                  return;
               }

               return;
            case 6:
               int var24 = (Integer)var2.a;
               en var27;
               (var27 = new en(81)).a(39);
               var27.b(var24);
               cx.a.a(var27);
               var27.a();
               cg.f();
               return;
            case 7:
               gn var13;
               if ((var13 = this.a.a(true)) != null && var13 != this.a) {
                  this.a((do)this.a.a(true));
                  return;
               }

               return;
            case 8:
               do var12 = (do)var2.a;
               Vector var23 = new Vector();
               switch (var12.c) {
                  case 0:
                     var23.addElement(new cd(14, "Gắn ngọc", var12, this));
                     break;
                  case 1:
                     var23.addElement(new cd(15, "Tháo ngọc", var12, this));
                     break;
                  case 2:
                     var23.addElement(new cd(16, "Tháo ngọc nhanh", var12, this));
               }

               var23.addElement(new cd(6, "Tháo ra", new Integer(var12.a), this));
               var23.addElement(new cd(9, gw.a(28), var12, this));
               var23.addElement(new cd(10, gw.a(29), var12, this));
               this.a(var23, 0);
               return;
            case 9:
               this.a((do)var2.a);
               return;
            case 10:
               this.b((do)var2.a);
               return;
            case 11:
               gn var10;
               if ((var10 = this.a.a(true)) != null && var10 != this.a) {
                  var10 = (do)this.a.a(true);
                  Vector var22 = new Vector();
                  switch (var10.c) {
                     case 0:
                        var22.addElement(new cd(14, "Gắn ngọc", var10, this));
                        break;
                     case 1:
                        var22.addElement(new cd(15, "Tháo ngọc", var10, this));
                        break;
                     case 2:
                        var22.addElement(new cd(16, "Tháo ngọc nhanh", var10, this));
                  }

                  if (!var10.a) {
                     var22.addElement(new cd(6, "Tháo ra", new Integer(var10.a), this));
                  }

                  var22.addElement(new cd(7, gw.a(28), var10, this));
                  var22.addElement(new cd(12, gw.a(29), var10, this));
                  var22.addElement(new cd(13, gw.a(109), new Integer(var10.a), this));
                  this.a(var22, 0);
                  return;
               }

               return;
            case 12:
               gn var9;
               if ((var9 = this.a.a(true)) != null && var9 != this.a) {
                  this.b((do)this.a.a(true));
                  return;
               }

               return;
            case 13:
               cg.f();
               int var8 = (Integer)var2.a;
               en var21;
               (var21 = new en(81)).a(56);
               var21.b(var8);
               cx.a.a(var21);
               var21.a();
               return;
            case 14:
               this.a = ((do)var2.a).a;
               cg.f();
               int var7 = this.a;
               en var20;
               (var20 = new en(81)).a(73);
               var20.b(var7);
               cx.a.a(var20);
               var20.a();
               return;
            case 15:
               this.a = ((do)var2.a).a;
               cg.f();
               int var6 = this.a;
               en var19;
               (var19 = new en(81)).a(75);
               var19.b(var6);
               cx.a.a(var19);
               var19.a();
               return;
            case 16:
               this.a = ((do)var2.a).a;
               cg.f();
               int var5 = this.a;
               en var18;
               (var18 = new en(81)).a(78);
               var18.b(var5);
               cx.a.a(var18);
               var18.a();
               return;
            case 17:
               this.a.a(this.a.b == 0 ? 0 : this.a.b - 1);
               return;
            case 18:
               this.a.a(this.a.b == this.a.a - 1 ? this.a.a - 1 : this.a.b + 1);
         }
      } else {
         gn var4 = (gn)((Object[])var1)[1];
         boolean var15 = false;

         for(int var3 = 0; var3 < 5; ++var3) {
            if (var4 == this.a[var3]) {
               var15 = true;
               break;
            }
         }

         this.a = null;
         this.b = null;
         if (var15) {
            o var17;
            if ((var17 = (o)var4).a != null) {
               if (var17.a.c != null) {
                  this.a = gv.a.a(var17.a.c, BaseCanvas.w - 100 - 10);
               }

               this.b = var17.a;
            }
         } else {
            for(int var16 = 0; var16 < this.a.length; ++var16) {
               if (var4 == this.a[var16]) {
                  do var26;
                  if ((var26 = (do)var4).c != null) {
                     this.a = gv.a.a(var26.c, BaseCanvas.w - 100 - 10);
                  }

                  this.b = var26;
               }
            }

         }
      }
   }

   private void f() {
      for(int var1 = 0; var1 < 5; ++var1) {
         this.a[var1].c = null;
         this.a[var1].a = null;
      }

      for(int var3 = 0; var3 < this.a.length; ++var3) {
         if (this.a[var3].n == this.a.a) {
            o var2;
            (var2 = this.a(this.a[var3].b)).a = this.a[var3];
            var2.c = new cd(8, gw.a(63), this.a[var3], this);
         }
      }

   }

   public final void a(dn var1, do[] var2) {
      this.a.a(var1);
      this.a = var1;
      this.a = var2;

      for(int var3 = 0; var3 < this.a.length; ++var3) {
         this.a[var3].b = this;
      }

      this.f();
   }

   private void e(int var1) {
      this.b.b(this.a);
      this.b.b(this.b);
      int var2 = 0;

      for(int var3 = 0; var3 < this.a.length; ++var3) {
         if (b(this.a[var3].b, var1) && this.a[var3].n != this.a.a) {
            ++var2;
         }
      }

      if (var2 != 0) {
         do[] var6 = new do[var2];
         var2 = 0;

         for(int var4 = 0; var4 < this.a.length; ++var4) {
            if (b(this.a[var4].b, var1) && this.a[var4].n != this.a.a) {
               var6[var2] = this.a[var4];
               if (this.a[var4].n != 0 && this.a[var4].n != -1) {
                  var6[var2].a = false;
               } else {
                  var6[var2].a = true;
               }

               ++var2;
            }
         }

         this.a.a(var6);
         this.a.a(0);
         if (this.a.a > 1) {
            this.b.a(this.a);
            this.b.a(this.b);
         }
      } else {
         gj var7;
         (var7 = new gj("Không có item nào.", gv.a)).g = false;
         this.a.o();
         this.a.a(var7);
      }

      this.b.a(this.a);
   }

   public final void a(int var1) {
      fw.b(fw.a);
      do var2 = null;

      for(int var3 = 0; var3 < this.a.length; ++var3) {
         if (this.a[var3].a == var1) {
            var2 = this.a[var3];
            break;
         }
      }

      if (var2 != null) {
         var2.n = this.a.a;
         var1 = -1;
         o var5;
         if ((var5 = this.a(var2.b)).a != null) {
            var1 = var5.a.a;
         }

         for(int var6 = 0; var6 < this.a.length; ++var6) {
            if (var1 == this.a[var6].a) {
               this.a[var6].n = 0;
               break;
            }
         }

         this.f();
         this.a.o();
         this.a[0].n();
         this.b.b(this.a);
      }
   }

   public final void b(int var1) {
      fw.b(fw.a);
      do var2 = null;

      for(int var3 = 0; var3 < this.a.length; ++var3) {
         if (this.a[var3].a == var1) {
            var2 = this.a[var3];
            break;
         }
      }

      if (var2 != null) {
         var2.n = 0;
         this.f();
         this.a.o();
         this.a[0].n();
         this.b.b(this.a);
      }
   }

   private void a(do var1) {
      this.a = new r(0);
      this.a.a(0, var1.a, var1.b, var1.c, var1.b);
      this.a(this.a, false);
      this.a = var1;
   }

   public final void a(en var1) {
      try {
         if (this.a != null) {
            int var2 = var1.a().readInt();
            String var3 = var1.a().readUTF();
            String var4 = var1.a().readUTF();
            int var6 = var1.a().readInt();
            if (this.a.a == 0) {
               this.a.a(var6 - 6, var2, var3, var4, (byte)0);
            } else {
               this.a.a(1, var2, var3, var4, (byte)0);
            }

            this.a(this.a, true);
         }
      } catch (Exception var5) {
         var5.printStackTrace();
      }
   }

   public final void b(en var1) {
      try {
         byte var2 = var1.a().readByte();
         int var3 = var1.a().readInt();
         String var4 = var1.a().readUTF();
         String var5 = var1.a().readUTF();
         String var6 = var1.a().readUTF();
         int var7 = var1.a().readInt();
         int var8 = var1.a().readInt();
         int var9 = var1.a().readInt();
         int var10 = var1.a().readInt();
         int var11 = var1.a().readInt();
         int var12 = var1.a().readInt();
         int var13 = var1.a().readInt();
         byte var14 = var1.a().readByte();
         byte var19 = var1.a().readByte();
         if (this.a.a == 0) {
            if (var2 != -1) {
               for(int var25 = 0; var25 < this.a.length; ++var25) {
                  if (this.a[var25].a == var3) {
                     this.a[var25].c = var5;
                     this.a[var25].a = var6;
                     this.a[var25].g = var7;
                     this.a[var25].h = var8;
                     this.a[var25].i = var9;
                     this.a[var25].j = var10;
                     this.a[var25].k = var11;
                     this.a[var25].l = var12;
                     this.a[var25].m = var13;
                     this.a[var25].a = var14;
                     this.a[var25].a(var19);
                     break;
                  }
               }
            } else {
               do[] var15 = new do[this.a.length - 1];
               int var16 = 0;

               for(int var17 = 0; var17 < this.a.length; ++var17) {
                  if (this.a[var17].a != var3) {
                     int var20 = var16++;
                     var15[var20] = this.a[var17];
                  }
               }

               this.a = new do[var15.length];
               System.arraycopy(var15, 0, this.a, 0, this.a.length);
               this.a[0].n();
               this.b.b(this.a);
            }

            this.a[0].n();
         } else {
            do var26 = null;
            do var28 = null;

            for(int var30 = 0; var30 < this.a.length; ++var30) {
               if (this.a[var30].a == this.a.a[0]) {
                  var26 = this.a[var30];
               }

               if (this.a[var30].a == this.a.a[1]) {
                  var28 = this.a[var30];
               }
            }

            for(int var31 = 0; var31 < 5; ++var31) {
               if (var26 != null && this.a[var31].a != null && this.a[var31].a.a == var26.a || var28 != null && this.a[var31].a != null && this.a[var31].a.a == var28.a) {
                  this.a[var31].a = null;
                  this.a[var31].c = null;
               }
            }

            do var32;
            (var32 = new do()).a(this.a);
            var32.a = var3;
            var32.b = var4;
            var32.c = var5;
            var32.a = var6;
            var32.g = var7;
            var32.h = var8;
            var32.i = var9;
            var32.j = var10;
            var32.k = var11;
            var32.l = var12;
            var32.m = var13;
            var32.a = var14;
            var32.n = 0;
            var32.b = this;
            var32.a(var19);
            do[] var21 = new do[this.a.length - 1];
            var7 = 0;

            for(int var23 = 0; var23 < this.a.length; ++var23) {
               if (this.a[var23].a != this.a.a[0] && this.a[var23].a != this.a.a[1]) {
                  var9 = var7++;
                  var21[var9] = this.a[var23];
               }
            }

            var21[var7] = var32;
            this.a = new do[var21.length];
            System.arraycopy(var21, 0, this.a, 0, this.a.length);
            this.a[0].n();
            this.b.b(this.a);
         }

         this.x();
         s var27 = new s(var2 == 1);
         do var29;
         (var29 = new do()).a = var3;
         var29.b = var4;
         var29.a(var19);
         var27.a(var29, var5);
         this.a(var27, false);
      } catch (Exception var18) {
         var18.printStackTrace();
      }
   }

   private void b(do var1) {
      this.a = new r(1);
      this.a.a(0, var1.a, var1.b, var1.c, var1.b);
      this.a(this.a, false);
      this.a = var1;
   }

   public final void c(int var1) {
      fw.b(fw.a);
      boolean var2 = false;
      do[] var3 = new do[this.a.length];
      int var4 = 0;

      for(int var5 = 0; var5 < this.a.length; ++var5) {
         if (this.a[var5].a != var1) {
            int var6 = var4++;
            var3[var6] = this.a[var5];
         } else {
            var2 = true;
         }
      }

      if (var2) {
         this.a = new do[var3.length - 1];
         System.arraycopy(var3, 0, this.a, 0, this.a.length);
      }

      this.a[0].n();
      this.b.b(this.a);
   }

   public final void c(en var1) {
      try {
         int var2 = var1.a().readInt();

         for(int var3 = 0; var3 < this.a.length; ++var3) {
            if (this.a[var3].a == var2) {
               this.a[var3].b = var1.a().readUTF();
               this.a[var3].a = var1.a().readUTF();
               this.a[var3].c = var1.a().readUTF();
               this.a[var3].b = var1.a().readInt();
               this.a[var3].n = var1.a().readInt();
               this.a[var3].c = var1.a().readInt();
               this.a[var3].d = var1.a().readInt();
               this.a[var3].e = var1.a().readInt();
               this.a[var3].f = var1.a().readInt();
               this.a[var3].g = var1.a().readInt();
               this.a[var3].h = var1.a().readInt();
               this.a[var3].i = var1.a().readInt();
               this.a[var3].j = var1.a().readInt();
               this.a[var3].k = var1.a().readInt();
               this.a[var3].l = var1.a().readInt();
               this.a[var3].m = var1.a().readInt();
               this.a[var3].a = var1.a().readByte();
               this.a[var3].a(var1.a().readByte());
               this.a[var3].c = 1;
               break;
            }
         }

         this.a[0].n();
      } catch (Exception var4) {
         var4.printStackTrace();
      }
   }

   private void c(do var1) {
      if (this.b == var1 && this.b.c != null) {
         this.a = gv.a.a(this.b.c, BaseCanvas.w - 100 - 10);
      }
   }

   public final void d(en var1) {
      try {
         int var2 = var1.a().readInt();

         for(int var3 = 0; var3 < this.a.length; ++var3) {
            if (this.a[var3].a == var2) {
               this.a[var3].b = var1.a().readUTF();
               this.a[var3].a = var1.a().readUTF();
               this.a[var3].c = var1.a().readUTF();
               this.a[var3].b = var1.a().readInt();
               this.a[var3].n = var1.a().readInt();
               this.a[var3].c = var1.a().readInt();
               this.a[var3].d = var1.a().readInt();
               this.a[var3].e = var1.a().readInt();
               this.a[var3].f = var1.a().readInt();
               this.a[var3].g = var1.a().readInt();
               this.a[var3].h = var1.a().readInt();
               this.a[var3].i = var1.a().readInt();
               this.a[var3].j = var1.a().readInt();
               this.a[var3].k = var1.a().readInt();
               this.a[var3].l = var1.a().readInt();
               this.a[var3].m = var1.a().readInt();
               this.a[var3].a = var1.a().readByte();
               this.a[var3].a(var1.a().readByte());
               this.a[var3].c = 2;
               var1.a().readLong();
               this.a[var3].o = var1.a().readInt();
               this.a[var3].a = System.currentTimeMillis();
               this.c(this.a[var3]);
               break;
            }
         }

         this.a[0].n();
      } catch (Exception var4) {
         var4.printStackTrace();
      }
   }

   private static boolean b(int var0, int var1) {
      return var0 == var1 || var0 == var1 + 100;
   }

   public final void e(en param1) {
      // $FF: Couldn't be decompiled
   }
}
