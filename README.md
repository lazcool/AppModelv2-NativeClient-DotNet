<div class="ua-chrome ProseMirror" contenteditable="true" data-gramm="false"><p>Proof of concept is <a href="https://github.com/SkillsFundingAgency/das-shared-packages/blob/master/SFA.DAS.ApiTokens/SFA.DAS.ApiTokens.Client/ApiKeyHandler.cs">here</a>.</p><p>API authentication is handled differently in Web Api v1 (4.x), Web Api v2, .net core v1 and .net core v2, so the current DAS API's were examined to see which ones currently used static JWT keys, and what framework they used. It seems that all the API's that use JWT are implemented in Web.Api v2, so a solution was investigated that was compatible with that framework.</p><p>Unfortunately, whilst there is plenty online about implementing simultaneous auth schemes on an operation within .net core (e.g. <a href="https://mitchelsellers.com/blogs/2018/03/20/using-multiple-authentication-authorization-providers-in-aspnet-core">1</a><a href="https://mitchelsellers.com/blogs/2018/03/20/using-multiple-authentication-authorization-providers-in-aspnet-core,">,</a> <a href="https://github.com/aspnet/Security/issues/1708">2</a><a href="https://github.com/aspnet/Security/issues/1708,">,</a> <a href="https://stackoverflow.com/questions/45695382/how-do-i-setup-multiple-auth-schemes-in-asp-net-core-2-0)">3</a>)<a href="https://stackoverflow.com/questions/45695382/how-do-i-setup-multiple-auth-schemes-in-asp-net-core-2-0),">,</a> there is very little about implementing the same in legacy Web.Api, and what there is (e.g. <a href="https://stackoverflow.com/questions/28200136/asp-net-web-api-2-controller-with-multiple-authentication-filters">1</a>), wasn't particularly helpful. (Note, in .net core 1, multiple schemes are supported out-of-the-box. In .net core 2, the out-of-box support was removed, but it is still doable.)</p><p>Azure API Management was investigated to see if it supported multiple simultaneous authentication schemes (preferably through policy), but it appears to insist on using its own authentication scheme, and as far as I could tell, doesn't support multiple schemes. (It does provide some very nice features however!)</p><p>If it is determined that it <em>does</em> support multiple schemes, we'd have to <a href="https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-2.2">add OpenAPI support to the API's</a><a href="https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-2.2),">,</a> and <a href="https://docs.microsoft.com/en-us/azure/api-management/transform-api">transform the original API's in the response</a><a href="https://docs.microsoft.com/en-us/azure/api-management/transform-api),">,</a> in addition to adding the API management front-end. So although it has many benefits, it doesn't seem to be able to help us achieve multiple schemes.</p><p>So, the suggestion is to implement multiple scheme support compatible with Web.Api v2, and if there are any straggling DAS API's on v1 (not that I could find any), we'd update them to Web.Api v2.</p><p>The existing DAS API's use a custom Message Handler implemented in the <span style="white-space: pre-wrap;" class="code">das-shared-packages</span> repo, called <span style="white-space: pre-wrap;" class="code">ApiKeyHandler</span>, code <a href="https://github.com/SkillsFundingAgency/das-shared-packages/blob/master/SFA.DAS.ApiTokens/SFA.DAS.ApiTokens.Client/ApiKeyHandler.cs">here</a><a href="https://github.com/SkillsFundingAgency/das-shared-packages/blob/master/SFA.DAS.ApiTokens/SFA.DAS.ApiTokens.Client/ApiKeyHandler.cs).">.</a> I couldn't see a path to enhance the existing <span style="white-space: pre-wrap;" class="code">ApiKeyHandler</span> method to also authorise using Azure Active Directory (AAD), so alternative methods of authenticating using JWT tokens that could also authenticate using AAD were investigated.</p><p>Any replacement of the current JWT authentication method would need to create a principal compatible with the principal that <span style="white-space: pre-wrap;" class="code">ApiKeyHandler</span> currently creates. Also, an AAD authenticator would also have to create a compatible principal. <span style="white-space: pre-wrap;" class="code">ApiKeyHandler</span> creates a principal containing roles and audience (although at least the JWT for commitments v1 API seems to set the audience to what should actually be the subject!).</p><p>There seems to be a myriad of ways of implementing authorisation/authentication in web.api 2, see this <a href="https://msdn.microsoft.com/magazine/dn781361.aspx">article</a><a href="https://msdn.microsoft.com/magazine/dn781361.aspx).">.</a> It should be doable to either have two separate authorization filter attributes, and use them both on an operation (with a small amount of hoop jumping), or create a single attribute that attempts to authenticate using both schemes.</p><p>However, the path I suggest, and for which I have created a proof of concept to demonstrate that the approach works, is to utilise two OWIN authenticators (it is easier to implement AAD auth using it). Note, the AAD authentication method creates a <span style="white-space: pre-wrap;" class="code">ClaimsPrincipal</span>, whereas <span style="white-space: pre-wrap;" class="code">ApiKeyHandler</span> creates a <span style="white-space: pre-wrap;" class="code">GenericPrincipal</span>, but <span style="white-space: pre-wrap;" class="code">ClaimsPrincipal</span> derives from <span style="white-space: pre-wrap;" class="code">GenericPrincipal</span>, so it'll probably be fine, but there is a low probability that it could cause an issue that would need fixing.</p><p>The proof of concept is a forked azure authentication sample, <a href="https://github.com/lazcool/AppModelv2-NativeClient-DotNet/tree/complete">stored in GitHub</a>. It implements both AAD and plain JWT authentication on a single operation. There is a client console app (<a href="https://github.com/lazcool/AppModelv2-NativeClient-DotNet/blob/complete/DoubleTroubleClient/Program.cs">DoubleTroubleClient</a>) that calls the endpoint using <span style="white-space: pre-wrap;" class="code">SFA.DAS.Htpp</span> 's<span style="white-space: pre-wrap;" class="code">WithBearerAuthorisationHeader</span> for both <span style="white-space: pre-wrap;" class="code">JwtBearerTokenGenerator</span> and <span style="white-space: pre-wrap;" class="code">AzureActiveDirectoryBearerTokenGenerator</span> (this mimics how current DAS API clients    set up authentication when calling DAS API’s). The client also calls the endpoint without supplying a bearer authentication header, which receives a <span style="white-space: pre-wrap;" class="code">401 Unauthorized</span> to prove that authentication <em>is</em> taking place.</p><p>To successfully run the sample, you’ll either need to set up AAD app registrations for the client and server, and enter the relevant config into the code, or you can request the client secret from me and use the app registrations I’ve set up in my personal tenant (I didn’t want to check the secret into a public repo).</p><p>The dual authentication method demonstrated in the POC doesn’t use the existing <span style="white-space: pre-wrap;" class="code">ApiKeyHandler</span>. That means that we’d need to change how API’s currently authenticate JWT’s in addition to adding support for AAD, and both methods will need to be compatible with how <span style="white-space: pre-wrap;" class="code">ApiKeyHandler</span> sets up the principal with roles (and potentially the audience, but API’s seem to incorrectly set the audience). Unfortunately, <span style="white-space: pre-wrap;" class="code">ApiKeyHandler</span> doesn’t support roles in the standard manner (supplying a roles array claim), support for which we could get for <a href="https://www.jerriepelser.com/blog/using-roles-with-the-jwt-middleware/">free from OWIN</a>. Instead, we’ll have to implement compatible role handling (picking the roles from a space separated string in a <span style="white-space: pre-wrap;" class="code">data</span> claim) for the new JWT authentication code. I’ve written <a href="https://github.com/lazcool/AppModelv2-NativeClient-DotNet/blob/complete/TodoListService/Controllers/TodoListController.cs">code</a> to demonstrate setting the roles on the principal from the <span style="white-space: pre-wrap;" class="code">data</span> claim, but it’s implemented in the controller’s action. In the productionised code/library, setting the roles would have to be done as part of the pipeline (probably in a filter), for maximum compatibility. We’d also have to make sure it was executed early enough in the pipeline to be compatible with <span style="white-space: pre-wrap;" class="code">das-authorization</span>'s use of roles. Hopefully, we should be able to use AAD’s standard role handling.</p><p>Note that the POC is multi-tenant. We’d have to implement the authentication schemes as single tenant. The sample’s readme tells us how to do that.</p><p><strong>Appendix</strong></p><p><em><strong>Some existing API’s and how they handle authentication:</strong></em></p><p>Recruit AAD	netcoreapp2.2<br><a href="https://github.com/SkillsFundingAgency/das-recruit-api/blob/master/src/Recruit.Api/Startup.ConfigureServices.cs">https://github.com/SkillsFundingAgency/das-recruit-api/blob/master/src/Recruit.Api/Startup.ConfigureServices.cs</a></p><p>events web.api v2<br>ApiKeyHandler</p><p>das-providerevents<br>web.api v2<br>ApiKeyHandler</p><p>das-forecasting-api<br>AAD<br>netcoreapp2.2<br><a href="https://github.com/SkillsFundingAgency/das-forecasting-api/blob/master/src/SFA.DAS.Forecasting.Api/Startup.cs">https://github.com/SkillsFundingAgency/das-forecasting-api/blob/master/src/SFA.DAS.Forecasting.Api/Startup.cs</a></p><p>das-apprenticeship-programs-api<br>web.api v2<br>think no authentication required</p><p>EAS account api<br>web.api v2<br>AAD!!<br><br></p><p><em><strong>Some useful links</strong></em></p><p>OWIN UseWindowsAzureActiveDirectoryBearerAuthentication<br><a href="https://nicksnettravels.builttoroam.com/post-2017-01-28-securing-a-web-api-using-azure-active-directory-and-owin-aspx/">https://nicksnettravels.builttoroam.com/post-2017-01-28-securing-a-web-api-using-azure-active-directory-and-owin-aspx/</a><br>creates ClaimsPrincipal<br><a href="https://stackoverflow.com/questions/32474133/how-does-usewindowsazureactivedirectorybearerauthentication-work-in-validating-t">https://stackoverflow.com/questions/32474133/how-does-usewindowsazureactivedirectorybearerauthentication-work-in-validating-t</a><br>compatible with roles/audience? can we add middleware to get, and make available in same way as ApiKeyHandler?<br><a href="https://github.com/SkillsFundingAgency/das-shared-packages/blob/master/SFA.DAS.ApiTokens/SFA.DAS.ApiTokens.Client/ApiKeyHandler.cs">https://github.com/SkillsFundingAgency/das-shared-packages/blob/master/SFA.DAS.ApiTokens/SFA.DAS.ApiTokens.Client/ApiKeyHandler.cs</a><br>ApiKeyHandler creates and populates a GenericPrincipal, which derives from System.Security.Claims.ClaimsPrincipal<br>OWIN AD creates a ClaimsPrinciple, so hopefully should be ok (does anything use the more derived GenericPrincipal directly?)<br>other ways of implementing AD in web.api apart from OWIN? does any das API use other methods?</p><p>sample aad secured web.api<br><a href="https://github.com/Azure-Samples/active-directory-dotnet-daemon">https://github.com/Azure-Samples/active-directory-dotnet-daemon</a></p><p>what about using the newer Microsoft identity platform. documentation says it's .net/.net core, but all samples are for .net core.<br>is it .net standard? is it really useable in legacy .net? are there samples/docs for using it in legacy .net?</p><p><a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-expose-web-apis">https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-expose-web-apis</a><br>does this sample use the new ms id platform (aad v2)? yes<br><a href="https://github.com/azureadquickstarts/appmodelv2-nativeclient-dotnet">https://github.com/azureadquickstarts/appmodelv2-nativeclient-dotnet</a><br>would need to Restrict access to a single organization (single-tenant), instructions are in readme<br>how to extend? custom validator?</p><p>core how to...<br><a href="https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-2.2&amp;tabs=aspnetcore2x">https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-2.2&amp;tabs=aspnetcore2x</a><br>is this the version of the above for .net? no, core<br><a href="https://jakeydocs.readthedocs.io/en/latest/security/authorization/limitingidentitybyscheme.html">https://jakeydocs.readthedocs.io/en/latest/security/authorization/limitingidentitybyscheme.html</a></p><p>(manual jwt validation if necessary, see <a href="https://github.com/Azure-Samples/active-directory-dotnet-webapi-manual-jwt-validation)">https://github.com/Azure-Samples/active-directory-dotnet-webapi-manual-jwt-validation)</a><br><br></p><p>(how to set owin logging to verbose.. <a href="https://auth0.com/docs/quickstart/backend/webapi-owin/03-troubleshooting)">https://auth0.com/docs/quickstart/backend/webapi-owin/03-troubleshooting)</a></p><p>latest lib: <a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/">https://docs.microsoft.com/en-us/azure/active-directory/develop/</a><br><a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-app-configuration">https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-app-configuration</a></p><p><a href="https://medium.com/agilix/asp-net-core-supporting-multiple-authorization-6502eb79f934">https://medium.com/agilix/asp-net-core-supporting-multiple-authorization-6502eb79f934</a><br><a href="https://stackoverflow.com/questions/19938947/web-api-2-owin-bearer-token-authentication-accesstokenformat-null">https://stackoverflow.com/questions/19938947/web-api-2-owin-bearer-token-authentication-accesstokenformat-null</a><br><a href="https://bitoftech.net/2014/09/12/secure-asp-net-web-api-2-azure-active-directory-owin-middleware-adal/">https://bitoftech.net/2014/09/12/secure-asp-net-web-api-2-azure-active-directory-owin-middleware-adal/</a><span>﻿</span><br></p></div>



---
services: active-directory
platforms: dotnet
author: jmprieur
level: 200
client: Windows Desktop WPF
service: ASP.NET Web API
endpoint: AAD V2
---

# Calling an ASP.NET Web API protected by the Azure AD V2 endpoint from an Windows Desktop (WPF) application

## About this sample

### Scenario

You expose a Web API and you want to protect it so that only authenticated user can access it. This sample shows how to expose a ASP.NET Web API so it can accept tokens issued by personal accounts (including outlook.com, live.com, and others) as well as work and school accounts from any company or organization that has integrated with Azure Active Directory.

The sample also include a Windows Desktop application (WPF) that demonstrate how you can request an access token to access a Web APIs.

## How to run this sample

> Pre-requisites: This sample requires Visual Studio 2017. If you don't have it, download [Visual Studio 2017 for free](https://www.visualstudio.com/downloads/).

### Step 1: Download or clone this sample

You can clone this sample from your shell or command line:

  ```console
  git clone https://github.com/AzureADQuickStarts/AppModelv2-NativeClient-DotNet.git
  ```

### Step 2: Register your Web API - *TodoListService* in the *Application registration portal*

#### Choose the Azure AD tenant where you want to create your applications

If you want to register your apps manually, as a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory**.
   Change your portal session to the desired Azure AD tenant.

#### Register the service app (TodoListService)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `AppModelv2-NativeClient-DotNet-TodoListService`.
   - Change **Supported account types** to **Accounts in any organizational directory**.
   - Select **Register** to create the application.

1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project (`ClientId` in `TodoListService\Web.config`).
1. Select the **Expose an API** section, and:
   - Select **Add a scope**
   - accept the proposed Application ID URI (api://{clientId}) by selecting **Save and Continue**
   - Enter the following parameters:
     - for **Scope name** use `access_as_user`
     - Ensure the **Admins and users** option is selected for **Who can consent**
     - in **Admin consent display name** type `Access TodoListService as a user`
     - in **Admin consent description** type `Accesses the TodoListService Web API as a user`
     - in **User consent display name** type `Access TodoListService as a user`
     - in **User consent description** type `Accesses the TodoListService Web API as a user`
     - Keep **State** as **Enabled**
     - Select **Add scope**

#### Configure your *TodoListService* and *TodoListClient* projects to match the Web API you just registered

1. Open the solution in Visual Studio and then open the **Web.config** file under the root of **TodoListService** project.
1. Replace the value of `ida:ClientId` parameter with the **Client ID (Application Id)** from the application you just registered in the Application Registration Portal.

#### Add the new scope to the *TodoListClient*`s app.config

1. Open the **app.config** file located in **TodoListClient** project's root folder and then paste **Application Id** from the application you just registered for your *TodoListService* under `TodoListServiceScope` parameter, replacing the string `{Enter the Application Id of your TodoListService from the app registration portal}`.

   > Note: Make sure it uses the following format:
   >
   > `api://{TodoListService-Application-Id}/access_as_user` 
   >
   >(where {TodoListService-Application-Id} is the Guid representing the Application Id for your TodoListService).

### Step 3:  Register the client app (TodoListClient)

In this step, you configure your *TodoListClient* project by registering a new application in the Application registration portal. In the cases where the client and server are considered *the same application* you may also just reuse the same application registered in the 'Step 2.'. Using the same application is actually needed if you want users to sign-in with Microsoft personal accounts

#### Register the *TodoListClient* application in the *Application registration portal*

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `NativeClient-DotNet-TodoListClient`.
   - Change **Supported account types** to **Accounts in any organizational directory**.
   - Select **Register** to create the application.
1. From the app's Overview page, select the **Authentication** section.
   - In the **Redirect URLs** | **Suggested Redirect URLs for public clients (mobile, desktop)** section, check **urn:ietf:wg:oauth:2.0:oob**
   - Select **Save**.
1. Select the **API permissions** section
   - Click the **Add a permission** button and then,
   - Select the **My APIs** tab.
   - In the list of APIs, select the `AppModelv2-NativeClient-DotNet-TodoListService API`, or the name you entered for the Web API.
   - Check the **access_as_user** permission if it's not already checked. Use the search box if necessary.
   - Select the **Add permissions** button

#### Configure your *TodoListClient* project

1. In the *Application registration portal*, in the **Overview** page copy the value of the **Application (client) Id**
1. Open the **app.config** file located in the **TodoListClient** project's root folder and then paste the value in the `ida:ClientId` parameter value

### Step 4: Run your project

1. Press `<F5>` to run your project. Your *TodoListClient* should open.
1. Select **Sign in** in the top right and sign in with the same user you have used to register your application, or a user in the same directory.
1. At this point, if you are signing in for the first time, you may be prompted to consent to *TodoListService* Web Api.
1. The sign-in also request the access token to the *access_as_user* scope to access *TodoListService* Web Api and manipulate the *To-Do* list.

### Step 5: Pre-authorize your client application

One of the ways to allow users from other directories to access your Web API is by *pre-authorizing* the client applications to access your Web API by adding the Application Ids from client applications in the list of *pre-authorized* applications for your Web API. By adding a pre-authorized client, you will not require user to consent to use your Web API. Follow the steps below to pre-authorize your Web Application::

1. Go back to the *Application registration portal* and open the properties of your **TodoListService**.
1. In the **Expose an API** section, click on **Add a client application** under the *Authorized client applications* section.
1. In the *Client ID* field, paste the application ID of the `TodoListClient` application.
1. In the *Authorized scopes* section, select the scope for this Web API `api://<Application ID>/access_as_user`.
1. Press the **Add application** button at the bottom of the page.

### Step 6:  Run your project

1. Press `<F5>` to run your project. Your *TodoListClient* should open.
1. Select **Sign in** in the top right (or Clear Cache/Sign-in) and then sign-in either using a personal Microsoft account (live.com or hotmail.com) or work or school account.

## Optional: Restrict sign-in access to your application

By default, when you download this code sample and configure the application to use the Azure Active Directory v2 endpoint following the preceeding steps, both personal accounts - like outlook.com, live.com, and others - as well as Work or school accounts from any organizations that are integrated with Azure AD can request tokens and access your Web API. 

To restrict who can sign in to your application, use one of the options:

### Option 1: Restrict access to a single organization (single-tenant)

You can restrict sign-in access for your application to only user accounts that are in a single Azure AD tenant - including *guest accounts* of that tenant. This scenario is a common for *line-of-business applications*:

1. Open the **App_Start\Startup.Auth** file, and change the value of the metadata endpoint that's passed into the `OpenIdConnectSecurityTokenProvider` to `"https://login.microsoftonline.com/{Tenant ID}/v2.0/.well-known/openid-configuration"` (you can also use the Tenant Name, such as `contoso.onmicrosoft.com`).
2. In the same file, set the `ValidIssuer` property on the `TokenValidationParameters` to `"https://sts.windows.net/{Tenant Id}/"` and the `ValidateIssuer` argument to `true`.

#### Option 2: Use a custom method to validate issuers

You can implement a custom method to validate issuers by using the **IssuerValidator** parameter. For more information about how to use this parameter, read about the [TokenValidationParameters class](https://msdn.microsoft.com/library/system.identitymodel.tokens.tokenvalidationparameters.aspx) on MSDN.
