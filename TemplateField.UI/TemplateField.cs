using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;

using Newtonsoft.Json.Linq;

using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Data.Items;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Shell.Applications;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditor;
using Sitecore.Data;

using Control = Sitecore.Web.UI.HtmlControls.Control;

namespace iMedia.SC.UI
{
    public class TemplateField : Control, IContentField
    {
        public string Source
        {
            get
            {
                return StringUtil.GetString(ViewState["Source"]);
            }

            set
            {
                Assert.ArgumentNotNull(value, "value");
                ViewState["Source"] = value;
            }
        }

        public static string GetRenderedTemplateHtml(ID TemplateID, JObject ItemData) {
            string returnString = "";
            Sitecore.Data.Templates.Template template = Sitecore.Data.Managers.TemplateManager.GetTemplate(TemplateID, Sitecore.Context.ContentDatabase);

            EditorFormatter eFormatter = new EditorFormatter();

            Sitecore.Data.Templates.TemplateSection[] Sections = template.GetSections();

            HtmlGenericControl templateContainer = new HtmlGenericControl();

            templateContainer.ID = "TemplateContainer";
            templateContainer.TagName = "div";
            templateContainer.Attributes.Add("class", "TemplateContainer");

            Sitecore.Shell.Applications.ContentManager.Editor.Sections sCollection = new Sitecore.Shell.Applications.ContentManager.Editor.Sections();

            foreach (Sitecore.Data.Templates.TemplateSection s in Sections)
            {
                //returnString += "<br /><br /><br /><br />section name: " + s.Name;
                Sitecore.Shell.Applications.ContentManager.Editor.Section section = new Sitecore.Shell.Applications.ContentManager.Editor.Section(s);

                section.DisplayName = "Test";
                section.CollapsedByDefault = false;
                section.Name = "Test";

                sCollection.Add(section);

                Item standardValue = Sitecore.Context.ContentDatabase.GetItem("{15CE90C2-6489-48BE-A8CA-0089DFD5002D}");

                Sitecore.Data.Templates.TemplateField[] fields = s.GetFields();
                foreach (Sitecore.Data.Templates.TemplateField f in fields)
                {
                    Item Owner = Sitecore.Context.ContentDatabase.GetItem(f.ID);

                    Sitecore.Data.Fields.Field fieldItem = standardValue.Fields.Where(fi => fi.Name == f.Name).First();

                    Sitecore.Shell.Applications.ContentManager.Editor.Field field = new Sitecore.Shell.Applications.ContentManager.Editor.Field(fieldItem, f);

                    if (ItemData != null && ItemData[field.TemplateField.Name] != null)
                    {
                        field.Value = ItemData.Value<string>(field.TemplateField.Name);
                    }

                    section.Fields.Add(field);
                }

                eFormatter.IsFieldEditor = true;

                eFormatter.Arguments = new RenderContentEditorArgs();
                eFormatter.Arguments.EditorFormatter = eFormatter;
                eFormatter.Arguments.IsAdministrator = true;
                eFormatter.Arguments.Item = standardValue;
                eFormatter.Arguments.Parent = templateContainer;
                eFormatter.Arguments.ProcessorItem = new ProcessorItem(standardValue);
                eFormatter.Arguments.ReadOnly = false;
                eFormatter.Arguments.RenderTabsAndBars = true;
                eFormatter.Arguments.Sections = sCollection;
                eFormatter.Arguments.ShowInputBoxes = true;
                eFormatter.Arguments.ShowSections = true;
                eFormatter.Arguments.Language = Sitecore.Context.Language;

                eFormatter.RenderSection(section, templateContainer, false);
            }


            using (TextWriter stringWriter = new StringWriter())
            {
                using (HtmlTextWriter renderOnMe = new HtmlTextWriter(stringWriter))
                {
                    templateContainer.RenderControl(renderOnMe);
                    returnString += stringWriter.ToString();
                }
            }

            return returnString;
        }

        public string IDRoot { get; set; }
        public ID TemplateID { get; set; }

        HtmlInputHidden valueInput;
        HtmlGenericControl templateContainer;

        protected override void OnLoad(EventArgs e)
        {
            if (!Sitecore.Context.ClientPage.IsEvent)
            {
                var sourceVals = HttpUtility.ParseQueryString(Source);

                Assert.ArgumentNotNullOrEmpty(sourceVals["templateid"], "templateid");
                string tid = sourceVals["templateid"];

                Guid TemplateGUID = new Guid();

                Assert.IsTrue(Guid.TryParse(tid, out TemplateGUID), "template id is not a guid");

                Item TemplateItem = Sitecore.Context.ContentDatabase.GetItem(new ID(TemplateGUID));

                Assert.IsNotNull(TemplateItem, "Template item not found: " + tid + " database: " + Sitecore.Context.ContentDatabase.Name);

                //Sitecore.Data.Templates.TemplateSection tSection = new Sitecore.Data.Templates.TemplateSection()

                var root = new HtmlGenericControl
                {
                    TagName = "div",
                    ID = GetID("root")
                };
                root.Attributes.Add("class", "RepeatableRoot");
                root.Attributes.Add("templateid", TemplateItem.ID.ToString());

                var ControlContainer = new HtmlGenericControl
                {
                    TagName = "div",
                    ID = GetID("controlContainer")
                };
                ControlContainer.Attributes["class"] = "ControlContainer";

                var newButton = new System.Web.UI.WebControls.HyperLink
                {
                    ID = GetID("newButton"),
                    Text = "Create Item",
                    CssClass = "scContentButton"
                };

                var editButton = new System.Web.UI.WebControls.HyperLink
                {
                    ID = GetID("editButton"),
                    Text = "Edit Item",
                    CssClass = "scContentButton"
                };

                var deleteButon = new System.Web.UI.WebControls.HyperLink
                {
                    ID = GetID("deleteButton"),
                    Text = "Delete Item",
                    CssClass = "scContentButton"
                };

                var upButton = new System.Web.UI.WebControls.HyperLink
                {
                    ID = GetID("upButton"),
                    Text = "Move Up",
                    CssClass = "scContentButton"
                };

                var downButton = new System.Web.UI.WebControls.HyperLink
                {
                    ID = GetID("downButton"),
                    Text = "Move Down",
                    CssClass = "scContentButton"
                };

                valueInput = new HtmlInputHidden
                {
                    ID = GetID("value")
                };

                templateContainer = new HtmlGenericControl
                {
                    TagName = "div",
                    ID = GetID("templateContainer")
                };
                templateContainer.Attributes.Add("class", "TemplateContainer");

                newButton.Attributes.Add("onclick", "return scForm.postEvent(this,event,'repeatabletemplate:opendialog(fieldid=" + valueInput.ID + "&templateid=tid)')");
                editButton.Attributes.Add("onclick", "return scForm.postEvent(this,event,'repeatabletemplate:opendialog(fieldid=" + valueInput.ID + "&templateid=tid&itemindex=getSeletedItemIndex('+"+ templateContainer.ID + "+')&itemdata='+alert($('" + valueInput.ID + "').getValue())+')')");

                ControlContainer.Controls.Add(newButton);
                ControlContainer.Controls.Add(deleteButon);
                ControlContainer.Controls.Add(upButton);
                ControlContainer.Controls.Add(downButton);
                ControlContainer.Controls.Add(valueInput);
                root.Controls.Add(ControlContainer);
                
                /*if (templateContainer == null)
                {
                    var root = FindControl(GetID("root"));
                    if (root != null)
                        templateContainer = (HtmlGenericControl)root.FindControl(GetID("templateContainer"));
                }

                Assert.IsNotNull(templateContainer, "Template container not found");*/

                /*
                [{fields:[{name:"",value:"",type:""}]}];
                */
                templateContainer.Controls.Clear();
                string JSONString = GetValue();
                if (!string.IsNullOrEmpty(JSONString))
                {
                    JArray Items = JArray.Parse(JSONString);
                    for (int i = 0; i < Items.Count; i++)
                    {
                        JObject item = (JObject)Items[i];
                        var itemContainer = new HtmlGenericControl
                        {
                            ID = GetID("Item_" + i),
                            TagName = "div"
                        };
                        string type = item.Value<string>("type");
                        string name = item.Value<string>("name");
                        string value = item.Value<string>("value");

                        Label lblName = new Label
                        {
                            ID = GetID("Item_" + i + "_" + name + "_label"),
                            Value = name + ": "
                        };
                        Label lblValue = new Label
                        {
                            ID = GetID("Item_" + i + "_" + name + "_value"),
                            Value = value
                        };
                        itemContainer.Controls.Add(lblName);
                        itemContainer.Controls.Add(lblValue);
                        templateContainer.Controls.Add(itemContainer);
                    }
                }

                root.Controls.Add(templateContainer);

                this.Controls.Add(root);
            }
            
            base.OnLoad(e);
        }



        public string GetValue()
        {
            if (valueInput != null)
                return valueInput.Value;
            else return "";
        }



        public void SetValue(string value)
        {
            if (valueInput != null)
                valueInput.Value = value;
        }

    }



    public class SubItem
    {
        public ID ParentID { get; internal set; }
        public Dictionary<string, object> Fields { get; internal set; }
        public int OrderIndex { get; internal set; }
        public ID TemplateID { get; internal set; }
    }
}