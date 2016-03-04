(function(speak) {
  speak.component({
      name: "TemplateRenderer",
      options: [
          { name: "TemplateId", pluginProperty: "TemplateId" },
          { name: "SelectedItemJSON", pluginProperty: "SelectedItemJSON" },
          { name: "SelectedItemIndex", pluginProperty: "SelectedItemIndex" }
      ],
      events:
      [
          { name: "onSave", on: "onSave" }
      ],
      initialize: function(initial, app, el, sitecore) {
    }
  });
})(Sitecore.Speak);
