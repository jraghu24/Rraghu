﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using Umbraco.Web.Trees;
using umbraco;
using umbraco.BasePages;
using umbraco.cms.businesslogic.template;
using umbraco.cms.helpers;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using umbraco.uicontrols;

namespace Umbraco.Web.UI.Umbraco.Settings.Views
{
	public partial class EditView : global::umbraco.BasePages.UmbracoEnsuredPage
	{
		private Template _template;
		protected MenuIconI SaveButton;

		public EditView()
		{
			CurrentApp = global::umbraco.BusinessLogic.DefaultApps.settings.ToString();
		}

		/// <summary>
		/// The type of MVC/Umbraco view the editor is editing
		/// </summary>
		public enum ViewEditorType
		{
			Template,
			PartialView
		}

		/// <summary>
		/// Returns the type of view being edited
		/// </summary>
		protected ViewEditorType EditorType
		{
			get { return _template == null ? ViewEditorType.PartialView : ViewEditorType.Template; }
		}

		/// <summary>
		/// Returns the original file name that the editor was loaded with
		/// </summary>
		/// <remarks>
		/// this is used for editing a partial view
		/// </remarks>
		protected string OriginalFileName { get; private set; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!IsPostBack)
			{

				//configure screen for editing a template
				if (_template != null)
				{
					MasterTemplate.Items.Add(new ListItem(ui.Text("none"), "0"));
					var selectedTemplate = string.Empty;

					foreach (Template t in Template.GetAllAsList())
					{
						if (t.Id == _template.Id) continue;

						var li = new ListItem(t.Text, t.Id.ToString());
						li.Attributes.Add("id", t.Alias.Replace(" ", "") + ".cshtml");
						MasterTemplate.Items.Add(li);
					}

					try
					{
						if (_template.MasterTemplate > 0)
							MasterTemplate.SelectedValue = _template.MasterTemplate.ToString();
					}
					catch (Exception ex)
					{
					}

					MasterTemplate.SelectedValue = selectedTemplate;
					NameTxt.Text = _template.GetRawText();
					AliasTxt.Text = _template.Alias;
					editorSource.Text = _template.Design;

					ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<PartialViewsTree>().Tree.Alias)
					.SyncTree("-1,init," + _template.Path.Replace("-1,", ""), false);
				}
				else
				{
					//configure editor for editing a file....

					NameTxt.Text = OriginalFileName;
					var file = IOHelper.MapPath(SystemDirectories.MvcViews.EnsureEndsWith('/') + OriginalFileName);

					using (var sr = File.OpenText(file))
					{
						var s = sr.ReadToEnd();
						editorSource.Text = s;
					}					
					
					//string path = DeepLink.GetTreePathFromFilePath(file);
					//ClientTools
					//	.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadPython>().Tree.Alias)
					//	.SyncTree(path, false);
				}							
			}
		}


		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			//check if a templateId is assigned, meaning we are editing a template
			if (!Request.QueryString["templateID"].IsNullOrWhiteSpace())
			{
				_template = new Template(int.Parse(Request.QueryString["templateID"]));	
			}
			else if (!Request.QueryString["file"].IsNullOrWhiteSpace())
			{
				//we are editing a view (i.e. partial view)
				OriginalFileName = HttpUtility.UrlDecode(Request.QueryString["file"]);
			}
			else
			{
				throw new InvalidOperationException("Cannot render the editor without a supplied templateId or a file");
			}
			
			Panel1.hasMenu = true;

			SaveButton = Panel1.Menu.NewIcon();
			SaveButton.ImageURL = SystemDirectories.Umbraco + "/images/editor/save.gif";
			//SaveButton.OnClickCommand = "doSubmit()";
			SaveButton.AltText = ui.Text("save");
			SaveButton.ID = "save";

			Panel1.Text = ui.Text("edittemplate");
			pp_name.Text = ui.Text("name", base.getUser());
			pp_alias.Text = ui.Text("alias", base.getUser());
			pp_masterTemplate.Text = ui.Text("mastertemplate", base.getUser());

			// Editing buttons
			Panel1.Menu.InsertSplitter();
			MenuIconI umbField = Panel1.Menu.NewIcon();
			umbField.ImageURL = UmbracoPath + "/images/editor/insField.gif";
			umbField.OnClickCommand =
				ClientTools.Scripts.OpenModalWindow(
					IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" +
					editorSource.ClientID + "&tagName=UMBRACOGETDATA&mvcView=true", ui.Text("template", "insertPageField"), 640, 550);
			umbField.AltText = ui.Text("template", "insertPageField");


			// TODO: Update icon
			MenuIconI umbDictionary = Panel1.Menu.NewIcon();
			umbDictionary.ImageURL = GlobalSettings.Path + "/images/editor/dictionaryItem.gif";
			umbDictionary.OnClickCommand =
				ClientTools.Scripts.OpenModalWindow(
					IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" +
                    editorSource.ClientID + "&tagName=UMBRACOGETDICTIONARY&mvcView=true", ui.Text("template", "insertDictionaryItem"),
					640, 550);
			umbDictionary.AltText = "Insert umbraco dictionary item";

			//uicontrols.MenuIconI umbMacro = Panel1.Menu.NewIcon();
			//umbMacro.ImageURL = UmbracoPath + "/images/editor/insMacro.gif";
			//umbMacro.AltText = ui.Text("template", "insertMacro");
			//umbMacro.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/dialogs/editMacro.aspx?objectId=" + editorSource.ClientID, ui.Text("template", "insertMacro"), 470, 530);

			Panel1.Menu.NewElement("div", "splitButtonMacroPlaceHolder", "sbPlaceHolder", 40);


			if (_template == null)
			{
				InitializeEditorForPartialView();
			}
			else
			{
				InitializeEditorForTemplate();	
			}
			

			//Spit button
			Panel1.Menu.InsertSplitter();
			Panel1.Menu.NewElement("div", "splitButtonPlaceHolder", "sbPlaceHolder", 40);			
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
			ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
		}
		
		/// <summary>
		/// Configure the editor for partial view editing
		/// </summary>
		private void InitializeEditorForPartialView()
		{
			pp_masterTemplate.Visible = false;
			pp_alias.Visible = false;
			pp_name.Text = "Filename";
		}

		/// <summary>
		/// Configure the editor for editing a template
		/// </summary>
		private void InitializeEditorForTemplate()
		{
			if (UmbracoSettings.UseAspNetMasterPages)
			{
				Panel1.Menu.InsertSplitter();

				MenuIconI umbContainer = Panel1.Menu.NewIcon();
				umbContainer.ImageURL = UmbracoPath + "/images/editor/masterpagePlaceHolder.gif";
				umbContainer.AltText = ui.Text("template", "insertContentAreaPlaceHolder");
				umbContainer.OnClickCommand =
					ClientTools.Scripts.OpenModalWindow(
						IOHelper.ResolveUrl(SystemDirectories.Umbraco) +
						"/dialogs/insertMasterpagePlaceholder.aspx?&id=" + _template.Id,
						ui.Text("template", "insertContentAreaPlaceHolder"), 470, 320);

				MenuIconI umbContent = Panel1.Menu.NewIcon();
				umbContent.ImageURL = UmbracoPath + "/images/editor/masterpageContent.gif";
				umbContent.AltText = ui.Text("template", "insertContentArea");
				umbContent.OnClickCommand =
					ClientTools.Scripts.OpenModalWindow(
						IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/insertMasterpageContent.aspx?id=" +
						_template.Id, ui.Text("template", "insertContentArea"), 470, 300);
			}
		}

	}
}