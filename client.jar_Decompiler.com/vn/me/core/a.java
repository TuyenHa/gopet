package vn.me.core;

import javax.microedition.lcdui.Graphics;
import thong.sdk.IGameSDK;

final class a implements IGameSDK {
   private final BaseCanvas a;

   a(BaseCanvas var1) {
      this.a = var1;
   }

   public final void loop() {
      BaseCanvas.a(this.a);
   }

   public final void render() {
      BaseCanvas.b(this.a);
   }

   public final void setGraphics(Graphics var1) {
      BaseCanvas.g = var1;
   }
}
