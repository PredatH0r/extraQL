using System;
using System.Net;

namespace ExtraQL
{
  class XWebClient : WebClient
  {
    public int Timeout { get; set; }

    public XWebClient() : this(1000)
    {
    }

    public XWebClient(int timeout)
    {
      this.Timeout = timeout;
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
      var request = base.GetWebRequest(address);
      if (request != null)
        request.Timeout = this.Timeout;
      return request;
    }

    protected override WebResponse GetWebResponse(WebRequest request)
    {
      var response = base.GetWebResponse(request);
      var stream = response == null ? null : response.GetResponseStream();
      if (stream != null && this.Timeout >= 0)
      {
        stream.ReadTimeout = Timeout;
        stream.WriteTimeout = Timeout;
      }
      return response;
    }
  }
}
