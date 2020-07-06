// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace TonoAspNetCore
{
    /// <summary>
    /// Client Certificate Checker : Authentication module
    /// </summary>
    public class ClientCertAuthentication : AuthorizationHandler<ClientCertAuthentication>, IAuthorizationRequirement
    {
        private readonly List<ClientCertConfig> Configs = null;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="config"></param>
        public ClientCertAuthentication(IConfiguration config)
        {
            Configs = new List<ClientCertConfig>();
            if (config.GetSection("ClientCertConfig").Get(typeof(ClientCertConfig[])) is ClientCertConfig[] configs)
            {
                Configs.AddRange(configs);
            }
            for (var no = 0; no < 99; no++)
            {
                var nameCN = $"ClientCert{(no == 0 ? "" : $"_{no}")}_CN";
                var nameO = $"ClientCert{(no == 0 ? "" : $"_{no}")}_O";
                var nameTp = $"ClientCert{(no == 0 ? "" : $"_{no}")}_Thumbprint";
                var cn = config.GetValue<string>(nameCN);
                var o = config.GetValue<string>(nameO);
                var tp = config.GetValue<string>(nameTp);
                if (string.IsNullOrEmpty(cn) || string.IsNullOrEmpty(o) || string.IsNullOrEmpty(tp))
                {
                    //if (no != 0) break;   // To ignore un-sequence number
                }
                else
                {
                    Configs.Add(new ClientCertConfig
                    {
                        CN = cn,
                        O = o,
                        Thumbprint = tp,
                    });
                }
            }
        }

        /// <summary>
        /// Client Certificate check
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClientCertAuthentication requirement)
        {
            if (Configs.Count == 0)  // Fail safe : No setting No access
            {
                context.Fail();
                return Task.CompletedTask;
            }
            var res = context.Resource as AuthorizationFilterContext;
            if (res == null)
            {
                throw new Exception("ConfirmClientCertificate.HandleRequirementAsync.AuthorizationHandlerContext.Resource should be a AuthorizationFilterContext");
            }
            var clicert = res.HttpContext.Request.Headers["X-ARR-ClientCert"];  // This header parameter have added by Azure WebApp
            if (string.IsNullOrEmpty(clicert))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            try
            {
                var cert = new X509Certificate2(Convert.FromBase64String(clicert));

                foreach (var config in Configs)
                {
                    if (string.Compare(cert.Thumbprint.Trim(), config.Thumbprint, StringComparison.CurrentCultureIgnoreCase) != 0)
                    {
                        continue;
                    }
                    if (DateTime.Compare(DateTime.UtcNow, cert.NotBefore) < 0 || DateTime.Compare(DateTime.UtcNow, cert.NotAfter) > 0)
                    {
                        continue;
                    }
                    var foundSubject = false;
                    var subjects = cert.Subject.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in subjects)
                    {
                        if (string.Compare(s.Trim(), $"CN={config.CN}") == 0)
                        {
                            foundSubject = true;
                            break;
                        }
                    }
                    if (foundSubject == false)
                    {
                        continue;
                    }
                    var foundIssuerCN = false;
                    var foundIssuerO = false;
                    foreach (string s in cert.Issuer.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (foundIssuerCN == false && string.Compare(s.Trim(), $"CN={config.CN}") == 0)
                        {
                            foundIssuerCN = true;
                        }
                        if (foundIssuerO == false && string.Compare(s.Trim(), $"O={config.O}") == 0)
                        {
                            foundIssuerO = true;
                        }
                    }
                    if (foundIssuerCN == false || foundIssuerO == false)
                    {
                        continue;
                    }

                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
            catch (Exception)
            {
            }
            context.Fail();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Setting model POCO 
        /// </summary>
        public class ClientCertConfig
        {
            public string CN { get; set; }
            public string O { get; set; }
            public string Thumbprint { get; set; }
        }
    }

    /// <summary>
    /// Support ClientCertAuthentication to AuthorizationPolicyBuilder
    /// </summary>
    public static class ClientCertAuthenticationExtention
    {
        public static AuthorizationPolicyBuilder RequireClientCertificate(this AuthorizationPolicyBuilder builder, Func<IConfiguration> options)
        {
            return builder.AddRequirements(new ClientCertAuthentication(options?.Invoke()));
        }
    }
}
