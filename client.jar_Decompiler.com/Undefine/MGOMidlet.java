package Undefine;

import javax.microedition.lcdui.Display;
import javax.microedition.midlet.MIDlet;
import vn.me.core.BaseCanvas;

public class MGOMidlet extends MIDlet {
   protected void destroyApp(boolean var1) {
      .cg.c();
   }

   protected void pauseApp() {
      .cg.e_();
   }

   protected void startApp() {
      if (Display.getDisplay(this).getCurrent() != null) {
         .cg.f_();
      } else {
         if (BaseCanvas.instance == null) {
            .cg.a((MIDlet)this);
         }

         .cg.a();
      }
   }

   public void RunApp() {
      this.startApp();
   }
}
