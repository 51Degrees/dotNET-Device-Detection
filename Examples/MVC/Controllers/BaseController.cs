using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiftyOne.Foundation.Mobile.Detection;
using MVC.Models;
using System.Web.Routing;

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
        /// The Match propety of the ViewBag is set to directly expose the
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

