import java.util.Vector;

public final class eq implements Runnable {
   private eo a;
   private Vector a = new Vector();

   public eq(eo var1) {
      this.a = var1;
   }

   public final void a(en var1) {
      synchronized(this.a) {
         this.a.addElement(var1);
         this.a.notifyAll();
      }
   }

   public final void run() {
      while(true) {
         try {
            if (!this.a.a) {
               return;
            }

            synchronized(this.a) {
               while(this.a.size() > 0) {
                  if (this.a.a) {
                     en var2 = (en)this.a.elementAt(0);
                     this.a.removeElementAt(0);
                     byte[] var4;
                     if ((var4 = var2.a()) != null) {
                        if (var2.a) {
                           byte[] var5 = var4;
                           er var10 = this.a.a;
                           int[] var6;
                           (var6 = new int[((var5.length >> 3) + (var5.length % 8 == 0 ? 0 : 1) << 1) + 1])[0] = var5.length;
                           er.a(var5, var6, 1);
                           var10.a(var6);
                           var4 = er.a(var6, 0, var6.length << 2);
                        }

                        this.a.a.writeInt(var4.length + 1);
                        this.a.a.writeByte(var2.a ? 1 : 0);
                        this.a.a.write(var4);
                        eo var10000 = this.a;
                        var10000.a += var4.length;
                     } else {
                        this.a.a.writeInt(0);
                     }

                     eo var11 = this.a;
                     var11.a += 4;
                     this.a.a.flush();
                  }
               }

               try {
                  this.a.wait();
               } catch (InterruptedException var7) {
               }
            }
         } catch (Exception var9) {
            return;
         }
      }
   }

   public final void a() {
      synchronized(this.a) {
         this.a.removeAllElements();
         this.a.notifyAll();
      }
   }

   protected final void a(long var1) {
      byte[] var3 = new byte[9];
      this.a.getClass();
      var3[0] = 9;
      var3[1] = (byte)((int)(var1 >>> 56));
      var3[2] = (byte)((int)(var1 >>> 48));
      var3[3] = (byte)((int)(var1 >>> 40));
      var3[4] = (byte)((int)(var1 >>> 32));
      var3[5] = (byte)((int)(var1 >>> 24));
      var3[6] = (byte)((int)(var1 >>> 16));
      var3[7] = (byte)((int)(var1 >>> 8));
      var3[8] = (byte)((int)var1);
      this.a.a.write(var3);
      this.a.a.flush();
   }
}
