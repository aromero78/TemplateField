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

        public static string GetRenderedTemplateHtml(string TemplateID, string SelectedItemJSON, string ItemIndex) {
            Guid TemplateGUID = new Guid();
            
            Assert.IsTrue(Guid.TryParse(TemplateID, out TemplateGUID), "template id is not a guid");

            Item TemplateItem = Sitecore.Context.Database.GetItem(new ID(TemplateGUID));

            Assert.IsNotNull(TemplateItem, "Template item not found");

            Sitecore.Data.Templates.Template template = Sitecore.Data.Managers.TemplateManager.GetTemplate(TemplateItem);
                  
            EditorFormatter eFormatter = new EditorFormatter();
                  
            Sitecore.Data.Templates.TemplateSection[] sections = template.GetSections();

            HtmlGenericControl templateContainer = new HtmlGenericControl {
                ID = "TemplateContainer", 
                TagName = "div"
            };

            templateContainer.Attributes.Add("class", "TemplateContainer");
            templateContainer.Attributes.Add("ItemIndex", ItemIndex.ToString());

            JArray SelectItemValues = null;

            if(!string.IsNullOrEmpty(SelectedItemJSON))
                SelectItemValues = JArray.Parse(SelectedItemJSON);

            for (int i = 0; i < sections.Length; i++)
            {
                Sitecore.Data.Templates.TemplateSection s = sections[i];
                Sitecore.Shell.Applications.ContentManager.Editor.Section section = new Sitecore.Shell.Applications.ContentManager.Editor.Section(s);

                if (SelectItemValues != null)
                {
                    foreach (Sitecore.Shell.Applications.ContentManager.Editor.Field field in section.Fields)
                    {
                        if (SelectItemValues[field.TemplateField.Name] != null)
                        {
                            field.Value = SelectItemValues.Value<string>(field.TemplateField.Name);
                            //field.ItemField.Value = SelectItemValues.Value<string>(field.TemplateField.Name);
                        }
                    }
                }

                eFormatter.RenderSection(section, templateContainer, false);
            }

            string returnString = "";
            using (TextWriter stringWriter = new StringWriter())
            {
                using (HtmlTextWriter renderOnMe = new HtmlTextWriter(stringWriter))
                {
                    templateContainer.RenderControl(renderOnMe);
                    returnString = stringWriter.ToString();
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

                Item TemplateItem = Sitecore.Context.Database.GetItem(new ID(TemplateGUID));

                Assert.IsNotNull(TemplateItem, "Template item not found");

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

                var newButton = new Button
                {
                    ID = GetID("newButton"),
                    Value = "New Item",
                    CssClass = "Button"
                };

                var deleteButon = new Button
                {
                    ID = GetID("deleteButton"),
                    Value = "Delete Item",
                    CssClass = "Button"
                };

                var upButton = new Button
                {
                    ID = GetID("upButton"),
                    Value = "Move Up",
                    CssClass = "Button"
                };

                var downButton = new Button
                {
                    ID = GetID("downButton"),
                    Value = "Move Down",
                    CssClass = "Button"
                };

                valueInput = new HtmlInputHidden
                {
                    ID = GetID("value")
                };

                ControlContainer.Controls.Add(newButton);
                ControlContainer.Controls.Add(deleteButon);
                ControlContainer.Controls.Add(upButton);
                ControlContainer.Controls.Add(downButton);
                ControlContainer.Controls.Add(valueInput);
                root.Controls.Add(ControlContainer);

                templateContainer = new HtmlGenericControl
                {
                    TagName = "div",
                    ID = GetID("templateContainer")
                };
                templateContainer.Attributes.Add("class", "TemplateContainer");
                root.Controls.Add(templateContainer);

                this.Controls.Add(root);
            }

            if (templateContainer == null)
            {
                var root = FindControl(GetID("root"));
                if (root != null)
                    templateContainer = (HtmlGenericControl)root.FindControl(GetID("templateContainer"));
            }

            Assert.IsNotNull(templateContainer, "Template container not found");

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