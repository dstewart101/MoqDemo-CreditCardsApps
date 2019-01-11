using System;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        [Fact]
        public void AcceptHighIncomeApplications()
        {
            var sut = new CreditCardApplicationEvaluator();
            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 100000
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }



        [Fact]
        public void ReferYoungApplicants()
        {
            var sut = new CreditCardApplicationEvaluator();
            var application = new CreditCardApplication
            {
                Age = 19
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }
    }
}
