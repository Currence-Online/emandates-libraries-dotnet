﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using eMandates.Merchant.Library;
using eMandates.Merchant.Library.Misc;
using eMandates.Merchant.Website.Models;
using eMandates.Merchant.Library.Configuration;

namespace eMandates.Merchant.Website.Controllers
{
    public class ProductsController : Controller
    {
        private readonly List<ProductViewModel> products = new List<ProductViewModel>(new[]
        {
            new ProductViewModel { Id = "1", Price = 20, Name = "Product 1" },
            new ProductViewModel { Id = "2", Price = 20, Name = "Product 2" },
            new ProductViewModel { Id = "3", Price = 20, Name = "Product 3" },
            new ProductViewModel { Id = "4", Price = 20, Name = "Product 4" },
        });

        public ActionResult Index()
        {
            return View(products);
        }

        public ActionResult Buy(string id, string instrumentation)
        {
            var product = products.Find(p => p.Id == id);
            ViewBag.Product = product;

            if (String.IsNullOrEmpty(instrumentation) || instrumentation == "Core") return View(new DirectoryResponseViewModel { Source = new CoreCommunicator().Directory(), Instrumentation = Instrumentation.Core });

            if (instrumentation == "B2B") return View(new DirectoryResponseViewModel { Source = new B2BCommunicator().Directory(), Instrumentation = Instrumentation.B2B });

            throw new Exception("Instrumentation should either be Core or B2B.");
        }

        [HttpPost]
        public ActionResult Transaction()
        {
            return View(new NewMandateRequestViewModel { Instrumentation = Instrumentation.Core });
        }

        [HttpPost]
        public ActionResult NewMandateResult(NewMandateRequestViewModel model)
        {
            if (String.IsNullOrEmpty(model.Source.MessageId))
            {
                model.Source.MessageId = MessageIdGenerator.New();
            }

            if (model.Instrumentation == Instrumentation.Core) return View(new NewMandateResponseViewModel { Source = new CoreCommunicator().NewMandate(model.Source), Instrumentation = Instrumentation.Core });
            if (model.Instrumentation == Instrumentation.B2B) return View(new NewMandateResponseViewModel { Source = new B2BCommunicator().NewMandate(model.Source), Instrumentation = Instrumentation.B2B });

            throw new Exception("Instrumentation should either be Core or B2B.");
        }

        [HttpGet]
        public ActionResult Status(string trxid, string instrumentation)
        {
            if (String.IsNullOrEmpty(trxid)) throw new Exception("trxid must have a value.");

            if (String.IsNullOrEmpty(instrumentation) || instrumentation == "Core") return GetStatusResponse(trxid, Instrumentation.Core);

            if (instrumentation == "B2B") return GetStatusResponse(trxid, Instrumentation.B2B);


            throw new Exception("Instrumentation should either be Core or B2B.");
        }

        [HttpPost]
        public ActionResult Status(StatusRequestViewModel model)
        {
            return GetStatusResponse(model.Source.TransactionId, model.Instrumentation);
        }

        private ActionResult GetStatusResponse(string trxid, Instrumentation instrumentation)
        {
            if (instrumentation == Instrumentation.Core) return View(new CoreCommunicator().GetStatus(new StatusRequest(trxid)));

            if (instrumentation == Instrumentation.B2B) return View(new B2BCommunicator().GetStatus(new StatusRequest(trxid)));

            throw new Exception("Instrumentation should either be Core or B2B.");
        }


        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Amend()
        {
            return View(new AmendmentRequestViewModel { Instrumentation = Instrumentation.Core });
        }

        [HttpPost]
        public ActionResult AmendMandateResult(AmendmentRequestViewModel model)
        {
            if (String.IsNullOrEmpty(model.Source.MessageId))
            {
                model.Source.MessageId = MessageIdGenerator.New();
            }

            if (model.Instrumentation == Instrumentation.Core) return View(new AmendmentResponseViewModel { Source = new CoreCommunicator().Amend(model.Source), Instrumentation = Instrumentation.Core });
            if (model.Instrumentation == Instrumentation.B2B) return View(new AmendmentResponseViewModel { Source = new B2BCommunicator().Amend(model.Source), Instrumentation = Instrumentation.B2B });

            throw new Exception("Instrumentation should either be Core or B2B.");
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Cancel()
        {
            return View(new CancellationRequest());
        }

        [HttpPost]
        public ActionResult CancelMandateResult(CancellationRequest model)
        {
            if (String.IsNullOrEmpty(model.MessageId))
            {
                model.MessageId = MessageIdGenerator.New();
            }

            var response = new B2BCommunicator().Cancel(model);
            return View(response);
        }
    }
}