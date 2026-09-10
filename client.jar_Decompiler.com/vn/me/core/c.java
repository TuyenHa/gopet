package vn.me.core;

import javax.microedition.media.MediaException;
import javax.microedition.media.Player;
import thong.sdk.ISoundSDK;

public final class c implements ISoundSDK {
   private Player a;

   public c(Player var1) {
      this.a = var1;
   }

   public final void setLoopCount(int var1) {
      this.a.setLoopCount(var1);
   }

   public final void start() {
      try {
         this.a.start();
      } catch (MediaException var2) {
         var2.printStackTrace();
      }
   }

   public final void stop() {
      try {
         this.a.stop();
      } catch (MediaException var2) {
         var2.printStackTrace();
      }
   }

   public final void close() {
      this.a.close();
   }
}
