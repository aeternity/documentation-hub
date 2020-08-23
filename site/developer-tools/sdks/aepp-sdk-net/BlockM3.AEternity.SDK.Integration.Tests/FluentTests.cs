using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.ClientModels;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using BlockM3.AEternity.SDK.Sophia.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Account = BlockM3.AEternity.SDK.ClientModels.Account;
using Contract = BlockM3.AEternity.SDK.ClientModels.Contract;

namespace BlockM3.AEternity.SDK.Integration.Tests
{
    [TestClass]
    [TestCategory("Integration Tests")]
    public class FluentTests : BaseTest
    {
        [TestMethod]
        public void IdentityContractTest()
        {
            Account account = fluentClient.ConstructAccount(baseKeyPair);
            Contract contract = account.ConstructContract(TestConstants.TestContractSourceCode);

            Assert.AreEqual(TestConstants.TestContractByteCode, contract.ByteCode);

            ContractReturn ret = contract.Deploy(0, 0, 2000000000, 100000).WaitForFinish(TimeSpan.FromSeconds(30));

            ContractReturn<int> re = contract.StaticCall<int>("main", 0, 42);

            Assert.AreEqual(re.ReturnValue, 42);
        }

        [TestMethod]
        public void PaymentSplitContractTest()
        {
            Account account = fluentClient.ConstructAccount(baseKeyPair);
            BaseKeyPair rec1 = BaseKeyPair.Generate();
            BaseKeyPair rec2 = BaseKeyPair.Generate();
            BaseKeyPair rec3 = BaseKeyPair.Generate();
            //map(address, int)
            Dictionary<string, int> input = new Dictionary<string, int>();
            input.Add(rec1.PublicKey, 40);
            input.Add(rec2.PublicKey, 40);
            input.Add(rec3.PublicKey, 20);
            decimal paymentValue = 1m.ToAettos(Unit.AE);
            string paymentSplitterSource = File.ReadAllText(Path.Combine(ResourcePath, "contracts", "PaymentSplitter.aes"), Encoding.UTF8);
            Contract contract = account.ConstructContract(paymentSplitterSource);

            ContractReturn depReturn = contract.MeasureAndDeploy(0, 0, Constants.BaseConstants.MINIMAL_GAS_PRICE, "init", input).WaitForFinish(TimeSpan.FromSeconds(30));
            Assert.IsTrue(depReturn.Events.Any(a => a.Name == "AddingInitialRecipients"));

            ContractReturn callReturn = contract.MeasureAndCall("payAndSplit", Constants.BaseConstants.MINIMAL_GAS_PRICE, (ulong) paymentValue).WaitForFinish(TimeSpan.FromSeconds(30));
            Assert.IsTrue(callReturn.Events.Any(a => a.Name == "PaymentReceivedAndSplitted"));

            Assert.AreEqual(new BigInteger(paymentValue * 0.4m), fluentClient.ConstructAccount(rec1).Balance);
            Assert.AreEqual(new BigInteger(paymentValue * 0.4m), fluentClient.ConstructAccount(rec2).Balance);
            Assert.AreEqual(new BigInteger(paymentValue * 0.2m), fluentClient.ConstructAccount(rec3).Balance);
        }

        [TestMethod]
        public void NameServiceTest()
        {
            Random random = new Random();
            string domain = TestConstants.DOMAIN + random.Next() + TestConstants.NAMESPACE;
            Account account = fluentClient.ConstructAccount(baseKeyPair);
            PreClaim preclaim = account.PreClaimDomain(domain).WaitForFinish(TimeSpan.FromSeconds(30));
            Assert.AreEqual(preclaim.Domain, domain);
            Claim claim = preclaim.ClaimDomain().WaitForFinish(TimeSpan.FromSeconds(30));
            Assert.AreEqual(claim.Domain, domain);
            claim = claim.Update(10000, 50).WaitForFinish(TimeSpan.FromSeconds(30));
            Assert.AreEqual(claim.Domain, domain);
            bool res = claim.Revoke().WaitForFinish(TimeSpan.FromSeconds(30));
            Assert.IsTrue(res);
            Assert.ThrowsException<ApiException<Error>>(() => account.QueryDomain(domain), "Not Found");
        }

        [TestMethod]
        public void OracleTest()
        {
            BaseKeyPair kp = BaseKeyPair.Generate();
            Account oracleAccount = fluentClient.ConstructAccount(kp);
            Account account = fluentClient.ConstructAccount(baseKeyPair);
            ulong money = (ulong) 1m.ToAettos(Unit.AE);
            bool result = account.SendAmount(oracleAccount.KeyPair.PublicKey, money).WaitForFinish(TimeSpan.FromSeconds(30));
            Assert.IsTrue(result);
            oracleAccount.Refresh();
            Assert.AreEqual(oracleAccount.Balance, money);

            OracleServer<CityQuery, TemperatureResponse> query = oracleAccount.RegisterOracle<CityQuery, TemperatureResponse>().WaitForFinish(TimeSpan.FromSeconds(30));
            CityTemperatureService svc = new CityTemperatureService();
            svc.Server = query;
            Task.Factory.StartNew(svc.Start);
            OracleClient<CityQuery, TemperatureResponse> reg = account.GetOracle<CityQuery, TemperatureResponse>(query.OracleId);
            TemperatureResponse resp = reg.Ask(new CityQuery {City = "montevideo"}).WaitForFinish(TimeSpan.FromSeconds(300));
            Assert.AreEqual(resp.TemperatureCelsius, 24);
            resp = reg.Ask(new CityQuery {City = "sofia"}).WaitForFinish(TimeSpan.FromSeconds(30));
            Assert.AreEqual(resp.TemperatureCelsius, 25);
            resp = reg.Ask(new CityQuery {City = "hell"}).WaitForFinish(TimeSpan.FromSeconds(30));
            Assert.AreEqual(resp.TemperatureCelsius, 2000);
            svc.Stop();
        }
    }

    public class CityTemperatureService : OracleService<CityQuery, TemperatureResponse>
    {
        public override TemperatureResponse Answer(CityQuery ask)
        {
            if (ask.City.ToLowerInvariant() == "montevideo")
                return new TemperatureResponse {TemperatureCelsius = 24};
            if (ask.City.ToLowerInvariant() == "sofia")
                return new TemperatureResponse {TemperatureCelsius = 25};
            return new TemperatureResponse {TemperatureCelsius = 2000};
        }
    }

    public class CityQuery
    {
        [SophiaName("city")]
        public string City { get; set; }
    }

    public class TemperatureResponse
    {
        [SophiaName("temp_c")]
        public int TemperatureCelsius { get; set; }
    }
}