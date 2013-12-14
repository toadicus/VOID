//
//  ToolbarWrapper.cs
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
using KSP;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace VOID
{
	internal class ToolbarButtonWrapper
	{
		protected System.Type ToolbarManager;
		protected object TBManagerInstance;
		protected MethodInfo TBManagerAdd;
		protected System.Type IButton;
		protected object Button;
		protected PropertyInfo ButtonText;
		protected PropertyInfo ButtonTextColor;
		protected PropertyInfo ButtonTexturePath;
		protected PropertyInfo ButtonToolTip;
		protected PropertyInfo ButtonVisible;
		protected PropertyInfo ButtonVisibility;
		protected PropertyInfo ButtonEnalbed;
		protected EventInfo ButtonOnClick;
		protected System.Type ClickHandlerType;
		protected MethodInfo ButtonDestroy;
		protected System.Type GameScenesVisibilityType;

		public string Text
		{
			get
			{
				return this.ButtonText.GetValue(this.Button, null) as String;
			}
			set
			{
				this.ButtonText.SetValue(this.Button, value, null);
			}
		}

		public Color TextColor
		{
			get
			{
				return (Color)this.ButtonTextColor.GetValue(this.Button, null);
			}
			set
			{
				this.ButtonTextColor.SetValue(this.Button, value, null);
			}
		}

		public string TexturePath
		{
			get
			{
				return this.ButtonTexturePath.GetValue(this.Button, null) as string;
			}
			set
			{
				this.ButtonTexturePath.SetValue(this.Button, value, null);
			}
		}

		public string ToolTip
		{
			get
			{
				return this.ButtonToolTip.GetValue(this.Button, null) as string;
			}
			set
			{
				this.ButtonToolTip.SetValue(this.Button, value, null);
			}
		}

		public bool Visible
		{
			get
			{
				return (bool)this.ButtonVisible.GetValue(this.Button, null);
			}
			set
			{
				this.ButtonVisible.SetValue(this.Button, value, null);
			}
		}

		public bool Enabled
		{
			get
			{
				return (bool)this.ButtonEnalbed.GetValue(this.Button, null);
			}
			set
			{
				this.ButtonEnalbed.SetValue(this.Button, value, null);
			}
		}

		private ToolbarButtonWrapper() {}

		public ToolbarButtonWrapper(string ns, string id)
		{
			Tools.PostDebugMessage(string.Format(
				"{0}: Loading ToolbarManager.",
				this.GetType().Name
			));

			this.ToolbarManager = AssemblyLoader.loadedAssemblies
				.Select(a => a.assembly.GetExportedTypes())
				.SelectMany(t => t)
				.FirstOrDefault(t => t.FullName == "Toolbar.ToolbarManager");

			Tools.PostDebugMessage(string.Format(
				"{0}: Loaded ToolbarManager.  Getting Instance.",
				this.GetType().Name
				));

			this.TBManagerInstance = this.ToolbarManager.GetProperty(
					"Instance",
					System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
				)
				.GetValue(null, null);

			Tools.PostDebugMessage(string.Format(
				"{0}: Got ToolbarManager Instance '{1}'.  Getting 'add' method.",
				this.GetType().Name,
				this.TBManagerInstance
				));

			this.TBManagerAdd = this.ToolbarManager.GetMethod("add");

			Tools.PostDebugMessage(string.Format(
				"{0}: Got ToolbarManager Instance 'add' method.  Loading IButton.",
				this.GetType().Name
				));

			this.IButton = AssemblyLoader.loadedAssemblies
				.Select(a => a.assembly.GetExportedTypes())
				.SelectMany(t => t)
				.FirstOrDefault(t => t.FullName == "Toolbar.IButton");

			Tools.PostDebugMessage(string.Format(
				"{0}: Loaded IButton.  Adding Button with ToolbarManager.",
				this.GetType().Name
				));

			this.Button = this.TBManagerAdd.Invoke(this.TBManagerInstance, new object[] {ns, id});

			Tools.PostDebugMessage(string.Format(
				"{0}: Added Button '{1}' with ToolbarManager.  Getting 'Text' property",
				this.GetType().Name,
				this.Button.ToString()
				));

			this.ButtonText = this.IButton.GetProperty("Text");

			Tools.PostDebugMessage(string.Format(
				"{0}: Got 'Text' property.  Getting 'TextColor' property.",
				this.GetType().Name
				));

			this.ButtonTextColor = this.IButton.GetProperty("TextColor");

			Tools.PostDebugMessage(string.Format(
				"{0}: Got 'TextColor' property.  Getting 'TexturePath' property.",
				this.GetType().Name
				));

			this.ButtonTexturePath = this.IButton.GetProperty("TexturePath");
			
			Tools.PostDebugMessage(string.Format(
				"{0}: Got 'TexturePath' property.  Getting 'ToolTip' property.",
				this.GetType().Name
				));

			this.ButtonToolTip = this.IButton.GetProperty("ToolTip");

			Tools.PostDebugMessage(string.Format(
				"{0}: Got 'ToolTip' property.  Getting 'Visible' property.",
				this.GetType().Name
				));

			this.ButtonVisible = this.IButton.GetProperty("Visible");

			Tools.PostDebugMessage(string.Format(
				"{0}: Got 'Visible' property.  Getting 'Visibility' property.",
				this.GetType().Name
				));

			this.ButtonVisibility = this.IButton.GetProperty("Visibility");

			Tools.PostDebugMessage(string.Format(
				"{0}: Got 'Visibility' property.  Getting 'Enabled' property.",
				this.GetType().Name
				));

			this.ButtonEnalbed = this.IButton.GetProperty("Enabled");

			Tools.PostDebugMessage(string.Format(
				"{0}: Got 'Enabled' property.  Getting 'OnClick' event.",
				this.GetType().Name
				));

			this.ButtonOnClick = this.IButton.GetEvent("OnClick");
			this.ClickHandlerType = this.ButtonOnClick.EventHandlerType;

			Tools.PostDebugMessage(string.Format(
				"{0}: Got 'OnClick' event '{1}'.  Getting 'Destroy' method.",
				this.GetType().Name,
				this.ButtonOnClick.ToString()
				));

			this.ButtonDestroy = this.IButton.GetMethod("Destroy");

			Tools.PostDebugMessage(string.Format(
				"{0}: Got 'Destroy' property '{1}'.  Loading GameScenesVisibility class.",
				this.GetType().Name,
				this.ButtonDestroy.ToString()
				));

			this.GameScenesVisibilityType = AssemblyLoader.loadedAssemblies
				.Select(a => a.assembly.GetExportedTypes())
					.SelectMany(t => t)
					.FirstOrDefault(t => t.FullName == "Toolbar.GameScenesVisibility");

			Tools.PostDebugMessage(string.Format(
				"{0}: Got 'GameScenesVisibility' class '{1}'.",
				this.GetType().Name,
				this.GameScenesVisibilityType.ToString()
				));

			Tools.PostDebugMessage("ToolbarButtonWrapper built!");
		}

		public void AddButtonClickHandler(Action<object> Handler)
		{
			Delegate d = Delegate.CreateDelegate(this.ClickHandlerType, Handler.Target, Handler.Method);
			MethodInfo addHandler = this.ButtonOnClick.GetAddMethod();
			addHandler.Invoke(this.Button, new object[] { d });
		}

		public void SetButtonVisibility(params GameScenes[] gameScenes)
		{
			object GameScenesVisibilityObj = Activator.CreateInstance(this.GameScenesVisibilityType, gameScenes);
			this.ButtonVisibility.SetValue(this.Button, GameScenesVisibilityObj, null);
		}

		public void Destroy()
		{
			this.ButtonDestroy.Invoke(this.Button, null);
		}
	}
}
