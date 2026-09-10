package vn.me.core;

import javax.microedition.lcdui.Canvas;
import javax.microedition.lcdui.Display;
import javax.microedition.lcdui.Graphics;
import javax.microedition.midlet.MIDlet;
import thong.sdk.IPlatformSDK;
import thong.sdk.ISoundManagerSDK;

public final class BaseCanvas extends Canvas implements Runnable {
   private long a;
   public static boolean isRunning;
   public static boolean isPause;
   public static int ticks;
   public static int w;
   public static int h;
   public static int Field157;
   public static int Field158;
   public static int Field159;
   public static int Field160;
   public static int Field161;
   public static int Field162;
   public static .fw currentScreen;
   public MIDlet midlet;
   public static BaseCanvas instance;
   private int a;
   public static Graphics g;
   public .bc Field169;
   public .eo Field170;
   public int initialPressX;
   public int initialPressY;
   private static int b = 800;
   private static int c = 10;
   private static int d = 0;
   private int e = -1982;
   private int[][] a = new int[10][3];
   private int f = 0;
   private int g = 0;
   private int i = 7;
   private int j = 0;
   private int k = 0;
   public static IPlatformSDK iPlatformSDK;
   public static final int SPEED_TIME = 50;
   public static int curSpeedGame = 50;
   public static ISoundManagerSDK soundManagerSDK = new vn.me.core.b();
   private long b = 0L;

   public static BaseCanvas create(MIDlet var0) {
      if (instance == null) {
         instance = new BaseCanvas(var0);
      }

      return instance;
   }

   private BaseCanvas(MIDlet var1) {
      this.setFullScreenMode(true);
      w = this.getWidth();
      h = this.getHeight();
      a();
      d = this.a();
      this.midlet = var1;
      if (iPlatformSDK != null) {
         iPlatformSDK.setGameSDK(new vn.me.core.a(this));
      }

   }

   private int a() {
      if (this.getKeyCode(8) == -20) {
         return 1;
      } else if (System.getProperty("microedition.platform").indexOf("SonyEricsson") != -1) {
         return 2;
      } else {
         try {
            Class.forName("com.samsung.util.Vibration");
            return 3;
         } catch (Exception var2) {
            try {
               Class.forName("com.siemens.mp.io.File");
               return 5;
            } catch (Exception var1) {
               return 0;
            }
         }
      }
   }

   protected final void sizeChanged(int var1, int var2) {
      super.sizeChanged(var1, var2);
      w = var1;
      h = var2;
      a();
   }

   private static void a() {
      Field157 = w >> 1;
      Field158 = h >> 1;
      Field159 = w / 3;
      int var10000 = h;
      Field160 = 2 * w / 3;
      var10000 = h;
      Field161 = 3 * w / 4;
      Field162 = 3 * h / 4;
      var10000 = w;
      var10000 = h;
      var10000 = w;
      if (currentScreen != null) {
         currentScreen.e();
      }

   }

   public static void setCurrentScreen(.fw var0) {
      if (currentScreen != var0) {
         if (currentScreen != null) {
            currentScreen.l();
            currentScreen = null;
         }

         if (var0 == null) {
            currentScreen = null;
         } else {
            currentScreen = var0;
            var0.g_();
            System.out.println("vn.me.core.BaseCanvas.setCurrentScreen() " + var0.getClass().getName());
         }
      }
   }

   public static .fw getCurrentScreen() {
      return currentScreen;
   }

   public final void Method0() {
      (new Thread(this)).start();
   }

   private void b() {
      if (!isPause) {
         if (iPlatformSDK != null && this.b > System.currentTimeMillis()) {
            return;
         }

         try {
            long var1 = System.currentTimeMillis();
            ++ticks;
            if (this.a > 0) {
               --this.a;
               if (this.a == 0) {
                  Display.getDisplay(instance.midlet).vibrate(0);
               }
            }

            if (this.f != 0) {
               synchronized(this) {
                  for(int var4 = 0; var4 < this.f; ++var4) {
                     int[] var5;
                     switch ((var5 = this.a[var4])[0]) {
                        case 0:
                        case 1:
                           currentScreen.a(var5[0], var5[1]);
                           break;
                        case 2:
                           currentScreen.a(var5[1], var5[2]);
                           break;
                        case 3:
                           currentScreen.b_(var5[1], var5[2]);
                           break;
                        case 4:
                           currentScreen.c(var5[1], var5[2]);
                     }
                  }

                  this.f = 0;
               }
            }

            long var3 = System.currentTimeMillis();
            if (this.e != -1982 && this.a <= var3) {
               synchronized(this) {
                  if (this.f < 10) {
                     this.a[this.f][0] = 0;
                     this.a[this.f][1] = this.e;
                     ++this.f;
                  }
               }

               this.a = var3 + (long)c;
            }

            if (currentScreen != null) {
               currentScreen.c_();
               currentScreen.s();

               for(int var10 = 0; var10 < currentScreen.a.size(); ++var10) {
                  .fw var6;
                  if ((var6 = (.fw)currentScreen.a.elementAt(var10)).e) {
                     var6.c_();
                  }
               }
            }

            if (this.Field169 != null) {
               this.Field169.a();
            }

            if (iPlatformSDK == null) {
               this.repaint();
               this.serviceRepaints();
            }

            if (this.Field170 != null && this.Field170.a && this.Field170.b) {
               this.Field170.a();
            }

            long var11 = (long)curSpeedGame - (System.currentTimeMillis() - var1);
            this.b = System.currentTimeMillis() + var11;
            if (var11 > 0L && iPlatformSDK == null) {
               Thread.sleep(var11);
            }

            return;
         } catch (Exception var9) {
            var9.printStackTrace();
         }
      }

   }

   public final void run() {
      isRunning = true;

      while(isRunning && iPlatformSDK == null) {
         this.b();
      }

   }

   public final void keyPressed(int var1) {
      if ((var1 = a(var1)) != -6 && var1 != -7) {
         this.e = var1;
         this.a = System.currentTimeMillis() + (long)b;
      }

      if (!currentScreen.a(var1)) {
         synchronized(this) {
            if (this.f < 10) {
               this.a[this.f][0] = 0;
               this.a[this.f][1] = var1;
               ++this.f;
            }

         }
      }
   }

   public final void keyReleased(int var1) {
      this.e = -1982;
      var1 = a(var1);
      .fw var10000 = currentScreen;
      .fw.a();
      synchronized(this) {
         if (this.f < 10) {
            this.a[this.f][0] = 1;
            this.a[this.f][1] = var1;
            ++this.f;
         }

      }
   }

   private static int a(int var0) {
      if (d == 1) {
         switch (var0) {
            case -6:
               return -2;
            case -5:
               return -4;
            case -4:
            case -3:
            default:
               break;
            case -2:
               return -3;
         }
      } else if (d == 5) {
         switch (var0) {
            case -62:
               return -4;
            case -61:
               return -3;
            case -60:
               return -2;
            case -59:
               return -1;
            case -26:
               return -5;
            case -4:
               return -7;
            case -1:
               return -6;
         }
      }

      switch (var0) {
         case -204:
         case -8:
         case 8:
            return -8;
         case -39:
         case -2:
            return -2;
         case -38:
         case -1:
            return -1;
         case -22:
         case -7:
            return -7;
         case -21:
         case -6:
         case 4098:
            return -6;
         case -20:
         case -5:
         case 10:
            return -5;
         case -4:
            return -4;
         case -3:
            return -3;
         default:
            return var0;
      }
   }

   protected final void keyRepeated(int var1) {
   }

   private boolean a(int var1, int var2) {
      if (this.g == 0) {
         this.j = var1;
         this.k = var2;
         ++this.g;
         return false;
      } else {
         ++this.g;
         if (this.g > this.i) {
            return true;
         } else if (3 * w / 100 <= Math.abs(this.j - var1)) {
            this.g = this.i + 1;
            return true;
         } else if (3 * h / 100 <= Math.abs(this.k - var2)) {
            this.g = this.i + 1;
            return true;
         } else {
            return false;
         }
      }
   }

   protected final void pointerDragged(int var1, int var2) {
      synchronized(this) {
         if (this.a(var1, var2) && this.f < 10) {
            this.a[this.f][0] = 3;
            this.a[this.f][1] = var1;
            this.a[this.f][2] = var2;
            ++this.f;
         }

      }
   }

   protected final void pointerPressed(int var1, int var2) {
      this.initialPressX = var1;
      this.initialPressY = var2;
      synchronized(this) {
         if (this.f < 10) {
            this.a[this.f][0] = 2;
            this.a[this.f][1] = var1;
            this.a[this.f][2] = var2;
            ++this.f;
         }

      }
   }

   protected final void pointerReleased(int var1, int var2) {
      if (this.g == 0 && var1 != this.initialPressX && var2 != this.initialPressY) {
         this.a(this.initialPressX, this.initialPressY);
         if (this.a(var1, var2)) {
            this.pointerDragged(this.initialPressX, this.initialPressY);
            this.pointerDragged(var1, var2);
         }
      }

      this.g = 0;
      synchronized(this) {
         if (this.f < 10) {
            this.a[this.f][0] = 4;
            this.a[this.f][1] = var1;
            this.a[this.f][2] = var2;
            ++this.f;
         }

      }
   }

   private static void c() {
      if (currentScreen != null) {
         currentScreen.u();
      } else {
         g.setColor(0);
         g.fillRect(0, 0, w, h);
      }
   }

   protected final void paint(Graphics var1) {
      if (g != var1) {
         g = var1;
      }

      c();
   }

   public final void resetScreen() {
      Display.getDisplay(this.midlet).setCurrent(this);
      this.setFullScreenMode(true);
   }

   static void a(BaseCanvas var0) {
      var0.b();
   }

   static void b(BaseCanvas var0) {
      c();
   }
}
