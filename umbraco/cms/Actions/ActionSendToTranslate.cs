using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when a send to translate request occurs
	/// </summary>
	public class ActionSendToTranslate : IAction
	{
		//create singleton
		private static readonly ActionSendToTranslate m_instance = new ActionSendToTranslate();

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionSendToTranslate() { }

		public static ActionSendToTranslate Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return '5';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionSendToTranslate()", ClientTools.Scripts.GetAppActions);
			}
		}

		public string JsSource
		{
			get
			{
				return null;
			}
		}

		public string Alias
		{
			get
			{
				return "sendToTranslate";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprSendToTranslate";
			}
		}

		public bool ShowInNotifier
		{
			get
			{
				return true;
			}
		}
		public bool CanBePermissionAssigned
		{
			get
			{
				return true;
			}
		}

		#endregion
	}
}
