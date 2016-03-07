using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;

namespace iMedia.SC.UI
{
    public class TemplateFieldCommands : Command
    {
        public override void Execute(CommandContext Context)
        {
            Sitecore.Context.ClientPage.Start((object)this, "Run", Context.Parameters);
        }

        protected void Run(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {   
                string url = "/sitecore/client/Your Apps/RepeatableTemplates/RepeatableTemplateDialog.aspx?sc_lang=en&someParam" + args.Parameters["someParam"];
                SheerResponse.ShowModalDialog(new ModalDialogOptions(url)
                {
                    Width = "800",
                    Height = "700",
                    Response = true,
                    ForceDialogSize = true
                });

                args.WaitForPostBack();
            }
            else if (args.HasResult)
            {
                //not got this far yet...
            }
        }
    }
}
