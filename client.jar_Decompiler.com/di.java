import java.util.Vector;

public class di implements bf, gz {
   public boolean a;
   public de a;
   public boolean b;
   private int a;
   private int b;
   private int c;
   private do[] a;
   public ei[] a = new ei[2];
   public int[] a = new int[2];
   int[] b = new int[2];
   int[] c = new int[2];
   Vector a = new Vector();
   public boolean c = false;

   public di() {
      for(int var1 = 0; var1 < 2; ++var1) {
         this.a[var1] = null;
         this.a[var1] = -1;
         this.b[var1] = -1;
         this.c[var1] = -1;
      }

   }

   private int a(int var1) {
      for(int var2 = 0; var2 < this.a.length; ++var2) {
         if (var1 == this.a[var2]) {
            return var2;
         }
      }

      return 1;
   }

   public final void a(en var1) {
      try {
         int var2 = this.a(var1.a().readInt());
         this.a(var1.a().readInt(), var1.a().readInt());
         switch (var1.a().readByte()) {
            case 1:
               this.a.addElement(new e(this, 0, var2, (Object)null));
               break;
            case 4:
               var1.a().readInt();
               var1.a().readUTF();
               this.a.addElement(new e(this, 5, var2, new Integer(var1.a().readInt())));
         }

         int var3 = var1.a().readInt();

         for(int var4 = 0; var4 < var3; ++var4) {
            int var5 = this.a(var1.a().readInt());
            int var6 = var1.a().readInt();
            var1.a().readUTF();
            var1.a().readInt();
            var1.a().readInt();
            var1.a().readInt();
            int var7 = var1.a().readInt();
            int var8 = var1.a().readInt();
            var1.a().readInt();
            var1.a().readInt();
            if (var6 < 0) {
               this.a.addElement(new e(this, 4, var5, new int[]{var7, var8}));
            } else if (var6 >= 0 && var6 <= 2) {
               this.a.addElement(new e(this, 1, var5, new int[]{var6, var7}));
            } else if (var6 >= 101 && var6 < 125) {
               this.a.addElement(new e(this, 6, var5, new int[]{var6 - 101 + 8, var7, var8}));
            } else if (var6 >= 125) {
               this.a.addElement(new e(this, 11, var5, new int[]{var6, var7, var8}));
            }
         }

         ej.a.a(var2);
      } catch (Exception var9) {
         var9.printStackTrace();
      }
   }

   public final void b() {
      if (!this.a.isEmpty() && !this.a) {
         this.a = true;
         ((e)this.a.elementAt(0)).a();
         this.a.removeElementAt(0);
      }
   }

   public final void a(en var1, boolean var2) {
      try {
         if (var2) {
            this.a = var1.a().readInt();
            var1.a().readByte();
            this.c = var1.a().readInt();
            this.b = var1.a().readInt();
            var2 = var1.a().readByte();
            this.a = new do[var2];

            for(int var3 = 0; var3 < var2; ++var3) {
               String var4 = var1.a().readUTF();
               String var5 = var1.a().readUTF();
               this.a[var3] = new do();
               this.a[var3].a = 0;
               this.a[var3].a = var4;
               this.a[var3].b = var5;
            }

            if (this.a[0] == dv.a.c) {
               ((fr)dv.a).q();
            }

            this.a.addElement(new e(this, 7, (this.a(this.a) + 1) % 2, (Object)null));
            this.a.addElement(new e(this, 8, 0, (Object)null));
         } else {
            this.a = var1.a().readInt();
            var1.a().readByte();
            this.a.addElement(new e(this, 7, (this.a(this.a) + 1) % 2, (Object)null));
         }

         this.a.addElement(new e(this, 9, 0, (Object)null));
         this.a.addElement(new e(this, 2, 0, (Object)null));
      } catch (Exception var6) {
         var6.printStackTrace();
      }
   }

   public final void a(Object var1) {
      switch (((cd)((Object[])var1)[0]).a) {
         case 5:
            dv.a(11, 0, ef.a(11));
            return;
         default:
      }
   }

   public final void a(int var1, int var2) {
      for(int var3 = 0; var3 < this.a.length; ++var3) {
         if (this.a[var3] == null) {
            return;
         }

         if (this.a[var3].a) {
            this.a[var3].b = var2;
            this.a[var3].a = var1;
            this.a[var3].b = System.currentTimeMillis();
         }
      }

   }

   public final void a() {
      this.a = false;
   }

   public final void c() {
      this.a.addElement(new e(this, 12, 0, this.a));
      this.a.addElement(new e(this, 2, 0, (Object)null));
   }

   public static int a(di var0) {
      return var0.c;
   }

   public static int b(di var0) {
      return var0.b;
   }

   public static do[] a(di var0) {
      return var0.a;
   }

   public static int c(di var0) {
      return var0.a;
   }
}
