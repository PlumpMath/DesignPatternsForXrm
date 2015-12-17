﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lior.Xrm.Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

using System.Web;

namespace Lior.Xrm.BusinessUnit.BL.ServiceEventFactory
{

    public class SubjectEmailSender
    {
        List<Observer> _observers;
        IOrganizationService _service;
        new_incidentservice _target;

        public SubjectEmailSender(IOrganizationService service, new_incidentservice target)
        {
            _observers = new List<Observer>();
            _service = service;
            _target = target;
        }

        public void Attach(Observer observer)
        {
            if (observer.CanSendEmail())
                _observers.Add(observer);
        }

        public void SendEmailsToTeams(Guid FromWho)
        {
            List<Email> emails = CreateEmails(FromWho);
            foreach (var email in emails)
            {
                SendEmail(email.ActivityId);
            }
            //foreach (Observer o in _observers)
            //{
            //    var email = o.SendEmail(FromWho);
            //    SendEmail(email.ActivityId);
            //}
        }
     
        List<Email> CreateEmails(Guid FromWho)
        {
            List<Email> emails = new List<Email>();
            foreach (Observer o in _observers)
            {
                var email = o.SendEmail(FromWho);
                emails.Add(email);
            }
            return emails;
        }

        void SendEmail(Guid? emailId)
        {
            if (emailId == null) throw new ArgumentNullException("no email created");
            SendEmailRequest request = new SendEmailRequest
            {
                EmailId = emailId.Value,
                TrackingToken = string.Empty,
                IssueSend = true
            };
            _service.Execute(request);
        }

    }

    public class HelperEvent
    {
        new_flightoccurrence _orginalFlightOccurrence = null;
        new_incidentserviceparameter _paramters;
        IOrganizationService _service;
        new_incidentservice _target;
        public HelperEvent(IOrganizationService service, new_incidentservice target)
        {

            _service = service;
            _target = target;
        }

        public new_incidentserviceparameter GetParamters()
        {
            if (_paramters == null)
            {
                var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'> <entity name='new_incidentserviceparameter'><attribute name='new_name' /> <attribute name='new_israelstationteam' /> <attribute name='new_callcenterteam' /> <attribute name='new_cashierteam' /> <attribute name='new_groundoperationsteam' />  <attribute name='new_foodandprovisionsteam' /> <attribute name='new_customerrelationsteam' /> <attribute name='new_scheduleteam' /> <attribute name='new_placingteamsteam' />   <attribute name='new_cargoteam' /><attribute name='new_sundorteam' /> <filter type='and'><condition attribute='statecode' operator='eq' value='0' /></filter></entity></fetch>";
                EntityCollection col = this._service.RetrieveMultiple(new FetchExpression(fetchXml));
                var result = col.Entities.FirstOrDefault();
                if (result != null)
                    _paramters = result.ToEntity<new_incidentserviceparameter>();
            }
            return _paramters;
        }

        public new_flightoccurrence GetOriginalFlight()
        {

            if (_orginalFlightOccurrence == null)
            {
                Guid? flight = null;

                if (_target.Attributes.Contains("new_flight"))
                {
                    if (_target.new_Flight != null)
                        flight = _target.new_Flight.Id;
                    else
                        flight = null;
                }
                if (!flight.HasValue)
                    throw new ArgumentException("There is no original Flight");
                Entity temp = _service.Retrieve("new_flightoccurrence", flight.Value, new ColumnSet(true));
                _orginalFlightOccurrence = temp.ToEntity<new_flightoccurrence>();
            }


            return _orginalFlightOccurrence;
        }
    }

}