import java.io.DataInputStream;
import java.io.IOException;
import java.util.Hashtable;
import javax.microedition.lcdui.Image;

public final class bg {
   public Hashtable a = new Hashtable();
   private Image a = Image.createImage(1, 1);
   private Hashtable b = new Hashtable();

   public final void a(String var1, byte[] var2) {
      Image var3;
      if ((var3 = Image.createImage(var2, 0, var2.length)) != null) {
         this.a.put(var1, var3);
      }

   }

   public final Image a(String var1) {
      Image var2;
      if ((var2 = (Image)this.a.get(var1)) == null) {
         dc.a(var1, 1);
         this.a.put(var1, this.a);
         return null;
      } else {
         return var2 != this.a ? var2 : null;
      }
   }

   public final void a(String var1) {
      this.a.remove(var1);
   }

   public static dz[] a(String var0) {
      Image var2 = null;

      try {
         var2 = Image.createImage(var0 + ".png");
      } catch (IOException var3) {
         var3.printStackTrace();
      }

      dy var1;
      (var1 = new dy()).a(new DataInputStream(gv.a(var0)));

      for(int var4 = 0; var4 < var1.a.length; ++var4) {
         var1.a[var4].a = var2;
      }

      dz[] var5 = new dz[var1.a.length];

      for(int var6 = 0; var6 < var5.length; ++var6) {
         var5[var6] = new dz(var1.a[var6]);
      }

      return var5;
   }

   public final void b(String var1, byte[] var2) {
      Image var3;
      if ((var3 = Image.createImage(var2, 0, var2.length)) != null) {
         this.b.put(var1, var3);
      }

   }

   public final Image b(String var1) {
      Image var2;
      if ((var2 = (Image)this.b.get(var1)) == null) {
         dc.a(var1, 2);
         this.b.put(var1, this.a);
         return null;
      } else {
         return var2 != this.a ? var2 : null;
      }
   }
}
