<#import "template.ftl" as layout>
<@layout.template>
    <div class="main-login">
        <div class="img-login">
            <div class="row">
                <div class="col-12">
                    <div class="display-center">
                        <img src="${url.resourcesPath}/img/logo_v01.png" class="logo-v01" alt="Logo">
                    </div>
                </div>
                <div class="col-12">
                    <div class="display-center">
                        <div class="display-login">
                            <div class="title-v01">HỆ THỐNG QLVB & DHTN</div>
                            
                            <#if message?has_content && (message.type != 'warning' || !isAppInitiatedAction??)>
                                <div class="alert-error">
                                    <#if message.summary?has_content>
                                        <#assign msgKey = message.summary />
                                        <#-- Map các message key phổ biến trực tiếp thành tiếng Việt -->
                                        <#assign viMessages = {
                                            "invalidUserMessage": "Tên đăng nhập hoặc mật khẩu không đúng",
                                            "invalidPassword": "Tên đăng nhập hoặc mật khẩu không đúng",
                                            "accountDisabled": "Tài khoản đã bị vô hiệu hóa",
                                            "accountTemporarilyDisabled": "Tài khoản tạm thời bị khóa",
                                            "invalidUsername": "Tên đăng nhập hoặc mật khẩu không đúng",
                                            "invalidUsernameOrEmail": "Tên đăng nhập hoặc mật khẩu không đúng",
                                            "invalidUser": "Tên đăng nhập hoặc mật khẩu không đúng",
                                            "invalidCredentials": "Tên đăng nhập hoặc mật khẩu không đúng",
                                            "userNotFound": "Không tìm thấy người dùng",
                                            "usernameExists": "Tên đăng nhập đã tồn tại",
                                            "emailExists": "Email đã được sử dụng",
                                            "passwordNotSet": "Mật khẩu chưa được thiết lập",
                                            "invalidToken": "Token không hợp lệ",
                                            "sessionExpired": "Phiên đăng nhập đã hết hạn"
                                        } />
                                        <#if viMessages[msgKey]??>
                                            <p>${viMessages[msgKey]}</p>
                                        <#else>
                                            <#-- Thử dùng msg() function, nếu không được thì hiển thị key -->
                                            <#assign translatedMsg = msg(msgKey) />
                                            <#if translatedMsg != msgKey && translatedMsg != "" && translatedMsg?has_content>
                                                <p>${kcSanitize(translatedMsg)?no_esc}</p>
                                            <#else>
                                                <p>${kcSanitize(msgKey)?no_esc}</p>
                                            </#if>
                                        </#if>
                                    <#elseif messageHeaderText?has_content>
                                        <p>${kcSanitize(msg(messageHeaderText))?no_esc}</p>
                                    <#elseif message.type?has_content>
                                        <p>${msg(message.type)}</p>
                                    </#if>
                                </div>
                            </#if>
                            
                            <#if client?? && client.baseUrl?has_content>
                                <a href="${client.baseUrl}">${msg("Quay lại hệ thống QLVB & DHTN")}</a>
                            </#if>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</@layout.template>
