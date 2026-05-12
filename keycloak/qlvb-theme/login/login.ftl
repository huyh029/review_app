<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Hệ thống QLVB & DHTN - Đăng nhập</title>
  <link rel="icon" type="image/svg+xml" href="${url.resourcesPath}/img/icons.svg">
  <link rel="stylesheet" href="${url.resourcesPath}/css/login.css">
  <style>
    * {
      margin: 0;
      padding: 0;
      box-sizing: border-box;
    }

    body {
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      margin: 0;
      padding: 0;
    }

    .main-login {
      background-color: #285D32;
      min-height: 100vh;
    }

    .img-login {
      background-image: url('${url.resourcesPath}/img/bg.png');
      height: 100vh;
      background-size: cover;
      background-position: center;
      background-repeat: no-repeat;
      width: 100%;
    }

    .row {
      display: flex;
      height: 100vh;
    }

    .col-12 {
      flex: 0 0 50%;
      max-width: 50%;
    }

    .display-center {
      display: grid;
      place-items: center;
      height: 100vh;
    }

    .logo-v01 {
      width: 32%;
      margin-left: 12%;
    }

    .display-login {
      background-color: white;
      padding: 100px 32px;
      border-radius: 4px;
      box-shadow: 0 3px 10px rgba(0, 0, 0, 0.2);
      display: flex;
      flex-flow: column;
      width: 72%;
      max-width: 500px;
      text-align: center;
      margin-right: 12%;
    }

    .title-v01 {
      font-size: xx-large;
      font-weight: 600;
      color: #276100;
      margin-bottom: 52px;
    }

    .input-group {
      position: relative;
      margin-bottom: 18px;
    }

    .input-group .input-icon {
      position: absolute;
      left: 15px;
      top: 50%;
      transform: translateY(-50%);
      color: #666;
      z-index: 2;
      width: 16px;
      height: 16px;
    }

    .input-field {
      width: 100%;
      height: 40px;
      padding: 0 15px 0 45px;
      border: 1px solid #d9d9d9;
      border-radius: 4px;
      font-size: 14px;
      transition: all 0.3s ease;
      background-color: white;
    }

    .input-field:focus {
      outline: none;
      border-color: #1890ff;
      box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.2);
    }

    .input-field::placeholder {
      color: #bfbfbf;
    }

    .btn {
      width: 100%;
      height: 40px;
      border: none;
      border-radius: 4px;
      font-size: 14px;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.3s;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      margin-bottom: 12px;
    }

    .btn .btn-icon {
      width: 16px;
      height: 16px;
      flex-shrink: 0;
    }

    .loading {
      display: none;
    }

    .btn.is-loading .loading {
      display: inline-block;
      animation: spin 1s linear infinite;
    }

    .btn.is-loading .btn-text {
      display: none;
    }

    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }

    .btn-login {
      background: #276100;
      color: white;
      border: 1px solid #276100;
    }

    .btn-login:hover {
      background: #276109;
      border-color: #276109;
    }

    .btn-usb-login {
      background-color: white;
      color: #1890ff;
      border: 2px solid #1890ff;
      font-weight: 500;
    }

    .btn-usb-login:hover {
      background-color: #f0f8ff;
      border-color: #40a9ff;
      color: #40a9ff;
    }

    .btn-usb-login:focus {
      border-color: #40a9ff;
      box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.2);
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .btn-usb-login:disabled {
      color: #8c8c8c;
      border-color: #d9d9d9;
      background-color: white;
    }

    .btn-usb-login:disabled:hover {
      background-color: white;
      border-color: #d9d9d9;
      color: #8c8c8c;
    }

    .login-divider {
      display: flex;
      align-items: center;
      margin: 20px 0;
    }

    .login-divider::before,
    .login-divider::after {
      content: '';
      flex: 1;
      height: 1px;
      background: #d9d9d9;
    }

    .alert-error {
      background-color: #fff2f0;
      border: 1px solid #ffccc7;
      color: #ff4d4f;
      padding: 12px 16px;
      border-radius: 4px;
      margin-bottom: 18px;
      font-size: 14px;
    }

    @media (max-width: 768px) {
      .row {
        flex-direction: column;
        height: auto;
        min-height: 100vh;
      }

      .col-12 {
        flex: 1;
        max-width: 100%;
      }

      .display-center {
        height: auto;
        min-height: 50vh;
        padding: 20px;
      }

      .display-login {
        width: 90%;
        padding: 32px 24px;
        margin-right: 5%;
      }

      .logo-v01 {
        width: 60%;
        margin-left: 20%;
      }

      .title-v01 {
        font-size: large;
        margin-bottom: 32px;
      }
    }

    @media (max-width: 480px) {
      .display-login {
        width: 95%;
        margin-right: 2.5%;
        padding: 24px 16px;
      }

      .logo-v01 {
        width: 70%;
        margin-left: 15%;
      }

      .title-v01 {
        font-size: medium;
        margin-bottom: 24px;
      }
    }
  </style>
</head>
<body>
<div class="main-login">
  <div class="img-login">
    <div class="row">
      <div class="col-12">
        <div class="display-center">
          <img src="${url.resourcesPath}/img/logo_v01.png" class="logo-v01" alt="Logo" onerror="this.style.display='none'">
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
                    "sessionExpired": "Phiên đăng nhập đã hết hạn",
                    "cookieNotFoundMessage": "Lỗi khi đăng nhập"
                  } />
                  <#if viMessages[msgKey]??>
                    <p>${viMessages[msgKey]}</p>
                  <#else>
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

            <form id="kc-form-login" action="${url.loginAction}" method="post">
              <div class="input-group">
                <svg class="input-icon" width="16" height="16" viewBox="0 0 24 24" fill="#666">
                  <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                </svg>
                <input
                        type="text"
                        id="username"
                        name="username"
                        class="input-field"
                        placeholder="${msg("Tên tài khoản hoặc email")}"
                        autofocus
                        autocomplete="off"
                >
              </div>

              <div class="input-group">
                <svg class="input-icon" width="16" height="16" viewBox="0 0 24 24" fill="#666">
                  <path d="M18 8h-1V6c0-2.76-2.24-5-5-5S7 3.24 7 6v2H6c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V10c0-1.1-.9-2-2-2zM9 6c0-1.66 1.34-3 3-3s3 1.34 3 3v2H9V6zm9 14H6V10h12v10z"/>
                </svg>
                <input
                        type="password"
                        id="password"
                        name="password"
                        class="input-field"
                        placeholder="${msg("Mật khẩu")}"
                        autocomplete="off"
                >
              </div>

<#--              <#if realm.rememberMe && !usernameEditDisabled??>-->
<#--                <div class="checkbox">-->
<#--                  <label>-->
<#--                    <#if login.rememberMe??>-->
<#--                      <input tabindex="3" id="rememberMe" name="rememberMe" type="checkbox" checked> ${msg("rememberMe")}-->
<#--                    <#else>-->
<#--                      <input tabindex="3" id="rememberMe" name="rememberMe" type="checkbox"> ${msg("rememberMe")}-->
<#--                    </#if>-->
<#--                  </label>-->
<#--                </div>-->
<#--              </#if>-->

              <input type="hidden" id="id-hidden-input" name="credentialId" <#if auth.selectedCredential?has_content>value="${auth.selectedCredential}"</#if>/>
              <button type="submit" class="btn btn-login" id="kc-login" name="login">
                <i class="loading">⟳</i>
                <span class="btn-text">ĐĂNG NHẬP</span>
              </button>

              <div class="login-divider">
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>
</body>
</html>