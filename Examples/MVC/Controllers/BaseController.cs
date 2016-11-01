using System.Web.Mvc;
using FiftyOne.Foundation.Mobile.Detection;
using MVC.Models;
using System.Web.Routing;

/*
<tutorial>
<p>
The BaseController overrides the initialize method and creates a Match
object. It creates a new instance of the Device model using this match
and makes it available in the ViewBag for use in cshtml pages. It also
exposes the Match object directly via the ViewBag (the more efficient
method).
</p>
<p>
Now any view from a controller extending BaseController can access these
objects like
</p>
<pre class="prettyprint lang-html" style="border:solid 1px">
@ViewBag.Device.IsMobile
@ViewBag.Match.getValue("IsMobile")
</pre>
<p>to print the IsMobile property for Device and Match objects
respectively.</p>
<p>
Using the Match object also makes it easy to access data set properties
and match metrics like,
</p>
<pre class="prettyprint lang-html" style="border:solid 1px">
@ViewBag.Match.DataSet.Published
@ViewBag.Match.Method
</pre>
<p>
to print the data set published date and match method from the Match
object.
</p>
</tutorial>
*/
namespace MVC.Controllers
{
    // Snippet Start
    public class BaseController : Controller
    {
        /// <summary>
        /// If there is an active device detection provider then use this to
        /// add dynamic properties to the ViewBag. 
        /// </summary>
        /// <para>
        /// The Device property of the ViewBag is set to a new MVC model 
        /// instance which copies properties from the device match result.
        /// </para>
        /// <para>
        /// The Match property of the ViewBag is set to directly expose the
        /// match instance returned from device detection.
        /// </para>
        /// <param name="requestContext"></param>
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            
            if (WebProvider.ActiveProvider != null)
            {
                // Perform device detection on the headers provided in the
                // request.
                var match = WebProvider.ActiveProvider.Match(
                    requestContext.HttpContext.Request.Headers);
                
                // Create a model that is based on the match request from
                // device detection.
                ViewBag.Device = new Device(match);

                // Also expose the match result directly in the ViewBag
                // to compare the different access methods when used in the 
                // view.
                ViewBag.Match = match;
            }

            // Get the HTTP headers from the request to display their values in
            // the view.
            ViewBag.RequestHeaders =
                requestContext.HttpContext.Request.Headers;
        } 
    }
    // Snippet End
}

