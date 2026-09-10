import java.io.DataInputStream;
import java.io.DataOutputStream;
import javax.microedition.io.Connector;
import javax.microedition.io.SocketConnection;

public final class eo {
   public cy a;
   public DataOutputStream a;
   public DataInputStream a;
   private SocketConnection a;
   public boolean a;
   private eq a;
   private ep a;
   public int a;
   public int b;
   public er a;
   public String a;
   public int c;
   public boolean b = true;

   public final void a(ep var1) {
      this.a = var1;
   }

   public final void a(eq var1) {
      this.a = var1;
   }

   public final void a(en var1) {
      this.a.a(var1);
   }

   public final void a() {
      this.a.a();
   }

   public final void b() {
      try {
         this.a = null;
         this.c = -1;
         this.a = false;
         if (this.a != null) {
            this.a.a();
         }

         if (this.a != null) {
            this.a.close();
            this.a = null;
         }

         if (this.a != null) {
            this.a.close();
            this.a = null;
         }

         if (this.a != null) {
            this.a.close();
            this.a = null;
         }

         this.a = 0;
         this.b = 0;
      } catch (Exception var1) {
      }
   }

   public static void a(eo var0, String var1, int var2) {
      try {
         var0.a = (SocketConnection)Connector.open("socket://" + var1 + ":" + var2);
         var0.a = var0.a.openDataOutputStream();
         var0.a = var0.a.openDataInputStream();
         long var3 = System.currentTimeMillis();
         var0.a.a(var3);
         var0.a = new er(var3);
         var0.a = true;
         (new Thread(var0.a)).start();
         (new Thread(var0.a)).start();
         var0.a = var1;
         var0.c = var2;
      } catch (Exception var5) {
         var5.printStackTrace();
      }
   }
}
