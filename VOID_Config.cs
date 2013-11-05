//
//  VOID_Config.cs
//
//  Author:
//       toadicus <>
//
//  Copyright (c) 2013 toadicus
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using KSP;
using UnityEngine;

namespace VOID
{
	public class VOID_Config
	{
		/*
		 * Static Members
		 * */
		// Singleton
		private static VOID_Config _instance;
		public static VOID_Config Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new VOID_Config();
				}
				return _instance;
			}
		}

		/*
		 * Fields
		 * */
		protected List<IVOID_Module> Modules = new List<IVOID_Module>();
		/*
		 * Properties
		 * */

		/*
		 * Methods
		 * */
		public void LoadConfig()
		{
			Tools.PostDebugMessage ("VOID: Loading settings.");
			var config = KSP.IO.PluginConfiguration.CreateForType<VOIDFlightMaster> ();
			config.load ();

			int loadedConfigVersion = config.GetValue ("ConfigVersion", 0);

			if (loadedConfigVersion < 1 || true)
			{
				this.load_old_settings ();
				// this.write_settings ();
				// TODO: Enable this when the config update works for sure.
				// KSP.IO.File.Delete<VOID> ("VOID.cfg");
				return;
			}

			this.main_window_pos = config.GetValue("MAIN_WINDOW_POS", main_window_pos);
			this.void_window_pos = config.GetValue("VOID_WINDOW_POS", void_window_pos);
			this.atmo_window_pos = config.GetValue("ATMO_WINDOW_POS", atmo_window_pos);
			this.transfer_window_pos = config.GetValue("TAD_WINDOW_POS", transfer_window_pos);
			this.vessel_register_window_pos = config.GetValue("VESREG_WINDOW_POS", vessel_register_window_pos);
			this.data_logging_window_pos = config.GetValue("DATATIME_WINDOW_POS", data_logging_window_pos);
			this.vessel_info_window_pos = config.GetValue("VESINFO_WINDOW_POS", vessel_info_window_pos);
			this.misc_window_pos = config.GetValue("MISC_WINDOW_POS", misc_window_pos);
			this.body_op_window_pos = config.GetValue("CELINFO_WINDOW_POS", body_op_window_pos);
			this.rendezvous_info_window_pos = config.GetValue("RENDEZVOUS_WINDOW_POS", rendezvous_info_window_pos);
			this.main_icon_pos = config.GetValue("ICON_POS", main_icon_pos);

			this.toggleHUDModule = config.GetValue("HUD_MODULE", false);
			this.void_module = config.GetValue("VOID_MODULE", false);
			this.atmo_module = config.GetValue("ATMO_MODULE", false);
			this.tad_module = config.GetValue("TAD_MODULE", false);
			this.vessel_register_module = config.GetValue("VESREG_MODULE", false);
			this.data_time_module = config.GetValue("DATATIME_MODULE", false);
			this.vessel_info_module = config.GetValue("VESINFO_MODULE", false);
			this.misc_module = config.GetValue("MISC_MODULE", false);
			this.celestial_body_info_module = config.GetValue("CELINFO_MODULE", false);
			this.body_op_show_orbital = config.GetValue("CELINFO_SHOW_OBTL", false);
			this.body_op_show_physical = config.GetValue("CELINFO_SHOW_PHYS", false);
			this.main_gui_minimized = config.GetValue("MAIN_GUI_MINIMIZED", false);
			this.hide_on_pause = config.GetValue("HIDE_ON_PAUSE", false);
			this.disable_power_usage = config.GetValue("DISABLE_POWER_USAGE", true);
			this.show_tooltips = config.GetValue("SHOW_TOOLTIPS", false);
			this.rendezvous_module = config.GetValue("SHOW_RENDEZVOUS_INFO", false);
			this.hide_vesreg_info = config.GetValue("USE_KSP_TARGET", false);

			this.skin_index = config.GetValue ("SKIN_INDEX", 0);

			this.user_lang = (languages)Enum.Parse(typeof(languages), config.GetValue ("USER_LANG", "EN"));
		}

		private void LoadOldConfig()
		{
			if (KSP.IO.File.Exists<VOID_Config>("VOID.cfg", null))
			{
				string[] data = KSP.IO.File.ReadAllLines<VOID_Config>("VOID.cfg", null);
				string[] name_val;
				string[] temp;
				string name = "";
				string val = "";

				foreach (string s in data)
				{
					name_val = s.Split('=');
					name = name_val[0].Trim();
					val = name_val[1].Trim();

					if (val != "")
					{
						if (name == "MAIN WINDOW POS")
						{
							temp = val.Split(',');
							//window_0_pos = new Rect(Convert.ToSingle(temp[0].Trim()), Convert.ToSingle(temp[1].Trim()), 10, 10);
							main_window_pos.x = Convert.ToSingle(temp[0].Trim());
							main_window_pos.y = Convert.ToSingle(temp[1].Trim());
						}
						if (name == "VOID WINDOW POS")
						{
							temp = val.Split(',');
							void_window_pos = new Rect(Convert.ToSingle(temp[0].Trim()), Convert.ToSingle(temp[1].Trim()), 10f, 10f);
						}
						if (name == "ATMO WINDOW POS")
						{
							temp = val.Split(',');
							atmo_window_pos = new Rect(Convert.ToSingle(temp[0].Trim()), Convert.ToSingle(temp[1].Trim()), 10f, 10f);
						}
						if (name == "TAD WINDOW POS")
						{
							temp = val.Split(',');
							transfer_window_pos = new Rect(Convert.ToSingle(temp[0].Trim()), Convert.ToSingle(temp[1].Trim()), 10f, 10f);
						}
						if (name == "VESREG WINDOW POS")
						{
							temp = val.Split(',');
							vessel_register_window_pos = new Rect(Convert.ToSingle(temp[0].Trim()), Convert.ToSingle(temp[1].Trim()), 10f, 10f);
						}
						if (name == "DATATIME WINDOW POS")
						{
							temp = val.Split(',');
							data_logging_window_pos = new Rect(Convert.ToSingle(temp[0].Trim()), Convert.ToSingle(temp[1].Trim()), 10f, 10f);
						}
						if (name == "VESINFO WINDOW POS")
						{
							temp = val.Split(',');
							vessel_info_window_pos = new Rect(Convert.ToSingle(temp[0].Trim()), Convert.ToSingle(temp[1].Trim()), 10f, 10f);
						}
						if (name == "MISC WINDOW POS")
						{
							temp = val.Split(',');
							misc_window_pos = new Rect(Convert.ToSingle(temp[0].Trim()), Convert.ToSingle(temp[1].Trim()), 10f, 10f);
						}
						if (name == "CELINFO WINDOW POS")
						{
							temp = val.Split(',');
							body_op_window_pos = new Rect(Convert.ToSingle(temp[0].Trim()), Convert.ToSingle(temp[1].Trim()), 10f, 10f);
						}
						if (name == "RENDEZVOUS WINDOW POS")
						{
							temp = val.Split(',');
							rendezvous_info_window_pos = new Rect(Convert.ToSingle(temp[0].Trim()), Convert.ToSingle(temp[1].Trim()), 10f, 10f);
						}
						if (name == "ICON POS")
						{
							temp = val.Split(',');
							main_icon_pos = new Rect(Convert.ToSingle(temp[0].Trim()), Convert.ToSingle(temp[1].Trim()), 30f, 30f);
						}
						if (name == "HUD MODULE") toggleHUDModule = Boolean.Parse(val);
						if (name == "VOID MODULE") void_module = Boolean.Parse(val);
						if (name == "ATMO MODULE") atmo_module = Boolean.Parse(val);
						if (name == "TAD MODULE") tad_module = Boolean.Parse(val);
						if (name == "VESREG MODULE") vessel_register_module = Boolean.Parse(val);
						if (name == "DATATIME MODULE") data_time_module = Boolean.Parse(val);
						if (name == "VESINFO MODULE") vessel_info_module = Boolean.Parse(val);
						if (name == "MISC MODULE") misc_module = Boolean.Parse(val);
						if (name == "CELINFO MODULE") celestial_body_info_module = Boolean.Parse(val);
						if (name == "CELINFO SHOW OBTL") body_op_show_orbital = Boolean.Parse(val);
						if (name == "CELINFO SHOW PHYS") body_op_show_physical = Boolean.Parse(val);
						if (name == "MAIN GUI MINIMIZED") main_gui_minimized = Boolean.Parse(val);
						if (name == "HIDE ON PAUSE") hide_on_pause = Boolean.Parse(val);
						if (name == "SKIN INDEX") skin_index = Int32.Parse(val);
						if (name == "DISABLE POWER USAGE") disable_power_usage = Boolean.Parse(val);
						if (name == "SHOW TOOLTIPS") show_tooltips = Boolean.Parse(val);
						if (name == "SHOW RENDEZVOUS INFO") rendezvous_module = Boolean.Parse(val);
						if (name == "USE KSP TARGET") hide_vesreg_info = Boolean.Parse(val);
						if (name == "USER LANG") user_lang = (languages)Enum.Parse(typeof(languages), val);
					}
				}
			}
		}

		public void SaveConfig()
		{
			Tools.PostDebugMessage ("VOID: Writing settings.");
			try
			{
				var config = KSP.IO.PluginConfiguration.CreateForType<VOIDFlightMaster> ();
				config.load ();

				/*
				config.SetValue ("ConfigVersion", this.configVersion);
				config.SetValue("MAIN_WINDOW_POS", main_window_pos);
				config.SetValue("VOID_WINDOW_POS", void_window_pos);
				config.SetValue("ATMO_WINDOW_POS", atmo_window_pos);
				config.SetValue("TAD_WINDOW_POS", transfer_window_pos);
				config.SetValue("VESREG_WINDOW_POS", vessel_register_window_pos);
				config.SetValue("DATATIME_WINDOW_POS", data_logging_window_pos);
				config.SetValue("VESINFO_WINDOW_POS", vessel_info_window_pos);
				config.SetValue("MISC_WINDOW_POS", misc_window_pos);
				config.SetValue("CELINFO_WINDOW_POS", body_op_window_pos);
				config.SetValue("RENDEZVOUS_WINDOW_POS", rendezvous_info_window_pos);
				config.SetValue("ICON_POS", main_icon_pos);
				config.SetValue("HUD_MODULE", toggleHUDModule);
				config.SetValue("VOID_MODULE", void_module);
				config.SetValue("ATMO_MODULE", atmo_module);
				config.SetValue("TAD_MODULE", tad_module);
				config.SetValue("VESREG_MODULE", vessel_register_module);
				config.SetValue("DATATIME_MODULE", data_time_module);
				config.SetValue("VESINFO_MODULE", vessel_info_module);
				config.SetValue("MISC_MODULE", misc_module);
				config.SetValue("CELINFO_MODULE", celestial_body_info_module);
				config.SetValue("CELINFO_SHOW_OBTL", body_op_show_orbital);
				config.SetValue("CELINFO_SHOW_PHYS", body_op_show_physical);
				config.SetValue("MAIN_GUI_MINIMIZED", main_gui_minimized);
				config.SetValue("HIDE_ON_PAUSE", hide_on_pause);
				config.SetValue("SKIN_INDEX", skin_index);
				config.SetValue("DISABLE_POWER_USAGE", disable_power_usage);
				config.SetValue("SHOW_TOOLTIPS", show_tooltips);
				config.SetValue("SHOW_RENDEZVOUS_INFO", rendezvous_module);
				config.SetValue("USE_KSP_TARGET", hide_vesreg_info);
				config.SetValue("USER_LANG", user_lang);
				*/

				try
				{
					config.SetValue ("ConfigVersion", this.configVersion);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue (\"ConfigVersion\", this.configVersion);");
				}
				try
				{
					config.SetValue("MAIN_WINDOW_POS", main_window_pos);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"MAIN_WINDOW_POS\", main_window_pos);");
				}
				try
				{
					config.SetValue("VOID_WINDOW_POS", void_window_pos);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"VOID_WINDOW_POS\", void_window_pos);");
				}
				try
				{
					config.SetValue("ATMO_WINDOW_POS", atmo_window_pos);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"ATMO_WINDOW_POS\", atmo_window_pos);");
				}
				try
				{
					config.SetValue("TAD_WINDOW_POS", transfer_window_pos);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"TAD_WINDOW_POS\", transfer_window_pos);");
				}
				try
				{
					config.SetValue("VESREG_WINDOW_POS", vessel_register_window_pos);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"VESREG_WINDOW_POS\", vessel_register_window_pos);");
				}
				try
				{
					config.SetValue("DATATIME_WINDOW_POS", data_logging_window_pos);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"DATATIME_WINDOW_POS\", data_logging_window_pos);");
				}
				try
				{
					config.SetValue("VESINFO_WINDOW_POS", vessel_info_window_pos);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"VESINFO_WINDOW_POS\", vessel_info_window_pos);");
				}
				try
				{
					config.SetValue("MISC_WINDOW_POS", misc_window_pos);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"MISC_WINDOW_POS\", misc_window_pos);");
				}
				try
				{
					config.SetValue("CELINFO_WINDOW_POS", body_op_window_pos);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"CELINFO_WINDOW_POS\", body_op_window_pos);");
				}
				try
				{
					config.SetValue("RENDEZVOUS_WINDOW_POS", rendezvous_info_window_pos);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"RENDEZVOUS_WINDOW_POS\", rendezvous_info_window_pos);");
				}
				try
				{
					config.SetValue("ICON_POS", main_icon_pos);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"ICON_POS\", main_icon_pos);");
				}
				try
				{
					config.SetValue("HUD_MODULE", toggleHUDModule);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"HUD_MODULE\", toggleHUDModule);");
				}
				try
				{
					config.SetValue("VOID_MODULE", void_module);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"VOID_MODULE\", void_module);");
				}
				try
				{
					config.SetValue("ATMO_MODULE", atmo_module);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"ATMO_MODULE\", atmo_module);");
				}
				try
				{
					config.SetValue("TAD_MODULE", tad_module);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"TAD_MODULE\", tad_module);");
				}
				try
				{
					config.SetValue("VESREG_MODULE", vessel_register_module);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"VESREG_MODULE\", vessel_register_module);");
				}
				try
				{
					config.SetValue("DATATIME_MODULE", data_time_module);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"DATATIME_MODULE\", data_time_module);");
				}
				try
				{
					config.SetValue("VESINFO_MODULE", vessel_info_module);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"VESINFO_MODULE\", vessel_info_module);");
				}
				try
				{
					config.SetValue("MISC_MODULE", misc_module);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"MISC_MODULE\", misc_module);");
				}
				try
				{
					config.SetValue("CELINFO_MODULE", celestial_body_info_module);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"CELINFO_MODULE\", celestial_body_info_module);");
				}
				try
				{
					config.SetValue("CELINFO_SHOW_OBTL", body_op_show_orbital);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"CELINFO_SHOW_OBTL\", body_op_show_orbital);");
				}
				try
				{
					config.SetValue("CELINFO_SHOW_PHYS", body_op_show_physical);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"CELINFO_SHOW_PHYS\", body_op_show_physical);");
				}
				try
				{
					config.SetValue("MAIN_GUI_MINIMIZED", main_gui_minimized);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"MAIN_GUI_MINIMIZED\", main_gui_minimized);");
				}
				try
				{
					config.SetValue("HIDE_ON_PAUSE", hide_on_pause);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"HIDE_ON_PAUSE\", hide_on_pause);");
				}
				try
				{
					config.SetValue("SKIN_INDEX", skin_index);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"SKIN_INDEX\", skin_index);");
				}
				try
				{
					config.SetValue("DISABLE_POWER_USAGE", disable_power_usage);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"DISABLE_POWER_USAGE\", disable_power_usage);");
				}
				try
				{
					config.SetValue("SHOW_TOOLTIPS", show_tooltips);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"SHOW_TOOLTIPS\", show_tooltips);");
				}
				try
				{
					config.SetValue("SHOW_RENDEZVOUS_INFO", rendezvous_module);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"SHOW_RENDEZVOUS_INFO\", rendezvous_module);");
				}
				try
				{
					config.SetValue("USE_KSP_TARGET", hide_vesreg_info);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"USE_KSP_TARGET\", hide_vesreg_info);");
				}
				try
				{
					config.SetValue("USER_LANG", user_lang);
				}
				catch (NullReferenceException)
				{
					UnityEngine.Debug.LogError("config.SetValue(\"USER_LANG\", user_lang);");
				}
				config.save ();
			}
			catch (NullReferenceException)
			{
				return;
			}
		}
	}
}

