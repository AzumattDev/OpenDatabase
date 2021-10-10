using System.Linq;
using HarmonyLib;
using UnityEngine.SceneManagement;
using OpenDatabase.Handler;
using BepInEx.Configuration;
using UnityEngine;
using OpenDatabase.Utilities;

namespace OpenDatabase
{
    [HarmonyPatch]
    class Patches
    {

        static bool askedForYesNo = false;
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Load")]
        static void UpdateKnown()
        {
            if (SceneManager.GetActiveScene().name != "main") return;

            Player.m_localPlayer?.UpdateKnownRecipesList();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
        static void patchOtherDB()
        {

            if (SceneManager.GetActiveScene().name != "main") return;

            ItemsHandler.ReloadItems();
            RecipesHandler.ReloadRecipes();

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        static void PatchDB()
        {

            if (SceneManager.GetActiveScene().name != "main") return;

            ItemsHandler.ReloadItems();
            RecipesHandler.ReloadRecipes();
            
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.InputText))]
        static void ConsoleTextInput(Terminal __instance)
        {
            string command = __instance.m_input.text.ToLower();
            string _command = command;
            if (askedForYesNo) command = __instance.m_history.LastOrDefault();
            if (command.StartsWith("help"))
            {
                __instance.AddString("");
                __instance.AddString("[OpenDatabase]");
                __instance.AddString("opendatabase.generate all/items/recipes - Regenerates JSON files");
                __instance.AddString("opendatabase.reload - Reloads the database");
                __instance.AddString("opendatabase.config.reload - Reloads the config file");
            }

            if (command.Contains("opendatabase"))
            {
                if (SceneManager.GetActiveScene().name != "main")
                {
                    __instance.AddString("You need to be in a world to use this command!");
                    return;
                }
            }

            if (command == "opendatabase.reload")
            {
                __instance.AddString("Reloading database...");

                ItemsHandler.ReloadItems();
                RecipesHandler.ReloadRecipes();

                
                if (Player.m_localPlayer != null)
                {
                    
                    var list = Player.m_localPlayer.m_inventory.GetAllItems();
 
                    for (int i = 0; i< list.Count; i++)
                    {
                        var data = Helper.GetSharedDataBySharedName(list[i].m_shared.m_name);
                        list[i].m_shared = data;
                        
                    }
                    Player.m_localPlayer.UpdateKnownRecipesList();
                }

                __instance.AddString("Database has been reloaded!");
            }
            else if (command == "opendatabase.config.reload")
            {
                string _text = "";
                string _enabled;

                bool _en = OpenDatabase.modEnabled.Value;
                bool _ex = OpenDatabase.showZerosInJSON.Value;
                OpenDatabase.instance.Config.Reload();

                _enabled = (OpenDatabase.modEnabled.Value) ? "enabled" : "disabled";
                if (_en != OpenDatabase.modEnabled.Value)
                    _text = $"The mod has been {_enabled}. ";

                _enabled = (OpenDatabase.showZerosInJSON.Value) ? "shown" : "hidden";
                if (_ex != OpenDatabase.showZerosInJSON.Value)
                    _text += $"On JSON generation, zeros are {_enabled}";

                if (_text == "") _text = "Config file has been reloaded";

                __instance.AddString(_text);
            }
            else if (command.StartsWith("opendatabase.generate"))
            {
                string[] cmd = command.Split(' ');
                if (cmd.Length == 2)
                {
                    if (cmd[1] == "all")
                    {
                        if (!askedForYesNo)
                        {
                            __instance.AddString("All files inside plugins/OpenDatabase/ will be removed and regenerated. Are you sure ? yes/no");
                            askedForYesNo = true;
                        }
                        else
                        {
                            if (_command == "yes")
                            {
                                JSONHandler.ClearFolder(OpenDatabase.recipeFolder);
                                JSONHandler.ClearFolder(OpenDatabase.itemsFolder);
                                JSONHandler.CreateItemFiles();
                                JSONHandler.CreateRecipeFiles();

                                __instance.AddString("JSON Files are regenerated.");
                            }
                            askedForYesNo = false;
                        }
                    }
                    else if (cmd[1] == "items")
                    {
                        if (!askedForYesNo)
                        {
                            __instance.AddString("All files inside plugins/OpenDatabase/Items will be removed and regenerated. Are you sure ? yes/no");
                            askedForYesNo = true;
                        }
                        else
                        {
                            if (_command == "yes")
                            {
                                JSONHandler.ClearFolder(OpenDatabase.itemsFolder);
                                JSONHandler.CreateItemFiles();
                                __instance.AddString("Item Files are regenerated.");
                            }
                            askedForYesNo = false;
                        }
                    }
                    else if (cmd[1] == "recipes")
                    {
                        if (!askedForYesNo)
                        {
                            __instance.AddString("All files inside plugins/OpenDatabase/Recipes will be removed and regenerated. Are you sure ? yes/no");
                            askedForYesNo = true;
                        }
                        else
                        {
                            if (_command == "yes")
                            {
                                JSONHandler.ClearFolder(OpenDatabase.recipeFolder);
                                JSONHandler.CreateRecipeFiles();
                                __instance.AddString("Recipe Files are regenerated.");
                            }
                            askedForYesNo = false;
                        }
                    }
                }
            }
        }
  

    }
}
