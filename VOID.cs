///////////////////////////////////////////////////////////////////////////////
//
//    VOID - Vessel Orbital Information Display for Kerbal Space Program
//    Copyright (C) 2012 Iannic-ann-od
//    Copyright (C) 2013 Toadicus
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
///////////////////////////////////////////////////////////////////////////////
//
//  Much, much credit to Younata, Adammada, Nivvydaskrl and to all the authors
//  behind MechJeb, RemoteTech Relay Network, ISA MapSat, and Protractor for some
//  invaluable functions and making your nicely written code available to learn from.
//
///////////////////////////////////////////////////////////////////////////////
//
//  This software uses VesselSimulator and Engineer.Extensions from Engineer Redux.
//  Engineer Redux (c) 2013 cybutek
//  Used by permission.
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Engineer.VesselSimulator;

namespace VOID
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class VOID : MonoBehaviour
    {
        private bool debugging = false;

        private int window_base_id = -96518722;

        private double G_constant = .0000000000667;

        private Vessel vessel;
        private VOIDLabels label_strings = new VOIDLabels();

        private List<Color> hud_text_colors = new List<Color>();
        private List<CelestialBody> all_bodies = new List<CelestialBody>();
        private List<VesselType> all_vessel_types = new List<VesselType>();

        //Windows
        protected Rect main_window_pos = new Rect(Screen.width / 2, Screen.height / 2, 10f, 10f); //VOID main
        protected Rect void_window_pos = new Rect((Screen.width / 2) - 20, (Screen.height / 2) - 10, 10, 10);	//vessel orbital info
        protected Rect atmo_window_pos = new Rect((Screen.width / 2) - 40, (Screen.height / 2) - 20, 10, 10);	//atmospheric info
        protected Rect transfer_window_pos = new Rect((Screen.width / 2) - 60, (Screen.height / 2) - 30, 10, 10);	//transfer angle info
        protected Rect vessel_register_window_pos = new Rect((Screen.width / 2) - 80, (Screen.height / 2) - 40, 10, 10);    //neighboring vessels
        protected Rect data_logging_window_pos = new Rect((Screen.width / 2) - 100, (Screen.height / 2) - 50, 10, 10);   //data logging & time
        protected Rect vessel_info_window_pos = new Rect((Screen.width / 2) - 120, (Screen.height / 2) - 60, 10, 10);   //vessel info
        protected Rect misc_window_pos = new Rect((Screen.width / 2) - 160, (Screen.height / 2) - 80, 10, 10);   //miscellaneous
        protected Rect body_op_window_pos = new Rect((Screen.width / 2) - 140, (Screen.height / 2) - 70, 10, 10);  //body orbital/physical info
        protected Rect rendezvous_info_window_pos = new Rect((Screen.width / 2) - 180, (Screen.height / 2) - 90, 10, 10);  //target vessel info
        protected Rect lab_window_pos = new Rect(0, 0, 10, 10);    //The Lab

        //Icons
        protected Rect main_icon_pos;
        private Texture2D void_icon_off = new Texture2D(30, 30, TextureFormat.ARGB32, false);
        private Texture2D void_icon_on = new Texture2D(30, 30, TextureFormat.ARGB32, false);
        private Texture2D void_icon = new Texture2D(30, 30, TextureFormat.ARGB32, false);

        //GUI
        private bool gui_running = false;
        private int skin_index = 2;
        private bool main_gui_minimized = false;
        private bool gui_styles_set = false;

        //GUIStyles
        //private GUIStyle label_txt_left;
        private GUIStyle label_txt_center;
        private GUIStyle label_txt_center_bold;
        private GUIStyle label_txt_right;   //needed for body OP alignment
        //private GUIStyle button_txt_left;
        //private GUIStyle button_txt_center;
        //private GUIStyle button_txt_right;
        private GUIStyle label_hud;
        //private GUIStyle gs_tooltip;

        //Window toggles
        private bool void_module = false;
        private bool extended_orbital_info = false;
        private bool tad_module = false;
        private bool atmo_module = false;
        private bool data_time_module = false;
        private bool vessel_info_module = false;
        private bool test_module = false;
        private bool misc_module = false;
        private bool vessel_register_module = false;
        private bool hud_module = false;
        private bool celestial_body_info_module = false;
        private bool rendezvous_module = false;

        //Celestial body info window
        private bool body_op_show_orbital = true;
        private bool body_op_show_physical = true;
        private int body_OP_body_1_index = 1;
        private int body_OP_body_2_index = 2;
        private CelestialBody body_OP_selected_body_1 = new CelestialBody();
        private CelestialBody body_OP_selected_body_2 = new CelestialBody();

        //Data logging/time & cfg update
        private bool csv_logging = false;
        private List<string> csvList = new List<string>();
        private float csvWriteTimer = 0;
        private float csvCollectTimer = 0;
        private bool first_write = true;
        private float csv_log_interval;
        private string csv_log_interval_str = "0.5";
        private float cfg_update_timer = 0;
        private double stopwatch1;
        private bool stopwatch1_running = false;

        //Vessel Register
        private Vector2 vessel_register_scroll_pos = new Vector2();
        private Vessel vesreg_selected_vessel;
        private bool target_vessel_extended_orbital_info = false;
        private VesselType vessel_register_vessel_type = VesselType.Ship;
        private string vessel_register_vessel_situation = "Orbiting";
        private CelestialBody vessel_register_selected_body = null;
        private int vessel_register_body_index = 0;
        private int vessel_register_vessel_type_index = 0;
        private bool hide_vesreg_info = false;  //in rendezvous window

        //Miscellaneous window & http
        private string version_name = "VOID ";
        private string this_version = "0.9.1";
        private string latest_version = "";
        private bool misc_recvd_latest_version = false;
        private bool hide_on_pause = false;
        private int counter_hud_text_color = 0;
        private bool changing_icon_pos = false;
        private bool show_tooltips = true;
        private bool disable_power_usage = true;

        // Power consumption
        private float power_request_amount = 0.01f;
        private bool power_toggle = true;
        private bool power_available = false;

		// VesselSimulator
		Stage[] stages = new Stage[0];

        //Debugging / limbo
        private List<CelestialBody> tad_selected_bodies = new List<CelestialBody>();    //Bodies selected in Transfer Angle Info
        private bool run_once = true;   //for Update()
        private string settings_path;

        private enum languages { EN };
        private languages user_lang = languages.EN;
        private int user_lang_index;
        


        private void Innsewerants_writeData(string[] csvArray)
        {
            var efile = KSP.IO.File.AppendText<VOID>(vessel.vesselName + "_data.csv", null);
            foreach (string line in csvArray)
            {
                efile.Write(line);
            }
            efile.Close();
        }

        private void line_to_csvList()
        {
            //called if logging is on and interval has passed
            //writes one line to the csvList

            string line = "";
            if (first_write && !KSP.IO.File.Exists<VOID>(vessel.vesselName + "_data.csv", null))
            {
                first_write = false;
                line += "Mission Elapsed Time (s);Altitude ASL (m);Altitude above terrain (m);Orbital Velocity (m/s);Surface Velocity (m/s);Vertical Speed (m/s);Horizontal Speed (m/s);Gee Force (gees);Temperature (°C);Gravity (m/s²);Atmosphere Density (g/m³);\n";
            }
            //Mission time
            line += vessel.missionTime.ToString("F3") + ";";
            //Altitude ASL
            line += vessel.orbit.altitude.ToString("F3") + ";";
            //Altitude (true)
            double alt_true = vessel.orbit.altitude - vessel.terrainAltitude;
            if (vessel.terrainAltitude < 0) alt_true = vessel.orbit.altitude;
            line += alt_true.ToString("F3") + ";";
            //Orbital velocity
            line += vessel.orbit.vel.magnitude.ToString("F3") + ";";
            //surface velocity
            line += vessel.srf_velocity.magnitude.ToString("F3") + ";";
            //vertical speed
            line += vessel.verticalSpeed.ToString("F3") + ";";
            //horizontal speed
            line += vessel.horizontalSrfSpeed.ToString("F3") + ";";
            //gee force
            line += vessel.geeForce.ToString("F3") + ";";
            //temperature
            line += vessel.flightIntegrator.getExternalTemperature().ToString("F2") + ";";
            //gravity
            double r_vessel = vessel.mainBody.Radius + vessel.mainBody.GetAltitude(vessel.findWorldCenterOfMass());
            double g_vessel = (G_constant * vessel.mainBody.Mass) / Math.Pow(r_vessel, 2);
            line += g_vessel.ToString("F3") + ";";
            //atm density
            line += (vessel.atmDensity * 1000).ToString("F3") + ";";
            line += "\n";
            if (csvList.Contains(line) == false) csvList.Add(line);
            csvCollectTimer = 0f;
        }

        private void main_csv()
        {
            // CSV Logging
            // from ISA MapSat
            if (csv_logging)
            {
                //data logging is on
                //increment timers
                csvWriteTimer += Time.deltaTime;
                csvCollectTimer += Time.deltaTime;
            }
            else
            {
                //data logging is off
                //reset any timers and clear anything from csvList
                csvWriteTimer = 0f;
                csvCollectTimer = 0f;
                if (csvList.Count > 0) csvList.Clear();
            }

            if (csv_logging && csvCollectTimer >= csv_log_interval && vessel.situation != Vessel.Situations.PRELAUNCH)
            {
                //data logging is on, vessel is not prelaunch, and interval has passed
                //write a line to the list
                line_to_csvList();  //write to the csv
            }

            if (csvList.Count != 0 && csvWriteTimer >= 15f && csv_logging)
            {
                // csvList is not empty and interval between writings to file has elapsed
                //write it
                string[] csvData;
                csvData = (string[])csvList.ToArray();
                Innsewerants_writeData(csvData);
                csvList.Clear();
                csvWriteTimer = 0f;
            }
        }

        private void load_language()
        {
            ConfigNode lang_node = ConfigNode.Load(settings_path + "lang.dat");

            if (lang_node != null)
            {
                if (lang_node.HasNode(user_lang.ToString()))
                {
                    ConfigNode _lang = lang_node.GetNode(user_lang.ToString());

                    if (_lang.HasValue("void_primary")) label_strings.void_primary = _lang.GetValue("void_primary");
                    if (_lang.HasValue("void_altitude_asl")) label_strings.void_altitude_asl = _lang.GetValue("void_altitude_asl");
                    if (_lang.HasValue("void_velocity")) label_strings.void_velocity = _lang.GetValue("void_velocity");
                    if (_lang.HasValue("void_apoapsis")) label_strings.void_apoapsis = _lang.GetValue("void_apoapsis");
                    if (_lang.HasValue("void_periapsis")) label_strings.void_periapsis = _lang.GetValue("void_periapsis");
                }
            }
            else
            {
                //lang_node == null
				// Toadicus edit: added settings_path to the debug message for extra verbosity
                Debug.LogError("[VOID] Unable to load file " + settings_path + "lang.dat!");
            }
        }

        private void load_icons()
        {
			// Toadicus edit: changed the directory to VOID instead of RBR, because RBR seemed confusing.
            string path_icon_on = "VOID/Textures/void_icon_on";
            string path_icon_off = "VOID/Textures/void_icon_off";

            if (GameDatabase.Instance.ExistsTexture(path_icon_on) && GameDatabase.Instance.ExistsTexture(path_icon_off))
            {
                if (debugging) Debug.Log("icon textures exist, loading...");
                void_icon_on = GameDatabase.Instance.GetTexture(path_icon_on, false);
                void_icon_off = GameDatabase.Instance.GetTexture(path_icon_off, false);
                if (main_gui_minimized) void_icon = void_icon_off;
                else void_icon = void_icon_on;
            }
            else
            {
                if (debugging) Debug.LogError("[VOID] icon texture file(s) missing");
                //void_icons_loaded = false;
            }

            /*
            if (void_icons_loaded)
            {
                //print("VOID::icon textures load OK");
                main_icon_pos = new Rect((Screen.width / 2) - 250, Screen.height - 32, 30, 30);
                if (main_gui_minimized) void_icon = void_icon_off;
                else void_icon = void_icon_on;
            }
            else
            {
                //print("VOID::icon textures load ERROR");
                main_icon_pos = new Rect((Screen.width / 2) - 250, Screen.height - 22, 60, 20);
            }
            */
        }

        private void load_settings()
        {
            if (KSP.IO.File.Exists<VOID>("VOID.cfg", null))
            {
                string[] data = KSP.IO.File.ReadAllLines<VOID>("VOID.cfg", null);
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
                        if (name == "HUD MODULE") hud_module = Boolean.Parse(val);
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
                        if (name == "HUD COLOR") counter_hud_text_color = Int32.Parse(val);
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

        private void write_settings()
        {
            string settings = "";
            settings += "MAIN WINDOW POS = " + main_window_pos.x + " , " + main_window_pos.y + "\n";
            settings += "VOID WINDOW POS = " + void_window_pos.x + " , " + void_window_pos.y + "\n";
            settings += "ATMO WINDOW POS = " + atmo_window_pos.x + " , " + atmo_window_pos.y + "\n";
            settings += "TAD WINDOW POS = " + transfer_window_pos.x + " , " + transfer_window_pos.y + "\n";
            settings += "VESREG WINDOW POS = " + vessel_register_window_pos.x + " , " + vessel_register_window_pos.y + "\n";
            settings += "DATATIME WINDOW POS = " + data_logging_window_pos.x + " , " + data_logging_window_pos.y + "\n";
            settings += "VESINFO WINDOW POS = " + vessel_info_window_pos.x + " , " + vessel_info_window_pos.y + "\n";
            settings += "MISC WINDOW POS = " + misc_window_pos.x + " , " + misc_window_pos.y + "\n";
            settings += "CELINFO WINDOW POS = " + body_op_window_pos.x + " , " + body_op_window_pos.y + "\n";
            settings += "RENDEZVOUS WINDOW POS = " + rendezvous_info_window_pos.x + " , " + rendezvous_info_window_pos.y + "\n";
            settings += "ICON POS = " + main_icon_pos.x + " , " + main_icon_pos.y + "\n";
            settings += "HUD MODULE = " + hud_module + "\n";
            settings += "VOID MODULE = " + void_module + "\n";
            settings += "ATMO MODULE = " + atmo_module + "\n";
            settings += "TAD MODULE = " + tad_module + "\n";
            settings += "VESREG MODULE = " + vessel_register_module + "\n";
            settings += "DATATIME MODULE = " + data_time_module + "\n";
            settings += "VESINFO MODULE = " + vessel_info_module + "\n";
            settings += "MISC MODULE = " + misc_module + "\n";
            settings += "CELINFO MODULE = " + celestial_body_info_module + "\n";
            settings += "CELINFO SHOW OBTL = " + body_op_show_orbital + "\n";
            settings += "CELINFO SHOW PHYS = " + body_op_show_physical + "\n";
            settings += "MAIN GUI MINIMIZED = " + main_gui_minimized + "\n";
            settings += "HIDE ON PAUSE = " + hide_on_pause + "\n";
            settings += "HUD COLOR = " + counter_hud_text_color + "\n";
            settings += "SKIN INDEX = " + skin_index + "\n";
            settings += "DISABLE POWER USAGE = " + disable_power_usage + "\n";
            settings += "SHOW TOOLTIPS = " + show_tooltips + "\n";
            settings += "SHOW RENDEZVOUS INFO = " + rendezvous_module + "\n";
            settings += "USE KSP TARGET = " + hide_vesreg_info + "\n";
            settings += "USER LANG = " + user_lang + "\n";
            KSP.IO.File.WriteAllText<VOID>(settings, "VOID.cfg", null);
        }

        private void start_GUI()
        {
            RenderingManager.AddToPostDrawQueue(3, new Callback(draw_GUI));	//start the GUI
            gui_running = true;
        }

        private void stop_GUI()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(draw_GUI));	//stop the GUI
            gui_running = false;
        }

        private void consume_resources()
        {
            if (TimeWarp.deltaTime == 0) return;    //do nothing if paused
            if (vessel.vesselType == VesselType.EVA || disable_power_usage) power_available = true;    //power always available when EVA
            else
            {
                float recvd_amount = vessel.rootPart.RequestResource("ElectricCharge", power_request_amount * TimeWarp.fixedDeltaTime);
                if (recvd_amount > 0) power_available = true;    // doesn't always send 100% of demand so as long as it sends something
                else power_available = false;
            }
        }

        private void set_gui_styles()
        {
            //label_txt_left = new GUIStyle(GUI.skin.label);
            //label_txt_left.normal.textColor = Color.white;
            //label_txt_left.alignment = TextAnchor.UpperLeft;

            label_txt_center = new GUIStyle(GUI.skin.label);
            label_txt_center.normal.textColor = Color.white;
            label_txt_center.alignment = TextAnchor.UpperCenter;

            label_txt_center_bold = new GUIStyle(GUI.skin.label);
            label_txt_center_bold.normal.textColor = Color.white;
            label_txt_center_bold.alignment = TextAnchor.UpperCenter;
            label_txt_center_bold.fontStyle = FontStyle.Bold;

            label_txt_right = new GUIStyle(GUI.skin.label);
            label_txt_right.normal.textColor = Color.white;
            label_txt_right.alignment = TextAnchor.UpperRight;

            //button_txt_left = new GUIStyle(GUI.skin.button);
            //button_txt_left.normal.textColor = Color.white;
            //button_txt_left.alignment = TextAnchor.UpperLeft;

            //button_txt_center = new GUIStyle(GUI.skin.button);
            //button_txt_center.normal.textColor = Color.white;
            //button_txt_center.alignment = TextAnchor.UpperCenter;

            //button_txt_right = new GUIStyle(GUI.skin.button);
            //button_txt_right.normal.textColor = Color.white;
            //button_txt_right.alignment = TextAnchor.UpperRight;

            label_hud = new GUIStyle(GUI.skin.label);
            label_hud.normal.textColor = hud_text_colors[counter_hud_text_color];

            //gs_tooltip = new GUIStyle(GUI.skin.box);
            //gs_tooltip.normal.background = GUI.skin.window.normal.background;
            //gs_tooltip.normal.textColor = XKCDColors.LightGrey;
            //gs_tooltip.fontSize = 9;

            gui_styles_set = true;
        }

        private void set_hud_color_list()
        {
            hud_text_colors.Add(Color.green);
            hud_text_colors.Add(Color.black);
            hud_text_colors.Add(Color.white);
            hud_text_colors.Add(Color.red);
            hud_text_colors.Add(Color.blue);
            hud_text_colors.Add(Color.yellow);
            hud_text_colors.Add(Color.gray);
            hud_text_colors.Add(Color.cyan);
            hud_text_colors.Add(Color.magenta);
        }

        private void body_OP_show_orbital_info(CelestialBody body)
        {
            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label((body.orbit.ApA / 1000).ToString("##,#") + "km", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label(Tools.ConvertInterval(body.orbit.timeToAp), label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label((body.orbit.PeA / 1000).ToString("##,#") + "km", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label(Tools.ConvertInterval(body.orbit.timeToPe), label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label((body.orbit.semiMajorAxis / 1000).ToString("##,#") + "km", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label(body.orbit.eccentricity.ToString("F4") + "", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label(Tools.ConvertInterval(body.orbit.period), label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label(Tools.ConvertInterval(body.rotationPeriod), label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label((body.orbit.orbitalSpeed / 1000).ToString("F2") + "km/s", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			// Toadicus edit: convert mean anomaly into degrees.
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label((body.orbit.meanAnomaly * 180d / Math.PI).ToString("F3") + "°", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label(body.orbit.trueAnomaly.ToString("F3") + "°", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			// Toadicus edit: convert eccentric anomaly into degrees.
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label((body.orbit.eccentricAnomaly * 180d / Math.PI).ToString("F3") + "°", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label(body.orbit.inclination.ToString("F3") + "°", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label(body.orbit.LAN.ToString("F3") + "°", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label(body.orbit.argumentOfPeriapsis.ToString("F3") + "°", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (body.bodyName == "Sun") GUILayout.Label("N/A", label_txt_right, GUILayout.ExpandWidth(true));
            else
            {
                string body_tidally_locked = "No";
                if (body.tidallyLocked) body_tidally_locked = "Yes";
                GUILayout.Label(body_tidally_locked, label_txt_right, GUILayout.ExpandWidth(true));
            }
            //GUILayout.EndHorizontal();
        }

        private void body_OP_show_physical_info(CelestialBody body)
        {
            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label((body.Radius / 1000).ToString("##,#") + "km", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(((Math.Pow((body.Radius), 2) * 4 * Math.PI) / 1000).ToString("0.00e+00") + "km²", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            // divide by 1000 to convert m to km
            GUILayout.Label((((4d / 3) * Math.PI * Math.Pow(body.Radius, 3)) / 1000).ToString("0.00e+00") + "km³", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.Label(((4 / 3) * Math.PI * Math.Pow((vessel.mainBody.Radius / 1000), 3)).ToString(), txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(body.Mass.ToString("0.00e+00") + "kg", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            double p = body.Mass / (Math.Pow(body.Radius, 3) * (4d / 3) * Math.PI);
            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(p.ToString("##,#") + "kg/m³", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (name == "Sun") GUILayout.Label(body.sphereOfInfluence.ToString(), label_txt_right, GUILayout.ExpandWidth(true));
            else GUILayout.Label((body.sphereOfInfluence / 1000).ToString("##,#") + "km", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(body.orbitingBodies.Count.ToString(), label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //show # artificial satellites
            int num_art_sats = 0;
            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if (v.mainBody == body && v.situation.ToString() == "ORBITING") num_art_sats++;
            }

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(num_art_sats.ToString(), label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            double g_ASL = (G_constant * body.Mass) / Math.Pow(body.Radius, 2);
            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(Tools.MuMech_ToSI(g_ASL) + "m/s²", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("≈ " + Tools.MuMech_ToSI(body.maxAtmosphereAltitude) + "m", label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            string O2 = "No";
            if (body.atmosphereContainsOxygen == true) O2 = "Yes";
            GUILayout.Label(O2, label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            string ocean = "No";
            if (body.ocean == true) ocean = "Yes";
            GUILayout.Label(ocean, label_txt_right, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();
        }

        private string get_heading_text(double heading)
        {
            if (heading > 348.75 || heading <= 11.25) return "N";
            else if (heading > 11.25 && heading <= 33.75) return "NNE";
            else if (heading > 33.75 && heading <= 56.25) return "NE";
            else if (heading > 56.25 && heading <= 78.75) return "ENE";
            else if (heading > 78.75 && heading <= 101.25) return "E";
            else if (heading > 101.25 && heading <= 123.75) return "ESE";
            else if (heading > 123.75 && heading <= 146.25) return "SE";
            else if (heading > 146.25 && heading <= 168.75) return "SSE";
            else if (heading > 168.75 && heading <= 191.25) return "S";
            else if (heading > 191.25 && heading <= 213.75) return "SSW";
            else if (heading > 213.75 && heading <= 236.25) return "SW";
            else if (heading > 236.25 && heading <= 258.75) return "WSW";
            else if (heading > 258.75 && heading <= 281.25) return "W";
            else if (heading > 281.25 && heading <= 303.75) return "WNW";
            else if (heading > 303.75 && heading <= 326.25) return "NW";
            else if (heading > 326.25 && heading <= 348.75) return "NNW";
            else return "";
        }

        private void tad_targeting(CelestialBody body)
        {
            //Target Set/Unset buttons
            if (FlightGlobals.fetch.VesselTarget == null || (FlightGlobals.fetch.VesselTarget != null && FlightGlobals.fetch.VesselTarget.GetVessel() == null))
            {
                //No TGT set or TGT is a Body
                if ((CelestialBody)FlightGlobals.fetch.VesselTarget != body)
                {
                    if (GUILayout.Button("Set Target", GUILayout.ExpandWidth(false)))
                    {
                        FlightGlobals.fetch.SetVesselTarget(body);
                        if (debugging) Debug.Log("[VOID] KSP Target set to CelestialBody " + body.bodyName);
                    }
                }
                else if ((CelestialBody)FlightGlobals.fetch.VesselTarget == body)
                {
                    if (GUILayout.Button("Unset Target", GUILayout.ExpandWidth(false)))
                    {
                        FlightGlobals.fetch.SetVesselTarget(null);
                        if (debugging) Debug.Log("[VOID] KSP Target set to null");
                    }
                }
            }
            else if (FlightGlobals.fetch.VesselTarget == null || (FlightGlobals.fetch.VesselTarget != null && FlightGlobals.fetch.VesselTarget.GetVessel() != null))
            {
                //No TGT or TGT is a vessel
                if (GUILayout.Button("Set Target", GUILayout.ExpandWidth(false)))
                {
                    FlightGlobals.fetch.SetVesselTarget(body);
                    if (debugging) Debug.Log("[VOID] KSP Target set to CelestialBody " + body.bodyName);
                }
            }
        }

        private void display_transfer_angles_SUN2PLANET(CelestialBody body)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Phase angle (curr/trans):");
			GUILayout.Label(Tools.mrenigma03_calcphase(vessel, body).ToString("F3") + "° / " + Tools.Nivvy_CalcTransferPhaseAngle(vessel.orbit.semiMajorAxis, body.orbit.semiMajorAxis, vessel.mainBody.gravParameter).ToString("F3") + "°", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Transfer velocity:");
			GUILayout.Label((Tools.Younata_DeltaVToGetToOtherBody((vessel.mainBody.gravParameter / 1000000000), (vessel.orbit.semiMajorAxis / 1000), (body.orbit.semiMajorAxis / 1000)) * 1000).ToString("F2") + "m/s", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        private void display_transfer_angles_PLANET2PLANET(CelestialBody body)
        {
			double dv1 = Tools.Younata_DeltaVToGetToOtherBody((vessel.mainBody.referenceBody.gravParameter / 1000000000), (vessel.mainBody.orbit.semiMajorAxis / 1000), (body.orbit.semiMajorAxis / 1000));
			double dv2 = Tools.Younata_DeltaVToExitSOI((vessel.mainBody.gravParameter / 1000000000), (vessel.orbit.semiMajorAxis / 1000), (vessel.mainBody.sphereOfInfluence / 1000), Math.Abs(dv1));

			double trans_ejection_angle = Tools.Younata_TransferBurnPoint((vessel.orbit.semiMajorAxis / 1000), dv2, (Math.PI / 2.0), (vessel.mainBody.gravParameter / 1000000000));
			double curr_ejection_angle = Tools.Adammada_CurrentEjectionAngle(FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.orbit.referenceBody.rotationAngle, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent);

			double trans_phase_angle = Tools.Nivvy_CalcTransferPhaseAngle(vessel.mainBody.orbit.semiMajorAxis, body.orbit.semiMajorAxis, vessel.mainBody.referenceBody.gravParameter) % 360;
			double curr_phase_angle = Tools.Adammada_CurrrentPhaseAngle(body.orbit.LAN, body.orbit.orbitPercent, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent);

			double adj_phase_angle = Tools.adjustCurrPhaseAngle(trans_phase_angle, curr_phase_angle);
			double adj_trans_ejection_angle = Tools.adjust_transfer_ejection_angle(trans_ejection_angle, trans_phase_angle);
			double adj_curr_ejection_angle = Tools.adjust_current_ejection_angle(curr_ejection_angle);

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Phase angle (curr/trans):");
			GUILayout.Label(adj_phase_angle.ToString("F3") + "° / " + trans_phase_angle.ToString("F3") + "°", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Ejection angle (curr/trans):");
            GUILayout.Label(adj_curr_ejection_angle.ToString("F3") + "° / " + adj_trans_ejection_angle.ToString("F3") + "°", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Transfer velocity:");
            GUILayout.Label((dv2 * 1000).ToString("F2") + "m/s", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        private void display_transfer_angles_PLANET2MOON(CelestialBody body)
        {
			double dv1 = Tools.Younata_DeltaVToGetToOtherBody((vessel.mainBody.gravParameter / 1000000000), (vessel.orbit.semiMajorAxis / 1000), (body.orbit.semiMajorAxis / 1000));
//            double curr_phase_angle = Adammada_CurrrentPhaseAngle(body.orbit.LAN, body.orbit.orbitPercent, vessel.orbit.LAN, vessel.orbit.orbitPercent);
			double trans_phase_angle = Tools.Nivvy_CalcTransferPhaseAngle(vessel.orbit.semiMajorAxis, body.orbit.semiMajorAxis, vessel.mainBody.gravParameter);

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Phase angle (curr/trans):");
			GUILayout.Label(Tools.mrenigma03_calcphase(vessel, body).ToString("F3") + "° / " + trans_phase_angle.ToString("F3") + "°", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Transfer velocity:");
            GUILayout.Label((dv1 * 1000).ToString("F2") + "m/s", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        private void display_transfer_angles_MOON2MOON(CelestialBody body)
        {
			double dv1 = Tools.Younata_DeltaVToGetToOtherBody((vessel.mainBody.referenceBody.gravParameter / 1000000000), (vessel.mainBody.orbit.semiMajorAxis / 1000), (body.orbit.semiMajorAxis / 1000));
			double dv2 = Tools.Younata_DeltaVToExitSOI((vessel.mainBody.gravParameter / 1000000000), (vessel.orbit.semiMajorAxis / 1000), (vessel.mainBody.sphereOfInfluence / 1000), Math.Abs(dv1));
			double trans_ejection_angle = Tools.Younata_TransferBurnPoint((vessel.orbit.semiMajorAxis / 1000), dv2, (Math.PI / 2.0), (vessel.mainBody.gravParameter / 1000000000));

			double curr_phase_angle = Tools.Adammada_CurrrentPhaseAngle(body.orbit.LAN, body.orbit.orbitPercent, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent);
			double curr_ejection_angle = Tools.Adammada_CurrentEjectionAngle(FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.orbit.referenceBody.rotationAngle, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent);

			double trans_phase_angle = Tools.Nivvy_CalcTransferPhaseAngle(vessel.mainBody.orbit.semiMajorAxis, body.orbit.semiMajorAxis, vessel.mainBody.referenceBody.gravParameter) % 360;

			double adj_phase_angle = Tools.adjustCurrPhaseAngle(trans_phase_angle, curr_phase_angle);
            //double adj_ejection_angle = adjustCurrEjectionAngle(trans_phase_angle, curr_ejection_angle);

            //new stuff
            //
			double adj_trans_ejection_angle = Tools.adjust_transfer_ejection_angle(trans_ejection_angle, trans_phase_angle);
			double adj_curr_ejection_angle = Tools.adjust_current_ejection_angle(curr_ejection_angle);
            //
            //

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Phase angle (curr/trans):");
            GUILayout.Label(adj_phase_angle.ToString("F3") + "° / " + trans_phase_angle.ToString("F3") + "°", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Ejection angle (curr/trans):");
            GUILayout.Label(adj_curr_ejection_angle.ToString("F3") + "° / " + adj_trans_ejection_angle.ToString("F3") + "°", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Transfer velocity:");
            GUILayout.Label((dv2 * 1000).ToString("F2") + "m/s", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        // GUI
		// Toadicus edit: added colons for each label_string.
        private void void_gui(int window_id)
        {
			// Toadicus edit: added local sidereal longitude.
			double LSL = vessel.longitude + vessel.orbit.referenceBody.rotationAngle;
			LSL = Tools.FixDegreeDomain (LSL);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(label_strings.void_primary + ":");
            GUILayout.Label(vessel.mainBody.bodyName, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(label_strings.void_altitude_asl + ":");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.orbit.altitude) + "m", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(label_strings.void_velocity + ":");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.orbit.vel.magnitude) + "m/s", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(label_strings.void_apoapsis + ":");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.orbit.ApA) + "m", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Time to Ap:");
            GUILayout.Label(Tools.ConvertInterval(vessel.orbit.timeToAp), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(label_strings.void_periapsis + ":");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.orbit.PeA) + "m", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Time to Pe:");
            GUILayout.Label(Tools.ConvertInterval(vessel.orbit.timeToPe), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Inclination:");
            GUILayout.Label(vessel.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            double r_vessel = vessel.mainBody.Radius + vessel.mainBody.GetAltitude(vessel.findWorldCenterOfMass());
            double g_vessel = (G_constant * vessel.mainBody.Mass) / Math.Pow(r_vessel, 2);
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Gravity:");
			GUILayout.Label(Tools.MuMech_ToSI(g_vessel) + "m/s²", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            extended_orbital_info = GUILayout.Toggle(extended_orbital_info, "Extended info");

            if (extended_orbital_info)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Period:");
                GUILayout.Label(Tools.ConvertInterval(vessel.orbit.period), GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Semi-major axis:");
                GUILayout.Label((vessel.orbit.semiMajorAxis / 1000).ToString("##,#") + "km", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Eccentricity:");
                GUILayout.Label(vessel.orbit.eccentricity.ToString("F4"), GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

				// Toadicus edit: convert mean anomaly into degrees.
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Mean anomaly:");
                GUILayout.Label((vessel.orbit.meanAnomaly * 180d / Math.PI).ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("True anomaly:");
                GUILayout.Label(vessel.orbit.trueAnomaly.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

				// Toadicus edit: convert eccentric anomaly into degrees.
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Eccentric anomaly:");
                GUILayout.Label((vessel.orbit.eccentricAnomaly * 180d / Math.PI).ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Long. ascending node:");
                GUILayout.Label(vessel.orbit.LAN.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Arg. of periapsis:");
                GUILayout.Label(vessel.orbit.argumentOfPeriapsis.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

				// Toadicus edit: added local sidereal longitude.
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Local Sidereal Longitude:");
				GUILayout.Label(LSL.ToString("F3") + "°", label_txt_right);
				GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void atmo_gui(int window_id)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Altitude (true):");
            double alt_true = vessel.orbit.altitude - vessel.terrainAltitude;
			// HACK: This assumes that on worlds with oceans, all water is fixed at 0 m, and water covers the whole surface at 0 m.
			if (vessel.terrainAltitude < 0 && vessel.mainBody.ocean ) alt_true = vessel.orbit.altitude;
            GUILayout.Label(Tools.MuMech_ToSI(alt_true) + "m", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Latitude:");
			GUILayout.Label(Tools.GetLatitudeString(vessel), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Longitude:");
            GUILayout.Label(Tools.GetLongitudeString(vessel), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Heading:");
            GUILayout.Label(Tools.MuMech_get_heading(vessel).ToString("F2") + "° " + get_heading_text(Tools.MuMech_get_heading(vessel)), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Terrain elevation:");
            GUILayout.Label(Tools.MuMech_ToSI(vessel.terrainAltitude) + "m", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Surface velocity:");
            GUILayout.Label(Tools.MuMech_ToSI(vessel.srf_velocity.magnitude) + "m/s", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Vertical speed:");
            GUILayout.Label(Tools.MuMech_ToSI(vessel.verticalSpeed) + "m/s", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Horizontal speed:");
            GUILayout.Label(Tools.MuMech_ToSI(vessel.horizontalSrfSpeed) + "m/s", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Temperature:");
            GUILayout.Label(vessel.flightIntegrator.getExternalTemperature().ToString("F2") + "° C", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Atmosphere density:");
            GUILayout.Label(Tools.MuMech_ToSI(vessel.atmDensity * 1000) + "g/m³", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Pressure:");
            GUILayout.Label(vessel.staticPressure.ToString("F2") + " atms", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Atmosphere limit:");
            GUILayout.Label("≈ " + Tools.MuMech_ToSI(vessel.mainBody.maxAtmosphereAltitude) + "m", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

			// Toadicus edit: added Biome
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Biome:");
			GUILayout.Label(Tools.Toadicus_GetAtt(vessel).name, label_txt_right);
			GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void tad_gui(int window_id)
        {
            GUILayout.BeginVertical();

            if (vessel.mainBody.name == "Sun")  //Vessel is orbiting the Sun
            {
                foreach (CelestialBody body in vessel.mainBody.orbitingBodies)
                {
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    if (GUILayout.Button(body.bodyName))
                    {
                        //add or remove this body to this list of bodies to display more info on
                        if (tad_selected_bodies.Contains(body)) tad_selected_bodies.Remove(body);
                        else tad_selected_bodies.Add(body);
                    }
                    GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    if (tad_selected_bodies.Contains(body))
                    {
                        display_transfer_angles_SUN2PLANET(body);  //show phase angles for each selected body
                        tad_targeting(body);    //display Set/Unset Target button for each selected body
                    }
                }
            }
            else if (vessel.mainBody.referenceBody.name == "Sun")	//Vessel is orbiting a planet
            {
                foreach (CelestialBody body in vessel.mainBody.referenceBody.orbitingBodies)
                {
                    if (body.name != vessel.mainBody.name)	// show other planets
                    {
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        if (GUILayout.Button(body.bodyName))
                        {
                            //add or remove this body to this list of bodies to display more info on
                            if (tad_selected_bodies.Contains(body)) tad_selected_bodies.Remove(body);
                            else tad_selected_bodies.Add(body);
                        }
                        GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        if (tad_selected_bodies.Contains(body))
                        {
                            display_transfer_angles_PLANET2PLANET(body);
                            tad_targeting(body);    //display Set/Unset Target button
                        }
                    }
                }
                foreach (CelestialBody body in vessel.mainBody.orbitingBodies)	// show moons
                {
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    if (GUILayout.Button(body.bodyName))
                    {
                        //add or remove this body to this list of bodies to display more info on
                        if (tad_selected_bodies.Contains(body)) tad_selected_bodies.Remove(body);
                        else tad_selected_bodies.Add(body);
                    }
                    GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    if (tad_selected_bodies.Contains(body))
                    {
                        display_transfer_angles_PLANET2MOON(body);
                        tad_targeting(body);    //display Set/Unset Target button
                    }
                }
            }
            else if (vessel.mainBody.referenceBody.referenceBody.name == "Sun")	// Vessel is orbiting a moon
            {
                foreach (CelestialBody body in vessel.mainBody.referenceBody.orbitingBodies)
                {
                    if (body.name != vessel.mainBody.name)	// show other moons
                    {
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        if (GUILayout.Button(body.bodyName))
                        {
                            //add or remove this body to this list of bodies to display more info on
                            if (tad_selected_bodies.Contains(body)) tad_selected_bodies.Remove(body);
                            else tad_selected_bodies.Add(body);
                        }
                        GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        if (tad_selected_bodies.Contains(body))
                        {
                            display_transfer_angles_MOON2MOON(body);
                            tad_targeting(body);    //display Set/Unset Target button
                        }
                    }
                }
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void test_gui(int window_id)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("FlightGlobals.fetch.vesselTargetMode = " + FlightGlobals.fetch.vesselTargetMode.ToString());
            //if (debugging) Debug.Log("[VOID] vesselTargetMode OK");
            if (FlightGlobals.fetch.VesselTarget != null)
            {
                GUILayout.Label("VesselTarget == " + FlightGlobals.fetch.VesselTarget.ToString());
                //if (debugging) Debug.Log("[VOID] VesselTarget OK");
                GUILayout.Label("vesselTargetTransform == " + FlightGlobals.fetch.vesselTargetTransform.ToString());
                //if (debugging) Debug.Log("[VOID] vesselTargetTransform OK");
                GUILayout.Label("vesselTargetDirection == " + FlightGlobals.fetch.vesselTargetDirection.ToString());
                //if (debugging) Debug.Log("[VOID] vesselTargetDirection OK");
                GUILayout.Label("vesselTargetDelta == " + FlightGlobals.fetch.vesselTargetDelta.ToString());
                //if (debugging) Debug.Log("[VOID] vesselTargetDelta OK");
            }
            else GUILayout.Label("VesselTarget == null");


            //check whats in here
            //GUI.skin.font.fontNames
            //foreach (String f in GUI.skin.font.fontNames)
            //{
            //    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //    GUILayout.Label(f);
            //    GUILayout.EndHorizontal();
            //}




            //fail
            //if (Event.current.keyCode == KeyCode.Z)
            //{
            //    FlightCtrlState s = new FlightCtrlState();
            //    vessel.FeedInputFeed();
            //    s.mainThrottle = 1;
            //}



            //MapView.MapIsEnabled = false;
            //fail



            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //GUILayout.Label(DateTime.Now.ToString("d MMMM"));
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //GUILayout.Label("situation:", GUILayout.ExpandWidth(true));
            //GUILayout.Label(vessel.situation.ToString(), label_txt_right);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //GUILayout.Label("vessel.missionTime:", GUILayout.ExpandWidth(true));
            //GUILayout.Label(vessel.missionTime.ToString(), label_txt_right);
            //GUILayout.EndHorizontal();

            //Orbit target_orbit = new Orbit();
            //target_orbit = FlightGlobals.fetch.VesselTarget.GetOrbit();


            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //GUILayout.Label("target_orbit.altitude", GUILayout.ExpandWidth(true));
            //GUILayout.Label(target_orbit.altitude.ToString("F2"), label_txt_right);
            //GUILayout.EndHorizontal();


            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //GUILayout.Label("vessel.packed", GUILayout.ExpandWidth(true));
            //GUILayout.Label(vessel.packed.ToString(), label_txt_right);
            //GUILayout.EndHorizontal();


            //q = 1/2 p v^2
            //p = local air density
            //v = velocity
            //part.dynamicPressureAtm
            double q = .5 * vessel.atmDensity * Math.Pow(vessel.orbit.vel.magnitude, 2);
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("dynamic pressure q:", GUILayout.ExpandWidth(true));
            GUILayout.Label(q.ToString("F4"), label_txt_right);
            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //GUILayout.Label("part.dynamicPressureAtm:", GUILayout.ExpandWidth(true));
            //GUILayout.Label(part.dynamicPressureAtm.ToString("F3"), label_txt_right);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //GUILayout.Label("part.staticPressureAtm:", GUILayout.ExpandWidth(true));
            //GUILayout.Label(part.staticPressureAtm.ToString("F3"), label_txt_right);
            //GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("vessel.staticPressure:", GUILayout.ExpandWidth(true));
            GUILayout.Label(vessel.staticPressure.ToString("F3"), label_txt_right);
            GUILayout.EndHorizontal();





            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("FlightLogger.getMissionStats():", GUILayout.ExpandWidth(true));
            GUILayout.Label(FlightLogger.getMissionStats(), label_txt_right);
            GUILayout.EndHorizontal();

            //parse this out
            //"Total Distance Traveled: 552m"
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            string temp = FlightLogger.getMissionStats();
            temp = temp.Substring(temp.IndexOf("Total Distance Traveled:"));
            //GUILayout.Label(temp, GUILayout.ExpandWidth(true));
            //int colon_pos = temp.IndexOf(":");
            temp = temp.Substring(temp.IndexOf(":") + 1);
            //GUILayout.Label(temp, GUILayout.ExpandWidth(true));
            int m_pos = temp.IndexOf("m");
            temp = temp.Substring(0, m_pos + 1);
            temp = temp.Trim();

            GUILayout.Label("Distance traveled:", GUILayout.ExpandWidth(true));
            GUILayout.Label(temp, label_txt_right);

            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //GUILayout.Label("power_request_interval:", GUILayout.ExpandWidth(true));
            //GUILayout.Label(power_request_interval.ToString(), label_txt_right);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //GUILayout.Label("power_request_amount:", GUILayout.ExpandWidth(true));
            //GUILayout.Label(power_request_amount.ToString(), label_txt_right);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //GUILayout.Label("Tools.MuMech_get_heading():", GUILayout.ExpandWidth(true));
            //GUILayout.Label(Tools.MuMech_get_heading().ToString("F2"), label_txt_right);
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            //GUILayout.Label("num_fixedupdate_calls:", GUILayout.ExpandWidth(true));
            //GUILayout.Label(num_fixedupdate_calls.ToString(), label_txt_right);
            //GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("vessel.flightIntegrator.currentDragForce:", GUILayout.ExpandWidth(true));
            GUILayout.Label(vessel.flightIntegrator.currentDragForce.ToString(), label_txt_right);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("vessel.flightIntegrator.drag:", GUILayout.ExpandWidth(true));
            GUILayout.Label(vessel.flightIntegrator.drag.ToString(), label_txt_right);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("vessel.flightIntegrator.dragArea:", GUILayout.ExpandWidth(true));
            GUILayout.Label(vessel.flightIntegrator.dragArea.ToString(), label_txt_right);
            GUILayout.EndHorizontal();

            //

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void celestial_body_info_gui(int window_id)
        {
            //print("starting celestial_body_info_gui()...");
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            GUILayout.BeginVertical(GUILayout.Width(150));
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(150));

            body_OP_selected_body_1 = all_bodies[body_OP_body_1_index];
            body_OP_selected_body_2 = all_bodies[body_OP_body_2_index];

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("<", GUILayout.ExpandWidth(false)))
            {
                body_OP_body_1_index--;
                if (body_OP_body_1_index < 0) body_OP_body_1_index = all_bodies.Count - 1;
            }
            GUILayout.Label(all_bodies[body_OP_body_1_index].bodyName, label_txt_center_bold, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
            {
                body_OP_body_1_index++;
                if (body_OP_body_1_index > all_bodies.Count - 1) body_OP_body_1_index = 0;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(150));
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("<", GUILayout.ExpandWidth(false)))
            {
                body_OP_body_2_index--;
                if (body_OP_body_2_index < 0) body_OP_body_2_index = all_bodies.Count - 1;
            }
            GUILayout.Label(all_bodies[body_OP_body_2_index].bodyName, label_txt_center_bold, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
            {
                body_OP_body_2_index++;
                if (body_OP_body_2_index > all_bodies.Count - 1) body_OP_body_2_index = 0;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            //}

            //toggle for orbital info chunk
            if (GUILayout.Button("Orbital Characteristics", GUILayout.ExpandWidth(true))) body_op_show_orbital = !body_op_show_orbital;

            if (body_op_show_orbital)
            {
                //begin orbital into horizontal chunk
                //print("begin orbital info section...");
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                //begin orbital value labels column
                GUILayout.BeginVertical(GUILayout.Width(150));

                //print("printing row labels...");

                GUILayout.Label("Apoapsis:");
                GUILayout.Label("Time to Ap:");
                GUILayout.Label("Periapsis:");
                GUILayout.Label("Time to Pe:");
                GUILayout.Label("Semi-major axis:");
                GUILayout.Label("Eccentricity:");
                GUILayout.Label("Orbital period:");
                GUILayout.Label("Rotational period:");
                GUILayout.Label("Velocity:");
                GUILayout.Label("Mean anomaly:");
                GUILayout.Label("True anomaly:");
                GUILayout.Label("Eccentric anomaly:");
                GUILayout.Label("Inclination:");
                GUILayout.Label("Long. ascending node:");
                GUILayout.Label("Arg. of periapsis:");
                GUILayout.Label("Tidally locked:");

                //end orbital value labels column
                GUILayout.EndVertical();

                //begin primary orbital values column
                GUILayout.BeginVertical(GUILayout.Width(150));

                body_OP_show_orbital_info(body_OP_selected_body_1);

                //end primary orbital values column
                GUILayout.EndVertical();

                //begin secondary orbital values column
                GUILayout.BeginVertical(GUILayout.Width(150));

                body_OP_show_orbital_info(body_OP_selected_body_2);

                //end secondary orbital values column
                GUILayout.EndVertical();

                //end orbital info horizontal chunk
                GUILayout.EndHorizontal();
            }

            //toggle for physical info chunk
            if (GUILayout.Button("Physical Characteristics", GUILayout.ExpandWidth(true))) body_op_show_physical = !body_op_show_physical;

            if (body_op_show_physical)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                //begin physical info value label column
                GUILayout.BeginVertical(GUILayout.Width(150));

                GUILayout.Label("Radius:");
                GUILayout.Label("Surface area:");
                GUILayout.Label("Volume:");
                GUILayout.Label("Mass:");
                GUILayout.Label("Density:");
                GUILayout.Label("Sphere of influence:");
                GUILayout.Label("Natural satellites:");
                GUILayout.Label("Artificial satellites:");
                GUILayout.Label("Surface gravity:");
                GUILayout.Label("Atmosphere altitude:");
                GUILayout.Label("Atmospheric O\u2082:");
                GUILayout.Label("Has ocean:");

                //end physical info value label column
                GUILayout.EndVertical();

                //begin primary physical values column
                GUILayout.BeginVertical(GUILayout.Width(150));

                body_OP_show_physical_info(body_OP_selected_body_1);

                //end primary physical column
                GUILayout.EndVertical();

                //begin secondary physical values column
                GUILayout.BeginVertical(GUILayout.Width(150));

                body_OP_show_physical_info(body_OP_selected_body_2);

                //end target physical values column
                GUILayout.EndVertical();

                //end physical value horizontal chunk
                GUILayout.EndHorizontal();
            }

            GUI.DragWindow();
        }

        private void rendezvous_info_gui(int window_id)
        {
            Vessel rendezvessel = new Vessel();
            CelestialBody rendezbody = new CelestialBody();

            GUILayout.BeginVertical();

            //display both
            //Show Target Info
            GUILayout.Label("Target:", label_txt_center_bold);
            if (FlightGlobals.fetch.VesselTarget != null)
            {
                //a KSP Target (body or vessel) is selected
                if (FlightGlobals.fetch.vesselTargetMode == FlightGlobals.VesselTargetModes.Direction)
                {
                    //a Body is selected
                    rendezbody = vessel.patchedConicSolver.targetBody;
                    display_rendezvous_info(null, rendezbody);
                }
                else if (FlightGlobals.fetch.vesselTargetMode == FlightGlobals.VesselTargetModes.DirectionAndVelocity)
                {
                    //a Vessel is selected
                    rendezvessel = FlightGlobals.fetch.VesselTarget.GetVessel();
                    display_rendezvous_info(rendezvessel, null);
                }
                //Show Unset button for both options above
                if (GUILayout.Button("Unset Target", GUILayout.ExpandWidth(false)))
                {
                    FlightGlobals.fetch.SetVesselTarget(null);
                    if (debugging) Debug.Log("[VOID] KSP Target set to null");
                }

            }
            else
            {
                //no KSP Target selected
                GUILayout.Label("No Target Selected", label_txt_center_bold);
            }

            //Show Vessel Register vessel info
            if (hide_vesreg_info == false)
            {
                GUILayout.Label("Vessel Register:", label_txt_center_bold);
                if (vesreg_selected_vessel != null)
                {
                    rendezvessel = vesreg_selected_vessel;
                    display_rendezvous_info(rendezvessel, null);

                    //show set/unset buttons
                    if (FlightGlobals.fetch.VesselTarget == null || (FlightGlobals.fetch.VesselTarget != null && FlightGlobals.fetch.VesselTarget.GetVessel() != vesreg_selected_vessel))
                    {
                        //no Tgt set or Tgt is not this vessel
                        //show a Set button
                        if (GUILayout.Button("Set Target", GUILayout.ExpandWidth(false)))
                        {
                            FlightGlobals.fetch.SetVesselTarget(rendezvessel);
                            if (debugging) Debug.Log("[VOID] KSP Target set to " + rendezvessel.vesselName);
                        }
                    }
                    else if (FlightGlobals.fetch.VesselTarget != null && FlightGlobals.fetch.VesselTarget.GetVessel() == vesreg_selected_vessel)
                    {
                        //this VRegister Tgt is also set as KSP tgt
                        //show Unset button
                        //if (GUILayout.Button("Unset Target", GUILayout.ExpandWidth(false)))
                        //{
                       //     FlightGlobals.fetch.SetVesselTarget(null);
                       //     if (debugging) Debug.Log("[VOID] KSP Target set to null");
                       // }
                    }
                }
                else
                {
                    //vesreg Vessel is null
                    //targ = null;
                    GUILayout.Label("No Vessel Selected", label_txt_center_bold);
                }
            }

            hide_vesreg_info = GUILayout.Toggle(hide_vesreg_info, "Hide Vessel Register Info");

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(" ", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Close", GUILayout.ExpandWidth(false))) rendezvous_module = false;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void display_rendezvous_info(Vessel v, CelestialBody cb)
        {
            if (cb == null && v != null)
            {
                //Display vessel rendezvous info
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label(v.vesselName, label_txt_center_bold, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                if (v.situation == Vessel.Situations.ESCAPING || v.situation == Vessel.Situations.FLYING || v.situation == Vessel.Situations.ORBITING || v.situation == Vessel.Situations.SUB_ORBITAL)
                {
					// Toadicus edit: added local sidereal longitude.
					// Toadicus edit: added local sidereal longitude.
					double LSL = vessel.longitude + vessel.orbit.referenceBody.rotationAngle;
					LSL = Tools.FixDegreeDomain (LSL);

                    //display orbital info for orbiting/flying/suborbital/escaping vessels only
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label("Ap/Pe:");
                    GUILayout.Label(Tools.MuMech_ToSI(v.orbit.ApA) + "m / " + Tools.MuMech_ToSI(v.orbit.PeA) + "m", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label("Altitude:");
                    GUILayout.Label(Tools.MuMech_ToSI(v.orbit.altitude) + "m", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label("Inclination:");
                    GUILayout.Label(v.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    if (vessel.mainBody == v.mainBody)
                    {
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.Label("Relative inclination:");
                        GUILayout.Label(Vector3d.Angle(vessel.orbit.GetOrbitNormal(), v.orbit.GetOrbitNormal()).ToString("F3") + "°", GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }
                    //if (debugging) Debug.Log("[CHATR] v -> v relative incl OK");

                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label("Velocity:");
                    GUILayout.Label(Tools.MuMech_ToSI(v.orbit.vel.magnitude) + "m/s", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label("Relative velocity:");
                    GUILayout.Label(Tools.MuMech_ToSI(v.orbit.vel.magnitude - vessel.orbit.vel.magnitude) + "m/s", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label("Distance:");
                    GUILayout.Label(Tools.MuMech_ToSI((vessel.findWorldCenterOfMass() - v.findWorldCenterOfMass()).magnitude) + "m", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    //target_vessel_extended_orbital_info = GUILayout.Toggle(target_vessel_extended_orbital_info, "Extended info");

                    if (target_vessel_extended_orbital_info)
                    {
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.Label("Period:");
                        GUILayout.Label(Tools.ConvertInterval(v.orbit.period), GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.Label("Semi-major axis:");
                        GUILayout.Label((v.orbit.semiMajorAxis / 1000).ToString("##,#") + "km", GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.Label("Eccentricity:");
                        GUILayout.Label(v.orbit.eccentricity.ToString("F4"), GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

						// Toadicus edit: convert mean anomaly into degrees.
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.Label("Mean anomaly:");
                        GUILayout.Label((v.orbit.meanAnomaly * 180d / Math.PI).ToString("F3") + "°", GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.Label("True anomaly:");
                        GUILayout.Label(v.orbit.trueAnomaly.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

						// Toadicus edit: convert eccentric anomaly into degrees.
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.Label("Eccentric anomaly:");
                        GUILayout.Label((v.orbit.eccentricAnomaly * 180d / Math.PI).ToString("F3") + "°", GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.Label("Long. ascending node:");
                        GUILayout.Label(v.orbit.LAN.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.Label("Arg. of periapsis:");
                        GUILayout.Label(v.orbit.argumentOfPeriapsis.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

						// Toadicus edit: added local sidereal longitude.
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("Local Sidereal Longitude:");
						GUILayout.Label(LSL.ToString("F3") + "°", label_txt_right);
						GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label("Latitude:");
					GUILayout.Label(Tools.GetLatitudeString(vessel), GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label("Longitude:");
					GUILayout.Label(Tools.GetLongitudeString(vessel), GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label("Distance:");
                    GUILayout.Label(Tools.MuMech_ToSI((vessel.findWorldCenterOfMass() - v.findWorldCenterOfMass()).magnitude) + "m", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();
                }
            }
            else if (cb != null && v == null)
            {
                //Display CelstialBody rendezvous info
                GUILayout.Label(cb.bodyName, label_txt_center_bold);

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Ap/Pe:");
                GUILayout.Label(Tools.MuMech_ToSI(cb.orbit.ApA) + "m / " + Tools.MuMech_ToSI(cb.orbit.PeA) + "m", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                //if (debugging) Debug.Log("[VOID] Ap/Pe OK");

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Inclination:");
                GUILayout.Label(cb.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                //if (debugging) Debug.Log("[VOID] Inclination OK");

                if (cb.referenceBody == vessel.mainBody)
                {
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    GUILayout.Label("Relative inclination:");
                    GUILayout.Label(Vector3d.Angle(vessel.orbit.GetOrbitNormal(), cb.orbit.GetOrbitNormal()).ToString("F3") + "°", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();
                    //if (debugging) Debug.Log("[VOID] cb Relative inclination OK");
                }

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Distance:");
                GUILayout.Label(Tools.MuMech_ToSI((vessel.mainBody.position - cb.position).magnitude) + "m", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                //if (debugging) Debug.Log("[VOID] Distance OK");

                //SUN2PLANET:
                if (vessel.mainBody.bodyName == "Sun" && cb.referenceBody == vessel.mainBody)
                {
                    display_transfer_angles_SUN2PLANET(cb);
                    //if (debugging) Debug.Log("[VOID] SUN2PLANET OK");
                }

                //PLANET2PLANET
                else if (vessel.mainBody.referenceBody.bodyName == "Sun" && cb.referenceBody == vessel.mainBody.referenceBody)
                {
                    display_transfer_angles_PLANET2PLANET(cb);
                    //if (debugging) Debug.Log("[VOID] PLANET2PLANET OK");
                }

                //PLANET2MOON
                else if (vessel.mainBody.referenceBody.bodyName == "Sun" && cb.referenceBody == vessel.mainBody)
                {
                    display_transfer_angles_PLANET2MOON(cb);
                    //if (debugging) Debug.Log("[VOID] PLANET2MOON OK");
                }

                //MOON2MOON
                else if (vessel.mainBody.referenceBody.referenceBody.bodyName == "Sun" && cb.referenceBody == vessel.mainBody.referenceBody)
                {
                    display_transfer_angles_MOON2MOON(cb);
                    //if (debugging) Debug.Log("[VOID] MOON2MOON OK");
                }

                //else GUILayout.Label("Transfer angle information\nunavailable for this target");

            }
        }

        private void data_time_gui(int window_id)
        {
            GUIStyle txt_white = new GUIStyle(GUI.skin.label);
            txt_white.normal.textColor = txt_white.focused.textColor = Color.white;
            GUIStyle txt_green = new GUIStyle(GUI.skin.label);
            txt_green.normal.textColor = txt_green.focused.textColor = Color.green;
            GUIStyle txt_yellow = new GUIStyle(GUI.skin.label);
            txt_yellow.normal.textColor = txt_yellow.focused.textColor = Color.yellow;

            GUILayout.BeginVertical();

            GUILayout.Label("System time: " + DateTime.Now.ToString("HH:mm:ss"));
            GUILayout.Label(Tools.ConvertInterval(stopwatch1));

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Start"))
            {
                if (stopwatch1_running == false) stopwatch1_running = true;
            }
            if (GUILayout.Button("Stop"))
            {
                if (stopwatch1_running == true) stopwatch1_running = false;
            }
            if (GUILayout.Button("Reset"))
            {
                if (stopwatch1_running == true) stopwatch1_running = false;
                stopwatch1 = 0;
            }
            GUILayout.EndHorizontal();

            GUIStyle label_style = txt_white;
            string log_label = "Inactive";
            if (csv_logging && vessel.situation.ToString() == "PRELAUNCH")
            {
                log_label = "Awaiting launch";
                label_style = txt_yellow;
            }
            if (csv_logging && vessel.situation.ToString() != "PRELAUNCH")
            {
                log_label = "Active";
                label_style = txt_green;
            }
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            csv_logging = GUILayout.Toggle(csv_logging, "Data logging: ", GUILayout.ExpandWidth(false));
            GUILayout.Label(log_label, label_style, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Interval: ", GUILayout.ExpandWidth(false));
            csv_log_interval_str = GUILayout.TextField(csv_log_interval_str, GUILayout.ExpandWidth(true));
            GUILayout.Label("s", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            float new_log_interval;
            if (Single.TryParse(csv_log_interval_str, out new_log_interval)) csv_log_interval = new_log_interval;

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void vessel_info_gui(int window_id)
        {
            GUILayout.BeginVertical();

            GUILayout.Label(vessel.vesselName, label_txt_center_bold, GUILayout.ExpandWidth(true));

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("G-force:");
            GUILayout.Label(vessel.geeForce.ToString("F2") + " gees", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            int num_parts = 0;
            double total_mass = vessel.GetTotalMass();
            double resource_mass = 0;
            double max_thrust = 0;
            double final_thrust = 0;

            foreach (Part p in vessel.parts)
            {
                num_parts++;
                resource_mass += p.GetResourceMass();

                foreach (PartModule pm in p.Modules)
                {
                    if ((pm.moduleName == "ModuleEngines") && ((p.State == PartStates.ACTIVE) || ((Staging.CurrentStage > Staging.lastStage) && (p.inverseStage == Staging.lastStage))))
                    {
                        max_thrust += ((ModuleEngines)pm).maxThrust;
                        final_thrust += ((ModuleEngines)pm).finalThrust;
                    }
                }
            }

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Parts:");
            GUILayout.Label(num_parts.ToString("F0"), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Total mass:");
            GUILayout.Label(total_mass.ToString("F1") + " tons", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Resource mass:");
            GUILayout.Label(resource_mass.ToString("F1") + " tons", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

			if (stages.Length > Staging.lastStage)
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("DeltaV (Current Stage):");
				GUILayout.Label(Tools.MuMech_ToSI(stages[Staging.lastStage].deltaV).ToString() + "m/s", GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
			}

			if (stages.Length > 0)
			{
				double totalDeltaV = 0d;

				for (int i = 0; i < stages.Length; ++i)
				{
					totalDeltaV += stages [i].deltaV;
				}

				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("DeltaV (Total):");
				GUILayout.Label(Tools.MuMech_ToSI(totalDeltaV).ToString() + "m/s", GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
			}

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Throttle:");
            GUILayout.Label((vessel.ctrlState.mainThrottle * 100f).ToString("F0") + "%", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Thrust (curr/max):");
            GUILayout.Label(final_thrust.ToString("F1") + " / " + max_thrust.ToString("F1") + " kN", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            double gravity = vessel.mainBody.gravParameter / Math.Pow(vessel.mainBody.Radius + vessel.altitude, 2);
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("T:W (curr/max):");
            GUILayout.Label((final_thrust / (total_mass * gravity)).ToString("F2") + " / " + (max_thrust / (total_mass * gravity)).ToString("F2"), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            double g_ASL = (G_constant * vessel.mainBody.Mass) / Math.Pow(vessel.mainBody.Radius, 2);
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Max T:W @ surface:");
            GUILayout.Label((max_thrust / (total_mass * g_ASL)).ToString("F2"), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void misc_info_gui(int window_id)
        {
            GUIContent _content = new GUIContent();
            GUILayout.BeginVertical();

            //show_tooltips = GUILayout.Toggle(show_tooltips, "Show tooltips", GUILayout.ExpandWidth(true));
            disable_power_usage = GUILayout.Toggle(disable_power_usage, "Disable power usage");
            hide_on_pause = GUILayout.Toggle(hide_on_pause, "Hide windows on pause");
            data_time_module = GUILayout.Toggle(data_time_module, "Data Logging & Time");

            //Language
            languages[] languages_array = Enum.GetValues(typeof(languages)) as languages[];
            List<languages> languages_list = new List<languages>();

            foreach (var val in languages_array)
            {
                languages_list.Add(val);
            }

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            if (GUILayout.Button("◄"))
            {
                //need separate preset indices for chatter and each beep
                user_lang_index--;
                if (user_lang_index < 0) user_lang_index = languages_list.Count - 1;
                if (debugging) Debug.Log("[VOID] user_lang_index = " + user_lang_index);
                //user_lang = languages_list[user_lang_index];
                //load_language();
            }

            if (GUILayout.Button("[WIP]" + languages_list[user_lang_index].ToString()))
            {
                user_lang = languages_list[user_lang_index];
                load_language();
            }

            if (GUILayout.Button("►"))
            {
                user_lang_index++;
                if (user_lang_index >= languages_list.Count) user_lang_index = 0;
                if (debugging) Debug.Log("[VOID] user_lang_index = " + user_lang_index);
                //user_lang = languages_list[user_lang_index];
                //load_language();
            }

            GUILayout.EndHorizontal();

            //Skin picker
            GUISkin[] skin_array = AssetBase.FindObjectsOfTypeIncludingAssets(typeof(GUISkin)) as GUISkin[];
            List<GUISkin> skin_list = new List<GUISkin>();

            foreach (GUISkin _skin in skin_array)
            {
                if (_skin.name != "PlaqueDialogSkin" && _skin.name != "FlagBrowserSkin" && _skin.name != "SSUITextAreaDefault") skin_list.Add(_skin);
            }

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            GUILayout.Label("Skin:", GUILayout.ExpandWidth(false));

            _content.text = "◄";
            _content.tooltip = "Select previous skin";
            if (GUILayout.Button(_content, GUILayout.ExpandWidth(true)))
            {
                skin_index--;
                if (skin_index < 0) skin_index = skin_list.Count;
                if (debugging) Debug.Log("[VOID] new skin_index = " + skin_index + " :: skin_list.Count = " + skin_list.Count);
            }

            string skin_name = "";
            if (skin_index == 0) skin_name = "None";
            else skin_name = skin_list[skin_index - 1].name;
            _content.text = skin_name;
            _content.tooltip = "Current skin";
            GUILayout.Label(_content, label_txt_center, GUILayout.ExpandWidth(true));

            _content.text = "►";
            _content.tooltip = "Select next skin";
            if (GUILayout.Button(_content, GUILayout.ExpandWidth(true)))
            {
                skin_index++;
                if (skin_index > skin_list.Count) skin_index = 0;
                if (debugging) Debug.Log("[VOID] new skin_index = " + skin_index + " :: skin_list.Count = " + skin_list.Count);
            }

            GUILayout.EndHorizontal();

            //Change icon position
            if (changing_icon_pos == false)
            {
                _content.text = "Change icon position";
                _content.tooltip = "Move icon anywhere on the screen";
                if (GUILayout.Button(_content, GUILayout.ExpandWidth(false))) changing_icon_pos = true;
            }
            else GUILayout.Label("Click anywhere to set new icon position");

            //HUD color
            if (hud_module)
            {
                if (GUILayout.Button("Change HUD color", GUILayout.ExpandWidth(false)))
                {
                    counter_hud_text_color++;
                    if (counter_hud_text_color > hud_text_colors.Count - 1) counter_hud_text_color = 0;
                }
            }

            //if (show_tooltips && GUI.tooltip != "") tooltips(misc_window_pos);

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void vessel_register_gui(int window_id)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("<"))
            {
                vessel_register_body_index--;
                if (vessel_register_body_index < 0) vessel_register_body_index = all_bodies.Count - 1;
            }
            GUILayout.Label(all_bodies[vessel_register_body_index].bodyName, label_txt_center_bold, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(">"))
            {
                vessel_register_body_index++;
                if (vessel_register_body_index > all_bodies.Count - 1) vessel_register_body_index = 0;
            }
            GUILayout.EndHorizontal();

            vessel_register_selected_body = all_bodies[vessel_register_body_index];

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("<"))
            {
                vessel_register_vessel_type_index--;
                if (vessel_register_vessel_type_index < 0) vessel_register_vessel_type_index = all_vessel_types.Count - 1;
            }
            GUILayout.Label(all_vessel_types[vessel_register_vessel_type_index].ToString(), label_txt_center_bold, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(">"))
            {
                vessel_register_vessel_type_index++;
                if (vessel_register_vessel_type_index > all_vessel_types.Count - 1) vessel_register_vessel_type_index = 0;
            }
            GUILayout.EndHorizontal();

            vessel_register_vessel_type = all_vessel_types[vessel_register_vessel_type_index];

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Landed", GUILayout.ExpandWidth(true))) vessel_register_vessel_situation = "Landed";
            if (GUILayout.Button("Orbiting", GUILayout.ExpandWidth(true))) vessel_register_vessel_situation = "Orbiting";
            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(Tools.UppercaseFirst(vessel_register_vessel_situation) + " " + vessel_register_vessel_type.ToString() + "s  @ " + vessel_register_selected_body.bodyName, label_txt_center, GUILayout.ExpandWidth(true));
            //GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            vessel_register_scroll_pos = GUILayout.BeginScrollView(vessel_register_scroll_pos, false, false);

            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if (v != vessel && v.vesselType == vessel_register_vessel_type && v.mainBody == vessel_register_selected_body)
                {
                    if ((vessel_register_vessel_situation == "Landed" && (v.situation == Vessel.Situations.LANDED || v.situation == Vessel.Situations.PRELAUNCH || v.situation == Vessel.Situations.SPLASHED)) || (vessel_register_vessel_situation == "Orbiting" && (v.situation == Vessel.Situations.ESCAPING || v.situation == Vessel.Situations.FLYING || v.situation == Vessel.Situations.ORBITING || v.situation == Vessel.Situations.SUB_ORBITAL)))
                    {
                        if (GUILayout.Button(v.vesselName, GUILayout.ExpandWidth(true)))
                        {
                            if (vesreg_selected_vessel != v)
                            {
                                vesreg_selected_vessel = v; //set clicked vessel as selected_vessel
                                rendezvous_module = true;    //turn bool on to open the window if closed
                            }
                            else
                            {
                                vesreg_selected_vessel = null;
                            }
                        }
                    }
                }
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        //private void tooltips(Rect pos)
        //{
        //    if (show_tooltips && GUI.tooltip != "")
        //    {
        //        float w = 5.5f * GUI.tooltip.Length;
        //        float x = (Event.current.mousePosition.x < pos.width / 2) ? Event.current.mousePosition.x + 10 : Event.current.mousePosition.x - 10 - w;
        //        GUI.Box(new Rect(x, Event.current.mousePosition.y, w, 25f), GUI.tooltip, gs_tooltip);
        //    }
        //}

        private void main_gui(int window_id)
        {
            GUILayout.BeginVertical();

            if (power_available)
            {
                if (power_toggle)
                {
                    //Show menu toggles only when power is turned on and available
                    vessel_info_module = GUILayout.Toggle(vessel_info_module, "Vessel Infomation");
                    void_module = GUILayout.Toggle(void_module, "Orbital Information");
                    atmo_module = GUILayout.Toggle(atmo_module, "Surface & Atmospheric Information");
                    tad_module = GUILayout.Toggle(tad_module, "Transfer Angle Information");
                    rendezvous_module = GUILayout.Toggle(rendezvous_module, "Rendezvous Information");
                    celestial_body_info_module = GUILayout.Toggle(celestial_body_info_module, "Celestial Body Information");
                    vessel_register_module = GUILayout.Toggle(vessel_register_module, "Vessel Register");
                    misc_module = GUILayout.Toggle(misc_module, "Miscellaneous");
                    hud_module = GUILayout.Toggle(hud_module, "Show HUD");
                    //test_module = GUILayout.Toggle(test_module, "The Lab");
                }

                if (misc_recvd_latest_version && latest_version != "") GUILayout.Label(latest_version);

                string str = "ON";
                if (power_toggle) str = "OFF";
                if (GUILayout.Button("Power " + str)) power_toggle = !power_toggle;
            }
            else
            {
                GUIStyle label_txt_red = new GUIStyle(GUI.skin.label);
                label_txt_red.normal.textColor = Color.red;
                label_txt_red.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("-- POWER LOST --", label_txt_red);
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        protected void draw_GUI()
        {
            if (hide_on_pause == false || (hide_on_pause && PauseMenu.isOpen == false))
            {
                //Apply a skin if selected
                if (skin_index == 0) GUI.skin = null;
                else if (skin_index == 1) GUI.skin = AssetBase.GetGUISkin("KSP window 1");
                else if (skin_index == 2) GUI.skin = AssetBase.GetGUISkin("KSP window 2");
                else if (skin_index == 3) GUI.skin = AssetBase.GetGUISkin("KSP window 3");
                else if (skin_index == 4) GUI.skin = AssetBase.GetGUISkin("KSP window 4");
                else if (skin_index == 5) GUI.skin = AssetBase.GetGUISkin("KSP window 5");
                else if (skin_index == 6) GUI.skin = AssetBase.GetGUISkin("KSP window 6");
                else if (skin_index == 7) GUI.skin = AssetBase.GetGUISkin("KSP window 7");
                else if (skin_index == 8) GUI.skin = AssetBase.GetGUISkin("OrbitMapSkin");

                if (gui_styles_set == false) set_gui_styles();   //set GUI styles once AFTER setting skin

                void_icon = void_icon_off;  //icon off default
                if (power_toggle) void_icon = void_icon_on;     //or on if power_toggle==true
                if (GUI.Button(new Rect(main_icon_pos), void_icon, new GUIStyle())) main_gui_minimized = !main_gui_minimized;

                if (power_toggle && hud_module)
                {
                    if (power_available)
                    {
                        label_hud.normal.textColor = hud_text_colors[counter_hud_text_color];

                        double alt_true = vessel.orbit.altitude - vessel.terrainAltitude;
						// HACK: This assumes that on worlds with oceans, all water is fixed at 0 m, and water covers the whole surface at 0 m.
						if (vessel.terrainAltitude < 0 && vessel.mainBody.ocean ) alt_true = vessel.orbit.altitude;

                        GUI.Label(new Rect((Screen.width * .2083f), 0, 300f, 90f), "Obt Alt: " + Tools.MuMech_ToSI(vessel.orbit.altitude) + "m Obt Vel: " + Tools.MuMech_ToSI(vessel.orbit.vel.magnitude) + "m/s\nAp: " + Tools.MuMech_ToSI(vessel.orbit.ApA) + "m ETA " + Tools.ConvertInterval(vessel.orbit.timeToAp) + "\nPe: " + Tools.MuMech_ToSI(vessel.orbit.PeA) + "m ETA " + Tools.ConvertInterval(vessel.orbit.timeToPe) + "\nInc: " + vessel.orbit.inclination.ToString("F3") + "°" + "\nPrimary: " + vessel.mainBody.bodyName, label_hud);

						// Toadicus edit: Added "Biome: " line to surf/atmo HUD
						GUI.Label(new Rect((Screen.width * .625f), 0, 300f, 90f), "Srf Alt: " + Tools.MuMech_ToSI(alt_true) + "m Srf Vel: " + Tools.MuMech_ToSI(vessel.srf_velocity.magnitude) + "m/s\nVer: " + Tools.MuMech_ToSI(vessel.verticalSpeed) + "m/s Hor: " + Tools.MuMech_ToSI(vessel.horizontalSrfSpeed) + "m/s\nLat: " + Tools.GetLatitudeString(vessel, "F3") + " Lon: " + Tools.GetLongitudeString(vessel, "F3") + "\nHdg: " + Tools.MuMech_get_heading(vessel).ToString("F2") + "° " + get_heading_text(Tools.MuMech_get_heading(vessel)) + "\nBiome: " + Tools.Toadicus_GetAtt(vessel).name, label_hud);
                    }
                    else
                    {
                        label_hud.normal.textColor = Color.red;
                        GUI.Label(new Rect((Screen.width * .2083f), 0, 300f, 70f), "-- POWER LOST --", label_hud);
                        GUI.Label(new Rect((Screen.width * .625f), 0, 300f, 70f), "-- POWER LOST --", label_hud);
                    }
                }

                int window_id = window_base_id;

                if (main_gui_minimized == false) main_window_pos = GUILayout.Window(window_id, main_window_pos, main_gui, version_name + this_version, GUILayout.Width(250), GUILayout.Height(50));
				if (power_toggle && power_available)
                {
                    //hide these windows if power is turned off or unavailable
                    if (void_module) void_window_pos = GUILayout.Window(++window_id, void_window_pos, void_gui, "Orbital Information", GUILayout.Width(250), GUILayout.Height(50));
                    if (atmo_module) atmo_window_pos = GUILayout.Window(++window_id, atmo_window_pos, atmo_gui, "Surface & Atmospheric Information", GUILayout.Width(250), GUILayout.Height(50));
                    if (tad_module) transfer_window_pos = GUILayout.Window(++window_id, transfer_window_pos, tad_gui, "Transfer Angle Information", GUILayout.Width(315), GUILayout.Height(50));
                    if (vessel_register_module) vessel_register_window_pos = GUILayout.Window(++window_id, vessel_register_window_pos, vessel_register_gui, "Vessel Register", GUILayout.Width(250), GUILayout.Height(375));
                    if (data_time_module) data_logging_window_pos = GUILayout.Window(++window_id, data_logging_window_pos, data_time_gui, "Data Logging & Time", GUILayout.Width(250), GUILayout.Height(50));
                    if (vessel_info_module)
					{
						if ((TimeWarp.WarpMode == TimeWarp.Modes.LOW) || (TimeWarp.CurrentRate <= TimeWarp.MaxPhysicsRate))
						{
							SimManager.Instance.RequestSimulation();
						}

						vessel_info_window_pos = GUILayout.Window(++window_id, vessel_info_window_pos, vessel_info_gui, "Vessel Information", GUILayout.Width(250), GUILayout.Height(50));
					}
                    if (misc_module) misc_window_pos = GUILayout.Window(++window_id, misc_window_pos, misc_info_gui, "Miscellaneous", GUILayout.Width(275), GUILayout.Height(50));
                    if (test_module) lab_window_pos = GUILayout.Window(++window_id, lab_window_pos, test_gui, "The Lab", GUILayout.Width(350), GUILayout.Height(50));
                    if (rendezvous_module) rendezvous_info_window_pos = GUILayout.Window(++window_id, rendezvous_info_window_pos, rendezvous_info_gui, "Rendezvous Information", GUILayout.Width(315), GUILayout.Height(50));
                    if (celestial_body_info_module) body_op_window_pos = GUILayout.Window(++window_id, body_op_window_pos, celestial_body_info_gui, "Celestial Body Information", GUILayout.Width(420), GUILayout.Height(50));
                }
            }
        }

        public void Awake()
        {
            settings_path = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(VOID)) + "/"; //returns "X:/full/path/to/GameData/RBR/Plugins/PluginData/VOID"
            load_icons();
            load_settings();
            load_language();
            set_hud_color_list();
        }

        public void Update()
        {
            //Icon relocation
            if (changing_icon_pos && Input.GetMouseButtonDown(0))
            {
                main_icon_pos = new Rect(Input.mousePosition.x - 15f, Screen.height - Input.mousePosition.y - 15f, 30f, 30f);
                changing_icon_pos = false;
            }

            if (FlightGlobals.ActiveVessel != null)
            {
                vessel = FlightGlobals.ActiveVessel;

                if (run_once)
                {
                    if (debugging) Debug.Log("[VOID] running run_once...");
                    all_bodies.AddRange(FlightGlobals.Bodies);
                    vessel_register_body_index = all_bodies.IndexOf(vessel.mainBody);

                    Array va = System.Enum.GetValues(typeof(VesselType));
                    all_vessel_types = va.OfType<VesselType>().ToList();
                    vessel_register_vessel_type_index = all_vessel_types.IndexOf(vessel.vesselType);

                    run_once = false;
                }

				if (vessel == FlightGlobals.ActiveVessel)
				{

					if (SimManager.Instance.Stages != null)
					{
						stages = SimManager.Instance.Stages;
					}

					SimManager.Instance.Gravity = vessel.mainBody.gravParameter / Math.Pow(vessel.mainBody.Radius, 2);
					SimManager.Instance.TryStartSimulation();
				}

                if (gui_running == false) start_GUI();

                cfg_update_timer += 1;
                if (cfg_update_timer >= 15f)
                {
                    write_settings();
                    cfg_update_timer = 0f;
                }

                if (power_available)
                {
	                main_csv(); // a function to group together all the Data Logging/csv functions
	                if (stopwatch1_running) stopwatch1 += Time.deltaTime;
                }
            }
            else
            {
                //activevessel == null
                if (gui_running) stop_GUI();
            }
        }

        public void FixedUpdate()
        {
            if (power_toggle) consume_resources();
        }
    }
}
