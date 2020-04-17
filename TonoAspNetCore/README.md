# Tono - Tools Of New Operation

"Tools of new operation" library/framework is for agile SoE(SoI) style development.   
Expected a lot of developer create original simulators quickly and find business core rule of improvement continuously. That is why this is named "tools of new operation".

## Libraries

### Tono.AspNetCore
![](https://aqtono.com/tomarika/tono/TonoAspNetCoreIcon.png)  

#### ClientCertAuthentication  
Support client certification to ASP.NET Core application on Azure WebApp.  
To use this object, follow below steps.  

##### At ARM (WebApp)

* ARM -> WebApp -> Settings -> Configulation -> General Settings -> "Incoming client certificates" -> "Require incoming certificate" = ON  
* If necessary, edit "Certificate exclusion paths"  
** NOTE : If you use AzureAD, add call-back urls such as **/signin-oidc** here.  

![](https://aqtono.com/tomarika/tono/TonoAspNetCore/ClientCertSettingOnAzure01.png)  


* ARM -> WebApp -> Settings -> Configulation -> Application Settings  
* Add below Client Certificate information <span style="color: green; ">(1...99)</span>
	1. **ClientCert_<span style="color: green; ">1</span>_CN** = *your-cert-site.azurewebsites.net*
    2. **ClientCert_<span style="color: green; ">1</span>_O** = *YOUR CERT ORGANIZATION*
    3. **ClientCert_<span style="color: green; ">1</span>_Thumbprint** = *A thumbprint of your acceptable client certification*   
![](https://aqtono.com/tomarika/tono/TonoAspNetCore/ClientCertSettingOnAzure02.png)  


* ARM -> WebApp -> Settings -> TLS/SSL Settings -> Protocol Settings
    1. HTTPS Only = On
    2. Incoming client certificates = On
    
![](https://aqtono.com/tomarika/tono/TonoAspNetCore/ClientCertSettingOnAzure03.png)  


##### At Your Code

1. Startup.cs   
    Add **RequireClientCertificate** to your **AuthorizationPolicyBuilder()** like below.  
```
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(AzureADDefaults.AuthenticationScheme).AddAzureAD(options => Configuration.Bind("AzureAd", options));
            services.AddRazorPages().AddMvcOptions(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClientCertificate(() => Configuration) // ADD THIS LINE TO ENABLE CLIENT CERTIFICATES CHECK
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.Configure<MyConfig>(this.Configuration.GetSection("MyConfig"));
        }
```  

2. appsettings.Development.json
    Add client certificate settings for local debug environment.   
```
  "ClientCertConfig": [
    {
      "CN": "mysite.azurewebsites.net",
      "O": "MY SAMPLE CORPORATION",
      "Thumbprint": "12345678901234567890AAAAABBBBBCCCCCDDDDD"
    },
    {
      "CN": "mysite.azurewebsites.net",
      "O": "MY SAMPLE CORPORATION",
      "Thumbprint": "12345678901234567890FFFFFEEEEEDDDDDCCCCC"
    }
  ],
```  
Below style (Azure WebApp style) is also acceptable   

```
  "ClientCert_1_CN": "mysite.azurewebsites.net",
  "ClientCert_1_O": "MY SAMPLE CORPORATION",
  "ClientCert_1_Thumbprint": "12345678901234567890AAAAABBBBBCCCCCDDDDD"
  "ClientCert_2_CN": "mysite.azurewebsites.net",
  "ClientCert_2_O": "MY SAMPLE CORPORATION",
  "ClientCert_2_Thumbprint": "12345678901234567890FFFFFEEEEEDDDDDCCCCC"
```  


