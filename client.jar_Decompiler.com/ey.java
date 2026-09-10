import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class ey extends fj {
   public int a;
   private int c;
   private String a;
   private boolean a;
   private String b;
   private byte b;
   private byte c;
   private Image a;
   private String e;
   private Object a;

   public ey() {
      this.c = "SCREEN_GUILD";
      this.d = gw.a(58);
      this.f = true;
      this.a = cp.c();
      this.n = cg.e;
   }

   private bt a() {
      gn var1;
      return (var1 = this.a.a(true)) != null ? (bt)var1 : null;
   }

   public final void b() {
      super.b();
      if ((this.a & 2) != 0) {
         gs.a(10, 20, BaseCanvas.w - 20, 22);
         dj.a(gw.a(59), 2, BaseCanvas.w >> 1);
         int var6 = 50;
         gs.a(10, 50, BaseCanvas.w - 20, BaseCanvas.h - 50 - 25);
         String[] var8 = (String[])this.a;

         for(int var9 = 0; var9 < var8.length; ++var9) {
            String var4 = var8[var9];
            var6 += gv.a.b + 4;
            gv.a.a(BaseCanvas.g, "(str) " + var4, 20, var6, 0);
         }

      } else {
         if ((this.a & 32) != 0) {
            Object[] var1;
            String var2 = (var1 = this.a)[0].toString();
            String[] var3 = var1[1] != null ? (String[])var1[1] : null;
            BaseCanvas.g.setColor(0);
            BaseCanvas.g.fillRect(0, 0, BaseCanvas.w, BaseCanvas.h);
            gs.a(10, 20, BaseCanvas.w - 20, 22);
            dj.a(var2, 2, BaseCanvas.w >> 1);
            gs.a(10, 42, 64, 80);
            gs.a(74, 42, BaseCanvas.w - 20 - 64, 80);
            gv.a.a(BaseCanvas.g, var3 == null ? "(str)Điểm vinh dự: 0" : var3[0], 90, 50, 0);
            gv.a.a(BaseCanvas.g, var3 == null ? "(str)Đóng đóng góp: 0" : var3[1], 90, 68, 0);
            gs.a(10, 122, BaseCanvas.w - 20, BaseCanvas.h - 122 - 5 - gs.m);
            if (this.a == null) {
               this.a = cp.a(this.e, (byte)3);
            }

            int var5 = 118;
            if (this.a != null) {
               BaseCanvas.g.drawImage(this.a, 42, 115, 33);
            }

            if (var3 == null) {
               gv.a.a(BaseCanvas.g, "(str) Chưa vào bang hội.", 20, 118 + gv.a.b + 4, 0);
               return;
            }

            for(int var7 = 2; var7 < var3.length; ++var7) {
               var5 += gv.a.b + 4;
               gv.a.a(BaseCanvas.g, var3[var7], 20, var5, 0);
            }
         }

      }
   }

   public final void a(Object var1) {
      cd var5;
      switch ((var5 = (cd)((Object[])var1)[0]).a) {
         case 1:
            Vector var6;
            (var6 = new Vector()).addElement(new cd(3, gw.a(59), this));
            var6.addElement(new cd(4, "Thông tin bang chúng", this));
            var6.addElement(new cd(5, "Top nộp quỹ", this));
            var6.addElement(new cd(6, "Top điểm phát triển", this));
            this.a(var6, 0);
            return;
         case 2:
         default:
            if ((this.a & 1) != 0) {
               switch (var5.a) {
                  case 2:
                     bt var18;
                     if (this.a && !((dd)var5.a).a.trim().toUpperCase().equals(dv.a.c) && (var18 = this.a()) != null) {
                        cd var23 = new cd(9, gw.a(60), this);
                        Vector var31 = new Vector();
                        var23.a = var18.a;
                        var31.addElement(var23);
                        this.a(var31, 2);
                        return;
                     }

                     return;
                  case 9:
                     if (var5.a != null) {
                        gd.k();
                        int var17 = ((dd)var5.a).a;
                        en var34;
                        (var34 = new en(81)).a(91);
                        var34.a(6);
                        var34.b(var17);
                        cx.a.a(var34);
                        var34.a();
                        return;
                     }

                     return;
                  case 13:
                     if (this.b > 1) {
                        gd.k();
                        dc.a(this.b - 1, false);
                        return;
                     }

                     return;
                  case 14:
                     if (this.b < this.c) {
                        gd.k();
                        dc.a(this.b + 1, false);
                        return;
                     }

                     return;
                  default:
                     return;
               }
            } else if ((this.a & 4) != 0) {
               switch (var5.a) {
                  case 2:
                     bt var16;
                     if (this.b == null && (var16 = this.a()) != null && var16.a != null) {
                        cd var22;
                        (var22 = new cd(7, gw.a(61), this)).a = var16.a;
                        gd.b("Bạn muốn gia nhập: " + ((dd)var16.a).a, var22, cg.b);
                        return;
                     }

                     return;
                  case 3:
                  case 4:
                  case 5:
                  case 6:
                  case 8:
                  case 9:
                  case 10:
                  case 11:
                  case 12:
                  default:
                     return;
                  case 7:
                     if (var5.a == null) {
                        gd.a("Không tìm thấy bang hội", true);
                        return;
                     }

                     gd.k();
                     int var30 = ((dd)var5.a).a;
                     en var15;
                     (var15 = new en(81)).a(91);
                     var15.a(2);
                     var15.b(var30);
                     cx.a.a(var15);
                     var15.a();
                     return;
                  case 13:
                     if (this.b > 1) {
                        gd.k();
                        dc.f(this.b - 1);
                        return;
                     }

                     return;
                  case 14:
                     if (this.b < this.c) {
                        gd.k();
                        dc.f(this.b + 1);
                        return;
                     }

                     return;
                  case 15:
                     gd.a(gw.a(140), new cd(16, gw.a(6), this), cg.b);
                     return;
                  case 16:
                     if (fw.a != null && fw.a instanceof gi) {
                        String var33;
                        if ((var33 = ((gi)fw.a).a(0)).length() != 0) {
                           en var14;
                           (var14 = new en(81)).a(91);
                           var14.a(13);
                           var14.a(var33);
                           cx.a.a(var14);
                           var14.a();
                           return;
                        }

                        return;
                     }

                     return;
               }
            } else if ((this.a & 8) != 0) {
               switch (var5.a) {
                  case 2:
                     bt var21;
                     if ((var21 = this.a()) != null) {
                        Vector var29 = new Vector();
                        var5 = new cd(7, a.a(4), this);
                        cd var32 = new cd(8, a.a(101), this);
                        var5.a = var21.a;
                        var32.a = var21.a;
                        var29.addElement(var5);
                        var29.addElement(var32);
                        this.a(var29, 2);
                        return;
                     }

                     return;
                  case 7:
                     if (var5.a != null) {
                        gd.k();
                        int var28 = ((dd)var5.a).a;
                        en var12;
                        (var12 = new en(81)).a(91);
                        var12.a(5);
                        var12.b(var28);
                        var12.a(true);
                        cx.a.a(var12);
                        var12.a();
                        return;
                     }

                     return;
                  case 8:
                     if (var5.a != null) {
                        gd.k();
                        int var27 = ((dd)var5.a).a;
                        en var11;
                        (var11 = new en(81)).a(91);
                        var11.a(5);
                        var11.b(var27);
                        var11.a(false);
                        cx.a.a(var11);
                        var11.a();
                        return;
                     }

                     return;
                  default:
                     return;
               }
            } else if ((this.a & 16) != 0) {
               switch (var5.a) {
                  case 2:
                     bt var20;
                     if ((var20 = this.a()) != null) {
                        dd var26 = (dd)var20.a;
                        (var5 = new cd(8, gw.a(6), this)).a = var26;
                        gd.a(gw.a(141) + var26.a, (cd)null, var5, cg.b, true);
                        return;
                     }

                     return;
                  case 7:
                     this.t();
                     return;
                  case 8:
                     if (var5.a != null) {
                        int var25 = ((dd)var5.a).a;
                        en var9;
                        (var9 = new en(81)).a(91);
                        var9.a(10);
                        var9.b(var25);
                        cx.a.a(var9);
                        var9.a();
                        this.t();
                        return;
                     }

                     return;
                  default:
                     return;
               }
            } else if ((this.a & 64) != 0) {
               switch (var5.a) {
                  case 13:
                     if (this.b > 1) {
                        gd.k();
                        dc.c(this.c, this.b - 1);
                        return;
                     }

                     return;
                  case 14:
                     if (this.b < this.c) {
                        gd.k();
                        dc.c(this.c, this.b + 1);
                        return;
                     }

                     return;
                  default:
                     return;
               }
            } else if ((this.a & 128) != 0) {
               switch (var5.a) {
                  case 13:
                     if (this.b > 1) {
                        gd.k();
                        dc.d(this.c, this.b - 1);
                        return;
                     }

                     return;
                  case 14:
                     if (this.b < this.c) {
                        gd.k();
                        dc.d(this.c, this.b + 1);
                        return;
                     }

                     return;
                  default:
                     return;
               }
            } else {
               if ((this.a & 256) != 0) {
                  switch (var5.a) {
                     case 2:
                        if (this.a) {
                           bt var19 = this.a();
                           cd var24 = new cd(7, gw.a(6), this);
                           dd var8 = (dd)var19.a;
                           var24.a = var8;
                           gd.b("Bạn chắc chắn muốn nhường quyền bang chủ cho " + var8.a, var24, cg.b);
                           return;
                        }

                        return;
                     case 7:
                        if (var5.a != null) {
                           dd var2 = (dd)var5.a;
                           int var3 = this.c;
                           int var7 = var2.a;
                           en var4;
                           (var4 = new en(81)).a(91);
                           var4.a(12);
                           var4.b(var3);
                           var4.b(var7);
                           cx.a.a(var4);
                           var4.a();
                           return;
                        }

                        return;
                     case 13:
                        if (this.b > 1) {
                           gd.k();
                           dc.a(this.b - 1, true);
                           return;
                        }

                        return;
                     case 14:
                        if (this.b < this.c) {
                           gd.k();
                           dc.a(this.b + 1, true);
                           return;
                        }

                        return;
                     default:
                        return;
                  }
               }

               return;
            }
         case 3:
            gd.k();
            dc.a();
            return;
         case 4:
            gd.k();
            dc.a(1, false);
            return;
         case 5:
            gd.k();
            dc.d(this.c, 1);
            return;
         case 6:
            gd.k();
            dc.c(this.c, 1);
      }
   }

   public final void a(byte var1, byte var2, int var3, boolean var4, String var5, Vector var6, boolean var7) {
      if (var7) {
         this.a = 256;
      } else {
         this.a = 1;
      }

      this.c(this.a);
      this.c = var3;
      this.a = var4;
      this.a = var5;
      this.b = var5;
      this.b = var1;
      this.c = var2;
      this.c(var6);
      this.f();
      this.l = new cd(1, "menu", this);
      this.d = this.a;
      if (this.a.b() > 0) {
         this.a.a(0).n();
      }

   }

   public final void a(int var1, String[] var2) {
      this.a = 2;
      this.c = var1;
      this.c(this.a);
      this.a = var2;
      this.d = null;
      this.l = new cd(1, "menu", this);
      this.m = null;
      this.n = cg.e;
   }

   public final void a(Vector var1) {
      this.a = 8;
      this.c(this.a);
      this.c(var1);
      if (this.a.b() > 0) {
         this.a.a(0).n();
      }

      this.n = cg.e;
      this.l = null;
   }

   public final void a(String var1, Vector var2, byte var3, byte var4) {
      this.a = 4;
      this.c(this.a);
      this.b = var1;
      this.a = false;
      this.a = null;
      this.l = null;
      this.b = var3;
      this.c = var4;
      this.c(var2);
      this.f();
      if (this.a.b() > 0) {
         this.a.a(0).n();
      }

      this.l = new cd(15, gw.a(142), this);
   }

   public final void a(byte var1, byte var2, Vector var3) {
      this.a = 64;
      this.c(this.a);
      this.d = "Top điểm phát triển";
      this.b = var1;
      this.c = var2;
      this.c(var3);
      this.f();
      this.l = new cd(1, "menu", this);
      this.m = null;
      this.n = cg.e;
      if (this.a.b() > 0) {
         this.a.a(0).n();
      }

   }

   public final void b(byte var1, byte var2, Vector var3) {
      this.a = 128;
      this.c(this.a);
      this.d = "Top nộp quỹ";
      this.b = var1;
      this.c = var2;
      this.c(var3);
      this.f();
      this.l = new cd(1, "menu", this);
      this.m = null;
      this.n = cg.e;
      if (this.a.b() > 0) {
         this.a.a(0).n();
      }

   }

   public final void b(Vector var1) {
      this.a = 16;
      this.c(this.a);
      this.c(var1);
      this.n = new cd(7, gw.a(0), this);
      this.l = null;
      this.m = null;
      if (this.a.b() > 0) {
         this.a.a(0).n();
      }

   }

   public final void a(boolean var1, int var2, String var3, String var4, Object var5) {
      this.a = 32;
      this.c(this.a);
      this.d = null;
      this.e = var4;
      this.a = new Object[]{var3, var5};
      if (var1) {
         this.l = new cd("Mời gia nhập", new ez(this));
         this.l.a = new Integer(var2);
      } else {
         this.l = null;
      }

      this.n = cg.e;
   }

   private void f() {
      go var1 = new go(0, 0, BaseCanvas.w, gs.m);
      if (this.b > 1) {
         gb var2;
         (var2 = new gb("<<<")).q = 17;
         var2.a(gs.p, gs.p, 30, gs.m);
         var2.d = new cd(13, gw.a(139), this);
         var1.a(var2);
      }

      if (this.b < this.c) {
         gb var3;
         (var3 = new gb(">>>")).q = 17;
         var3.a(BaseCanvas.w - 30 - gs.p, gs.p, 30, gs.m);
         var3.d = new cd(14, gw.a(138), this);
         var1.a(var3);
      }

      this.a.a(var1);
      this.a.b(1);
   }

   public final void a(int var1) {
      int var2 = this.a.b();

      gn var3;
      do {
         --var2;
         if (var2 < 0) {
            return;
         }
      } while((var3 = this.a.a(var2)) == null || !(var3 instanceof bt) || ((dd)((bt)var3).a).a != var1);

      this.a.b(var3);
      this.a.b(1);
      if (this.a.b() > 0) {
         this.a.a(0).n();
      }
   }
}
