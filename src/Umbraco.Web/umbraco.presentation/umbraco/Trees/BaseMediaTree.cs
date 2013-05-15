using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.interfaces;
using Umbraco.Core;
using Media = umbraco.cms.businesslogic.media.Media;
using Property = umbraco.cms.businesslogic.property.Property;

namespace umbraco.cms.presentation.Trees
{
	public abstract class BaseMediaTree : BaseTree
	{
        private DisposableTimer _timer;
        private User _user;

		public BaseMediaTree(string application)
			: base(application)
		{
						
		}

		/// <summary>
		/// Returns the current User. This ensures that we don't instantiate a new User object 
		/// each time.
		/// </summary>
		protected User CurrentUser
		{
			get
			{
				return (_user == null ? (_user = UmbracoEnsuredPage.CurrentUser) : _user);
			}
		}
      		
        public override void RenderJS(ref StringBuilder Javascript)
        {
            if (!string.IsNullOrEmpty(this.FunctionToCall))
            {
                Javascript.Append("function openMedia(id) {\n");
                Javascript.Append(this.FunctionToCall + "(id);\n");
                Javascript.Append("}\n");
            }
            else if (!this.IsDialog)
            {
                Javascript.Append(
					@"
function openMedia(id) {
	" + ClientTools.Scripts.GetContentFrame() + ".location.href = 'editMedia.aspx?id=' + id;" + @"
}
");
            }
        }

        //Updated Render method for improved performance, but currently not usable because of backwards compatibility 
        //with the OnBeforeTreeRender/OnAfterTreeRender events, which sends an array for legacy Media items.
        /*public override void Render(ref XmlTree tree)
        {
            _timer = DisposableTimer.Start(x => LogHelper.Debug<BaseMediaTree>("Media tree loaded" + " (took " + x + "ms)"));

            var service = base.Services.EntityService;
            var entities = service.GetChildren(m_id, UmbracoObjectTypes.Media);
            
            var args = new TreeEventArgs(tree);
            OnBeforeTreeRender(entities, args);

            foreach (UmbracoEntity entity in entities)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = entity.Id.ToString(CultureInfo.InvariantCulture);
                xNode.Text = entity.Name;

                xNode.HasChildren = entity.HasChildren;
                xNode.Source = this.IsDialog ? GetTreeDialogUrl(entity.Id) : GetTreeServiceUrl(entity.Id);

                xNode.Icon = entity.ContentTypeIcon;
                xNode.OpenIcon = entity.ContentTypeIcon;

                xNode.Menu = this.ShowContextMenu ? new List<IAction>(new IAction[] { ActionRefresh.Instance }) : null;

                if (IsDialog == false)
                {
                    xNode.Action = "javascript:openMedia(" + entity.Id + ");";
                }
                else
                {
                    if (this.DialogMode == TreeDialogModes.fulllink)
                    {
                        if (string.IsNullOrEmpty(entity.UmbracoFile) == false)
                        {
                            xNode.Action = "javascript:openMedia('" + entity.UmbracoFile + "');";
                        }
                        else
                        {
                            if (string.Equals(entity.ContentTypeAlias, Constants.Conventions.MediaTypes.Folder, StringComparison.OrdinalIgnoreCase))
                            {
                                xNode.Action = "javascript:jQuery('.umbTree #" + entity.Id.ToString(CultureInfo.InvariantCulture) + "').click();";
                            }
                            else
                            {
                                xNode.Action = null;
                                xNode.Style.DimNode();
                            }
                        }
                    }
                    else
                    {
                        xNode.Action = "javascript:openMedia('" + entity.Id.ToString(CultureInfo.InvariantCulture) + "');";
                    }
                }

                OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                if (xNode != null)
                {
                    tree.Add(xNode);
                    OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                }
            }

            //stop the timer and log the output
            _timer.Dispose();

            OnAfterTreeRender(entities, args);
        }*/

        public override void Render(ref XmlTree tree)
        {
            //_timer = DisposableTimer.Start(x => LogHelper.Debug<BaseMediaTree>("Media tree loaded" + " (took " + x + "ms)"));
          
			Media[] docs = new Media(m_id).Children;

            var args = new TreeEventArgs(tree);
            OnBeforeTreeRender(docs, args);

            foreach (Media dd in docs)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = dd.Id.ToString();
                xNode.Text = dd.Text;

                // Check for dialog behaviour
                if (!this.IsDialog)
                {
                    if (!this.ShowContextMenu)
                        xNode.Menu = null;
                    xNode.Action = "javascript:openMedia(" + dd.Id + ");";
                }
                else
                {
                    if (this.ShowContextMenu)
                        xNode.Menu = new List<IAction>(new IAction[] { ActionRefresh.Instance });
                    else
                        xNode.Menu = null;
                    if (this.DialogMode == TreeDialogModes.fulllink)
                    {
                        string nodeLink = GetLinkValue(dd, dd.Id.ToString());
                        if (!String.IsNullOrEmpty(nodeLink))
                        {
                            xNode.Action = "javascript:openMedia('" + nodeLink + "');";
                        }
                        else
                        {
                            if (string.Equals(dd.ContentType.Alias, Constants.Conventions.MediaTypes.Folder, StringComparison.OrdinalIgnoreCase))
                            {
                                xNode.Action = "javascript:jQuery('.umbTree #" + dd.Id.ToString() + "').click();";
                            }
                            else
                            {
                                xNode.Action = null;
                                xNode.Style.DimNode();
                            }
                        }
                    }
                    else
                    {
                        xNode.Action = "javascript:openMedia('" + dd.Id.ToString() + "');";
                    }
                }

				xNode.HasChildren = dd.HasChildren;
                if (this.IsDialog)
                    xNode.Source = GetTreeDialogUrl(dd.Id);
                else
                    xNode.Source = GetTreeServiceUrl(dd.Id);                    

                if (dd.ContentType != null)
                {
                    xNode.Icon = dd.ContentType.IconUrl;
                    xNode.OpenIcon = dd.ContentType.IconUrl;                    
                }

                OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                if (xNode != null)
                {
                    tree.Add(xNode);
                    OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                }
            }
            //_timer.Dispose();
            OnAfterTreeRender(docs, args);
        }

        /// <summary>
		/// Returns the value for a link in WYSIWYG mode, by default only media items that have a 
		/// DataTypeUploadField are linkable, however, a custom tree can be created which overrides
		/// this method, or another GUID for a custom data type can be added to the LinkableMediaDataTypes
		/// list on application startup.
		/// </summary>
		/// <param name="dd"></param>
		/// <param name="nodeLink"></param>
		/// <returns></returns>
        public virtual string GetLinkValue(Media dd, string nodeLink)
        {
            var props = dd.GenericProperties;
			foreach (Property p in props)
			{				
				Guid currId = p.PropertyType.DataTypeDefinition.DataType.Id;
				if (LinkableMediaDataTypes.Contains(currId) &&  !String.IsNullOrEmpty(p.Value.ToString()))
				{
					return p.Value.ToString();
				}
			}
            return "";
        }

		/// <summary>
		/// By default, any media type that is to be "linkable" in the WYSIWYG editor must contain
		/// a DataTypeUploadField data type which will ouput the value for the link, however, if 
		/// a developer wants the WYSIWYG editor to link to a custom media type, they will either have
		/// to create their own media tree and inherit from this one and override the GetLinkValue 
		/// or add another GUID to the LinkableMediaDataType list on application startup that matches
		/// the GUID of a custom data type. The order of property types on the media item definition will determine the output value.
		/// </summary>
		public static List<Guid> LinkableMediaDataTypes { get; protected set; }

	}
}
