import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;

public final class en {
   public int a;
   private ByteArrayOutputStream a;
   private DataOutputStream a;
   private DataInputStream a;
   public boolean a;

   public en(int var1) {
      this(var1, (byte)0);
   }

   public en(int var1, byte var2) {
      this.a = true;
      this.a = var1;
      this.a = true;
   }

   public en(byte[] var1) {
      this.a = true;
      byte[] var2 = new byte[var1.length - 1];
      System.arraycopy(var1, 1, var2, 0, var2.length);
      this.a = var1[0];
      this.a = new DataInputStream(new ByteArrayInputStream(var2));
   }

   public final byte[] a() {
      if (this.a == null) {
         return new byte[]{(byte)this.a};
      } else {
         byte[] var1;
         byte[] var2;
         (var2 = new byte[(var1 = this.a.toByteArray()).length + 1])[0] = (byte)this.a;
         System.arraycopy(var1, 0, var2, 1, var1.length);
         return var2;
      }
   }

   public final DataInputStream a() {
      return this.a;
   }

   private DataOutputStream a() {
      if (this.a == null) {
         this.a = new ByteArrayOutputStream();
         this.a = new DataOutputStream(this.a);
      }

      return this.a;
   }

   public final void a(int var1) {
      try {
         this.a().writeByte(var1);
      } catch (IOException var2) {
      }
   }

   public final void a(String var1) {
      try {
         this.a().writeUTF(var1);
      } catch (IOException var2) {
      }
   }

   public final void b(int var1) {
      try {
         this.a().writeInt(var1);
      } catch (IOException var2) {
      }
   }

   public final void a(boolean var1) {
      try {
         this.a().writeBoolean(var1);
      } catch (IOException var2) {
      }
   }

   public final void a() {
      try {
         if (this.a != null) {
            this.a.close();
         }

         if (this.a != null) {
            this.a.close();
         }

      } catch (IOException var1) {
      }
   }
}
