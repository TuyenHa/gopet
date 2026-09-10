import java.io.IOException;
import java.util.Enumeration;
import java.util.Hashtable;
import java.util.Vector;
import javax.microedition.lcdui.Image;
import thong.sdk.ISoundManagerSDK;
import vn.me.core.BaseCanvas;

public final class fr extends ew {
   private dj a;
   private gh a;
   private gh b;
   private gh c;
   private gh d;
   private gh e;
   private gh f;
   private gh g;
   private gh h;
   private gh i;
   private gh j;
   private gh k;
   private gh l;
   private gh m;
   private gh n;
   private gh o;
   private gh p;
   private gh q;
   public dz a;
   public dz b;
   public dz c;
   public Vector b = new Vector();
   public Hashtable a;
   private n a;
   private n b;
   private n c;
   private go a;
   private cd b;
   private cd c;
   public int b;
   public int c;
   public static Image a;
   private boolean g = false;
   private int d;
   private long a;
   private gi a;
   public static boolean d = false;
   private int e;
   private int f;
   private int g;
   private int h;
   private long b;
   public static hb[] a = new hb[0];
   private static gy a = new gy(15, 15, 30, 30);

   public fr(dv var1) {
      super(var1);
      this.e = true;
      this.a = (dj)cq.a().a;
      this.a = new Hashtable();
      if (a == null) {
         try {
            a = Image.createImage("/pet/button/heal.png");
            return;
         } catch (IOException var2) {
            var2.printStackTrace();
         }
      }

   }

   public final void a(int var1, int var2, int var3, int var4) {
      this.e = var1;
      this.f = var2;
      this.g = var4;
      this.h = var3;
      this.b = System.currentTimeMillis();
   }

   public final di a(int var1) {
      return (di)this.a.get(new Integer(var1));
   }

   public final de a(int var1) {
      for(int var2 = 0; var2 < this.b.size(); ++var2) {
         de var3;
         if ((var3 = (de)this.b.elementAt(var2)).a == var1) {
            return var3;
         }
      }

      return null;
   }

   public final void c_() {
      super.c_();
      Enumeration var1 = this.a.elements();

      while(var1.hasMoreElements()) {
         di var2;
         (var2 = (di)var1.nextElement()).b();
         if (var2.b) {
            this.a.remove(new Integer(var2.a[0]));
         }
      }

      long var4 = System.currentTimeMillis();
      if (this.g && var4 - this.a >= (long)(this.d * 1000)) {
         this.g = false;
      }
   }

   public final void q() {
      this.b.b(this.a);
      this.l = this.b;
      this.n = this.c;
   }

   public final void a(int var1, int var2) {
      gy var3;
      if (el.a && (var3 = a).a <= var1 && var3.b <= var2 && var3.a + var3.a.a >= var1 && var3.b + var3.a.b >= var2) {
         el.a = false;
      } else {
         super.a(var1, var2);
      }
   }

   protected final void g() {
      Vector var1;
      (var1 = new Vector()).addElement(new cd(340, gw.a(80), this));
      var1.addElement(new cd(341, gw.a(87), this));
      var1.addElement(new cd(335, gw.a(92), this));
      var1.addElement(new cd(336, gw.a(90), this));
      var1.addElement(new cd(331, gw.a(93), this));
      var1.addElement(new cd(3286, gw.a(147), this));
      var1.addElement(new cd(330, gw.a(91), this));
      var1.addElement(new cd(317, gw.a(43), this));
      var1.addElement(new cd(327, gw.a(89), this));
      var1.addElement(new cd(0, a.a(267), this));
      var1.addElement(new cd(3281, gw.a(88), this));
      var1.addElement(new cd(10000, a.a(139), this));
      this.a(var1, 0);
   }

   public final void a(Object var1) {
      cd var2;
      switch ((var2 = (cd)((Object[])var1)[0]).a) {
         case 305:
            if (this.b == 0) {
               try {
                  this.d = new gh(this, Image.createImage("/pet/button/play.png"), new cd(307, gw.a(94), this));
                  this.e = new gh(this, Image.createImage("/pet/button/kiss.png"), new cd(308, gw.a(95), this));
                  this.f = new gh(this, Image.createImage("/pet/button/puke.png"), new cd(309, gw.a(96), this));
               } catch (IOException var5) {
                  var5.printStackTrace();
               }
            }

            gh[] var29 = new gh[]{this.d, this.e, this.f};
            this.k();
            this.a((gh[])var29);
            return;
         case 307:
            dc.a(1);
            ee var41 = dv.a;
            if (((df)dv.a).a != null) {
               this.a.addElement(new bi(this, this, 1, dv.a));
            }

            this.k();
            return;
         case 308:
            dc.a(0);
            ee var40 = dv.a;
            if (((df)dv.a).a != null) {
               this.a.addElement(new bi(this, this, 0, dv.a));
            }

            this.k();
            return;
         case 309:
            dc.a(2);
            ee var10000 = dv.a;
            if (((df)dv.a).a != null) {
               this.a.addElement(new bi(this, this, 2, dv.a));
            }

            this.k();
            return;
         case 310:
            dj.a = 0;
            this.k();
            dc.b(1);
            cg.f();
            return;
         case 311:
            this.k();
            cg.f();
            en var28;
            (var28 = new en(81)).a(28);
            cx.a.a(var28);
            var28.a();
            return;
         case 312:
            this.k();
            en var27;
            (var27 = new en(81)).a(30);
            cx.a.a(var27);
            var27.a();
            return;
         case 313:
            this.k();
            if (this.a != null) {
               dh var26;
               if ((var26 = ((df)this.a).a) == null) {
                  cg.a_(gw.a(98));
                  return;
               }

               cg.a(gw.a(99) + var26.e + gw.a(100) + var26.c + "?", new cd(314, gw.a(47), new Integer(this.a.c), this), new cd(315, gw.a(97), this));
               return;
            }

            return;
         case 314:
            int var25 = (Integer)var2.a;
            en var39;
            (var39 = new en(81)).a(12);
            var39.b(var25);
            cx.a.a(var39);
            var39.a();
         case 315:
            break;
         case 317:
            cg.f();
            en var24;
            (var24 = new en(81)).a(5);
            cx.a.a(var24);
            var24.a();
            return;
         case 318:
            if (this.b != 1) {
               this.b = 1;
               en var23;
               (var23 = new en(81)).a(37);
               var23.a(1);
               cx.a.a(var23);
               var23.a();
               return;
            }

            return;
         case 319:
            if (this.b != 2) {
               this.b = 2;
               en var22;
               (var22 = new en(81)).a(37);
               var22.a(2);
               cx.a.a(var22);
               var22.a();
               return;
            }

            return;
         case 320:
            this.A();
            return;
         case 321:
            if (this.b != 3) {
               this.b = 3;
               en var21;
               (var21 = new en(81)).a(37);
               var21.a(3);
               var21.b(0);
               cx.a.a(var21);
               var21.a();
               return;
            }

            return;
         case 322:
            int var20 = (Integer)var2.a;
            if (this.b != 4 && this.c != var20) {
               this.b = 4;
               this.c = var20;
               en var38;
               (var38 = new en(81)).a(37);
               var38.a(4);
               var38.b(var20);
               cx.a.a(var38);
               var38.a();
            }

            this.x();
            return;
         case 323:
            this.k();
            ((df)dv.a).a.a = true;
            dc.a(true);
            dv.a.c = true;
            return;
         case 324:
            cg.f();
            en var37;
            (var37 = new en(81)).a(54);
            cx.a.a(var37);
            var37.a();
            return;
         case 325:
            dj.a = 0;
            this.k();
            if (this.a != null) {
               dc.b(this.a.c, 0);
               cg.f();
               return;
            }

            return;
         case 326:
            this.k();
            if (this.a != null) {
               cg.f();
               dc.b(this.a.c, 1);
               return;
            }

            return;
         case 327:
            cg.a(gw.a(89), new String[]{gw.a(101), gw.a(102), gw.a(103)}, new int[]{2, 2, 2}, new cd(328, gw.a(51), this), cg.b);
            return;
         case 328:
            gi var19;
            String var36 = (var19 = (gi)fw.a).a(0);
            String var3 = var19.a(1);
            if (var19.a(2).equals(var3) && !"".equals(var3)) {
               this.x();
               cg.f();
               cx.c(var36, var3);
               return;
            }

            cg.a_(gw.a(104));
            return;
         case 330:
            cg.f();
            en var18;
            (var18 = new en(81)).a(62);
            cx.a.a(var18);
            var18.a();
            return;
         case 331:
            (new h()).d();
            return;
         case 332:
            cg.f();
            en var17;
            (var17 = new en(81)).a(90);
            var17.a(1);
            cx.a.a(var17);
            var17.a();
            this.k();
            return;
         case 333:
            this.k();
            if (this.a != null) {
               cg.f();
               int var16 = this.a.c;
               en var35;
               (var35 = new en(81)).a(91);
               var35.a(17);
               var35.b(var16);
               cx.a.a(var35);
               var35.a();
               return;
            }

            return;
         case 334:
            cg.f();
            en var15;
            (var15 = new en(81)).a(92);
            var15.a(1);
            cx.a.a(var15);
            var15.a();
            return;
         case 335:
            cg.f();
            en var34;
            (var34 = new en(81)).a(92);
            var34.a(2);
            cx.a.a(var34);
            var34.a();
            return;
         case 336:
            cg.f();
            en var14;
            (var14 = new en(81)).a(91);
            var14.a(20);
            cx.a.a(var14);
            var14.a();
            return;
         case 337:
            this.k();
            if (this.a != null) {
               cg.f();
               int var13 = this.a.c;
               en var33;
               (var33 = new en(81)).a(96);
               var33.b(var13);
               cx.a.a(var33);
               var33.a();
               return;
            }

            return;
         case 338:
            this.k();
            cg.f();
            dc.h(cg.a.d);
            return;
         case 339:
            this.k();
            if (this.a != null) {
               cg.f();
               dc.h(this.a.c);
               return;
            }

            return;
         case 340:
            Vector var12;
            (var12 = new Vector()).addElement(new cd(3401, gw.a(105), this));
            var12.addElement(new cd(3402, gw.a(80), this));
            var12.addElement(new cd(3403, gw.a(108), this));
            var12.addElement(new cd(3404, gw.a(107), this));
            this.a(var12, 0);
            return;
         case 341:
            Vector var32;
            (var32 = new Vector()).addElement(new cd(3411, gw.a(106), this));
            var32.addElement(new cd(3412, gw.a(87), this));
            this.a(var32, 0);
            return;
         case 342:
            cg.f();
            int var11 = this.a.c;
            en var31;
            (var31 = new en(121)).a(3);
            var31.b(var11);
            cx.a.a(var31);
            var31.a();
            return;
         case 3281:
            this.z();
            break;
         case 3282:
            ISoundManagerSDK.hasPermissionPlayBgSound = !ISoundManagerSDK.hasPermissionPlayBgSound;
            if (ISoundManagerSDK.currentSoundBg != null && !ISoundManagerSDK.hasPermissionPlayBgSound) {
               ISoundManagerSDK.currentSoundBg.stop();
            } else if (ISoundManagerSDK.currentSoundBg != null && ISoundManagerSDK.hasPermissionPlayBgSound) {
               ISoundManagerSDK.currentSoundBg.start();
            }

            ISoundManagerSDK.saveMusicState();
            this.z();
            break;
         case 3283:
            ISoundManagerSDK.hasPermissionPlayEffSound = !ISoundManagerSDK.hasPermissionPlayEffSound;
            ISoundManagerSDK.saveMusicState();
            this.z();
            break;
         case 3284:
            a.a(a.a, -99999);

            try {
               cx.a.b();
            } catch (Exception var4) {
            }

            (new fx(0)).a(0, true);
            return;
         case 3285:
            el.a = !el.a;
            break;
         case 3286:
            (new h()).d();
            h.a = 1;
            return;
         case 3401:
            cg.f();
            en var10;
            (var10 = new en(121)).a(1);
            cx.a.a(var10);
            var10.a();
            return;
         case 3402:
            this.a = gd.a(gw.a(80), new cd(34021, gw.a(6), this), cg.b);
            return;
         case 3403:
            cg.f();
            en var9;
            (var9 = new en(121)).a(2);
            cx.a.a(var9);
            var9.a();
            return;
         case 3404:
            cg.f();
            en var8;
            (var8 = new en(121)).a(10);
            cx.a.a(var8);
            var8.a();
            return;
         case 3411:
            cg.f();
            en var7;
            (var7 = new en(121)).a(13);
            cx.a.a(var7);
            var7.a();
            return;
         case 3412:
            (new m()).a();
            return;
         case 34021:
            cg.f();
            String var6 = this.a.a(0);
            this.a.j();
            en var30;
            (var30 = new en(121)).a(4);
            var30.a(var6);
            cx.a.a(var30);
            var30.a();
            return;
         default:
            super.a(var1);
            return;
      }

      fw.b(fw.a);
   }

   private void z() {
      Vector var1;
      (var1 = new Vector()).addElement(new cd(3282, gw.a(113) + (ISoundManagerSDK.hasPermissionPlayBgSound ? gw.a(111) : gw.a(112)), this));
      var1.addElement(new cd(3283, gw.a(114) + (ISoundManagerSDK.hasPermissionPlayEffSound ? gw.a(111) : gw.a(112)), this));
      var1.addElement(new cd(3284, gw.a(143), this));
      var1.addElement(new cd(3285, gw.a(146), this));
      this.a(var1, 0);
   }

   private void A() {
      dn var1;
      if ((var1 = this.a(dv.a.c).a[0].a).b.length == 0) {
         cg.c(gw.a(110));
      } else {
         bl var2 = new bl();
         cd[] var3 = new cd[var1.b.length];

         for(int var4 = 0; var4 < var1.a.length; ++var4) {
            var3[var4] = new cd(322, gw.a(6), new Integer(var1.a[var4]), this);
         }

         var2.a(var1.a, var1.a, var1.b, var1.b, var3);
         var2.a(true);
      }
   }

   protected final void h() {
      if (d && BaseCanvas.ticks % 50 > 10) {
         BaseCanvas.g.drawImage(cp.b, 1, 49, 0);
         cp.b.getWidth();
      }

      dp var1 = dj.a;
      cp.c().a(BaseCanvas.g, dj.b, 20, 2, 0);
      BaseCanvas.g.drawRegion(cp.c, dp.a * 14, 0, 14, 14, 0, 2, 2, 0);
      cp.c().a(BaseCanvas.g, dj.c, 20, 16, 0);
      BaseCanvas.g.drawImage(var1.a.a, 2, 18, 0);
      int var13 = 75;

      for(int var3 = 0; var3 < a.length; ++var3) {
         hb var4;
         if ((var4 = a[var3]).a == null) {
            var4.a = dj.a.a(var4.a);
         } else {
            BaseCanvas.g.drawImage(var4.a, 2, var13, 0);
            cp.c().a(BaseCanvas.g, var4.b, var4.a.getWidth() + 4, var13 + (var4.a.getHeight() / 2 - 7), 0);
            var13 += var4.a.getHeight() + 5;
         }
      }

      long var21;
      if ((var21 = System.currentTimeMillis()) - dp.a > 100L) {
         dp.a = (dp.a + 1) % 5;
         dp.a = var21;
      }

      long var5 = (System.currentTimeMillis() - this.b) / 1000L;
      var13 = this.e;
      if (this.h != 0 && var5 > (long)this.f) {
         int var2;
         var13 = var2 = (int)((long)(var13 + 1) + (var5 - (long)this.f) / (long)this.h);
         if (var2 > this.g) {
            var13 = this.g;
         }
      }

      int var19 = 2;
      cp.c().a(BaseCanvas.g, "(nluong)" + var13, 2, 34, 0);
      if (this.g) {
         BaseCanvas.g.drawImage(cp.k, 2, 52, 0);
         var19 = 19;
         cp.c().a(BaseCanvas.g, ed.b(((long)(this.d * 1000) - (System.currentTimeMillis() - this.a)) / 1000L), 19, 52, 0);
      }

      j();
      if (this.a.a != null && this.c) {
         var13 = dj.g > dj.f ? dj.g : dj.f;
         int var22 = -(cp.d().a(ed.a((long)var13)) << 1);
         BaseCanvas.g.drawImage(this.a.c, BaseCanvas.w - 63 + var22, 15, 17);
         cp.c().a(BaseCanvas.g, String.valueOf(this.a.a.c), BaseCanvas.w - 63 + var22, 22, 17);
         if (dj.a) {
            int var11 = dj.d;
            int var12 = dj.f;
            var19 = dj.e;
            dj.a(44, var11, var12, var19, dj.g, BaseCanvas.w - 48 + var22, 20, true);
         } else {
            int var27 = dj.b;
            int var29 = dj.f;
            var19 = dj.c;
            dj.a(44, var27, var29, var19, dj.g, BaseCanvas.w - 48 + var22, 20, true);
         }
      }

      Object var16;
      di var23;
      if (dv.a != null && BaseCanvas.w > 200 && (var16 = this.a.get(new Integer(dv.a.c))) != null && (var23 = (di)var16).a[0] == dv.a.c) {
         dn var30;
         var19 = (var30 = var23.a[1].a).k > var30.j ? var30.k : var30.j;
         int var17 = -(cp.d().a(ed.a((long)var19)) << 1 << 1) - 60;
         BaseCanvas.g.drawImage(this.a.d, BaseCanvas.w - 63 + var17, 15, 17);
         cp.c().a(BaseCanvas.g, "0", BaseCanvas.w - 63 + var17, 22, 17);
         int var24 = var30.h;
         if (var30.a) {
            var24 = var30.m;
         }

         int var26 = var30.j;
         var19 = var30.i;
         dj.a(44, var24, var26, var19, var30.k, BaseCanvas.w - 48 + var17, 20, true);
      }

      if (this.a.c) {
         int var18 = BaseCanvas.w - 44;
         Image var25;
         if ((var25 = dj.a.a(this.a.a)) != null) {
            long var28 = System.currentTimeMillis();
            if (this.a.b - (var28 - this.a.a) / 1000L >= 10L || var28 % 10L < 5L) {
               BaseCanvas.g.drawImage(var25, var18, 50, 0);
            }

            if (var19 <= 0) {
               this.a.c = false;
            }
         }
      }

      this.d();
      if (el.a) {
         BaseCanvas.g.drawRect(a.a, a.b, a.a.a, a.a.b);
      }

   }

   public final void m() {
      this.x();
      int var10000 = cg.a.d;
      String var3 = cg.a.b;
      this.a = true;
      if (this.a == null) {
         try {
            this.c = new gh(this, a, new cd(323, gw.a(115), this));
            this.a = new gh(this, Image.createImage("/pet/button/interact.png"), new cd(305, gw.a(116), this));
            this.g = new gh(this, Image.createImage("/pet/button/petinfor2.png"), new cd(310, gw.a(117), this));
            this.h = new gh(this, Image.createImage("/pet/button/equip.png"), new cd(311, gw.a(118), this));
            this.i = new gh(this, Image.createImage("/pet/button/inventory.png"), new cd(312, gw.a(119), this));
            this.b = new gh(this, Image.createImage("/pet/button/xam.png"), new cd(332, gw.a(120), this));
            this.o = new gh(this, Image.createImage("/pet/button/guildSkill.png"), new cd(338, gw.a(121), this));
         } catch (IOException var2) {
            var2.printStackTrace();
         }
      }

      this.a((gh[])(new gh[]{this.c, this.b, this.i, this.g, this.h, this.o, this.a}));
   }

   public final void n() {
      this.x();
      this.a = true;
      if (this.j == null) {
         this.j = new gh(this, this.a.a, new cd(313, a.a(667), this));
         Image var1 = null;
         Image var2 = null;
         Image var3 = null;
         Image var4 = null;
         Image var6 = null;

         try {
            var1 = Image.createImage("/pet/button/petinfor2.png");
            var2 = Image.createImage("/pet/button/equip.png");
            var3 = Image.createImage("/pet/button/guild.png");
            var4 = Image.createImage("/pet/button/guildSkill.png");
            var6 = Image.createImage("/pet/button/ketban.png");
         } catch (IOException var7) {
            var7.printStackTrace();
         }

         this.l = new gh(this, var1, new cd(325, gw.a(123), this));
         this.m = new gh(this, var2, new cd(326, gw.a(124), this));
         this.n = new gh(this, var3, new cd(333, gw.a(58), this));
         this.k = new gh(this, this.a.a, new cd(337, gw.a(122), this));
         this.p = new gh(this, var4, new cd(339, gw.a(121), this));
         this.q = new gh(this, var6, new cd(342, gw.a(80), this));
      }

      this.a((gh[])(new gh[]{this.q, this.k, this.j, this.l, this.m, this.n, this.p}));
   }

   public final void a(en var1) {
      try {
         int var2 = var1.a().readInt();
         byte var4 = var1.a().readByte();
         df var5;
         if ((var5 = (df)this.a.a(var2)) != null && var5.a != null) {
            this.a.addElement(new bi(this, this, var4, var5));
         }
      } catch (Exception var3) {
         var3.printStackTrace();
      }
   }

   public final void r() {
      if (this.a == null) {
         Image var1 = null;
         Image var2 = null;
         Image var4 = null;

         try {
            var1 = Image.createImage("/pet/battle/attack.png");
            var2 = Image.createImage("/pet/battle/skill.png");
            var4 = Image.createImage("/pet/battle/potion.png");
         } catch (IOException var5) {
            var5.printStackTrace();
         }

         this.a = new ga(var1);
         this.b = new ga(var2);
         this.c = new ga(var4);
         int var3 = var1.getWidth();
         int var6;
         int var7 = ((var6 = BaseCanvas.w - 60 - 2) - (var3 << 2)) / 8;
         this.a.a(20, 0, 16, 16);
         int var8 = var3 + 20 + var7;
         this.b.a(var8, 0, 16, 16);
         this.c.a(var8 + var3 + var7, 0, 16, 16);
         this.a.d = new cd(318, gw.a(55), this);
         this.b.d = new cd(320, gw.a(125), this);
         this.c.d = new cd(321, gw.a(126), this);
         this.a = new go(60, BaseCanvas.h - 37, var6, 16);
         this.a.c = 4;
         this.a.h = true;
         this.a.k = true;
         this.a.b(2);
         this.a.a(this.a);
         this.a.a(this.b);
         this.a.a(this.c);
      }

      this.b.b(false);
      this.c.b(false);
      this.a.n();
      this.b.a(this.a);
      this.b = this.l;
      this.c = this.n;
      this.l = null;
      this.n = null;
      this.b = -1;
      this.c = -1;
   }

   protected final void i_() {
      Vector var1 = new Vector();
      if (this.a.a != 12) {
         var1.addElement(new cd(gw.a(127), new fs(this)));
         var1.addElement(new cd(gw.a(128), new ft(this)));
         var1.addElement(cg.k);
         var1.addElement(cg.j);
         var1.addElement(new cd(324, gw.a(130), this));
         var1.addElement(cg.i);
         var1.addElement(this.a);
      } else {
         var1.addElement(new cd(41, gw.a(129), cg.a));
         var1.addElement(cg.j);
         var1.addElement(cg.i);
         var1.addElement(this.a);
      }

      this.a(var1, 1);
   }

   public final void b(int var1) {
      if (cp.k == null) {
         try {
            cp.k = Image.createImage("/clock.png");
         } catch (IOException var3) {
            var3.printStackTrace();
         }
      }

      this.d = var1;
      this.a = System.currentTimeMillis();
      this.g = true;
   }
}
