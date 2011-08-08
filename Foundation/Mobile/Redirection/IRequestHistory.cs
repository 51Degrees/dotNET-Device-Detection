using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace FiftyOne.Foundation.Mobile.Redirection
{
    interface IRequestHistory
    {
        bool IsPresent(HttpRequest request);
        void Set(HttpRequest request);
        void Remove(HttpRequest request);
    }
}
