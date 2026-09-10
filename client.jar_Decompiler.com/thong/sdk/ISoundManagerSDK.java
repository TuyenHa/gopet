package thong.sdk;

import java.io.IOException;
import java.io.InputStream;
import vn.me.core.BaseCanvas;

public abstract class ISoundManagerSDK {
   public static ISoundSDK currentSoundBg = null;
   public static boolean hasPermissionPlayBgSound = true;
   public static boolean hasPermissionPlayEffSound = true;
   public static final String hasPermissionPlayBgSoundRMS = "permissonBGSound";
   public static final String hasPermissionPlayEffSoundRMS = "permissonEffSound";
   protected int currentVolume = 100;

   public abstract ISoundSDK load(InputStream var1);

   public abstract ISoundSDK load(String var1);

   public void setVolume(int var1) {
      this.currentVolume = var1;
   }

   public static void loadMusicState() {
      hasPermissionPlayBgSound = .a.a("permissonBGSound", true);
      hasPermissionPlayEffSound = .a.a("permissonEffSound", true);
      if (!hasPermissionPlayBgSound && currentSoundBg != null) {
         currentSoundBg.stop();
      }

   }

   public static void saveMusicState() {
      .a.a("permissonBGSound", hasPermissionPlayBgSound);
      .a.a("permissonEffSound", hasPermissionPlayEffSound);
   }

   public static synchronized void setCurrentBgSound(ISoundSDK var0) {
      if (currentSoundBg != null) {
         currentSoundBg.stop();
         currentSoundBg.close();
      }

      currentSoundBg = var0;
      if (hasPermissionPlayBgSound && currentSoundBg != null) {
         currentSoundBg.setLoopCount(-1);
         currentSoundBg.start();
      }

   }

   public static void playSoundEffect(ISoundSDK var0) {
      if (var0 == null) {
         throw new NullPointerException();
      } else {
         if (hasPermissionPlayEffSound) {
            var0.start();
         }

      }
   }

   public static void playBgSound(String var0) {
      if (hasPermissionPlayBgSound) {
         try {
            setCurrentBgSound(BaseCanvas.soundManagerSDK.load("/sound/" + var0 + ".wav"));
         } catch (IOException var1) {
            var1.printStackTrace();
         }
      }
   }

   public static void playSoundEffect(String var0) {
      if (hasPermissionPlayEffSound) {
         try {
            playSoundEffect(BaseCanvas.soundManagerSDK.load("/sound/" + var0 + ".wav"));
         } catch (IOException var1) {
            var1.printStackTrace();
         }
      }
   }
}
