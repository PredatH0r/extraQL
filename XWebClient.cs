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
  }
}
