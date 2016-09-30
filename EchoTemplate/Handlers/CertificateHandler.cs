using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace EchoTemplate.Handlers
{
    public class CertificateValidationHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = await request.Content.ReadAsStringAsync();

            //validate certificate
            var isValid = false;

            if (request.Headers.Contains("Signature") && request.Headers.Contains("SignatureCertChainUrl"))
            {
                //check signature url (https://s3.amazonaws.com/echo.api/echo-api-cert.pem)

                var certUrl = new Uri(request.Headers.GetValues("SignatureCertChainUrl").First().Replace("/../", "/"));
                isValid = ((certUrl.Port == 443 || certUrl.IsDefaultPort)
                    && certUrl.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                    && certUrl.Host.Equals("s3.amazonaws.com", StringComparison.OrdinalIgnoreCase)
                    && certUrl.AbsolutePath.StartsWith("/echo.api/"));

                if (!isValid)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

                byte[] certificate = null;

                //download certificate, cache and compare to signature
                using (var web = new WebClient())
                {
                    certificate = web.DownloadData(certUrl);

                    var cert = new X509Certificate2(certificate);
                    var effectiveDate = DateTime.MinValue;
                    var expiryDate = DateTime.MinValue;
                    var hasSubject = cert.Subject.Contains("CN=echo-api.amazon.com");

                    //Verify that the signing certificate has not expired (examine both the Not Before and Not After dates)
                    if ((DateTime.TryParse(cert.GetExpirationDateString(), out expiryDate)
                        && expiryDate > DateTime.UtcNow)
                        && hasSubject
                        && (DateTime.TryParse(cert.GetEffectiveDateString(), out effectiveDate)
                        && effectiveDate < DateTime.UtcNow))
                    {
                        //Base64 decode the Signature header value on the request to obtain the encrypted signature.
                        var signatureString = request.Headers.GetValues("Signature").First();
                        byte[] signature = Convert.FromBase64String(signatureString);

                        using (var sha1 = new SHA1Managed())
                        {
                            var data = sha1.ComputeHash(Encoding.UTF8.GetBytes(body));
                            var rsa = (RSACryptoServiceProvider)cert.PublicKey.Key;
                            if (rsa != null)
                            {
                                //Compare the asserted hash value and derived hash values to ensure that they match.
                                isValid = rsa.VerifyHash(data, CryptoConfig.MapNameToOID("SHA1"), signature);
                            }
                        }
                    }
                }
            }

            if (!isValid)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

            return await base.SendAsync(request, cancellationToken);
        }
    }
}