using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;

namespace SkinChanger
{
    class MenuConfig
    {
        public Menu TargetSelectorMenu;
        private const string MenuName = "E#SkinChanger";
        public static Menu MainMenu { get; set; } = new Menu(MenuName, MenuName, true);
        public static void Load()
        {
           SkinMenu = MainMenu.Add(new Menu("SkinChanger", "SkinChanger"));
           UseSkin =  SkinMenu.Add(new MenuBool("UseSkin", "Use SkinChanger"));
           SkinChanger = SkinMenu.Add(new MenuList("Skins", "Skins", new[] { "Default", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" , "11" , "12" , "13","14","15"}));
            MainMenu.Attach();
        }
        public static Menu SkinMenu;
        public static MenuBool UseSkin;
        public static MenuList SkinChanger;
    }
}