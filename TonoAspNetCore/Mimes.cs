// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.StaticFiles;

namespace TonoAspNetCore
{
    public static class Mimes
    {
        public const string BMP = "image/bmp";
        public const string JPEG = "image/jpeg";
        public const string PNG = "image/png";
        public const string TIFF = "image/tiff";
        public const string WEBP = "image/webp";
        public const string ICO = "image/vnd.microsoft.icon";

        public const string MIDI = "audio/midi";
        public const string AAC = "audio/aac";
        public const string WAV = "audio/wav";
        public const string MP3 = "audio/mpeg";

        public const string AVI = "video/x-msvideo";
        public const string WEBM = "video/webm";
        public const string MPEG = "video/mpeg";
        public const string TS = "video/mp2t";  // MPEG transport stream

        public const string PDF = "application/pdf";
        public const string DOC = "application/msword";
        public const string DOCX = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string PPT = "application/vnd.ms-powerpoint";
        public const string PPTX = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        public const string VSD = "application/vnd.visio";
        public const string XLS = "application/vnd.ms-excel";
        public const string XSLX = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string RTF = "application/rtf";

        public const string ZIP = "application/zip";
        public const string RAR = "application/vnd.rar";
        public const string JAR = "application/java-archive";
        public const string Z7 = "application/x-7z-compressed"; // 7Z file

        public const string TTF = "font/ttf";
        public const string EOT = "application/vnd.ms-fontobject";
        public const string OTF = "font/otf";
        public const string WOFF = "font/woff";
        public const string WOFF2 = "font/woff2";

        public const string TXT = "text/plain";
        public const string JSON = "application/json";
        public const string SVG = "image/svg+xml";
        public const string CSS = "text/css";
        public const string CSV = "text/csv";
        public const string JS = "text/javascript";
        public const string XHTML = "application/xhtml+xml";
        public const string XML = "text/xml";

        // For Windows Installer
        public const string APPX = "application/appx";
        public const string MSIX = "application/msix";
        public const string APPXBUNDLE = "application/appxbundle";
        public const string MSIXBUNDLE = "application/msixbundle";
        public const string APPINSTALLER = "application/appinstaller";
        public const string CER = "application/x-x509-ca-cert";

        /// <summary>
        /// Add (override) ContentTypes for MSIX Installer
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="isOverride">true=Override existing value</param>
        /// <returns>the refference of "provider"</returns>
        public static FileExtensionContentTypeProvider AddMsix(FileExtensionContentTypeProvider provider, bool isOverride = false)
        {
            var map = new (string Ext, string ContentType)[]
            {
                (".appx",APPX),
                (".msix",MSIX),
                (".appxbundle",APPXBUNDLE),
                (".msixbundle",MSIXBUNDLE),
                (".appinstaller",APPINSTALLER),
                (".cer",CER),
            };
            foreach (var em in map)
            {
                if (provider.Mappings.ContainsKey(em.Ext))
                {
                    if (isOverride)
                    {
                        provider.Mappings.Remove(em.Ext);
                    }
                    else
                    {
                        continue;
                    }
                }
                provider.Mappings[em.Ext] = em.ContentType;
            }
            return provider;
        }
    }
}
