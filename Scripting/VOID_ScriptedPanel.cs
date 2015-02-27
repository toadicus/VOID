﻿// VOID
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
	public class VOID_ScriptedPanel : VOID_WindowModule, IConfigNode
	{
		public const string LINE_KEY = "PANEL_LINE";
		public const string TITLE_KEY = "Title";
		public const string POSITION_KEY = "WindowPos";

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

				GUILayout.Label(line.LabelFunction.Invoke() + ":");
				GUILayout.FlexibleSpace();
				GUILayout.Label(line.ValueFunction.Invoke(), GUILayout.ExpandWidth (false));

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

		public Func<string> LabelFunction
		{
			get;
			private set;
		}

		public Func<string> ValueFunction
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
				this.LabelFunction = this.parseFunctionScript(value) as Func<string>;

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
				this.ValueFunction = this.parseFunctionScript(value) as Func<string>;

				this.valueScript = value;
			}
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

				this.LabelFunction = this.parseFunctionScript(this.LabelScript) as Func<string>;
			}

			if (node.TryGetValue(VALUE_KEY, out valueScript))
			{
				this.ValueScript = valueScript;

				this.ValueFunction = this.parseFunctionScript(this.ValueScript) as Func<string>;
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

		private Delegate parseFunctionScript(string script)
		{
			LambdaExpression scriptExpression = null;

			try
			{
				ScriptParser parser = new ScriptParser(script);

				scriptExpression = parser.Parse();

				return scriptExpression.Compile();
			}
			catch (VOIDScriptSyntaxException x1)
			{
				Tools.PostErrorMessage("Failed to parse script: '{0}'.", this.LabelScript);
				throw x1;
			}
			catch (Exception x2)
			{
				Tools.PostErrorMessage("Failed to compile script: '{0}'.\nExpression: {1}",
					this.LabelScript,
					scriptExpression
				);

				throw x2;
			}
		}
	}
}
