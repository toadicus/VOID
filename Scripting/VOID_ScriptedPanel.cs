// VOID
//
// VOID_PanelConfig.cs
//
// Copyright © 2015, toadicus
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation and/or other
//    materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its contributors may be used
//    to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using ToadicusTools;
using VOID;

namespace VOID_ScriptedPanels
{
	[VOID_GameModes(new Game.Modes[] {})]
	[VOID_Scenes(new GameScenes[] {})]
	public class VOID_ScriptedPanel : VOID_WindowModule, IConfigNode
	{
		public const string PANEL_KEY = "VOID_PANEL";
		public const string LINE_KEY = "PANEL_LINE";
		public const string TITLE_KEY = "Title";
		public const string POSITION_KEY = "WindowPos";
		public const string SCENES_KEY = "ValidScenes";
		public const string MODES_KEY = "ValidModes";

		private static readonly List<UrlDir.UrlFile> voidScriptFiles = new List<UrlDir.UrlFile>();
		private static readonly List<VOID_ScriptedPanel> panels = new List<VOID_ScriptedPanel>();

		public static IList<VOID_ScriptedPanel> Panels
		{
			get
			{
				return panels.AsReadOnly();
			}
		}

		public static void LoadScriptedPanels()
		{
			if (GameDatabase.Instance != null && GameDatabase.Instance.IsReady())
			{
				if (voidScriptFiles.Count == 0)
				{
					foreach (var cfgFile in GameDatabase.Instance.root.AllConfigFiles)
					{
						if (cfgFile.ContainsConfig(PANEL_KEY))
						{
							voidScriptFiles.Add(cfgFile);

							Tools.PostLogMessage("VOID_ScriptedPanel: Added config file {0} (url: {1}, fullPath: {2}",
								cfgFile, cfgFile.url, cfgFile.fullPath);
						}
					}
				}

				panels.Clear();

				foreach (UrlDir.UrlFile file in voidScriptFiles)
				{
					var configs = UrlDir.UrlConfig.CreateNodeList(file.parent, file);

					foreach (var panelConfig in configs)
					{
						if (panelConfig.config.name == PANEL_KEY)
						{
							panels.Add(new VOID_ScriptedPanel(panelConfig.config));
						}
					}
				}
			}
		}

		private List<VOID_PanelLine> panelLines;

		public IList<VOID_PanelLine> PanelLines
		{
			get
			{
				return this.panelLines.AsReadOnly();
			}
		}

		public VOID_ScriptedPanel() : base()
		{
			this.panelLines = new List<VOID_PanelLine>();
		}

		public VOID_ScriptedPanel(ConfigNode node) : this()
		{
			this.Load(node);
		}

		public void Load(ConfigNode node)
		{
			this.Name = node.GetValue(TITLE_KEY, string.Empty);

			string positionString;

			if (node.TryGetValue(POSITION_KEY, out positionString))
			{
				Vector2 positionVector = KSPUtil.ParseVector2(positionString);

				this.WindowPos.x = positionVector.x;
				this.WindowPos.y = positionVector.y;
			}

			string scenesString;

			if (node.TryGetValue(SCENES_KEY, out scenesString))
			{
				scenesString = scenesString.ToLower();

				if (scenesString == "all")
				{
					this.validScenes = (GameScenes[])Enum.GetValues(typeof(GameScenes));
				}

				string[] scenesArray = scenesString.Split(',');

				List<GameScenes> scenes = new List<GameScenes>();

				foreach (string sceneString in scenesArray)
				{
					GameScenes scene;

					try
					{
						scene = (GameScenes)Enum.Parse(typeof(GameScenes), sceneString.Trim(), true);
						scenes.Add(scene);
					}
					catch
					{
						Tools.PostErrorMessage(
							"{0}: Failed parsing {1}: '{2}' not a valid {3}.",
							this.Name,
							SCENES_KEY,
							sceneString,
							typeof(GameScenes).Name
						);
					}
				}

				this.validScenes = scenes.ToArray();
			}

			string modesString;

			if (node.TryGetValue(MODES_KEY, out modesString))
			{
				string[] modesArray = modesString.Split(',');

				List<Game.Modes> modes = new List<Game.Modes>();

				foreach (string modeString in modesArray)
				{
					Game.Modes mode;

					try
					{
						mode = (Game.Modes)Enum.Parse(typeof(Game.Modes), modeString.Trim(), true);
						modes.Add(mode);
					}
					catch
					{
						Tools.PostErrorMessage(
							"{0}: Failed parsing {1}: '{2}' not a valid {3}.",
							this.Name,
							SCENES_KEY,
							modeString,
							typeof(GameScenes).Name
						);
					}
				}

				this.validModes = modes.ToArray();
			}

			if (node.HasNode(LINE_KEY))
			{
				foreach (var lineNode in node.GetNodes(LINE_KEY))
				{
					VOID_PanelLine line = new VOID_PanelLine(lineNode);

					this.panelLines.Add(line);
				}

				this.panelLines.Sort((x, y) => x.LineNumber.CompareTo(y.LineNumber));
			}
		}

		public void Save(ConfigNode node)
		{
			node.SafeSetValue(TITLE_KEY, this.Name);

			node.SafeSetValue(POSITION_KEY, string.Format("{0}, {1}", this.WindowPos.x, this.WindowPos.y));

			node.ClearNodes();

			foreach (var line in this.panelLines)
			{
				ConfigNode lineNode = node.AddNode(LINE_KEY);

				line.Save(lineNode);
			}
		}

		public override void ModuleWindow(int id)
		{
			foreach (var line in this.panelLines)
			{
				GUILayout.BeginHorizontal();

				if (line.LabelFunction != null)
				{
					object labelObj = line.LabelFunction.DynamicInvoke();

					if (labelObj is string)
					{
						GUILayout.Label((string)labelObj);
					}
					else if (labelObj is GUIContent)
					{
						GUILayout.Label((GUIContent)labelObj);
					}
					else
					{
						GUILayout.Label(labelObj.ToString());
					}
				}

				if (line.LabelFunction != null && line.ValueFunction != null)
				{
					GUILayout.FlexibleSpace();
				}

				if (line.ValueFunction != null)
				{
					object valueObj = line.ValueFunction.DynamicInvoke();

					if (valueObj is string)
					{
						GUILayout.Label((string)valueObj);
					}
					else if (valueObj is GUIContent)
					{
						GUILayout.Label((GUIContent)valueObj);
					}
					else
					{
						GUILayout.Label(valueObj.ToString());
					}
				}

				GUILayout.EndHorizontal();
			}

			base.ModuleWindow(id);
		}
	}

	public class VOID_PanelLine : IConfigNode
	{
		public const string LABEL_KEY = "Label";
		public const string VALUE_KEY = "Value";
		public const string LINENO_KEY = "LineNumber";

		private string labelScript;
		private string valueScript;

		public ushort LineNumber;

		public Delegate LabelFunction
		{
			get;
			private set;
		}

		public Delegate ValueFunction
		{
			get;
			private set;
		}

		public string LabelScript
		{
			get
			{
				return this.labelScript;
			}
			set
			{
				this.LabelFunction = this.parseFunctionScript(value, ParsingCell.Label);

				this.labelScript = value;
			}
		}

		public string ValueScript
		{
			get
			{
				return this.valueScript;
			}
			set
			{
				this.ValueFunction = this.parseFunctionScript(value, ParsingCell.Value);

				this.valueScript = value;
			}
		}

		public GUIContent LabelErrorContent
		{
			get;
			private set;
		}

		public GUIContent ValueErrorContent
		{
			get;
			private set;
		}

		public VOID_PanelLine()
		{
			this.LineNumber = ushort.MaxValue;
			this.labelScript = string.Empty;
			this.valueScript = string.Empty;
		}

		public VOID_PanelLine(ConfigNode node) : this()
		{
			this.Load(node);
		}

		public void Load(ConfigNode node)
		{
			string labelScript;
			string valueScript;

			if (node.TryGetValue(LABEL_KEY, out labelScript))
			{
				this.LabelScript = labelScript;
			}

			if (node.TryGetValue(VALUE_KEY, out valueScript))
			{
				this.ValueScript = valueScript;
			}

			string lineNo;
			ushort lineNumber;

			if (node.TryGetValue(LINENO_KEY, out lineNo))
			{
				if (ushort.TryParse(lineNo, out lineNumber))
				{
					this.LineNumber = lineNumber;
				}
			}
		}

		public void Save(ConfigNode node)
		{
			if (node.HasValue(LABEL_KEY))
			{
				node.SetValue(LABEL_KEY, this.LabelScript);
			}
			else
			{
				node.AddValue(LABEL_KEY, this.LabelScript);
			}

			if (node.HasValue(VALUE_KEY))
			{
				node.SetValue(VALUE_KEY, this.ValueScript);
			}
			else
			{
				node.AddValue(VALUE_KEY, this.ValueScript);
			}

			if (node.HasValue(LINENO_KEY))
			{
				node.SetValue(LINENO_KEY, this.LineNumber.ToString());
			}
			else
			{
				node.AddValue(LINENO_KEY, this.LineNumber.ToString());
			}
		}

		private Delegate parseFunctionScript(string script, ParsingCell cell)
		{
			LambdaExpression scriptExpression = null;

			try
			{
				ScriptParser parser = new ScriptParser(script);

				scriptExpression = parser.Parse();

				Tools.PostDebugMessage(this, "Parsed expression '{0}' from '{1}'.", scriptExpression, script);

				return scriptExpression.Compile();
			}
			catch (VOIDScriptSyntaxException x1)
			{
				return this.getSyntaxErrorContent("Syntax Error", x1.Message, cell);
			}
			catch (VOIDScriptParserException x2)
			{
				return this.getSyntaxErrorContent("Parser Error", x2.Message + " Please report!", cell);
			}
			catch (Exception x3)
			{
				Tools.PostErrorMessage(
					"Compiler error processing VOIDScript line '{0}'.  Please report!\n{1}: {2}\n{3}",
					script, x3.GetType().Name, x3.Message, x3.StackTrace);
				return this.getSyntaxErrorContent("Compiler Error", x3.GetType().Name + " " + x3.Message, cell);
			}
		}

		private Func<GUIContent> getSyntaxErrorContent(string message, string tooltip, ParsingCell cell)
		{
			switch (cell)
			{
				case ParsingCell.Label:
					if (this.LabelErrorContent == null)
					{
						this.LabelErrorContent = new GUIContent(message);
					}

					this.LabelErrorContent.tooltip = tooltip;

					return () => this.LabelErrorContent;
				case ParsingCell.Value:
					if (this.ValueErrorContent == null)
					{
						this.ValueErrorContent = new GUIContent(message);
					}
					this.ValueErrorContent.tooltip = tooltip;

					return () => this.ValueErrorContent;
				default:
					return () => new GUIContent(message);
			}
		}

		private enum ParsingCell
		{
			Label,
			Value
		}
	}
}

