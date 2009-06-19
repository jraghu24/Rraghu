
<%@ Page Language="C#" MasterPageFile="../../masterpages/umbracoPage.Master" AutoEventWireup="true" Codebehind="editScript.aspx.cs" Inherits="umbraco.cms.presentation.settings.scripts.editScript"
  ValidateRequest="False" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
  <script language="javascript" type="text/javascript">
   
    function doSubmit() {
      umbraco.presentation.webservices.codeEditorSave.SaveScript(jQuery('#<%= NameTxt.ClientID %>').val(),'<%= NameTxt.Text %>', jQuery('#<%= editorSource.ClientID %>').val(), submitSucces, submitFailure);
    }

    function submitSucces(t) {
      if (t != 'true') {
        top.UmbSpeechBubble.ShowMessage('error', '<%= umbraco.ui.Text("speechBubbles", "fileErrorHeader") %>', '<%= umbraco.ui.Text("speechBubbles", "fileErrorText") %>');
      }
      else {
        top.UmbSpeechBubble.ShowMessage('save', '<%= umbraco.ui.Text("speechBubbles", "fileSavedHeader") %>', '')
      }
    }
    function submitFailure(t) {
      top.UmbSpeechBubble.ShowMessage('error', '<%= umbraco.ui.Text("speechBubbles", "fileErrorHeader") %>', '<%= umbraco.ui.Text("speechBubbles", "fileErrorText") %>')
    }    
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:UmbracoPanel ID="Panel1" runat="server" Width="608px" Height="336px" hasMenu="true">
      <cc1:Pane ID="Pane7" runat="server" Height="44px" Width="528px">
        <cc1:PropertyPanel runat="server" ID="pp_name">
              <asp:TextBox ID="NameTxt" Width="350px" runat="server"></asp:TextBox>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel runat="server" id="pp_path">
              <asp:Literal ID="lttPath" runat="server"/>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="pp_source" runat="server">
            <cc1:CodeArea ID="editorSource" runat="server" AutoResize="true" OffSetX="47" OffSetY="47" />
        </cc1:PropertyPanel>
       </cc1:Pane>
    </cc1:UmbracoPanel>
</asp:Content>