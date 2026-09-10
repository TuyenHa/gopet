import java.io.DataInputStream;
import java.io.IOException;
import javax.microedition.lcdui.Image;

public final class bi extends aa {
   private bh a;
   private int a;
   private int b;
   private final fr a;

   public bi(fr var1, ew var2, int var3, ee var4) {
      super((byte)var3, var4);
      this.a = var1;
      switch (var3) {
         case 0:
            if (var1.a == null) {
               Image var13 = null;

               try {
                  var13 = Image.createImage("/pet/petInteract/kiss.png");
               } catch (IOException var7) {
                  var7.printStackTrace();
               }

               dy var10;
               (var10 = new dy()).a(new DataInputStream(gv.a("/pet/petInteract/kiss")));

               for(int var16 = 0; var16 < var10.a.length; ++var16) {
                  var10.a[var16].a = var13;
               }

               var1.a = new dz(var10.a[0]);
            }

            this.a = System.currentTimeMillis();
            return;
         case 1:
            if (var1.b == null) {
               Image var12 = null;

               try {
                  var12 = Image.createImage("/pet/petInteract/play.png");
               } catch (IOException var6) {
                  var6.printStackTrace();
               }

               dy var9;
               (var9 = new dy()).a(new DataInputStream(gv.a("/pet/petInteract/play")));

               for(int var15 = 0; var15 < var9.a.length; ++var15) {
                  var9.a[var15].a = var12;
               }

               var1.b = new dz(var9.a[0]);
            }

            this.a = System.currentTimeMillis();
            return;
         case 2:
            if (var1.c == null) {
               Image var11 = null;

               try {
                  var11 = Image.createImage("/pet/petInteract/poke.png");
               } catch (IOException var5) {
                  var5.printStackTrace();
               }

               dy var8;
               (var8 = new dy()).a(new DataInputStream(gv.a("/pet/petInteract/poke")));

               for(int var14 = 0; var14 < var8.a.length; ++var14) {
                  var8.a[var14].a = var11;
               }

               var1.c = new dz(var8.a[0]);
            }

            this.a = System.currentTimeMillis();
            return;
         default:
      }
   }

   public final void a() {
      switch (this.a) {
         case 0:
            switch (this.b) {
               case 0:
                  this.a = new bh(this.a, 0);
                  dh var4 = ((df)this.a).a;
                  this.a.i = var4.i + 5;
                  this.a.j = var4.j + 1;
                  this.a.a((eh)this.a);
                  this.a.c(this.a);
                  this.b = 1;
                  if (this.a.c == dv.a.c) {
                     this.a.b = false;
                     return;
                  }

                  return;
               case 1:
                  if (System.currentTimeMillis() - this.a >= 2000L) {
                     this.a.b(this.a);
                     this.a = true;
                     if (this.a.c == dv.a.c) {
                        this.a.b = true;
                        return;
                     }

                     return;
                  }

                  return;
               default:
                  return;
            }
         case 1:
            switch (this.b) {
               case 0:
                  this.a = new bh(this.a, 1);
                  dh var3;
                  if ((var3 = ((df)this.a).a) == null) {
                     return;
                  } else {
                     this.a = var3.i;
                     this.b = var3.j;
                     var3.j = this.a.j;
                     if (var3.i < this.a.i) {
                        var3.i = this.a.i - 40;
                        var3.a = 2;
                        this.a.g = 0;
                     } else {
                        var3.i = this.a.i - 40;
                        var3.a = 0;
                        this.a.g = 1;
                     }

                     this.a.i = var3.i + this.a.i >> 1;
                     this.a.j = var3.j + 1;
                     this.a.a((eh)this.a);
                     this.a.c(this.a);
                     this.b = 1;
                     if (this.a.c == dv.a.c) {
                        this.a.b = false;
                        return;
                     }

                     return;
                  }
               case 1:
                  if (System.currentTimeMillis() - this.a >= 8000L) {
                     this.a.b(this.a);
                     this.a = true;
                     if (this.a.c == dv.a.c) {
                        this.a.b = true;
                     }

                     dh var2;
                     (var2 = ((df)this.a).a).i = this.a;
                     var2.j = this.b;
                     return;
                  }

                  return;
               default:
                  return;
            }
         case 2:
            switch (this.b) {
               case 0:
                  this.a = new bh(this.a, 2);
                  dh var1 = ((df)this.a).a;
                  this.a.i = var1.i + 5;
                  this.a.j = var1.j + 1;
                  this.a.a((eh)this.a);
                  this.a.c(this.a);
                  this.b = 1;
                  if (this.a.c == dv.a.c) {
                     this.a.b = false;
                     return;
                  }

                  return;
               case 1:
                  if (System.currentTimeMillis() - this.a >= 2000L) {
                     this.a.b(this.a);
                     this.a = true;
                     if (this.a.c == dv.a.c) {
                        this.a.b = true;
                        return;
                     }

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
