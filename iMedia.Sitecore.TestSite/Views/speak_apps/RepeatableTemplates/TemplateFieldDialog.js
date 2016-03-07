define(["sitecore", "require"], function (Sitecore) {
    var TemplateFieldDialog = Sitecore.Definitions.App.extend({
        initialized: function () {
            console.log("Test App");
        }
    });

    return TemplateFieldDialog;
});