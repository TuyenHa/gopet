package thong.sdk;

public interface IPlatformSDK {
   IAsssetSDK getAssetSDK();

   ISoundManagerSDK getSoundManagerSDK();

   void setGameSDK(IGameSDK var1);
}
