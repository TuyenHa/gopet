import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.Vector;
import javax.microedition.rms.RecordStore;
import vn.me.core.BaseCanvas;

public class a {
   public static int a = 1;
   public short[] a;
   public short[] b;
   public short[] c;
   public short[] d;
   public int[] a;
   public int[] b;
   public int[] c;
   public int[] d;
   public short[] e;
   short[] f;
   boolean a = false;
   Vector a = new Vector();
   public static String a = "mLanguage";
   private static Class a;

   private a(boolean var1) {
   }

   public static a a(String var0, bm var1) {
      if (var1 == null) {
         System.out.println("defpackage.ActorFactory.Method485()");
         throw new IllegalArgumentException("Image Loader cannot be null");
      } else {
         a var2 = new a(false);
         InputStream var3 = null;

         try {
            if (BaseCanvas.iPlatformSDK == null) {
               var3 = (a == null ? (a = a("a")) : a).getResourceAsStream(var0);
            } else {
               var3 = BaseCanvas.iPlatformSDK.getAssetSDK().load(var0);
            }
         } catch (Exception var19) {
            var19.printStackTrace();
         }

         DataInputStream var4 = new DataInputStream(var3);

         try {
            try {
               var4.readShort();
               var4.readUTF();
               byte var26;
               short[] var5 = new short[(var26 = var4.readByte()) << 1];

               for(int var6 = 0; var6 < var26; ++var6) {
                  var5[2 * var6] = var4.readShort();
                  var5[2 * var6 + 1] = var4.readShort();
               }

               short var28;
               short[] var27 = new short[(var28 = var4.readShort()) << 2];

               for(int var7 = 0; var7 < var28; ++var7) {
                  var27[4 * var7] = var4.readShort();
                  var27[4 * var7 + 1] = (short)var4.readByte();
                  var27[4 * var7 + 2] = var4.readShort();
                  var27[4 * var7 + 3] = var4.readShort();
               }

               short[] var34 = new short[var4.readShort()];
               var28 = var4.readShort();
               short var8 = 0;
               short[] var9 = new short[var28 << 1];

               for(int var10 = 0; var10 < var28; ++var10) {
                  var9[2 * var10] = var8;
                  short var11 = var4.readShort();

                  for(short var12 = 0; var12 < var11; ++var12) {
                     short var13 = (short)(var8 + 1);
                     var34[var8] = var4.readShort();
                     var8 = (short)(var13 + 1);
                     var34[var13] = var4.readShort();
                     short var14 = (short)(var8 + 1);
                     var34[var8] = var4.readShort();
                     var8 = (short)(var14 + 1);
                     var34[var14] = (short)var4.readByte();
                  }

                  var9[2 * var10 + 1] = (short)(var8 - 1);
               }

               short var37 = var4.readShort();
               byte var40 = var4.readByte();
               short[] var42 = new short[var37 << 2];
               short var43 = 0;
               short[] var36 = new short[var40];
               short var47 = 0;

               for(int var30 = 0; var30 < var40; ++var30) {
                  var36[var30] = var47;
                  var37 = var4.readShort();

                  for(short var15 = 0; var15 < var37; ++var15) {
                     short var16 = (short)(var43 + 1);
                     var42[var43] = var4.readShort();
                     var43 = (short)(var16 + 1);
                     var42[var16] = var4.readShort();
                     short var17 = (short)(var43 + 1);
                     var42[var43] = var4.readShort();
                     var43 = (short)(var17 + 1);
                     var42[var17] = var4.readShort();
                  }

                  var47 += var37;
                  var2.a.addElement(var1.a(var0));
               }

               int[] var39 = new int[(var28 = var4.readShort()) * 5];

               for(int var48 = 0; var48 < var28; ++var48) {
                  var39[5 * var48] = var4.readShort();
                  var39[5 * var48 + 1] = var4.readShort();
                  var39[5 * var48 + 2] = var4.readShort();
                  var39[5 * var48 + 3] = var4.readShort();
                  var39[5 * var48 + 4] = var4.readInt();
               }

               short var49;
               int[] var50 = new int[(var49 = var4.readShort()) * 3];

               for(int var45 = 0; var45 < var49; ++var45) {
                  var50[3 * var45] = var4.readShort();
                  var50[3 * var45 + 1] = var4.readShort();
                  var50[3 * var45 + 2] = var4.readInt();
               }

               int[] var51 = new int[(var43 = var4.readShort()) * 3];

               for(int var22 = 0; var22 < var43; ++var22) {
                  var51[3 * var22] = var4.readShort();
                  var51[3 * var22 + 1] = var4.readShort();
                  var51[3 * var22 + 2] = var4.readInt();
               }

               short var23;
               int[] var25 = new int[(var23 = var4.readShort()) * 5];

               for(int var32 = 0; var32 < var23; ++var32) {
                  var25[5 * var32] = var4.readShort();
                  var25[5 * var32 + 1] = var4.readShort();
                  var25[5 * var32 + 2] = var4.readShort();
                  var25[5 * var32 + 3] = var4.readShort();
                  var25[5 * var32 + 4] = var4.readInt();
               }

               short[] var24 = new short[(var28 = var4.readShort()) << 1];

               for(int var41 = 0; var41 < var28; ++var41) {
                  var24[2 * var41] = var4.readShort();
               }

               var4.close();
               var2.a = var5;
               var2.b = var27;
               var2.f = var9;
               var2.c = var34;
               var2.d = var42;
               var2.a = var39;
               var2.b = var50;
               var2.c = var51;
               var2.d = var25;
               var2.e = var36;
               System.out.println("defpackage.ActorFactory.Method485() z");
               return var2;
            } catch (Exception var20) {
               var20.printStackTrace();
            }
         } catch (Throwable var21) {
            try {
               var4.close();
            } catch (IOException var18) {
               var18.printStackTrace();
            }
         }

         return null;
      }
   }

   public static String a(int var0) {
      switch (a) {
         case 0:
            switch (var0) {
               case 3:
                  return "Tiện ích";
               case 4:
                  return "Chấp nhận";
               case 5:
                  return "T.Khoản";
               case 9:
                  return "Thêm bạn";
               case 18:
                  return "Nhiệm vụ";
               case 21:
                  return "Quay lại";
               case 24:
                  return "Ngân hàng";
               case 25:
                  return "Đậu";
               case 30:
                  return "Game cần mở trình duyệt web. Hãy thoát game nếu như không thấy hiển thị trang web.";
               case 40:
                  return "Gọi trợ giúp";
               case 41:
                  return "Thôi";
               case 46:
                  return "Không thể gởi tin nhắn đăng ký. Xin kiểm tra tiền và thử khởi động lại game.";
               case 51:
                  return "Đổi";
               case 55:
                  return "Nạp";
               case 58:
                  return "Chat";
               case 63:
                  return "Xóa dữ liệu";
               case 64:
                  return "Đóng";
               case 73:
                  return "Bạn muốn xoá dữ liệu?";
               case 77:
                  return "Bạn muốn thoát?";
               case 82:
                  return "Kết nối thất bại. Xin kiểm tra lại GPRS, 3G, Wifi hoặc cập nhật lại máy chủ.";
               case 83:
                  return "Đang kết nối...";
               case 87:
                  return "Tiếp tục";
               case 93:
                  return "ngày";
               case 94:
                  return "Xóa hết";
               case 95:
                  return "Xóa";
               case 101:
                  return "Từ chối";
               case 103:
                  return "Hủy";
               case 116:
                  return "Tải";
               case 123:
                  return "Cảm xúc";
               case 129:
                  return "English";
               case 130:
                  return "Nhập tên nhân vật";
               case 132:
                  return "Vui lòng nhập Nick ID muốn đăng ký vào ô trên.";
               case 133:
                  return "Bạn phải nhập password đăng ký.";
               case 134:
                  return "Khu giải trí";
               case 138:
                  return "Sự kiện";
               case 139:
                  return "Thoát";
               case 140:
                  return "Kb, bạn có muốn thoát khỏi ứng dụng không?";
               case 154:
                  return "Quên mật khẩu";
               case 159:
                  return "Bạn bè";
               case 167:
                  return "đã được người khác sử dụng. Xin chọn tên khác.";
               case 174:
                  return "Bạn phải dùng số điện thoại đăng ký nick để lấy mật khẩu.";
               case 178:
                  return "Tên muốn lấy mật khẩu không được rỗng!";
               case 184:
                  return "mGold";
               case 185:
                  return "Đổi vàng lấy đậu";
               case 186:
                  return "Đổi vàng lấy thóc";
               case 189:
                  return "Đến khu số";
               case 203:
                  return "Bạn có tin nhắn mới.";
               case 205:
                  return "Hỗ trợ";
               case 209:
                  return "giờ";
               case 212:
                  return "Bạn phải có 1 mã số để nạp.";
               case 213:
                  return "Bạn phải có thẻ cào có 2 mã số để nạp.";
               case 220:
                  return "Thông tin";
               case 221:
                  return "vào danh sách bạn bè?";
               case 222:
                  return "Giới thiệu";
               case 223:
                  return "Không thể lấy thông tin\nVui lòng xem thông tin tại đia chỉ\nhttps://gopettae.com.";
               case 231:
                  return "Vào";
               case 247:
                  return "Ngôn ngữ";
               case 249:
                  return "Quy định";
               case 265:
                  return "Đang đăng nhập...";
               case 266:
                  return "Đăng nhập";
               case 267:
                  return "Đăng xuất";
               case 275:
                  return "Menu";
               case 276:
                  return "Tin nhắn";
               case 278:
                  return "phút";
               case 284:
                  return "Di chuyển";
               case 291:
                  return "Bạn cần có ít nhất 1000 đồng trong tài khoản chính để lấy mật khẩu.";
               case 298:
                  return "Tên";
               case 299:
                  return "Chuyển cho";
               case 300:
                  return "Không";
               case 307:
                  return "Bạn không có tin nhắn mới.";
               case 322:
                  return "Password bạn vừa gõ không khớp với password phía trên.";
               case 330:
                  return "Nếu chưa có tên,";
               case 331:
                  return "Nếu chưa có tên, xin đăng ký";
               case 337:
                  return "OK";
               case 344:
                  return "Hoặc soạn tin nhắn ";
               case 348:
                  return "M.Kh:";
               case 350:
                  return "Thành công, xin vui lòng chờ hệ thống gởi tin nhắn mật khẩu mới cho bạn";
               case 352:
                  return "Xin chờ...";
               case 353:
                  return "\nBản quyền 2011 ME Corp.\nGiấy phép cung cấp MXH số 35/GXN-TTĐT. Cấp ngày 06/05/2011.";
               case 356:
                  return "Số điện thoại này có phải là số dùng để đăng ký nick không?";
               case 362:
                  return "xin đăng ký.";
               case 363:
                  return "Xin chờ";
               case 364:
                  return "Trước";
               case 371:
                  return "Tên chương trình";
               case 375:
                  return "Mật khẩu";
               case 376:
                  return "Số lượng";
               case 384:
                  return "Khu vực";
               case 385:
                  return "Đăng ký";
               case 386:
                  return "Đang đăng ký...";
               case 387:
                  return "Đã gửi thông tin đăng ký thành công. Xin thoát game và chờ giây lát.";
               case 390:
                  return "Xóa bộ nhớ tạm thành công.";
               case 391:
                  return "Thông báo";
               case 393:
                  return "Tên không được trống.";
               case 394:
                  return "Mật khẩu không được rỗng.";
               case 397:
                  return "Nhập lại";
               case 400:
                  return "Thóc";
               case 419:
                  return "Chọn";
               case 422:
                  return "Chọn nhân vật";
               case 428:
                  return "Chọn máy chủ";
               case 435:
                  return "Đang gửi tin nhắn...";
               case 439:
                  return "Không thể gởi tin nhắn nạp tiền. Xin kiểm tra tiền và thử khởi động lại game.";
               case 440:
                  return "Đã nạp tiền xong. Xin chờ tin nhắn xác nhận. Lưu ý bạn chỉ nạp được 3 lần trong 5 phút.";
               case 441:
                  return " Gửi tới ";
               case 459:
                  return "Cấu hình";
               case 471:
                  return "Khu mua sắm";
               case 473:
                  return "Chưa hoàn thành nhập";
               case 482:
                  return "Gõ lại";
               case 488:
                  return "Nhớ thông tin";
               case 491:
                  return "Thành công!";
               case 495:
                  return "Góp ý";
               case 496:
                  return "Không hỗ trợ phương thức này.";
               case 503:
                  return "Tặng bạn bè";
               case 504:
                  return "Cảm ơn bạn đã góp ý!";
               case 519:
                  return "Nhập tên cần lấy lại mật khẩu";
               case 532:
                  return "Tặng mGold";
               case 548:
                  return "Cập nhật";
               case 552:
                  return "Kết nối thất bại. Bạn có muốn cập nhật thông tin máy chủ không?";
               case 553:
                  return "Không thể cập nhật danh sách máy chủ. Mời bạn thử lại sau";
               case 557:
                  return "Phiên bản";
               case 558:
                  return "Rung";
               case 559:
                  return "Tiếng Việt";
               case 560:
                  return "Xem";
               case 574:
                  return "Bạn muốn đi đâu?";
               case 580:
                  return "Có";
               case 595:
                  return "của bạn";
               case 596:
                  return "mGold hiện tại của bạn";
               case 604:
                  return "Bạn đã dùng ";
               case 611:
                  return "Chế độ rung";
               case 664:
                  return "Sau";
               case 666:
                  return "KHU VỰC";
               case 667:
                  return "Thách đấu";
               case 668:
                  return "Chuyển map";
               case 670:
                  return "Trò chơi trong nhà";
               case 671:
                  return "Thú cưng";
               default:
                  return String.valueOf(var0);
            }
         case 1:
            switch (var0) {
               case 3:
                  return "Utilities";
               case 4:
                  return "Accept";
               case 5:
                  return "Account";
               case 9:
                  return "Add friend";
               case 18:
                  return "Task";
               case 21:
                  return "Back";
               case 24:
                  return "Bank";
               case 25:
                  return "Đậu";
               case 30:
                  return "The game needs to open a web browser. Quit the game if you don't see the website displayed.";
               case 40:
                  return "Need helps";
               case 41:
                  return "Stop";
               case 46:
                  return "Unable to send registration messages. Please check the money and try to restart the game.";
               case 51:
                  return "Change";
               case 55:
                  return "Nạp";
               case 58:
                  return "Chat";
               case 63:
                  return "Clear cache";
               case 64:
                  return "Close";
               case 73:
                  return "Do you want clear cache?";
               case 77:
                  return "Do you want exit?";
               case 82:
                  return "... Connection failed. Please check GPRS, 3G, Wifi or update the server again.";
               case 83:
                  return "Conecting.....";
               case 87:
                  return "Continue...";
               case 93:
                  return "day";
               case 94:
                  return "Remove all";
               case 95:
                  return "remove";
               case 101:
                  return "Refuse";
               case 103:
                  return "Cancel";
               case 116:
                  return "Download";
               case 123:
                  return "Emotion";
               case 129:
                  return "English";
               case 130:
                  return "Type your character name";
               case 132:
                  return "Please enter the Nick ID you wish to register in the box above.";
               case 133:
                  return "You must enter your registration password.";
               case 134:
                  return "Recreation area";
               case 138:
                  return "Events";
               case 139:
                  return "Exit";
               case 140:
                  return "Kb, do you want to quit the app?";
               case 154:
                  return "Forgot password";
               case 159:
                  return "Friend";
               case 167:
                  return "have been used by others. Please choose a different name.";
               case 174:
                  return "You must use the nick registration phone number to get the password.";
               case 178:
                  return "The name you want to get the password must not be empty!";
               case 184:
                  return "mGold";
               case 185:
                  return "Đổi vàng lấy đậu";
               case 186:
                  return "Đổi vàng lấy thóc";
               case 189:
                  return "Go to place";
               case 203:
                  return "You have new message.";
               case 205:
                  return "Help";
               case 209:
                  return "hour";
               case 212:
                  return "You must have 1 code to deposit.";
               case 213:
                  return "You must have a scratch card with 2 numbers to load.";
               case 220:
                  return "Information";
               case 221:
                  return "to your friends list?";
               case 222:
                  return "Introduce";
               case 223:
                  return "Unable to obtain informationn\nPlease see the information at the addressn\nhttp://gopettae.com.";
               case 231:
                  return "Enter";
               case 247:
                  return "Language";
               case 249:
                  return "Regulation";
               case 265:
                  return "Logging in...";
               case 266:
                  return "Log";
               case 267:
                  return "Logout";
               case 275:
                  return "Menu";
               case 276:
                  return "Message";
               case 278:
                  return "phút";
               case 284:
                  return "Move";
               case 291:
                  return "You need to have at least 1000 dong in the main account to get the password.";
               case 298:
                  return "Name";
               case 299:
                  return "Transfer to";
               case 300:
                  return "No";
               case 307:
                  return "You don't have new messages.";
               case 322:
                  return "The password you just typed does not match the password above.";
               case 330:
                  return "If you dont have a name,";
               case 331:
                  return "If you don't have a name, please register";
               case 337:
                  return "OK";
               case 344:
                  return "Or compose a message ";
               case 348:
                  return "Pass:";
               case 350:
                  return "Success, please wait for the system to send you a new password message";
               case 352:
                  return "Wating...";
               case 353:
                  return "";
               case 356:
                  return "Is this phone number used to register a nick?";
               case 362:
                  return "please register.";
               case 363:
                  return "Please wait";
               case 364:
                  return "Before";
               case 371:
                  return "Program Name";
               case 375:
                  return "Password";
               case 376:
                  return "Amount";
               case 384:
                  return "Zones";
               case 385:
                  return "Register";
               case 386:
                  return "Registing...";
               case 387:
                  return "Registration information has been successfully submitted. Please quit the game and wait a moment.";
               case 390:
                  return "Cache cleared successfully.";
               case 391:
                  return "Announcement";
               case 393:
                  return "Names must not be blank.";
               case 394:
                  return "The password must not be empty.";
               case 397:
                  return "Re-enter";
               case 400:
                  return "Rice grains";
               case 419:
                  return "Select";
               case 422:
                  return "Select character";
               case 428:
                  return "Select server";
               case 435:
                  return "Đang gửi tin nhắn...";
               case 439:
                  return "Unable to send a recharge message. Please check the money and try to restart the game.";
               case 440:
                  return "The deposit has been completed. Please wait for the confirmation message. Note that you can only reload 3 times in 5 minutes.";
               case 441:
                  return " Send to ";
               case 459:
                  return "Config";
               case 471:
                  return "Market place";
               case 473:
                  return "Incomplete input in the cells";
               case 482:
                  return "Re-type";
               case 488:
                  return "Remember the information";
               case 491:
                  return "Successful!";
               case 495:
                  return "Comments";
               case 496:
                  return "This method is not supported.";
               case 503:
                  return "Give to Friends";
               case 504:
                  return "Thank you for your feedback!";
               case 519:
                  return "Enter the name you want to retrieve the password";
               case 532:
                  return "Tặng mGold";
               case 548:
                  return "Update";
               case 552:
                  return "Connection failed. Do you want to update the server information?";
               case 553:
                  return "The server list cannot be updated. Please try again later";
               case 557:
                  return "Version";
               case 558:
                  return "Rung";
               case 559:
                  return "Vietnamese";
               case 560:
                  return "View";
               case 574:
                  return "Where do you want to go?";
               case 580:
                  return "Yes";
               case 595:
                  return "of you";
               case 596:
                  return "mGold hiện tại của bạn";
               case 604:
                  return "Bạn đã dùng ";
               case 611:
                  return "Chế độ rung";
               case 664:
                  return "Sau";
               case 666:
                  return "ZONE";
               case 667:
                  return "Challenge";
               case 668:
                  return "Map transfer";
               case 670:
                  return "Indoor Games";
               case 671:
                  return "Pet";
               default:
                  return String.valueOf(var0);
            }
         default:
            return "";
      }
   }

   public static ee a(int var0, byte var1, dv var2) {
      return new df(var0, var1, var2);
   }

   public static byte[] a(String var0) {
      RecordStore var1 = null;

      try {
         RecordStore var7;
         var1 = var7 = RecordStore.openRecordStore(var0, false);
         byte[] var8 = var7.getRecord(1);
         if (var1 != null) {
            try {
               var1.closeRecordStore();
            } catch (Exception var4) {
            }
         }

         return var8;
      } catch (Exception var5) {
         if (var1 != null) {
            try {
               var1.closeRecordStore();
               return null;
            } catch (Exception var3) {
               return null;
            }
         } else {
            return null;
         }
      } catch (Throwable var6) {
         if (var1 != null) {
            try {
               var1.closeRecordStore();
            } catch (Exception var2) {
            }
         }

         return null;
      }
   }

   public static void a(String var0, byte[] var1) {
      try {
         RecordStore var3;
         if ((var3 = RecordStore.openRecordStore(var0, true)).getNumRecords() > 0) {
            var3.setRecord(1, var1, 0, var1.length);
         } else {
            var3.addRecord(var1, 0, var1.length);
         }

         var3.closeRecordStore();
      } catch (Exception var2) {
         var2.printStackTrace();
      }
   }

   public static void a(String var0, String var1) {
      try {
         a(var0, var1.getBytes("UTF-8"));
      } catch (Exception var2) {
      }
   }

   public static String a(String var0) {
      byte[] var2;
      if ((var2 = a(var0)) == null) {
         return null;
      } else {
         try {
            return new String(var2, "UTF-8");
         } catch (Exception var1) {
            return new String(var2);
         }
      }
   }

   public static void a(String var0, int var1) {
      ByteArrayOutputStream var2 = new ByteArrayOutputStream();
      DataOutputStream var3 = new DataOutputStream(var2);

      try {
         var3.writeInt(var1);
         a(var0, var2.toByteArray());

         try {
            var3.close();
         } catch (Exception var5) {
            return;
         }
      } catch (Exception var7) {
         try {
            var3.close();
         } catch (Exception var4) {
            return;
         }
      } catch (Throwable var8) {
         try {
            var3.close();
            return;
         } catch (Exception var6) {
         }
      }

   }

   public static boolean a(String var0, boolean var1) {
      byte[] var7;
      if ((var7 = a(var0)) != null) {
         DataInputStream var8 = new DataInputStream(new ByteArrayInputStream(var7));

         try {
            var1 = var8.readBoolean();

            try {
               var8.close();
            } catch (Exception var4) {
            }

            return var1;
         } catch (Exception var5) {
            try {
               var8.close();
               return true;
            } catch (Exception var2) {
               return true;
            }
         } catch (Throwable var6) {
            try {
               var8.close();
            } catch (Exception var3) {
            }
         }
      }

      return true;
   }

   public static void a(String var0, boolean var1) {
      ByteArrayOutputStream var2 = new ByteArrayOutputStream();
      DataOutputStream var3 = new DataOutputStream(var2);

      try {
         var3.writeBoolean(var1);
         a(var0, var2.toByteArray());

         try {
            var3.close();
         } catch (Exception var5) {
            return;
         }
      } catch (Exception var7) {
         try {
            var3.close();
         } catch (Exception var4) {
            return;
         }
      } catch (Throwable var8) {
         try {
            var3.close();
            return;
         } catch (Exception var6) {
         }
      }

   }

   public static Integer a(String var0) {
      byte[] var7;
      if ((var7 = a(var0)) != null) {
         DataInputStream var8 = new DataInputStream(new ByteArrayInputStream(var7));

         try {
            int var1 = var8.readInt();

            try {
               var8.close();
            } catch (Exception var4) {
            }

            return new Integer(var1);
         } catch (Exception var5) {
            try {
               var8.close();
               return null;
            } catch (Exception var2) {
               return null;
            }
         } catch (Throwable var6) {
            try {
               var8.close();
            } catch (Exception var3) {
            }
         }
      }

      return null;
   }

   public static void a(String var0) {
      try {
         RecordStore.deleteRecordStore(var0);
      } catch (Exception var1) {
      }
   }

   public static void a() {
      String[] var0;
      if ((var0 = RecordStore.listRecordStores()) != null) {
         for(int var1 = 0; var1 < var0.length; ++var1) {
            String var2 = var0[var1];

            try {
               RecordStore.deleteRecordStore(var2);
            } catch (Exception var3) {
            }
         }

      }
   }

   private static Class a(String var0) {
      try {
         return Class.forName(var0);
      } catch (ClassNotFoundException var1) {
         throw new NoClassDefFoundError(var1.getMessage());
      }
   }
}
