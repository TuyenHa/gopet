import java.io.IOException;
import java.util.Vector;
import javax.microedition.lcdui.Image;
import vn.me.core.BaseCanvas;

public final class dj implements ak, bc, gz {
   private boolean d = false;
   public Image a;
   public dh a;
   public Image b;
   public Image c;
   public Image d;
   public static Image e;
   public static dp a;
   public static bg a;
   public static int a;
   public static dx a;
   public static int b;
   public static int c;
   public static boolean a;
   public static int d;
   public static int e;
   private static int h;
   private static int i;
   public static boolean b;
   public long a;
   public long b;
   public boolean c;
   public String a;
   public static long c = -1L;
   public static long d = -1L;
   private static long e = 0L;
   public static String b = "";
   public static String c = "";
   public static String d = "";
   public static int f = 100;
   public static int g = 100;

   public dj() {
      a = new dp(this);
      a = new bg();

      try {
         this.a = Image.createImage("/pet/Gem.png");
         e = Image.createImage("/pet/arrow2.png");
         this.b = Image.createImage("/pet/tiemnang.png");
         gu.a("/common.dat", 25);
         this.c = Image.createImage("/pet/level.png");
         this.d = Image.createImage("/pet/level_red.png");
      } catch (IOException var3) {
         var3.printStackTrace();
      }

      gg.a = new String[]{"(ngoc)", "(dau)", "(thoc)", "(vang)", "(str)", "(agi)", "(int)", "(atk)", "(def)", "(hp)", "(mp)", "(water)", "(thunder)", "(rock)", "(fire)", "(dark)", "(tree)", "(light)", "(sao)", "(chien)", "(bthu)", "(codo)", "(coxanh)", "(nha)", "(nguoi)", "(saoden)", "(chienluc)", "(nluong)", "(diem)", "(lua)"};

      try {
         gg.a = Image.createImage("/pet/icons.png");
      } catch (IOException var2) {
         var2.printStackTrace();
      }

      (a = new dx()).a = bg.a("/pet/battle/SlashEffect");
      a.a = bg.a("/pet/battle/gong")[0];
      a.b = bg.a("/pet/battle/attackEffect")[0];
   }

   private static void a(int var0, int var1, int var2, int var3) {
      f = var1;
      g = var3;
      dh var4 = ((df)dv.a).a;
      if (var0 != b) {
         a = true;
         d = b;
         h = var0;
         if (var4 != null) {
            var4.a(String.valueOf(var0 - b), 0, System.currentTimeMillis() + 100L);
         }
      }

      if (var2 != c) {
         a = true;
         e = c;
         i = var2;
         if (var4 != null) {
            var4.a(String.valueOf(var2 - c), 1, System.currentTimeMillis() + 600L);
         }
      }

   }

   private static void a(long var0, long var2, long var4) {
      c = var0;
      b = ed.a(var0);
      d = var2;
      c = ed.a(var2);
      ed.a(var4);
   }

   public static void a(String var0, int var1, int var2) {
      String var3 = null;
      switch (var1) {
         case 0:
         case 3:
            var3 = "(str) ";
            break;
         case 1:
         case 4:
            var3 = "(agi) ";
            break;
         case 2:
         case 5:
            var3 = "(int) ";
      }

      var0 = (var3 + var0).trim();
      cp.c().a(BaseCanvas.g, var0, var2, 22, 17);
   }

   private static gp[] a(en var0) {
      int var1;
      gp[] var2 = new gp[var1 = var0.a().readInt()];

      for(int var3 = 0; var3 < var1; ++var3) {
         var2[var3] = new gp(var0.a().readByte(), var0.a().readUTF(), var0.a().readShort(), var0.a().readShort(), var0.a().readBoolean(), var0.a().readBoolean(), var0.a().readByte());
      }

      return var2;
   }

   public final void a(en param1) {
      // $FF: Couldn't be decompiled
   }

   private static es a() {
      es var0 = null;
      fw var1;
      if (!((var1 = BaseCanvas.getCurrentScreen()) instanceof es)) {
         for(int var2 = 0; var2 < var1.a.size(); ++var2) {
            fw var3;
            if ((var3 = (fw)var1.a.elementAt(var2)) instanceof es) {
               var0 = (es)var3;
               break;
            }
         }
      } else {
         var0 = (es)var1;
      }

      return var0;
   }

   private static fr a() {
      fr var0 = null;
      fw var1;
      if (!((var1 = BaseCanvas.getCurrentScreen()) instanceof fr)) {
         for(int var2 = 0; var2 < var1.a.size(); ++var2) {
            fw var3;
            if ((var3 = (fw)var1.a.elementAt(var2)) instanceof fr) {
               var0 = (fr)var3;
               break;
            }
         }
      } else {
         var0 = (fr)var1;
      }

      return var0;
   }

   private static dn a(en var0) {
      dn var1 = new dn();

      try {
         var1.a = var0.a().readInt();
         var1.a = var0.a().readUTF();
         var1.c = var0.a().readByte();
         var1.a = var0.a().readShort();
         var1.b = var0.a().readUTF();
         var1.b = var0.a().readInt();
         var1.h = var0.a().readInt();
         var1.i = var0.a().readInt();
         var1.j = var0.a().readInt();
         var1.k = var0.a().readInt();
         byte var2 = var0.a().readByte();
         var1.a = new int[var2];
         var1.a = new String[var2];

         for(int var3 = 0; var3 < var2; ++var3) {
            var1.a[var3] = var0.a().readInt();
            var1.a[var3] = var0.a().readUTF();
         }
      } catch (Exception var4) {
         var4.printStackTrace();
      }

      return var1;
   }

   private static dn b(en var0) {
      dn var1 = new dn();

      try {
         var1.a = var0.a().readInt();
         var1.a = var0.a().readUTF();
         var1.c = var0.a().readByte();
         var1.a = var0.a().readShort();
         var1.b = var0.a().readUTF();
         var1.b = var0.a().readInt();
         var1.c = var0.a().readInt();
         var1.d = var0.a().readInt();
         var1.e = var0.a().readInt();
         var1.f = var0.a().readInt();
         var1.g = var0.a().readInt();
         var1.h = var0.a().readInt();
         var1.i = var0.a().readInt();
         var1.j = var0.a().readInt();
         var1.k = var0.a().readInt();
         byte var2 = var0.a().readByte();
         var1.b = new String[var2];
         var1.a = new String[var2];
         var1.a = new int[var2];
         var1.b = new int[var2];

         for(int var3 = 0; var3 < var2; ++var3) {
            var1.a[var3] = var0.a().readInt();
            var1.a[var3] = var0.a().readUTF();
            var1.b[var3] = var0.a().readUTF();
            var1.b[var3] = var0.a().readInt();
         }
      } catch (Exception var4) {
         var4.printStackTrace();
      }

      return var1;
   }

   public final void a(Object var1) {
      cd var2;
      Object[] var4;
      switch ((var2 = (cd)((Object[])(var4 = var1))[0]).a) {
         case 0:
            de var7 = (de)var2.a;
            Boolean var3 = Boolean.TRUE;
            if (((Object[])var4).length > 2) {
               var3 = (Boolean)((Object[])var4)[2];
            }

            if (var3.equals(Boolean.TRUE)) {
               (var4 = new d(var7.i, var7.j - 20, true)).a();
               fw.e.addElement(var4);
            }

            int var6 = var7.a;
            en var8;
            (var8 = new en(81)).a(36);
            var8.b(var6);
            cx.a.a(var8);
            var8.a();
            return;
         default:
      }
   }

   public static void a(int var0, int var1, int var2, int var3, int var4, int var5) {
      a(20, var0, var1, var2, var3, var4, var5, false);
   }

   public static void a(int var0, int var1, int var2, int var3, int var4, int var5, int var6, boolean var7) {
      BaseCanvas.g.translate(var5, var6);
      if (var7) {
         cp.d().a(BaseCanvas.g, ed.a((long)var1) + "/" + ed.a((long)var2), 0, 0, 0);
         BaseCanvas.g.translate(0, 9);
      }

      BaseCanvas.g.setColor(3691038);
      BaseCanvas.g.fillRect(0, 0, var0, 4);
      var1 = var1 * (var0 - 2) / var2;
      BaseCanvas.g.setColor(9830022);
      BaseCanvas.g.fillRect(1, 1, var1, 1);
      BaseCanvas.g.setColor(2021890);
      BaseCanvas.g.fillRect(1, 2, var1, 1);
      BaseCanvas.g.translate(0, 5);
      if (var7) {
         cp.d().a(BaseCanvas.g, ed.a((long)var3) + "/" + ed.a((long)var4), 0, 0, 0);
         BaseCanvas.g.translate(0, 9);
      }

      BaseCanvas.g.setColor(75607);
      BaseCanvas.g.fillRect(0, 0, var0, 4);
      var0 = var3 * (var0 - 2) / var4;
      BaseCanvas.g.setColor(6455253);
      BaseCanvas.g.fillRect(1, 1, var0, 1);
      BaseCanvas.g.setColor(2836897);
      BaseCanvas.g.fillRect(1, 2, var0, 1);
      if (var7) {
         BaseCanvas.g.translate(-var5, -var6 - 23);
      } else {
         BaseCanvas.g.translate(-var5, -var6 - 5);
      }
   }

   public final void a() {
      if (a) {
         boolean var1;
         boolean var2 = var1 = ed.a(h - d) < 2;
         if (var1) {
            b = h;
            var2 = true;
         } else {
            d += h - d >> 1;
         }

         boolean var3 = var1 = ed.a(i - e) < 2;
         if (var1) {
            c = i;
            var3 = true;
         } else {
            e += i - e >> 1;
         }

         if (var2 && var3) {
            a = false;
         }
      }

   }

   private void a(int var1, en var2) {
      try {
         switch (var1) {
            case 1:
               String var104 = var2.a().readUTF();
               byte var115 = var2.a().readByte();
               byte var125 = var2.a().readByte();
               var1 = var2.a().readInt();
               Vector var130 = new Vector();
               int var134 = var104.length() == 0 ? 1 : 0;

               for(int var139 = 0; var139 < var1; ++var139) {
                  int var141 = var2.a().readInt();
                  var2.a().readInt();
                  var130.addElement(new dd(var141, (String)null, var2.a().readUTF(), var2.a().readUTF(), (byte)var134));
               }

               boolean var140 = false;
               ey var93;
               if (BaseCanvas.currentScreen != null && "SCREEN_GUILD".equals(BaseCanvas.currentScreen.c)) {
                  var93 = (ey)BaseCanvas.currentScreen;
               } else {
                  var93 = new ey();
                  var140 = true;
               }

               var93.a(var104.length() == 0 ? null : var104, var130, var115, var125);
               var93.a(0, var140);
               return;
            case 2:
               return;
            case 3:
               int var9 = var2.a().readInt();
               boolean var103 = var2.a().readBoolean();
               var2.a().readInt();
               String var114 = var2.a().readUTF();
               var2.a().readInt();
               var2.a().readInt();
               byte var124 = var2.a().readByte();
               byte var129 = var2.a().readByte();
               var1 = var2.a().readByte();
               Vector var133 = new Vector();
               int var138 = var103 ? 1 : 0;

               for(int var10 = 0; var10 < var1; ++var10) {
                  int var11 = var2.a().readInt();
                  String var144 = var2.a().readUTF();
                  var133.addElement(new dd(var11, var144.length() == 0 ? null : var144, var2.a().readUTF(), var2.a().readUTF(), (byte)var138));
               }

               boolean var142 = var2.a().readBoolean();
               boolean var143 = false;
               ey var91;
               if (BaseCanvas.currentScreen != null && "SCREEN_GUILD".equals(BaseCanvas.currentScreen.c)) {
                  var91 = (ey)BaseCanvas.currentScreen;
               } else {
                  var91 = new ey();
                  var143 = true;
               }

               var91.a(var124, var129, var9, var103, var114, var133, var142);
               var91.a(0, var143);
               return;
            case 4:
               byte var12 = var2.a().readByte();
               Vector var102 = new Vector();

               for(int var112 = 0; var112 < var12; ++var112) {
                  var1 = var2.a().readInt();
                  String var123 = var2.a().readUTF();
                  var102.addElement(new dd(var1, var123.length() == 0 ? null : var123, var2.a().readUTF(), var2.a().readUTF(), (byte)1));
               }

               boolean var113 = false;
               ey var89;
               if (BaseCanvas.currentScreen != null && "SCREEN_GUILD".equals(BaseCanvas.currentScreen.c)) {
                  var89 = (ey)BaseCanvas.currentScreen;
               } else {
                  var89 = new ey();
                  var113 = true;
               }

               var89.a(var102);
               var89.a(0, var113);
               return;
            case 5:
               var1 = var2.a().readInt();
               boolean var122 = var2.a().readBoolean();
               gd.l();
               if (var122 && BaseCanvas.currentScreen != null && "SCREEN_GUILD".equals(BaseCanvas.currentScreen.c)) {
                  ((ey)BaseCanvas.currentScreen).a(var1);
                  return;
               }

               return;
            case 6:
               var1 = var2.a().readInt();
               boolean var94 = var2.a().readBoolean();
               gd.l();
               if (var94 && BaseCanvas.currentScreen != null && "SCREEN_GUILD".equals(BaseCanvas.currentScreen.c)) {
                  ((ey)BaseCanvas.currentScreen).a(var1);
                  return;
               }

               return;
            case 7:
               return;
            case 8:
               return;
            case 9:
               var1 = var2.a().readInt();
               Vector var101 = new Vector();

               for(int var110 = 0; var110 < var1; ++var110) {
                  var101.addElement(new dd(var2.a().readInt(), (String)null, var2.a().readUTF(), "", (byte)1));
               }

               if (BaseCanvas.currentScreen != null && "SCREEN_GUILD".equals(BaseCanvas.currentScreen.c) && ((ey)BaseCanvas.currentScreen).a == 16) {
                  return;
               }

               ey var111;
               (var111 = new ey()).b(var101);
               var111.a(0, true);
               return;
            case 10:
            case 11:
            case 13:
            case 18:
            case 21:
            default:
               return;
            case 12:
               return;
            case 14:
               int var100 = var2.a().readInt();
               String[] var109 = new String[var1 = var2.a().readByte()];

               for(int var120 = 0; var120 < var1; ++var120) {
                  var109[var120] = var2.a().readUTF();
               }

               boolean var121 = false;
               ey var84;
               if (BaseCanvas.currentScreen != null && "SCREEN_GUILD".equals(BaseCanvas.currentScreen.c)) {
                  var84 = (ey)BaseCanvas.currentScreen;
               } else {
                  var84 = new ey();
                  var121 = true;
               }

               var84.a(var100, var109);
               var84.a(1, var121);
               return;
            case 15:
               byte var99 = var2.a().readByte();
               byte var108 = var2.a().readByte();
               var1 = var2.a().readByte();
               Vector var119 = new Vector();

               for(int var127 = 0; var127 < var1; ++var127) {
                  int var132 = var2.a().readInt();
                  String var137 = var2.a().readUTF();
                  var119.addElement(new dd(var132, var137.length() == 0 ? null : var137, var2.a().readUTF(), var2.a().readUTF(), (byte)0));
               }

               boolean var128 = false;
               ey var82;
               if (BaseCanvas.currentScreen != null && "SCREEN_GUILD".equals(BaseCanvas.currentScreen.c)) {
                  var82 = (ey)BaseCanvas.currentScreen;
               } else {
                  var82 = new ey();
                  var128 = true;
               }

               var82.a(var99, var108, var119);
               var82.a(0, var128);
               return;
            case 16:
               byte var131 = var2.a().readByte();
               byte var136 = var2.a().readByte();
               var1 = var2.a().readByte();
               Vector var98 = new Vector();

               for(int var106 = 0; var106 < var1; ++var106) {
                  int var118 = var2.a().readInt();
                  String var126 = var2.a().readUTF();
                  var98.addElement(new dd(var118, var126.length() == 0 ? null : var126, var2.a().readUTF(), var2.a().readUTF(), (byte)0));
               }

               boolean var107 = false;
               ey var80;
               if (BaseCanvas.currentScreen != null && "SCREEN_GUILD".equals(BaseCanvas.currentScreen.c)) {
                  var80 = (ey)BaseCanvas.currentScreen;
               } else {
                  var80 = new ey();
                  var107 = true;
               }

               var80.b(var131, var136, var98);
               var80.a(0, var107);
               return;
            case 17:
               String var117 = var2.a().readUTF();
               int var6 = var2.a().readInt();
               String var97 = var2.a().readUTF();
               boolean var105 = var2.a().readBoolean();
               String[] var7 = null;
               if (var2.a().readBoolean()) {
                  String[] var8;
                  var7 = var8 = new String[7];
                  var8[0] = var2.a().readUTF();
                  var7[1] = var2.a().readUTF();
                  var7[2] = var2.a().readUTF();
                  var7[3] = var2.a().readUTF();
                  var7[4] = var2.a().readUTF();
                  var7[5] = var2.a().readUTF();
                  var7[6] = var2.a().readUTF();
               }

               boolean var135 = false;
               ey var78;
               if (BaseCanvas.currentScreen != null && "SCREEN_GUILD".equals(BaseCanvas.currentScreen.c)) {
                  var78 = (ey)BaseCanvas.currentScreen;
               } else {
                  var78 = new ey();
                  var135 = true;
               }

               var78.a(var105, var6, var117, var97, var7);
               var78.a(1, var135);
               return;
            case 19:
               var1 = var2.a().readBoolean();
               gd.b(var2.a().readUTF(), var1 ? new cd("sửa", new dl(this)) : null, cd.b);
               return;
            case 20:
               BaseCanvas.getCurrentScreen().w();
               var1 = var2.a().readInt();
               var2.a().readUTF();
               byte var96 = var2.a().readByte();
               h var4 = new h();

               for(int var116 = 0; var116 < var96; ++var116) {
                  h.a(var2.a().readUTF(), var2.a().readUTF());
               }

               var4.a(var1);
               var4.d();
               return;
            case 22:
               if (h.a) {
                  h.a(var2.a().readUTF(), var2.a().readUTF());
                  return;
               }

               du var5;
               (var5 = new du(var2.a().readUTF() + ": " + var2.a().readUTF())).a();
               fw.e.addElement(var5);
               return;
            case 23:
               fr var75;
               if ((var75 = a()) == null) {
                  return;
               }

               int var95 = var2.a().readInt();

               for(int var145 = 0; var145 < var95; ++var145) {
                  var75.a.a(var2.a().readInt()).b = var2.a().readUTF();
               }

               return;
            case 24:
               BaseCanvas.currentScreen.x();
               boolean var3 = var2.a().readByte() == 1;
               int var64 = var2.a().readInt();
               int[] var65 = new int[3];
               int[] var66 = new int[3];
               String[] var67 = new String[3];
               String[] var68 = new String[3];
               String[] var69 = new String[3];

               for(int var70 = 0; var70 < 3; ++var70) {
                  var65[var70] = var2.a().readInt();
                  var66[var70] = var2.a().readInt();
                  var67[var70] = var2.a().readUTF();
                  var68[var70] = var2.a().readUTF();
                  var69[var70] = var2.a().readUTF();
               }

               fw var146 = BaseCanvas.currentScreen;
               ca var71 = null;
               if (var146 instanceof ca) {
                  var71 = (ca)var146;
               } else {
                  int var72 = 0;

                  while(var72 < var146.a.size()) {
                     fw var73;
                     if ((var73 = (fw)var146.a.elementAt(var72)) instanceof ca) {
                        var71 = (ca)var73;
                     } else {
                        ++var72;
                     }
                  }
               }

               boolean var147 = var71 == null;
               if (var71 == null) {
                  var71 = new ca(var3 ? 0 : 1);
               }

               var71.a(var64);
               var71.a(var65, var66, var67, var69, var68);
               var71.a(0, var147);
         }
      } catch (Exception var74) {
         var74.printStackTrace();
      }
   }

   public static void b() {
      if (a != null) {
         a.a();
      }

   }

   private static gt[] a(en var0) {
      gt[] var1 = new gt[var0.a().readInt()];

      for(int var2 = 0; var2 < var1.length; ++var2) {
         var1[var2] = new gt(var0.a().readInt(), var0.a().readUTF(), var0.a().readShort(), var0.a().readShort(), var0.a().readBoolean(), var0.a().readByte(), (long)var0.a().readInt());
      }

      return var1;
   }
}
