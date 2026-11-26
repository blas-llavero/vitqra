using System;
using System.Text.RegularExpressions;

namespace Vitqra
{
    public static class VitqraBrowserAddress
    {
        // Syntactic public HTTPS address check. It does not resolve or pin DNS.
        public static bool Valid(string input)
        {
            if(string.IsNullOrWhiteSpace(input)||input.Length>2048||
               !Uri.TryCreate(input.Trim(),UriKind.Absolute,out var uri)||
               uri.Scheme!="https"||!uri.IsDefaultPort||!string.IsNullOrEmpty(uri.UserInfo)||
               uri.HostNameType!=UriHostNameType.Dns||uri.IsLoopback)return false;
            string host=uri.IdnHost.ToLowerInvariant();
            return Regex.IsMatch(host,@"^[a-z0-9.-]+\.[a-z][a-z0-9-]*$")&&
                !Regex.IsMatch(host,@"\.(local|internal|localhost|test|invalid|example|onion|arpa)$");
        }
    }
}
