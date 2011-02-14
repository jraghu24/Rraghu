﻿using System;
using System.Web.WebPages;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;

namespace umbraco.MacroEngines
{
    public abstract class BaseContext<T> : WebPage, IMacroContext {

        private MacroModel _macro;
        private INode _node;
        protected T CurrentModel;
        protected IParameterDictionary ParameterDictionary;
        protected ICultureDictionary CultureDictionary;

        public IParameterDictionary Parameter { get { return ParameterDictionary; } }
        public ICultureDictionary Dictionary { get { return CultureDictionary; } }

        public MacroModel Macro { get { return _macro; } }
        public INode Node { get { return _node; } }

        public T Current { get { return CurrentModel; } }
        public new dynamic Model { get { return CurrentModel; } }

        public virtual void SetMembers(MacroModel macro, INode node) {
            if (macro == null)
                throw new ArgumentNullException("macro");
            if (node == null)
                throw new ArgumentNullException("node");
            _macro = macro;
            ParameterDictionary = new UmbracoParameterDictionary(macro.Properties);
            CultureDictionary = new UmbracoCultureDictionary();
            _node = node;
        }

        protected override void ConfigurePage(WebPageBase parentPage) {
            if (parentPage == null)
                return;
            //Inject SetMembers Into New Context
            if (parentPage is IMacroContext) {
                var macroContext = (IMacroContext)parentPage;
                SetMembers(macroContext.Macro, macroContext.Node);
            }
        }
    }
}
