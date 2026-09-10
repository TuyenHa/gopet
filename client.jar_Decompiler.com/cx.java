import java.io.IOException;
import java.io.InputStream;
import java.util.Vector;
import javax.microedition.io.Connector;
import javax.microedition.io.HttpConnection;
import vn.me.core.BaseCanvas;

public final class cx {
   public static eo a;
   public static String a;
   public static gz a;
   public static gz b;
   public static int a = 19180;
   public static String b = "123.30.104.18";
   public static String c = "123.30.104.18";
   public static int b = 19180;
   public static int c = 0;
   public static String d = "http://ip.qmobi.net/gopetServer.txt";
   public static String e = "http://gopettae.com/help.txt";
   public static String f = "http://mgo.vn/index.php/staticpage/legal";
   public static Vector a = new Vector();

   public static boolean a() {
      return a != null && a.a && c.equals(a.a) && b == a.c;
   }

   public static void a(String var0) {
      en var1;
      (var1 = new en(9)).a(var0);
      a.a(var1);
   }

   public static void a() {
      b();
      BaseCanvas.instance.Field170 = new eo();
      eo var0;
      a = var0 = BaseCanvas.instance.Field170;
      cq var1 = cq.a();
      var0.a = var1;
      a.a(new ep(a));
      a.a(new eq(a));
      int var2 = b;
      String var4 = c;
      var0 = a;
      (new Thread(new am(var0, var4, var2))).start();
   }

   public static void b() {
      if (a != null) {
         a.b();
         a.a = null;
         a = null;
      }

   }

   public static void c() {
      en var0;
      (var0 = new en(-36, (byte)0)).a(0);
      var0.b(c);
      var0.a(cg.b());
      var0.a(System.getProperty("microedition.platform") + ";" + System.getProperty("microedition.configuration") + ";" + System.getProperty("microedition.profiles") + ";" + System.getProperty("microedition.hostname") + ";gopet");
      var0.b(BaseCanvas.w);
      var0.b(BaseCanvas.h);
      var0.a(gw.a());
      var0.a("2.4.9");
      a.a(var0);
      System.out.println("defpackage.GlobalService.Method237()");
   }

   public static void a(String var0, String var1, String var2) {
      en var3;
      (var3 = new en(1, (byte)0)).a(var0);
      var3.a(var1);
      var3.a(BaseCanvas.instance.midlet.getAppProperty("RefCode"));
      var3.a(var2);
      var0 = cg.g();
      var1 = cg.e();
      var2 = cg.f();
      String var4 = cg.d();
      String var5 = cg.h();
      var3.a(var0);
      var3.a(var1);
      var3.a(var2);
      var3.a(var4);
      if (var5 != null) {
         var3.a(var5);
      }

      a.a(var3);
      var3.a();
   }

   public static void a(String var0, String var1) {
      en var2;
      (var2 = new en(35, (byte)0)).a(var0);
      var2.a(var1);
      a.a(var2);
      var2.a();
   }

   public static void a(String var0, String var1, gz var2, gz var3) {
      (new Thread(new ai(var1, var0, var2, var3))).start();
   }

   public static void b(String var0, String var1) {
      en var2;
      (var2 = new en(57)).a(var0);
      var2.a(var1);
      a.a(var2);
   }

   public static void d() {
      en var0;
      (var0 = new en(73)).a(1);
      a.a(var0);
   }

   public static void a(int var0) {
      en var1;
      (var1 = new en(72)).a(var0);
      a.a(var1);
   }

   public static void a(String var0, gz var1, gz var2) {
      HttpConnection var3 = null;

      try {
         HttpConnection var11;
         var3 = var11 = (HttpConnection)Connector.open(var0);
         var11.setRequestMethod("GET");
         var3.setRequestProperty("Content-Type", "//text plain");
         var3.setRequestProperty("Connection", "close");
         if (var3.getResponseCode() == 200) {
            InputStream var4 = var3.openInputStream();
            int var12;
            if ((var12 = (int)var3.getLength()) != -1) {
               byte[] var13 = new byte[var12];
               var4.read(var13);
               String var14 = new String(var13, "UTF-8");
               if (var1 != null) {
                  var1.a(var14);
                  if (var3 != null) {
                     try {
                        var3.close();
                        return;
                     } catch (IOException var7) {
                        return;
                     }
                  }

                  return;
               }
            }
         }

         if (var3 != null) {
            try {
               var3.close();
            } catch (IOException var8) {
            }
         }
      } catch (IOException var9) {
         if (var3 != null) {
            try {
               var3.close();
            } catch (IOException var6) {
            }
         }
      } catch (Throwable var10) {
         if (var3 != null) {
            try {
               var3.close();
            } catch (IOException var5) {
               var5.printStackTrace();
            }
         }

         var10.printStackTrace();
      }

      var2.a((Object)null);
   }

   public static void a(int[] var0) {
      en var1 = new en(24);

      for(int var2 = 0; var2 < var0.length; ++var2) {
         var1.b(var0[var2]);
      }

      a.a(var1);
      var1.a();
   }

   public static void b(int var0) {
      en var1;
      (var1 = new en(81)).a(44);
      var1.b(var0);
      a.a(var1);
      var1.a();
   }

   public static void a(int var0, int var1) {
      en var2;
      (var2 = new en(79)).b(var0);
      var2.b(var1);
      a.a(var2);
      var2.a();
   }

   public static void a(int var0, int var1, int var2, String[] var3) {
      en var4;
      (var4 = new en(var0)).a(var1);
      var4.b(var2);
      var4.b(var3.length);

      for(int var5 = 0; var5 < var3.length; ++var5) {
         String var6 = var3[var5];
         var4.a(var6);
      }

      a.a(var4);
      var4.a();
   }

   public static void a(String var0, byte var1) {
      en var2;
      (var2 = new en(96)).a(0);
      var2.a(var1);
      var2.a(var0);
      a.a(var2);
      var2.a();
   }

   public static void a(String var0, int var1) {
      en var2;
      (var2 = new en(21)).a(var0);
      var2.a((byte)var1);
      a.a(var2);
      var2.a();
   }

   private static void e(int var0) {
      en var1 = new en(var0);
      a.a(var1);
      var1.a();
   }

   public static void c(String var0, String var1) {
      en var2;
      (var2 = new en(93)).a(2);
      var2.a(7);
      var2.a(var0);
      var2.a(var1);
      a.a(var2);
      var2.a();
   }

   public static void a(int var0, int var1, byte var2, int[] var3) {
      if (a != null) {
         en var4;
         (var4 = new en(27)).b(var1);
         var4.a(var2);
         var4.b(var0);
         var4.b(var3.length);

         for(int var5 = 0; var5 < var3.length; ++var5) {
            var1 = var3[var5];
            var4.b(var1);
         }

         if (a != null) {
            a.a(var4);
         }

         var4.a();
      }
   }

   public static void a(int var0, int var1, int var2) {
      en var3;
      (var3 = new en(25)).b(var0);
      var3.b(var1);
      var3.b(var2);
      a.a(var3);
      var3.a();
   }

   public static void c(int var0) {
      en var1;
      (var1 = new en(7)).b(var0);
      a.a(var1);
      var1.a();
   }

   public static void b(String var0) {
      en var1;
      (var1 = new en(101)).a(var0);
      a.a(var1);
      var1.a();
   }

   public static void e() {
      e(44);
   }

   public static void a(br var0, String var1, String var2) {
      en var3;
      (var3 = new en(71, (byte)0)).a(var0.b);
      var3.a(var0.a);
      var3.a(var1);
      if (var2 != null) {
         var3.a(var2);
      }

      var3.a(BaseCanvas.instance.midlet.getAppProperty("RefCode"));
      a.a(var3);
   }

   public static void f() {
      if (a()) {
         b();
         dv var10000 = cq.a.a;
         dv.a();
         (new fb()).d(0);
         gq.a();
         dv.a = null;
         cq.d();
         a = null;
      }

   }

   public static void a(gz var0) {
      (new Thread(new aj(var0, (gz)null))).start();
   }

   public static void c(String var0) {
      a.removeAllElements();
      if (var0.indexOf(";") == -1 && var0.length() > 0) {
         a.addElement(a(var0));
      } else {
         while(var0.indexOf(";") != -1) {
            a.addElement(a(var0.substring(0, var0.indexOf(";"))));
            String var1;
            var0 = var1 = var0.substring(var0.indexOf(";") + 1);
            if (var1.indexOf(";") == -1 && var0.length() > 0) {
               a.addElement(a(var0));
            }
         }

      }
   }

   private static dw a(String var0) {
      String var1 = var0.substring(0, var0.indexOf("|"));
      var0 = var0.substring(var0.indexOf("|") + 1);
      return new dw(var1, var0.substring(0, var0.indexOf(":")), Integer.parseInt(var0.substring(var0.indexOf(":") + 1).trim()));
   }

   public static void d(int var0) {
      en var1;
      (var1 = new en(122)).a(2);
      var1.b(var0);
      a.a(var1);
      var1.a();
   }

   public static void a(int var0, boolean var1) {
      en var2;
      (var2 = new en(45)).a(4);
      var2.b(var0);
      var2.a(var1);
      a.a(var2);
      var2.a();
   }

   public static void g() {
      e(64);
   }

   public static void b(int var0, int var1) {
      en var2;
      (var2 = new en(122)).a(5);
      var2.b(var0);
      var2.b(var1);
      a.a(var2);
      var2.a();
   }

   public static void c(int var0, int var1) {
      en var2;
      (var2 = new en(122)).a(3);
      var2.b(var0);
      var2.b(var1);
      a.a(var2);
      var2.a();
   }

   public static void d(int var0, int var1) {
      en var2;
      (var2 = new en(125)).a(1);
      var2.b(var0);
      var2.a(var1);
      a.a(var2);
      var2.a();
   }

   public static void h() {
      en var0;
      (var0 = new en(125)).a(2);
      a.a(var0);
      var0.a();
   }

   public static void i() {
      en var0;
      (var0 = new en(42)).a(12);
      a.a(var0);
      var0.a();
   }
}
