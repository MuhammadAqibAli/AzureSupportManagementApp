using AzureSupportManagement.Interface;
using AzureSupportManagement.Models;
using AzureSupportManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSupportManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISupportService _supportService;

        public HomeController(ILogger<HomeController> logger, ISubscriptionService subscriptionService, ISupportService supportService)
        {
            _logger = logger;
            _subscriptionService = subscriptionService;
            _supportService = supportService;
        }

        public IActionResult Index()
        {
            var subscriptions = _subscriptionService.GetSubscriptions();
            return View(subscriptions);
        }

        public IActionResult Tickets(string id)
        {
            var subscription = _subscriptionService.GetSubscriptions().Where(x => x.subscriptionId == id).FirstOrDefault();
            ViewBag.subscriptionName = subscription.displayName;
            ViewBag.subscriptionId = subscription.subscriptionId;
            var data = _supportService.GetSupportTicketList(id);

            return View(data);
        }

        public IActionResult TicketDetail(string SupportTicketId, string Subscription)
        {
            var subscription = _subscriptionService.GetSubscriptions().Where(x => x.subscriptionId == Subscription).FirstOrDefault();
            ViewBag.subscriptionName = subscription.displayName;
            ViewBag.subscriptionId = subscription.subscriptionId;
            var data = _supportService.GetSupportTicketList(Subscription);
            if (data.Count > 0)
            {
                return View(data.Where(x => x.SupportTicketId == SupportTicketId).FirstOrDefault());
            }
            return View();
        }

        public IActionResult Subscriptions()
        {
            var subscriptions = _subscriptionService.GetSubscriptions();
            return View(subscriptions);
        }

        public IActionResult CreateTicket(string SubscriptionId)
        {
            return View();
        }

        public ActionResult GetSubscriptionList()
        {
            var subscriptions = _subscriptionService.GetSubscriptions();
            return Json(JsonConvert.SerializeObject(subscriptions.ToArray()));
        }

        public ActionResult GetClassificationList(string serviceType, string subscriptionId)
        {
            var classifications = _supportService.GetProblemClassifications(serviceType, subscriptionId);
            return Json(JsonConvert.SerializeObject(classifications.ToArray()));
        }

        public ActionResult CreateSupportRequest(Ticket ticket)
        {
            var classifications = _supportService.CreateTicket(ticket);
            return Json(classifications);
        }

        public ActionResult UpdateTicketStatus(UpdateStatus ticket)
        {
            var classifications = _supportService.UpdateTicketStatus(ticket);
            return Json(classifications);
        }

        public ActionResult CreateCommunication(Communication communication)
        {
            var classifications = _supportService.CreateCommunication(communication);
            return Json(classifications);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
