import java.io.DataInputStream;
import java.io.IOException;

public final class dy {
   public aw[] a;
   public ax[] a;
   public ay[] a;
   private int a;

   public final void a(DataInputStream var1) {
      try {
         this.a = var1.readByte();
         byte var2 = var1.readByte();
         this.a = new ay[var2];

         for(int var3 = 0; var3 < var2; ++var3) {
            this.a[var3] = new ay();
            this.a[var3].a = var1.readInt();
            this.a[var3].b = var1.readInt();
            this.a[var3].c = var1.readInt();
            this.a[var3].d = var1.readInt();
         }

         byte var11 = var1.readByte();
         this.a = new ax[var11];

         for(int var9 = 0; var9 < var11; ++var9) {
            this.a[var9] = new ax();
            ax var4 = this.a[var9];
            var1.readByte();
            byte var5 = var1.readByte();
            var4.a = new byte[var5];
            var4.a = new int[var5];
            var4.b = new int[var5];
            var4.b = new byte[var5];

            for(int var6 = 0; var6 < var5; ++var6) {
               var4.a[var6] = var1.readByte();
               var4.a[var6] = var1.readInt();
               var4.b[var6] = var1.readInt();
               var4.b[var6] = var1.readByte();
            }

            var4.a = new int[this.a][3];
            byte var15 = var1.readByte();

            for(int var7 = 0; var7 < var15; ++var7) {
               var5 = var1.readByte();
               var4.a[var5][0] = 1;
               var4.a[var5][1] = var1.readInt();
               var4.a[var5][2] = var1.readInt();
            }
         }

         var2 = var1.readByte();
         this.a = new aw[var2];

         for(int var12 = 0; var12 < var2; ++var12) {
            this.a[var12] = new aw();
            this.a[var12].a = this;
            aw var14 = this.a[var12];
            var1.readByte();
            var14.a = var1.readInt();
            byte var16 = var1.readByte();
            var14.b = var16;
            var14.a = new byte[var16];
            var14.a = new int[var16];

            for(int var17 = 0; var17 < var16; ++var17) {
               var14.a[var17] = var1.readByte();
               var14.a[var17] = var1.readInt();
               if (var14.a[var17] == -1) {
                  var14.a[var17] = var14.a;
               }
            }
         }

         var1.close();
      } catch (IOException var8) {
         var8.printStackTrace();
      }
   }
}
