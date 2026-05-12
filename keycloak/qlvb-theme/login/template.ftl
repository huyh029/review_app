<#import "lib.ftl" as lib>
<#macro template>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>${msg("loginTitleHtml")}</title>
    <link rel="icon" href="${url.resourcesPath}/img/icons.svg" type="image/svg+xml">
    <link rel="stylesheet" href="${url.resourcesPath}/css/login.css">
</head>
<body>
    <#nested>
</body>
</html>
</#macro>
