import javax.microedition.lcdui.Command;
import javax.microedition.lcdui.Display;
import javax.microedition.lcdui.TextBox;
import vn.me.core.BaseCanvas;

public final class ge extends gn implements gz {
   public String a;
   private String b;
   private String c;
   private int d;
   private int e;
   public int a;
   private int f;
   private int g;
   private int h;
   private int i;
   private int j;
   public int b;
   private static boolean a;
   private boolean b;
   private int k;
   private String d;
   public int c;
   private int l;
   private gg a;
   private gg b;
   private cd f;
   public static cd a;
   private int m;
   private int n;
   private int o;
   private String e;
   public gz a;
   private int p;
   private static final int[] a = new int[]{18, 14, 11, 9, 6, 4, 2};
   private static int q = 0;
   private static String[] a = new String[]{" 0", ".,@?!_1\"/$-():*+<=>;%&~#%^&*{}[];'/1", "abc2áàảãạâấầẩẫậăắằẳẵặ2", "def3đéèẻẽẹêếềểễệ3", "ghi4íìỉĩị4", "jkl5", "mno6óòỏõọôốồổỗộơớờởỡợ6", "pqrs7", "tuv8úùủũụưứừửữự8", "wxyz9ýỳỷỹỵ9", "*", "#"};
   private static String[] b = new String[]{"0", "1", "abc2", "def3", "ghi4", "jkl5", "mno6", "pqrs7", "tuv8", "wxyz9", "0", "0"};
   private static String[] c = new String[]{"abc", "Abc", "ABC", "123"};
   private static int J = 35;
   private static int K = 42;

   public ge() {
      this(0, 0, 0, 0, "");
   }

   public ge(int var1, int var2, int var3, int var4) {
      this(var1, var2, var3, var4, (String)null);
   }

   private ge(int var1, int var2, int var3, int var4, String var5) {
      gg var10007 = gv.b;
      this(var1, var2, var3, var4, var5, gv.b, gv.a);
   }

   private ge(int var1, int var2, int var3, int var4, String var5, gg var6, gg var7) {
      super(var1, var2, var3, var4);
      this.a = "";
      this.b = "";
      this.c = "";
      this.d = 0;
      this.e = 0;
      this.a = 500;
      this.f = 0;
      this.g = -1982;
      this.h = 0;
      this.i = 0;
      this.j = 10;
      this.b = 0;
      this.b = true;
      this.c = 0;
      this.l = 0;
      this.a = gv.b;
      this.b = gv.b;
      int var10000 = gs.d;
      this.e = "";
      this.p = 0;
      this.a = var6;
      this.b = var7;
      this.c = var5 == null ? 0 : var6.a(var5) + 2 * gs.p;
      this.d = var5;
      this.a = "";
      q = this.a.a() - 1;
      cd var8 = new cd(0, gw.a(3), this);
      this.f = var8;
      this.e = var8;
      this.k = this.b.a("ABC") + 5;
      this.x = 1;
      this.i();
   }

   public final void a(int var1, int var2, int var3, int var4) {
      super.a(var1, var2, var3, var4);
      if (this.c != 0) {
         String var5 = this.d;

         for(this.c = this.a.a(var5) + this.y; this.c > (this.t << 1) / 3; this.c = this.a.a(var5) + this.y) {
            var5 = var5.substring(0, var5.length() - 1);
         }

         this.d = var5;
         this.i();
      }
   }

   public final void a(int var1) {
      this.b = var1;
      switch (var1) {
         case 0:
         case 2:
         case 3:
            this.l = 0;
            break;
         case 1:
            this.l = 3;
      }

      this.k = 0;
      if (this.b) {
         this.k = this.b.a("ABC") + 5;
      }

   }

   public final void a(String var1) {
      this.b(gv.a.a(var1) + (this.y << 1));
      this.d = var1;
   }

   public final void h() {
      if (this.d > 0 && this.a.length() > 0) {
         this.a = this.a.substring(0, this.d - 1) + this.a.substring(this.d, this.a.length());
         --this.d;
         this.i();
         this.j();
      }
   }

   private void i() {
      if (this.a.length() == 0) {
         this.c = this.e;
         this.c = this.b;
      } else {
         this.c = this.a;
      }

      this.m = this.c;
      this.n = this.t - 1 - this.c;
      this.o = this.u - 1;
      if (this.f < 0 && this.a.a(this.c) + this.f < this.n - this.y - 13 - this.k) {
         this.f = this.n - 10 - this.k - this.a.a(this.c);
      }

      if (this.f + this.a.a(this.c.substring(0, this.d)) <= 0) {
         this.f = -this.a.a(this.c.substring(0, this.d));
         this.f += 40;
      } else if (this.f + this.a.a(this.c.substring(0, this.d)) >= this.n - 12 - this.k) {
         this.f = this.n - 10 - this.k - this.a.a(this.c.substring(0, this.d)) - (this.y << 1);
      }

      if (this.f > 0) {
         this.f = 0;
      }

      if (this.a != null) {
         this.a.a(new Object[]{new cd(-6, (String)null, (gz)null), this});
      }

   }

   private boolean a(int var1) {
      if (this.b == 3) {
         if ((var1 < 48 || var1 > 57) && (var1 < 65 || var1 > 90) && (var1 < 97 || var1 > 122)) {
            return false;
         }
      } else if (this.b == 1 && (var1 < 48 || var1 > 57)) {
         return false;
      }

      if (this.a.length() < this.a) {
         String var2 = this.a.substring(0, this.d) + (char)var1;
         if (this.d < this.a.length()) {
            var2 = var2 + this.a.substring(this.d, this.a.length());
         }

         this.a = var2;
         ++this.d;
         this.j();
         this.i();
         return true;
      } else {
         return true;
      }
   }

   public final void e() {
      if (gs.a == 0) {
         gs.a(this);
      } else {
         if (this.d) {
            BaseCanvas.g.setColor(1655180);
         } else {
            BaseCanvas.g.setColor(3033945);
         }

         BaseCanvas.g.drawRect(this.m, 0, this.n, this.o);
         if (this.d) {
            BaseCanvas.g.setColor(gs.d);
            BaseCanvas.g.drawRect(this.m + 1, 1, this.n - 2, this.o - 2);
         }

      }
   }

   public final void a() {
      if (gs.a != 1) {
         if (this.d) {
            BaseCanvas.g.setColor(gs.f);
            if (this.c == 0) {
               BaseCanvas.g.fillRoundRect(0, 0, this.t - 1, this.u - 1, gs.q, gs.q);
            } else {
               BaseCanvas.g.fillRoundRect(0 + this.c, 0, this.t - 1 - this.c, this.u - 1, gs.q, gs.q);
            }
         }

         if (!a && this.b) {
            BaseCanvas.g.setColor(gs.g);
            BaseCanvas.g.fillRect(this.t - this.k - 3, 3, this.k, this.o - 4);
            BaseCanvas.g.fillRect(this.t - 3, 4, 1, this.o - 6);
            this.b.a(BaseCanvas.g, c[this.l], this.t - 4, this.o - this.b.a() >> 1, 24);
         }
      } else {
         if (this.d || this.x > 0) {
            BaseCanvas.g.setColor(16777215);
            BaseCanvas.g.fillRect(this.m + 1, 1, this.n - 1, this.u - 2);
         }

         if (!a && this.b != 1) {
            BaseCanvas.g.setColor(8955067);
            BaseCanvas.g.fillRect(this.t - this.k - 3, 3, this.k, this.o - 5);
            this.b.a(BaseCanvas.g, c[this.l], this.t - 3, this.o - this.b.a() >> 1, 24);
         }
      }
   }

   public final void b() {
      if (this.a.length() == 0) {
         this.c = this.e;
      } else if (this.b == 2) {
         this.c = this.b;
      } else {
         this.c = this.a;
      }

      if (this.c == 0) {
         this.a.a(BaseCanvas.g, this.c, gs.p + this.f, this.u - this.a.a() - this.y - this.x >> 1, 20);
      } else {
         BaseCanvas.g.clipRect(0, 0, this.m + this.n - this.k - 6, this.o);
         if (this.d) {
            gv.a.a(BaseCanvas.g, this.d, -this.p, this.u - this.a.a() >> 1, 20);
         } else {
            gv.a.a(BaseCanvas.g, this.d, 0, this.u - this.a.a() >> 1, 20);
         }

         BaseCanvas.g.setClip(this.m + 3, 0, this.n - this.k - 6, this.o);
         this.a.a(BaseCanvas.g, this.c, gs.p + this.f + this.c, this.o - this.a.a() >> 1, 20);
      }

      if (this.d && this.h == 0 && (this.j > 0 || this.e / 5 % 2 == 0)) {
         BaseCanvas.g.setColor(gs.h);
         if (this.c == 0) {
            BaseCanvas.g.fillRect(gs.p + this.f + this.a.a(this.c.substring(0, this.d)) + 1, (this.u - q >> 1) + 1, 1, q);
            return;
         }

         BaseCanvas.g.fillRect(gs.p + this.f + this.c + this.a.a(this.c.substring(0, this.d)) + 1, (this.u - q >> 1) + 1, 1, q);
      }

   }

   private void j() {
      if (this.b == 2) {
         this.b = "";

         for(int var1 = 0; var1 < this.a.length(); ++var1) {
            this.b = this.b + "*";
         }

         if (this.h <= 0 || this.d <= 0) {
            return;
         }

         this.b = this.b.substring(0, this.d - 1) + this.a.charAt(this.d - 1) + this.b.substring(this.d, this.b.length());
      }

   }

   public final void c() {
      super.c();
      ++this.e;
      if (this.h > 0) {
         --this.h;
         if (this.h == 0) {
            this.i = 0;
            if (this.l == 1 && this.g != J) {
               this.l = 0;
            }

            this.g = -1982;
            this.j();
         }
      }

      if (this.j > 0) {
         --this.j;
      }

      if (System.currentTimeMillis() > 100L && this.d != null && this.c < this.a.a(this.d)) {
         int var1 = this.p + 1;
         this.p = var1;
         this.p = var1 >= this.a.a(this.d) ? -this.c : this.p;
      }
   }

   public final String a() {
      return this.a;
   }

   public final void b(String var1) {
      if (var1 != null) {
         this.g = -1982;
         this.h = 0;
         this.i = 0;
         this.a = var1;
         this.c = var1;
         this.j();
         this.d = var1.length();
         this.i();
      }
   }

   public final boolean c(int var1, int var2) {
      if (this.j) {
         this.j = false;
         TextBox var3;
         (var3 = new TextBox("", "", 500, 0)).addCommand(new Command(gw.a(6), 4, 0));
         var3.addCommand(new Command(gw.a(0), 2, 0));
         var3.setCommandListener(new an(this, var3));
         if (this.b == 2) {
            var3.setConstraints(65536);
         } else if (this.b == 1) {
            var3.setConstraints(2);
         } else {
            var3.setConstraints(0);
         }

         var3.setString(this.a);
         var3.setMaxSize(this.a);
         Display.getDisplay(BaseCanvas.instance.midlet).setCurrent(var3);
         return true;
      } else {
         return false;
      }
   }

   public final boolean a(int var1, int var2) {
      if (var1 == 1) {
         return false;
      } else if (var2 == -8) {
         this.h();
         return true;
      } else {
         if (var2 >= 65 && var2 <= 122) {
            a = true;
            this.k = 0;
         }

         if (a) {
            if (var2 == 45) {
               if (var2 == this.g && this.h < a[0] && this.b != 1) {
                  this.a = this.a.substring(0, this.d == 0 ? 0 : this.d - 1) + '_';
                  this.c = this.a;
                  this.j();
                  this.i();
                  this.g = -1982;
                  return true;
               }

               this.g = 45;
            }

            if (this.f && gg.a && var2 == K && this.b == 0 && a != null) {
               a.a(new Object[]{a, this});
               return true;
            }

            if (this.b == 1) {
               if (var2 >= 48 && var2 <= 57) {
                  return this.a(var2);
               }

               return false;
            }

            if (var2 >= 32) {
               return this.a(var2);
            }
         }

         if (var2 == J) {
            if (this.b != 1) {
               var1 = this.l + 1;
               this.l = var1;
               this.l = var1 % 4;
            }

            this.h = 1;
            this.g = var2;
            return true;
         } else if (this.f && gg.a && var2 == K && this.b == 0 && a != null) {
            a.a(new Object[]{a, this});
            return true;
         } else {
            if (var2 == 42) {
               var2 = 58;
            }

            if (var2 == 35) {
               var2 = 59;
            }

            if (var2 >= 48 && var2 <= 59) {
               if (this.b != 0 && this.b != 2 && this.b != 3) {
                  if (this.b == 1) {
                     this.a(var2);
                     this.h = 1;
                     return true;
                  } else {
                     return true;
                  }
               } else {
                  var1 = var2;
                  String var6 = this.b == 3 ? b : a;
                  if (var1 == this.g) {
                     this.i = (this.i + 1) % ((Object[])var6)[var1 - 48].length();
                     char var3 = ((Object[])var6)[var1 - 48].charAt(this.i);
                     var6 = this.a.substring(0, this.d > 0 ? this.d - 1 : 0) + (this.l == 0 ? Character.toLowerCase(var3) : (this.l == 1 ? Character.toUpperCase(var3) : (this.l == 2 ? Character.toUpperCase(var3) : ((Object[])var6)[var1 - 48].charAt(((Object[])var6)[var1 - 48].length() - 1))));
                     if (this.d < this.a.length()) {
                        var6 = var6 + this.a.substring(this.d, this.a.length());
                     }

                     this.a = var6;
                     this.h = a[0];
                     this.j();
                  } else if (this.a.length() < this.a) {
                     if (this.l == 1 && this.g != -1982) {
                        this.l = 0;
                     }

                     this.i = 0;
                     char var9 = ((Object[])var6)[var1 - 48].charAt(this.i);
                     var6 = this.a.substring(0, this.d) + (this.l == 0 ? Character.toLowerCase(var9) : (this.l == 1 ? Character.toUpperCase(var9) : (this.l == 2 ? Character.toUpperCase(var9) : ((Object[])var6)[var1 - 48].charAt(((Object[])var6)[var1 - 48].length() - 1))));
                     if (this.d < this.a.length()) {
                        var6 = var6 + this.a.substring(this.d, this.a.length());
                     }

                     this.a = var6;
                     this.h = a[0];
                     ++this.d;
                     this.j();
                     this.i();
                  }

                  this.g = var1;
                  return true;
               }
            } else {
               this.i = 0;
               this.g = -1982;
               if (var2 == -3) {
                  if (this.d > 0) {
                     --this.d;
                     this.i();
                     this.j = 10;
                     return true;
                  } else {
                     return false;
                  }
               } else if (var2 != -4) {
                  this.g = var2;
                  return false;
               } else if (this.d < this.a.length()) {
                  ++this.d;
                  this.i();
                  this.j = 10;
                  return true;
               } else {
                  return false;
               }
            }
         }
      }
   }

   public final void a(Object var1) {
      if ((cd)((Object[])var1)[0] == this.f) {
         this.h();
      }

   }

   public final void b(int var1) {
      if (var1 > this.t - 60) {
         var1 = this.t - 60;
      }

      this.c = var1;
      this.i();
   }
}
