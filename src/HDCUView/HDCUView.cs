﻿using Contract;
using Impl;
using System.ComponentModel.Composition;

namespace HDCUView
{
    [Export(typeof(IView))]
    public class HDCUView : BaseView, IView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public HDCUView() : base("CDM", "CDU") { }

        /// <summary>
        /// Creates an HCDU Table instance. 
        /// </summary>
        /// <param name="ctx">Context for the command. </param>
        /// <returns>new HCDU table</returns>
        protected override BaseTable CreateTableInstance(IContext ctx)
        {
            return new HDCUTable(ctx, viewName);
        }
    }
}