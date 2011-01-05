<%@ Register Namespace="umbraco" TagPrefix="umb" Assembly="umbraco" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" Codebehind="about.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.about" %>

<asp:Content ContentPlaceHolderID="body" runat="server">
      
      
      <div style="padding-right: 5px; padding-left: 5px; padding-bottom: 0px; padding-top: 10px;  text-align: center;">
        
        <img src="../images/umbracoSplash.png" />
          
        <p style="padding-right: 5px; padding-left: 5px; padding-bottom: 0px; margin: 0px; padding-top: 5px">
          umbraco v
          <asp:Literal ID="version" runat="server"></asp:Literal><br />
          <br />
          Copyright � 2001 -
          <asp:Literal ID="thisYear" runat="server"></asp:Literal>
          Umbraco / Niels Hartvig<br />
          Developed by the <a href="http://our.umbraco.org/wiki/about/core-team" target="_blank">Umbraco Core
              Team</a><br />
          <br />
          
          The umbraco framework is licensed under <a href="http://umbraco.org/redir/license"
            target="_blank">the open source license MIT</a>, the umbraco UI is licensed under
          <a href="http://umbraco.org/redir/license" target="_blank">the "umbraco license"</a><br />
          <br />
          Visit <a href="http://umbraco.org/redir/from-about" target="_blank">umbraco.org</a>
          for more information.<br />
          <br />
          Dedicated to Gry, August, Villum and Oliver!<br />
        </p>
      </div>
</asp:Content>
   
