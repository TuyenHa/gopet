public final class er {
   private int[] a = new int[4];

   public er(long var1) {
      byte[] var3 = new byte[]{(byte)((int)(255L & var1 >> 56)), (byte)((int)(255L & var1 >> 48)), (byte)((int)(255L & var1 >> 40)), (byte)((int)(255L & var1 >> 32)), (byte)((int)(255L & var1 >> 24)), (byte)((int)(255L & var1 >> 16)), (byte)((int)(255L & var1 >> 8)), (byte)((int)(255L & var1)), (byte)((int)(255L & var1 >> 56)), (byte)((int)(255L & var1 >> 48)), (byte)((int)(255L & var1 >> 40)), (byte)((int)(255L & var1 >> 32)), (byte)((int)(255L & var1 >> 24)), (byte)((int)(255L & var1 >> 16)), (byte)((int)(255L & var1 >> 8)), (byte)((int)(255L & var1))};
      int var8 = 0;

      for(int var2 = 0; var2 < 4; ++var2) {
         this.a[var2] = var3[var8++] & 255 | (var3[var8++] & 255) << 8 | (var3[var8++] & 255) << 16 | (var3[var8++] & 255) << 24;
      }

   }

   final void a(int[] var1) {
      if (var1.length % 2 == 1) {
         for(int var2 = 1; var2 < var1.length; var2 += 2) {
            int var3 = 32;
            int var4 = var1[var2];
            int var5 = var1[var2 + 1];

            for(int var6 = 0; var3-- > 0; var5 += ((var4 << 4) + this.a[2] ^ var4) + (var6 ^ var4 >>> 5) + this.a[3]) {
               var6 -= 1640531527;
               var4 += ((var5 << 4) + this.a[0] ^ var5) + (var6 ^ var5 >>> 5) + this.a[1];
            }

            var1[var2] = var4;
            var1[var2 + 1] = var5;
         }
      }

   }

   final void b(int[] var1) {
      if (var1.length % 2 == 1) {
         for(int var2 = 1; var2 < var1.length; var2 += 2) {
            int var3 = 32;
            int var4 = var1[var2];
            int var5 = var1[var2 + 1];

            for(int var6 = -957401312; var3-- > 0; var6 += 1640531527) {
               var5 -= ((var4 << 4) + this.a[2] ^ var4) + (var6 ^ var4 >>> 5) + this.a[3];
               var4 -= ((var5 << 4) + this.a[0] ^ var5) + (var6 ^ var5 >>> 5) + this.a[1];
            }

            var1[var2] = var4;
            var1[var2 + 1] = var5;
         }
      }

   }

   static void a(byte[] var0, int[] var1, int var2) {
      if (var2 + (var0.length >> 2) <= var1.length) {
         int var3 = 0;
         int var4 = 24;
         int var5 = var2;

         for(var1[var2] = 0; var3 < var0.length; ++var3) {
            var1[var5] |= (var0[var3] & 255) << var4;
            if (var4 == 0) {
               var4 = 24;
               ++var5;
               if (var5 < var1.length) {
                  var1[var5] = 0;
               }
            } else {
               var4 -= 8;
            }
         }
      }

   }

   static byte[] a(int[] var0, int var1, int var2) {
      if (var2 <= var0.length - var1 << 2) {
         byte[] var3 = new byte[var2];
         var1 = var1;
         int var4 = 0;

         for(int var5 = 0; var5 < var2; ++var5) {
            var3[var5] = (byte)(var0[var1] >> 24 - (var4 << 3));
            ++var4;
            if (var4 == 4) {
               var4 = 0;
               ++var1;
            }
         }

         return var3;
      } else {
         return null;
      }
   }
}
