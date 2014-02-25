using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using miiCard.Consumers.Service.v1.Financial;

namespace miiCard.Consumers.Service.v1
{
    public class MiiCardOAuthFinancialService : MiiCardOAuthServiceBase
    {
        public MiiCardOAuthFinancialService(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
            : base(consumerKey, consumerSecret, accessToken, accessTokenSecret)
        {

        }

        public MiiCardOAuthFinancialService(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, X509Certificate2 x509certificate, bool useDeclarativeBindings)
            : base(consumerKey, consumerSecret, accessToken, accessTokenSecret, useDeclarativeBindings)
        {

        }

        public MiiCardOAuthFinancialService(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, X509Certificate2 x509certificate, Binding binding, EndpointAddress endpointAddress)
            : base(consumerKey, consumerSecret, accessToken, accessTokenSecret, binding, endpointAddress)
        {

        }

        public MiiApiResponse<bool> IsRefreshInProgress()
        {
            var response = this.GetAuthenticatedServiceMethodResult(x => x.IsRefreshInProgress());

            return new MiiApiResponse<bool>(response.Status, response.ErrorCode, response.ErrorMessage, response.Data, response.IsTestUser);
        }

        public MiiApiResponse<bool> IsRefreshInProgressCreditCards()
        {
            var response = this.GetAuthenticatedServiceMethodResult(x => x.IsRefreshInProgressCreditCards());

            return new MiiApiResponse<bool>(response.Status, response.ErrorCode, response.ErrorMessage, response.Data, response.IsTestUser);
        }

        public MiiApiResponse<FinancialRefreshStatus> RefreshFinancialData()
        {
            var response = this.GetAuthenticatedServiceMethodResult(x => x.RefreshFinancialData());

            return new MiiApiResponse<FinancialRefreshStatus>(response.Status, response.ErrorCode, response.ErrorMessage, response.Data, response.IsTestUser);
        }

        public MiiApiResponse<FinancialRefreshStatus> RefreshFinancialDataCreditCards()
        {
            var response = this.GetAuthenticatedServiceMethodResult(x => x.RefreshFinancialDataCreditCards());

            return new MiiApiResponse<FinancialRefreshStatus>(response.Status, response.ErrorCode, response.ErrorMessage, response.Data, response.IsTestUser);
        }

        public MiiApiResponse<MiiFinancialData> GetFinancialTransactions()
        {
            var response = this.GetAuthenticatedServiceMethodResult(x => x.GetFinancialTransactions());

            return new MiiApiResponse<MiiFinancialData>(response.Status, response.ErrorCode, response.ErrorMessage, response.Data, response.IsTestUser);
        }

        public MiiApiResponse<MiiFinancialData> GetFinancialTransactionsCreditCards()
        {
            var response = this.GetAuthenticatedServiceMethodResult(x => x.GetFinancialTransactionsCreditCards());

            return new MiiApiResponse<MiiFinancialData>(response.Status, response.ErrorCode, response.ErrorMessage, response.Data, response.IsTestUser);
        }

        private T GetAuthenticatedServiceMethodResult<T>(Func<FinancialClient, T> serviceMethodInvoker)
        {
            FinancialClient client = null;
            try
            {
                if (this.UseDeclarativeBindingConfiguration)
                {
                    client = new FinancialClient();
                }
                else
                {
                    client = new FinancialClient(this.Binding, this.EndpointAddress);
                }

                client.Endpoint.Contract = ContractDescription.GetContract(typeof(IFinancial));

                using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = this.GetOAuthHeader(client.Endpoint.Address.Uri, true);
                    return serviceMethodInvoker(client);
                }
            }
            finally
            {
                client.Close();
            }
        }

        protected override EndpointAddress GetDefaultEndpointAddress()
        {
            return new EndpointAddress(ServiceUrls.FinancialServiceEndpoint);
        }
    }
}