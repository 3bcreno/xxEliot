using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utils;
using EnsoulSharp.SDK.MenuUI;
using System;
using System.Linq;

namespace SkinChanger
{
    class Program
    {
        public static AIHeroClient Player => ObjectManager.Player;
        static void Main(string[] args)
        {
            Bootstrap.Init(args);
            GameEvent.OnGameLoad += GameEvent_OnGameLoad;
        }

        private static void GameEvent_OnGameLoad()
        {
            Chat.Print("<b><font color=\"#FFFFFF\">[</font></b><b><font color=\"#00e5e5\">SkinChanger</font></b><b><font color=\"#FFFFFF\">]</font></b><b><font color=\"#FFFFFF\"> Loaded</font></b>");
            Game.OnUpdate += OnUpdate;

            MenuConfig.Load();
        }

        private static void OnUpdate(EventArgs args)
        {
            SKIN();
        }

        public static void SKIN()
        {
            if (MenuConfig.UseSkin)
            {
                switch (MenuConfig.SkinChanger.Index)
                {
                    case 0:
                        Player.SetSkin(0);
                        break;
                    case 1:
                        Player.SetSkin(1);
                        break;
                    case 2:
                        Player.SetSkin(2);
                        break;
                    case 3:
                        Player.SetSkin(3);
                        break;
                    case 4:
                        Player.SetSkin(4);
                        break;
                    case 5:
                        Player.SetSkin(5);
                        break;
                    case 6:
                        Player.SetSkin(6);
                        break;
                    case 7:
                        Player.SetSkin(7);
                        break;
                    case 8:
                        Player.SetSkin(8);
                        break;
                    case 9:
                        Player.SetSkin(9);
                        break;
                    case 10: 
                        Player.SetSkin(10);
                        break;
                    case 11:
                        Player.SetSkin(11);
                        break;
                    case 12:
                        Player.SetSkin(12);
                        break;
                    case 13:
                        Player.SetSkin(13);
                        break;
                    case 14:
                        Player.SetSkin(14);
                        break;
                    case 15:
                        Player.SetSkin(15);
                        break;
                }

            }
            else
            {
                Player.SetSkin(Player.CharacterData.SkinID);
            }
        }
    }
}
