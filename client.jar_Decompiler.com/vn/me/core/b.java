package vn.me.core;

import java.io.InputStream;
import javax.microedition.media.Manager;
import javax.microedition.media.MediaException;
import thong.sdk.ISoundManagerSDK;
import thong.sdk.ISoundSDK;

public final class b extends ISoundManagerSDK {
   public final ISoundSDK load(InputStream var1) {
      try {
         return new vn.me.core.c(Manager.createPlayer(var1, "audio/x-wav"));
      } catch (MediaException var2) {
         var2.printStackTrace();
         return null;
      }
   }

   public final ISoundSDK load(String var1) {
      return this.load(.gv.a(var1));
   }
}
