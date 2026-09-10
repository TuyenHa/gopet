import java.util.Hashtable;

public final class dx {
   public dz[] a;
   public dz a;
   public dz b;
   private Hashtable a = new Hashtable(24);

   public final dz a(int var1) {
      Integer var2 = new Integer(var1);
      dz var3;
      if ((var3 = (dz)this.a.get(var2)) == null) {
         switch (var1) {
            case 0:
               var3 = bg.a("/pet/battle/skills/SongKich")[0];
               break;
            case 1:
               var3 = bg.a("/pet/battle/skills/Satthuong")[0];
               break;
            case 2:
               var3 = bg.a("/pet/battle/skills/CuongNo")[0];
               break;
            case 3:
               var3 = bg.a("/pet/battle/skills/Bang")[0];
               break;
            case 4:
               var3 = bg.a("/pet/battle/skills/SamSet")[0];
               break;
            case 5:
               var3 = bg.a("/pet/battle/skills/Lua")[0];
               break;
            case 6:
               var3 = bg.a("/pet/battle/skills/hadoc")[0];
               break;
            case 7:
               var3 = bg.a("/pet/battle/skills/daogam")[0];
               break;
            case 8:
               var3 = bg.a("/pet/battle/skills/voanh")[0];
               break;
            case 9:
               var3 = bg.a("/pet/battle/skills/fandame")[0];
               break;
            case 10:
               var3 = bg.a("/pet/battle/skills/hutmau")[0];
               break;
            case 11:
               var3 = bg.a("/pet/battle/skills/manaburn")[0];
               break;
            case 12:
               var3 = bg.a("/pet/battle/skills/thiencanthu")[0];
               break;
            case 13:
               var3 = bg.a("/pet/battle/skills/lienhoancuoc")[0];
               break;
            case 14:
               var3 = bg.a("/pet/battle/skills/lachan")[0];
               break;
            case 15:
               var3 = bg.a("/pet/battle/skills/Tornado")[0];
               break;
            case 16:
               var3 = bg.a("/pet/battle/skills/Xayda")[0];
               break;
            case 17:
               var3 = bg.a("/pet/battle/skills/MoonShine")[0];
               break;
            case 18:
               var3 = bg.a("/pet/battle/skills/Thorns")[0];
               break;
            case 19:
               var3 = bg.a("/pet/battle/skills/ThorHammer")[0];
               break;
            case 20:
               var3 = bg.a("/pet/battle/skills/Sword")[0];
               break;
            case 21:
               var3 = bg.a("/pet/battle/skills/Shuriken")[0];
               break;
            case 22:
               var3 = bg.a("/pet/battle/skills/ZeusWraith")[0];
               break;
            case 23:
               var3 = bg.a("/pet/battle/skills/Meteor")[0];
         }

         this.a.put(var2, var3);
      }

      return var3;
   }

   public final void a() {
      this.a.clear();
   }
}
