import java.io.DataInputStream;
import vn.me.core.BaseCanvas;

public final class eg extends eh implements gz {
   private dv a;
   private int a;
   private int b;
   private int c;
   private int d;
   private int e;
   private int f;
   private int g = 0;
   private int h = 1;
   private long b = System.currentTimeMillis();

   public eg() {
   }

   public static eg a(dv var0, DataInputStream var1) {
      try {
         byte var2 = var1.readByte();
         byte var3 = var1.readByte();
         short var4 = var1.readShort();
         short var5 = var1.readShort();
         byte var6 = var1.readByte();
         byte var7 = var1.readByte();
         byte var8 = var1.readByte();
         byte var9 = var1.readByte();
         byte var10 = var1.readByte();
         byte var11 = -1;
         byte var12 = 0;
         String var13 = "";
         if (var2 != 0) {
            var3 = -1;
            var11 = var1.readByte();
            var12 = var1.readByte();
            var13 = var1.readUTF();
            var1.readByte();
         }

         return new eg(var0, var4, var5, var6, var7, var8, var9, var10, var3, var11, var12, var13);
      } catch (Exception var14) {
         var14.printStackTrace();
         return null;
      }
   }

   private eg(dv var1, int var2, int var3, int var4, int var5, int var6, int var7, int var8, byte var9, int var10, int var11, String var12) {
      this.a = var1;
      this.a = var9;
      eg var13;
      String var14;
      if (this.a != -1) {
         var13 = this;
         switch (this.a) {
            case 0:
               var14 = "Nhà hẻm";
               break;
            case 1:
               var14 = "Nhà mặt tiền";
               break;
            case 2:
               var14 = "Biệt thự";
               break;
            case 3:
               var14 = "Dinh thự";
               break;
            case 4:
               var14 = "Nhà";
               break;
            case 5:
            default:
               var14 = "";
               break;
            case 6:
               var14 = "Thú cưng";
               break;
            case 7:
               var14 = "Vườn";
               break;
            case 8:
               var14 = "Phòng vé";
               break;
            case 9:
               var14 = "";
               break;
            case 10:
               var14 = "Cà phê";
               break;
            case 11:
               var14 = "Khu";
               break;
            case 12:
               var14 = "Hộp thư";
               break;
            case 13:
               var14 = "Caro";
               break;
            case 14:
               var14 = "Cờ tướng";
               break;
            case 15:
               var14 = "Tiến lên";
               break;
            case 16:
               var14 = "Phỏm";
               break;
            case 17:
               var14 = "Thời trang";
               break;
            case 18:
               var14 = "Nón";
               break;
            case 19:
               var14 = "Giày";
               break;
            case 20:
               var14 = "Mỹ viện";
               break;
            case 21:
               var14 = "Tóc";
               break;
            case 22:
               var14 = "Vật phẩm";
               break;
            case 23:
               var14 = "Gara";
               break;
            case 24:
               var14 = "Trò chơi trong nhà";
               break;
            case 25:
               var14 = "Pet shop";
               break;
            case 26:
               var14 = "Đấu trường";
               break;
            case 27:
               var14 = "";
               break;
            case 28:
               var14 = "";
               break;
            case 29:
               var14 = "";
               break;
            case 30:
               var14 = "";
               break;
            case 31:
               var14 = "";
               break;
            case 32:
               var14 = "";
         }
      } else {
         this.h = true;
         var13 = this;
         var14 = var12;
      }

      var13.e = var14;
      this.i = var2;
      this.j = var3;
      this.f = var4;
      this.b = var7;
      this.c = var8;
      this.d = var10;
      this.e = var11;
      this.a(new gy(var5, var6, this.b, this.c));
      this.a = new cd("Chọn", this);
   }

   public final void a(int var1, int var2) {
      long var3;
      if ((var3 = System.currentTimeMillis()) - this.b > 50L) {
         this.b = var3;
         this.g += this.h;
         if (this.g < -1 || this.g > 2) {
            this.h = -this.h;
         }
      }

      var2 = -var2 + this.j - this.f - cp.g.getHeight() - this.m;
      if (this.a == -1) {
         cp.c().a(BaseCanvas.g, this.e, this.i - var1, var2 + 20, 3);
      } else {
         cp.c().a(BaseCanvas.g, this.e, this.i - var1, var2 - 23, 3);
      }
   }

   public final void a_(int var1, int var2) {
      long var3;
      if ((var3 = System.currentTimeMillis()) - this.b > 50L) {
         this.b = var3;
         this.g += this.h;
         if (this.g < -1 || this.g > 2) {
            this.h = -this.h;
         }
      }

      if (this.a != -1) {
         BaseCanvas.g.drawImage(cp.g, this.i - var1, -var2 + this.j - this.f - cp.g.getHeight() - this.m, 17);
      }

   }

   public final void a(Object var1) {
      switch (this.a) {
         case -1:
            dv.a(this.d, this.e, ef.a(this.d));
            return;
         case 0:
            this.a.a(new Object[]{new cd(2, "", this.a)});
            return;
         case 1:
            this.a.a(new Object[]{new cd(3, "", this.a)});
            return;
         case 2:
            this.a.a(new Object[]{new cd(4, "", this.a)});
            return;
         case 3:
            this.a.a(new Object[]{new cd(5, "", this.a)});
            return;
         case 4:
            this.a.a(new Object[]{new cd(502, "", this.a)});
            return;
         case 5:
         case 6:
         case 24:
         case 25:
         default:
            return;
         case 7:
            this.a.a(new Object[]{new cd(10, "", this.a)});
            return;
         case 8:
            dv var10000 = this.a;
            dv.a(this, new gn[]{new gj(a.a(574)), new gb(new cd(1202, a.a(134), this.a)), new gb(new cd(1203, a.a(471), this.a))});
            return;
         case 9:
            cg.a.a((Object)(new Object[]{new cd(18, "", cg.a)}));
            return;
         case 10:
            this.a.a(new Object[]{new cd(2019, "", this.a)});
            return;
         case 11:
            this.a.a(new Object[]{new cd(1103, "", this.a)});
            return;
         case 12:
            this.a.a(new Object[]{new cd(2020, "", this.a)});
            return;
         case 13:
            this.a.a(new Object[]{new cd(602, "", this.a)});
            return;
         case 14:
            this.a.a(new Object[]{new cd(603, "", this.a)});
            return;
         case 15:
            this.a.a(new Object[]{new cd(606, "", this.a)});
            return;
         case 16:
            this.a.a(new Object[]{new cd(607, "", this.a)});
            return;
         case 17:
            cg.f();
            en var5;
            (var5 = new en(81)).a(60);
            cx.a.a(var5);
            var5.a();
            return;
         case 18:
            this.a.a(new Object[]{new cd(1003, "", this.a)});
            return;
         case 19:
            this.a.a(new Object[]{new cd(1004, "", this.a)});
            return;
         case 20:
            this.a.a(new Object[]{new cd(1005, "", this.a)});
            return;
         case 21:
            this.a.a(new Object[]{new cd(1006, "", this.a)});
            return;
         case 22:
            this.a.a(new Object[]{new cd(1009, "", this.a)});
            return;
         case 23:
            this.a.a(new Object[]{new cd(1008, "", this.a)});
            return;
         case 26:
            cg.f();
            en var4;
            (var4 = new en(81)).a(58);
            var4.a(0);
            cx.a.a(var4);
            var4.a();
            return;
         case 27:
            cg.f();
            dc.d(1);
            return;
         case 28:
            cg.f();
            dc.d(2);
            return;
         case 29:
            cg.f();
            dc.d(3);
            return;
         case 30:
            cg.f();
            dc.d(4);
            return;
         case 31:
            cg.f();
            en var3;
            (var3 = new en(81)).a(21);
            cx.a.a(var3);
            var3.a();
            return;
         case 32:
            dj var2;
            if ((var2 = (dj)cq.a().a).a == null) {
               cg.a_("Bạn không dẫn theo pet");
            } else {
               dj.a = 1;
               cg.f();
               dc.b(var2.a.b);
            }
      }
   }
}
